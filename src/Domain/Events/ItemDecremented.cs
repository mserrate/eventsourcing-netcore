using System;

namespace Domain.Events
{
    public class ItemDecremented
    {
        public Guid ItemId { get; }
        public Guid CartId { get; }

        public ItemDecremented(Guid cartId, Guid itemId)
        {
            CartId = cartId;
            ItemId = itemId;
        }
    }
}