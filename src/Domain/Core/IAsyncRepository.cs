namespace Domain.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAsyncRepository
    {
        Task<TAggregate> GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate;
        Task<TAggregate> GetById<TAggregate>(Guid id, int version) where TAggregate : class, IAggregate;
        Task Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders);
        Task Save(IAggregate aggregate);
    }
}