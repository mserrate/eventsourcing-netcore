using System;

namespace Domain.Events
{
    public class ItemRemovedFromCart
    {
        public Guid ItemId { get; }
        public Guid CartId { get; }

        public ItemRemovedFromCart(Guid cartId, Guid itemId)
        {
            CartId = cartId;
            ItemId = itemId;
        }
    }
}