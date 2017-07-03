using System.Collections.Generic;
using System.Linq;

namespace Pagination.Elasticsearch
{
    public class AllResponse<T> where T: class, new()
    {
        public long TotalCount { get; set; }
        public IEnumerable<T> Data { get; set; }

        public static AllResponse<T> Empty => new AllResponse<T> { TotalCount = 0, Data = Enumerable.Empty<T>() };
    }
}
