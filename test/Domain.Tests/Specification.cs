using System;
using System.Linq;
using System.Collections.Generic;
using Domain.Core;
using Xunit;

namespace Domain.Tests
{
    public abstract class Specification<T>
        where T : AggregateBase
    {
        protected abstract T Given();
        protected abstract void When();
        protected T aggregate;
        protected List<object> producedEvents;
        protected Exception caught;

        public Specification()
        {
            try
            {
                aggregate = Given();
                When();
                producedEvents = (aggregate as IAggregate).GetUncommittedEvents().Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                caught = ex;
            }
        }
        
    }
}