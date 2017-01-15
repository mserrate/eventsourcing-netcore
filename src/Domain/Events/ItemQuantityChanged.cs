using System;

namespace Domain.Events
{
    public class ItemQuantityChanged
    {
        public Guid ItemId { get; }
        public Guid CartId { get; }
        public int Quantity { get; }

        public ItemQuantityChanged(Guid cartId, Guid itemId, int quantity)
        {
            CartId = cartId;
            ItemId = itemId;
            Quantity = quantity;
        }
    }
}