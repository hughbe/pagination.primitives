using System;
using System.Collections.Generic;
using System.Linq;
using Nest;

namespace Pagination.Elasticsearch
{
    public class PagedRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string OrderingKey { get; set; }
        public bool Descending { get; set; }

        public IList<SortField> SortFields => new SortField[]
        {
            new SortField
            {
                Field = OrderingKey,
                Order = Descending ? SortOrder.Descending : SortOrder.Ascending
            }
        };

        public virtual IEnumerable<QueryContainer> GetQuery() => Enumerable.Empty<QueryContainer>();

        public QueryContainer Query => new QueryContainer(new BoolQuery
        {
            Must = GetQuery()
        });

        private T GetAs<T>(IDictionary<string, object> dictionary, string key)
        {
            object value = dictionary.GetValueOrDefault(key);
            if (value == null)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
