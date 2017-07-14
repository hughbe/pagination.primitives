using System.Threading.Tasks;

namespace Pagination.Elasticsearch
{
    public static class ElasticsearchRepositoryExtensions
    {
        public static async Task<Result<ElasticsearchPagedResponse<T>>> Paged<T>(this ElasticsearchRepository<T> repository, PagedRequest request) where T : class, new()
        {
            if (request == null)
            {
                return Result<ElasticsearchPagedResponse<T>>.Error("Invalid request.");
            }

            return await repository.Paged(request.PageNumber, request.PageSize, request.Query, request.SortFields);
        }

        public static async Task<Result<AllResponse<T>>> All<T>(this ElasticsearchRepository<T> repository, PagedRequest request) where T : class, new()
        {
            if (request == null)
            {
                return Result<AllResponse<T>>.Error("Invalid request.");
            }

            return await repository.All(request.Query, request.SortFields);
        }
    }
}
