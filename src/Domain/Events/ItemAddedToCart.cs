using System;

namespace Domain.Events
{
    public class ItemAddedToCart
    {
        public Guid ItemId { get; }
        public Guid CartId { get; }

        public ItemAddedToCart(Guid cartId, Guid itemId)
        {
            CartId = cartId;
            ItemId = itemId;
        }
    }
}