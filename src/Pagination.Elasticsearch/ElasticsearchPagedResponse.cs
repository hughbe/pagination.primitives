using System.Collections.Generic;
using System.Threading.Tasks;
using Pagination.Primitives;
using Nest;
using Newtonsoft.Json;

namespace Pagination.Elasticsearch
{
    public class ElasticsearchPagedResponse<T> : PagedResponse<T> where T : class, new()
    {
        public long TotalCount { get; set; }
        public long NumberOfPages => (TotalCount - 1) / PageSize + 1;

        internal ElasticsearchRepository<T> Repository { get; set; }

        [JsonIgnore]
        public QueryContainer Query { get; set; }

        [JsonConverter(typeof(SortJsonConverter))]
        public IList<SortField> Sort { get; set; }

        public override async Task<PagedResponse<T>> PreviousPage()
        {
            // Nothing before.
            if (PageNumber == 1)
            {
                return null;
            }

            // Ask the repository for the previous page.
            return await Repository.Paged(PageNumber - 1, PageSize, Query, Sort);
        }

        public override async Task<PagedResponse<T>> NextPage()
        {
            // Nothing after.
            if (PageNumber == NumberOfPages)
            {
                return null;
            }

            // Ask the repository for the next page.
            return await Repository.Paged(PageNumber + 1, PageSize, Query, Sort);
        }
    }
}
