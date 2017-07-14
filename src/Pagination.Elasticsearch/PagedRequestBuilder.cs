using System.Collections.Generic;
using System.Linq;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pagination.Elasticsearch
{
    [JsonConverter(typeof(PagedRequestBuilderJsonConverter))]
    public abstract class PagedRequestBuilder : PagedRequest
    {
        internal void LoadFrom(JObject jObject)
        {
            foreach (RequestFieldDescriptor fieldDescriptor in AllowedTerms)
            {
                JToken token = jObject.GetValue(fieldDescriptor.RequestFieldName);
                if (token == null)
                {
                    continue;
                }

                if (token.Type == JTokenType.Array)
                {
                    string[] values = token.Values<string>().ToArray();
                    if (values.Length == 0)
                    {
                        Queries.Add(new BoolQuery
                        {
                            MustNot = new QueryContainer[]
                            {
                                new ExistsQuery
                                {
                                    Field = fieldDescriptor.FieldName
                                }
                            }
                        });
                    }
                    else
                    {
                        Queries.Add(new TermsQuery
                        {
                            Field = fieldDescriptor.FieldName,
                            Terms = values
                        });
                    }
                }
                else
                {
                    Queries.Add(new TermQuery
                    {
                        Field = fieldDescriptor.FieldName,
                        Value = token.Value<string>()
                    });
                }
            }
        }

        private List<QueryContainer> Queries { get; } = new List<QueryContainer>();

        public override IEnumerable<QueryContainer> GetQuery() => Queries;

        protected virtual IEnumerable<RequestFieldDescriptor> AllowedTerms { get; } = Enumerable.Empty<RequestFieldDescriptor>();
    }
}
