using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;

namespace Pagination.Elasticsearch
{
    public class ElasticsearchRepository<T> where T: class, new()
    {
        private string DefaultIndex { get; }
        private ElasticClient Client { get; }

        public ElasticsearchRepository(ConnectionSettings settings, string defaultIndex, Func<CreateIndexDescriptor, ICreateIndexRequest> createIndex = null)
        {
            settings = settings.DefaultIndex(defaultIndex);
            DefaultIndex = defaultIndex;
            Client = new ElasticClient(settings);

            if (createIndex != null)
            {
                ICreateIndexResponse response = Client.CreateIndex(defaultIndex, createIndex);
                if (!response.IsValid && (response.ServerError != null && !response.ServerError.ToString().Contains("exists")))
                {
                    throw new InvalidOperationException(response.ServerError?.ToString() ?? response.OriginalException.Message);
                }
            }
        }

        public async Task<Result<T>> Save(T data, string type = null, Refresh refresh = Refresh.False)
        {
            IIndexResponse response = await Client.IndexAsync(data, idx => idx.Index(DefaultIndex).Refresh(refresh).Type(type));
            if (!response.IsValid)
            {
                return Result<T>.Error(response.ServerError?.ToString() ?? response.OriginalException.Message);
            }

            return data;
        }

        public async Task<Result<T>> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Result<T>.Error($"No such object with id {id}.");
            }

            IGetResponse<T> response = await Client.GetAsync<T>(id);
            if (!response.Found)
            {
                return Result<T>.Error($"No such object with id {id}.");
            }

            return response.Source;
        }

        public const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 50;
        public const int MaxPageSize = 10000;

        public async Task<Result<bool>> Any(object query)
        {
            Result<ElasticsearchPagedResponse<T>> result = await Paged(0, 1, query);
            if (!result)
            {
                return Result<bool>.Error(result.ErrorMessage);
            }

            return result.Value.Data.Any();
        }

        public async Task<ISearchResponse<T>> Search(Func<SearchDescriptor<T>, ISearchRequest> selector) => await Client.SearchAsync(selector);

        public async Task<Result<AllResponse<T>>> All(object query = null, IList<SortField> sort = null, string type = null)
        {
            Result<ElasticsearchPagedResponse<T>> response = await Paged(0, int.MaxValue, query, sort, type);
            if (!response)
            {
                return Result<AllResponse<T>>.Error(response.ErrorMessage);
            }

            return new AllResponse<T>
            {
                Data = response.Value.AllData(),
                TotalCount = response.Value.TotalCount
            };
        }

        public Task<Result<ElasticsearchPagedResponse<T>>> Paged(int pageNumber, int pageSize, object query = null, IList<SortField> sort = null, string type = null)
        {
            QueryContainer queryContainer = query as QueryContainer;
            if (queryContainer == null && query != null)
            {
                string stringRepresentation = JsonConvert.SerializeObject(query, Formatting.Indented);
                queryContainer = new QueryContainer(new RawQuery(stringRepresentation));
            }

            return Paged(pageNumber, pageSize, queryContainer, sort, type);
        }

        public async Task<Result<ElasticsearchPagedResponse<T>>> Paged(int pageNumber, int pageSize, QueryContainer query, IList<SortField> sort, string type = null)
        {
            // If the page number was invalid, use the default page number.
            pageNumber = Math.Max(DefaultPageNumber, pageNumber);

            // If the page size was invalid, use the default page size.
            pageSize = pageSize > 0 ? pageSize : DefaultPageSize;
            pageSize = Math.Min(pageSize, MaxPageSize);

            var content = new ElasticsearchPagedResponse<T>
            {
                Repository = this,
                Query = query,
                Sort = sort,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Page numbers start at 0 for Elasticsearch.
            int from = (content.PageNumber - 1) * pageSize;
            ISearchResponse<T> searchResponse = await Client.SearchAsync<T>(new SearchRequest<T>(DefaultIndex, type)
            {
                Query = query,
                Sort = sort?.Cast<ISort>()?.ToList(),
                From = from,
                Size = pageSize
            });
            if (!searchResponse.IsValid && searchResponse.ServerError?.Status != 404)
            {
                // Allow errors for sorting when there is no such mapping.
                if (searchResponse.ServerError?.Error?.CausedBy?.Reason?.Contains("in order to sort on") != true)
                {
                    string message = searchResponse.ServerError?.ToString() ?? searchResponse.OriginalException?.Message ?? searchResponse.ApiCall.ToString();
                    string debugInformation = searchResponse.DebugInformation?.ToString() ?? "No Debug Information";
                    return Result<ElasticsearchPagedResponse<T>>.Error(message + Environment.NewLine + debugInformation);
                }
            }

            content.Data = searchResponse.Documents.ToArray();
            content.TotalCount = searchResponse.Total;

            return content;
        }

        public async Task<Result<T>> Delete(string id)
        {
            Result<T> deletedDocument = await Get(id);
            if (!deletedDocument)
            {
                return deletedDocument;
            }

            IDeleteResponse response = await Client.DeleteAsync<T>(id);
            if (!response.IsValid)
            {
                return Result<T>.Error(response.ServerError?.ToString() ?? response.OriginalException.Message);
            }

            return deletedDocument;
        }
    }
}
