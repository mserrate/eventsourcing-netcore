using System;

namespace Domain.Events
{
    public class ShoppingCartCheckedOut
    {
        public Guid CartId { get; }

        public ShoppingCartCheckedOut(Guid cartId)
        {
            CartId = cartId;
        }
    }
}