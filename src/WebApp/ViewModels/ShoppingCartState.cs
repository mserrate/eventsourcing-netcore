using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Domain.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using static WebApp.ViewModels.ShoppingCartViewModel;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WebApp.ViewModels
{

    public class ShoppingCartViewModel
    {
        public Guid Id { get; set; }
        public bool IsCheckOut { get; set; }
        public List<Item> Items { get; }
        public decimal Total { get { return Items.Sum(i => i.Subtotal); } }

        public ShoppingCartViewModel()
        {
            Items = new List<Item>();
        }

        public class Item
        {
            public Guid ItemId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal { get { return Price * Quantity; }  }
        }
    }

    public class ShoppingCartState : IModelState<ShoppingCartViewModel>
    {
        private readonly IEventStoreConnection _connection;
        private readonly ProductsCache _cache;

        public ShoppingCartState(IEventStoreConnection connection, ProductsCache cache)
        {
            _connection = connection;
            _cache = cache;
        }

        public async Task<ShoppingCartViewModel> GetCurrentState(Guid id)
        {
            var streamEvents = new List<ResolvedEvent>();

            StreamEventsSlice currentSlice;
            var nextSliceStart = StreamPosition.Start;
            do
            {
                currentSlice =
                    await _connection.ReadStreamEventsForwardAsync(
                        string.Format("shoppingCart-{0}", id.ToString("N")), 
                        nextSliceStart,
                        200, 
                        false);

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            var viewModel = new ShoppingCartViewModel();

            //TODO: refactor this... disgusting...
            foreach (var e in currentSlice.Events)
            {
                var @event = DeserializeEvent(e.OriginalEvent.Metadata, e.OriginalEvent.Data);
                Console.WriteLine("type: " + @event.GetType());
                if (@event.GetType() == typeof(ShoppingCartCreated))
                {
                    var t = (ShoppingCartCreated)@event;
                    viewModel.Id = t.CartId;
                    viewModel.IsCheckOut = false;
                } 
                else if (@event.GetType() == typeof(ItemAddedToCart))
                {
                    var t = (ItemAddedToCart)@event;
                    viewModel.Id = t.CartId;
                    var item = _cache.GetPoduct(t.ItemId);
                    viewModel.Items.Add(new Item { ItemId = item.Id, Name = item.Name, Description = item.Description, Quantity = 1, Price = item.Price });
                } 
                else if (@event.GetType() == typeof(ItemRemovedFromCart))
                {
                    var t = (ItemRemovedFromCart)@event;
                    var item = viewModel.Items.Where(x => x.ItemId == t.ItemId).SingleOrDefault();
                    viewModel.Items.Remove(item);
                } 
                else if (@event.GetType() == typeof(ItemQuantityChanged)) 
                {
                    var t = (ItemQuantityChanged)@event;
                    var item = viewModel.Items.Where(x => x.ItemId == t.ItemId).SingleOrDefault();
                    item.Quantity = t.Quantity;
                } 
                else if (@event.GetType() == typeof(ShoppingCartCheckedOut)) 
                {
                    viewModel.IsCheckOut = true;
                } 
                else 
                {
                    throw new Exception("Unknown Event Type");
                }
            }

            return viewModel;
        }

        public static object DeserializeEvent(byte[] metadata, byte[] data)
        {
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property("EventClrTypeName").Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName));
        }
    }
}