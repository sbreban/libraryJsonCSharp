using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace networking
{
    abstract class JsonCreationConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
