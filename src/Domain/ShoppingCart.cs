using System;
using System.Collections.Generic;
using Domain.Core;
using Domain.Events;

namespace Domain
{
    public class ShoppingCart : AggregateBase
    {
        private bool _checkedOut;
        private Items _items;

        public ShoppingCart(Guid id)
            : this()
        {
            ApplyChange(new ShoppingCartCreated(id));
        }

        private ShoppingCart()
        {
            Register<ShoppingCartCreated>(_ => 
            {
                Id = _.CartId;
                _checkedOut = false;
                _items = new Items();
            });
            Register<ShoppingCartCheckedOut>(_ => _checkedOut = true);
            Register<ItemAddedToCart>(_ => _items.Add(new Item(_.ItemId)));
            Register<ItemRemovedFromCart>(_ => _items.Remove(_.ItemId));
            Register<ItemQuantityChanged>(_ => _items.Get(_.ItemId).ChangeQuantity(_.Quantity));
        }

        public void AddItem(Guid itemId)
        {
            ThrowIfCheckedOut();
            ApplyChange(new ItemAddedToCart(Id, itemId));
        }

        public void ChangeItemQuantity(Guid itemId, int quantity)
        {
            ThrowIfCheckedOut();
            ThrowIfItemNotInCart(itemId);

            if (quantity == 0)
                RemoveItem(itemId);            
            else
                ApplyChange(new ItemQuantityChanged(Id, itemId, quantity));
        }

        public void RemoveItem(Guid itemId)
        {
            ThrowIfCheckedOut();
            ThrowIfItemNotInCart(itemId);
            ApplyChange(new ItemRemovedFromCart(Id, itemId));
        }

        public void Checkout()
        {
            if (_checkedOut) return;
            ApplyChange(new ShoppingCartCheckedOut(Id));
        }

        private void ThrowIfCheckedOut()
        {
            if (_checkedOut)
                throw new InvalidOperationException("Shopping cart is already checked out");
        }

        private void ThrowIfItemNotInCart(Guid itemId)
        {
            if (!_items.Contains(itemId))
                throw new ArgumentException("Item $itemId not found in the cart", "itemId");
        }


        class Items
        {
            private readonly List<Item> _items;

            public Items()
            {
                _items = new List<Item>();
            }

            public void Add(Item item)
            {
                _items.Add(item);
            }

            public void Remove(Guid itemId)
            {
                _items.Remove(_items.Find(_ => _.ItemId == itemId));
            }

            public bool Contains(Guid itemId)
            {
                return _items.Exists(_ => _.ItemId == itemId);
            }

            public Item Get(Guid itemId)
            {
                return _items.Find(_ => _.ItemId == itemId);
            }
        }

        class Item
        {
            private readonly Guid _itemId;
            private int _quantity;

            public Item(Guid itemId)
            {
                _itemId = itemId;
                _quantity = 1;
            }

            public Guid ItemId { get { return _itemId; } }

            public void ChangeQuantity(int quantity)
            {
                _quantity = quantity;
            }
        }
    }
}