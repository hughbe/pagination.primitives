using System;
using System.Collections.Generic;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pagination.Elasticsearch
{
    public class SortJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(IList<SortField>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var fields = new List<SortField>();

            JArray sortFields = JArray.Load(reader);
            foreach (JObject sortField in sortFields.Children<JObject>())
            {
                foreach (var field in sortField)
                {
                    string ordering = field.Value.Value<string>("order");
                    fields.Add(new SortField
                    {
                        UnmappedType = FieldType.Keyword,
                        Field = field.Key,
                        Order = ordering != "asc" ? SortOrder.Descending : SortOrder.Ascending
                    });
                }
            }

            return fields;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IList<SortField> fields = (IList<SortField>)value;

            writer.WriteStartArray();
            foreach (SortField field in fields)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("field");
                serializer.Serialize(writer, field.Field.Name);

                writer.WritePropertyName("order");
                serializer.Serialize(writer, field.Order?.ToString() ?? "Descending");

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }

}
