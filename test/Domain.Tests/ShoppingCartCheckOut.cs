using System;
using System.Linq;
using Domain.Core;
using Domain.Events;
using Xunit;

namespace Domain.Tests
{
    public class ShoppingCartCheckOut
    {
        public class When_CheckingOut_a_ShoppingCart : Specification<ShoppingCart>
        {
            protected override ShoppingCart Given()
            {
                return new ShoppingCart(Guid.NewGuid());
            }

            protected override void When()
            {
                aggregate.Checkout();
            }

            [Fact]
            public void ShopingCart_is_CheckedOut()
            {
                Assert.IsType<ShoppingCartCheckedOut>(producedEvents.Last());
            }
        }

        public class When_Adding_Item_To_CheckedOut_ShoppingCart : Specification<ShoppingCart>
        {
            protected override ShoppingCart Given()
            {
                var agg = new ShoppingCart(Guid.NewGuid());
                agg.Checkout();
                return agg;
            }

            protected override void When()
            {
                aggregate.AddItem(Guid.NewGuid());
            }

            [Fact]
            public void Exception_Is_Thrown()
            {
                Assert.IsType<InvalidOperationException>(caught);
            }
        }

        public class When_Removing_Item_To_CheckedOut_ShoppingCart : Specification<ShoppingCart>
        {
            protected override ShoppingCart Given()
            {
                var agg = new ShoppingCart(Guid.NewGuid());
                agg.Checkout();
                return agg;
            }

            protected override void When()
            {
                aggregate.RemoveItem(Guid.NewGuid());
            }

            [Fact]
            public void Exception_Is_Thrown()
            {
                Assert.IsType<InvalidOperationException>(caught);
            }
        }        
    }
}