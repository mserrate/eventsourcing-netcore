using System;

namespace Domain.Events
{
    public class ItemIncremented
    {
        public Guid ItemId { get; }
        public Guid CartId { get; }

        public ItemIncremented(Guid cartId, Guid itemId)
        {
            CartId = cartId;
            ItemId = itemId;
        }
    }
}