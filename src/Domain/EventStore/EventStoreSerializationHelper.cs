using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.EventStore
{
    internal static class GetEventStoreSerializationHelper
    {
        public const string EventClrTypeHeader = "EventClrTypeName";
        public const string AggregateClrTypeHeader = "AggregateClrTypeName";
        public const string CommitIdHeader = "CommitId";
        private static readonly JsonSerializerSettings SerializerSettings;

        static GetEventStoreSerializationHelper()
        {
            SerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
        }

        public static EventData ToEventData(Guid eventId, object evnt, IDictionary<string, object> headers)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(evnt, SerializerSettings));

            var eventHeaders = new Dictionary<string, object>(headers)
            {
                {
                    EventClrTypeHeader, evnt.GetType().AssemblyQualifiedName
                }
            };
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, SerializerSettings));
            var typeName = evnt.GetType().Name;

            return new EventData(eventId, typeName, true, data, metadata);
        }

        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (!char.IsUpper(s[0]))
                return s;

            string camelCase = char.ToLowerInvariant(s[0]).ToString();
            if (s.Length > 1)
                camelCase += s.Substring(1);

            return camelCase;
        }

        public static bool TryDeserializeAggregateEvent(this ResolvedEvent rawEvent, out object deserializedEvent)
        {
            deserializedEvent = null;

            if (rawEvent.OriginalEvent.EventType.StartsWith("$") || rawEvent.OriginalEvent.EventStreamId.StartsWith("$"))
                return false;

            IDictionary<string, JToken> metadata;
            try
            {
                metadata = JObject.Parse(Encoding.UTF8.GetString(rawEvent.OriginalEvent.Metadata));
            }
            catch (JsonReaderException)
            {
                return false;
            }

            if (!metadata.ContainsKey("EventClrTypeName"))
                return false;

            Type deserializeTo;
            try
            {
                deserializeTo = Type.GetType((string)metadata["EventClrTypeName"], true);
            }
            catch (Exception) //TODO be more specific here
            {
                return false;
            }

            try
            {
                var jsonString = Encoding.UTF8.GetString(rawEvent.OriginalEvent.Data);
                deserializedEvent = JsonConvert.DeserializeObject(jsonString, deserializeTo);
                return deserializedEvent != null;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private static object ProcessRawEvent(ResolvedEvent rawEvent)
        {
            if (rawEvent.OriginalEvent.Metadata.Length > 0 && rawEvent.OriginalEvent.Data.Length > 0)
                return DeserializeEvent(rawEvent.OriginalEvent.Metadata, rawEvent.OriginalEvent.Data);
            return null;
        }

        /// <summary>
        /// Deserializes the event from the raw GetEventStore event to my event.
        /// Took this from a gist that James Nugent posted on the GetEventStore forumns.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object DeserializeEvent(byte[] metadata, byte[] data)
        {
            var eventClrTypeName = JObject.Parse(Encoding.UTF8.GetString(metadata)).Property("EventClrTypeName").Value;
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType((string)eventClrTypeName));
        }

    }
}