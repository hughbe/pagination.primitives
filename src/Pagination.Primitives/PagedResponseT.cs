using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pagination.Primitives
{
    public abstract class PagedResponse<T> : PagedResponse
    {
        /// <summary>
        /// The paged data retrieved from the original source.
        /// </summary>
        public IEnumerable<T> Data { get; set; }

        /// <summary>
        /// Gets the previous page of data of length PageSize.
        /// </summary>
        /// <returns>The previous page of data if there is a previous page, else null. This represents a slice of data of length PageSize.</returns>
        public abstract Task<PagedResponse<T>> PreviousPage();

        /// <summary>
        /// Gets the next page of data of length PageSize.
        /// </summary>
        /// <returns>The next page of data if there is a next page, else null. This represents a slice of data of length PageSize.</returns>
        public abstract Task<PagedResponse<T>> NextPage();

        /// <summary>
        /// Gets a combined list of all the data on pages after and including the current page.
        /// </summary>
        /// <returns>The flattened list representation of all data after and including the current page.</returns>
        public IEnumerable<T> AllData() => AllPages().SelectMany(response => response.Data);

        /// <summary>
        /// Gets a list of all pages after and including the current page.
        /// </summary>
        /// <returns>A list of all pages after and including the current page.</returns>
        public IEnumerable<PagedResponse<T>> AllPages()
        {
            yield return this;
            foreach (PagedResponse<T> remainingPages in AllPagesAfterThis())
            {
                yield return remainingPages;
            }
        }

        /// <summary>
        /// Gets a list of all pages after and not including the current page.
        /// </summary>
        /// <returns>A list of all pages after and not including the current page.</returns>
        public IEnumerable<PagedResponse<T>> AllPagesAfterThis()
        {
            PagedResponse<T> response = NextPage().Result;
            while (response != null)
            {
                yield return response;
                response = response.NextPage().Result;
            }
        }
    }
}
