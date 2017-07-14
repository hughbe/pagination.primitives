using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pagination.Elasticsearch
{
    public class PagedRequestBuilderJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            PagedRequestBuilder instance = (PagedRequestBuilder)Activator.CreateInstance(objectType);
            serializer.Populate(jObject.CreateReader(), instance);

            instance.LoadFrom(jObject);
            return instance;
        }

        public override bool CanConvert(Type objectType) => true;
    }
}
