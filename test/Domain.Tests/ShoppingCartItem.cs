using System;
using System.Linq;
using Domain.Core;
using Domain.Events;
using Xunit;

namespace Domain.Tests
{
    public class ShoppingCartItem
    {
        public class When_Changing_Unexisting_Item_Quantity : Specification<ShoppingCart>
        {
            protected override ShoppingCart Given()
            {
                return new ShoppingCart(Guid.NewGuid());
            }

            protected override void When()
            {
                aggregate.RemoveItem(Guid.NewGuid());
            }

            [Fact]
            public void Exception_Is_Thrown()
            {
                Assert.IsType<ArgumentException>(caught);
            }
        }

        public class When_Changing_Item_Quantity_To_Zero : Specification<ShoppingCart>
        {
            private Guid _itemId;

            protected override ShoppingCart Given()
            {
                var agg =  new ShoppingCart(Guid.NewGuid());
                _itemId = Guid.NewGuid();
                agg.AddItem(_itemId);
                return agg;
            }

            protected override void When()
            {
                aggregate.ChangeItemQuantity(_itemId, 0);
            }

            [Fact]
            public void Item_Is_Removed()
            {
                Assert.IsType<ItemRemovedFromCart>(producedEvents.Last());
            }
        }
    }
}