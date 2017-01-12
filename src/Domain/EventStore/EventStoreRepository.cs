using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Core;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Domain.EventStore
{
    public class EventStoreRepository : IAsyncRepository
    {
        private const int WritePageSize = 500;
        private const int ReadPageSize = 500;

        private readonly Func<Type, Guid, string> _aggregateIdToStreamName;
        private readonly IEventStoreConnection _eventStoreConnection;        

        public EventStoreRepository(IEventStoreConnection eventStoreConnection)
            : this(eventStoreConnection, (t, g) => string.Format("{0}-{1}", t.Name.ToCamelCase(), g.ToString("N")))
        {
        }

        public EventStoreRepository(IEventStoreConnection eventStoreConnection, Func<Type, Guid, string> aggregateIdToStreamName)
        {
            _eventStoreConnection = eventStoreConnection;
            _aggregateIdToStreamName = aggregateIdToStreamName;
        }

        public async Task<TAggregate> GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
        {
            return await GetById<TAggregate>(id, int.MaxValue);
        }

        public async Task<TAggregate> GetById<TAggregate>(Guid id, int version) where TAggregate : class, IAggregate
        {
            if (version <= 0)
                throw new InvalidOperationException("Cannot get version <= 0");

            var streamName = _aggregateIdToStreamName(typeof(TAggregate), id);
            var aggregate = ConstructAggregate<TAggregate>();

            var sliceStart = 0;
            StreamEventsSlice currentSlice;
            do
            {
                var sliceCount = sliceStart + ReadPageSize <= version
                                    ? ReadPageSize
                                    : version - sliceStart;

                currentSlice = await _eventStoreConnection.ReadStreamEventsForwardAsync(streamName, sliceStart, sliceCount, false);

                if (currentSlice.Status == SliceReadStatus.StreamNotFound)
                    throw new AggregateNotFoundException(id, typeof(TAggregate));

                if (currentSlice.Status == SliceReadStatus.StreamDeleted)
                    throw new AggregateDeletedException(id, typeof(TAggregate));

                sliceStart = currentSlice.NextEventNumber;

                foreach (var evnt in currentSlice.Events)
                    aggregate.ApplyEvent(GetEventStoreSerializationHelper.DeserializeEvent(evnt.OriginalEvent.Metadata, evnt.OriginalEvent.Data));
            }
            while (version > currentSlice.NextEventNumber && !currentSlice.IsEndOfStream);

            if (aggregate.Version != version && version < Int32.MaxValue)
                throw new AggregateVersionException(id, typeof(TAggregate), aggregate.Version, version);

            return aggregate;
        }

        private static TAggregate ConstructAggregate<TAggregate>()
        {
            return (TAggregate)Activator.CreateInstance(typeof(TAggregate), true);
        }

        public async Task Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
        {
            var commitHeaders = new Dictionary<string, object>
            {
                {GetEventStoreSerializationHelper.CommitIdHeader, commitId},
                {GetEventStoreSerializationHelper.AggregateClrTypeHeader, aggregate.GetType().AssemblyQualifiedName}
            };
            updateHeaders(commitHeaders);

            var streamName = _aggregateIdToStreamName(aggregate.GetType(), aggregate.Id);
            var newEvents = aggregate.GetUncommittedEvents().Cast<object>().ToList();
            var originalVersion = aggregate.Version - newEvents.Count;
            var expectedVersion = originalVersion == 0 ? ExpectedVersion.NoStream : originalVersion - 1;
            var eventsToSave = newEvents.Select(e => GetEventStoreSerializationHelper.ToEventData(Guid.NewGuid(), e, commitHeaders)).ToList();

            if (eventsToSave.Count < WritePageSize)
            {
                await _eventStoreConnection.AppendToStreamAsync(streamName, expectedVersion, eventsToSave);
            }
            else
            {
                var transaction = await _eventStoreConnection.StartTransactionAsync(streamName, expectedVersion);

                var position = 0;
                while (position < eventsToSave.Count)
                {
                    var pageEvents = eventsToSave.Skip(position).Take(WritePageSize);
                    await transaction.WriteAsync(pageEvents);
                    position += WritePageSize;
                }

                await transaction.CommitAsync();
            }

            aggregate.ClearUncommittedEvents();
        }

        public async Task Save(IAggregate aggregate)
        {
            await Save(aggregate, Guid.NewGuid(), d => { });
        }
    }
}