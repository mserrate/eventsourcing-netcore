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

        public ShoppingCartViewModel()
        {
            Items = new List<Item>();
        }

        public class Item
        {
            public Guid ItemId { get; set; }
            public int Count { get; set; }
        }
    }

    public class ShoppingCartState : IModelState<ShoppingCartViewModel>
    {
        private readonly IEventStoreConnection _connection;

        public ShoppingCartState(IEventStoreConnection connection)
        {
            _connection = connection;
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
                    viewModel.Items.Add(new Item { ItemId = t.ItemId});
                } 
                else if (@event.GetType() == typeof(ItemRemovedFromCart))
                {
                    var t = (ItemRemovedFromCart)@event;
                    var item = viewModel.Items.Where(x => x.ItemId == t.ItemId).SingleOrDefault();
                    viewModel.Items.Remove(item);
                } 
                else if (@event.GetType() == typeof(ItemIncremented)) 
                {
                    var t = (ItemIncremented)@event;
                    var item = viewModel.Items.Where(x => x.ItemId == t.ItemId).SingleOrDefault();
                    item.Count++;
                } 
                else if (@event.GetType() == typeof(ItemDecremented)) 
                {
                    var t = (ItemDecremented)@event;
                    var item = viewModel.Items.Where(x => x.ItemId == t.ItemId).SingleOrDefault();
                    item.Count--;
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