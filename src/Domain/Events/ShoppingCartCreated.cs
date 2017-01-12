using System;

namespace Domain.Events
{
    public class ShoppingCartCreated
    {
        public Guid CartId { get; }

        public ShoppingCartCreated(Guid cartId)
        {
            CartId = cartId;
        }
    }
}