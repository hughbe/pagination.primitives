using System;

namespace Pagination.Primitives
{
    public class PagedResponse
    {
        private int _pageNumber;
        private int _pageSize;

        /// <summary>
        /// Constructs a response with a page number of 0 and of page size of 0.
        /// </summary>
        public PagedResponse() : this(0, 0) { }

        /// <summary>
        /// Consturcts a response with the given page number and page size.
        /// </summary>
        /// <param name="pageNumber">The zero based page number to fetch.</param>
        /// <param name="pageSize">The size of the data to fetch each page.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="pageNumber"/> is less than zero
        /// -or-
        /// <paramref name="pageSize"/> is less than zero.
        /// </exception>
        public PagedResponse(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        /// <summary>
        /// The zero based page number that was fetched.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than zero.</exception>
        public int PageNumber
        {
            get => _pageNumber;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Argument cannot be negative.");
                }

                _pageNumber = value;
            }
        }

        /// <summary>
        /// The size of the data on each page.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than zero.</exception>
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Argument cannot be negative.");
                }

                _pageSize = value;
            }
        }

        /// <summary>
        /// The zero-based index that represents the position the first piece of data in this page in the original source.
        /// </summary>
        public int StartItemIndex => PageNumber > 0 ? (PageNumber - 1) * PageSize : 0;
        
        /// <summary>
        /// The zero-based index that represents the position the last piece of data in this page in the original source.
        /// </summary>
        public int EndItemIndex => PageNumber * PageSize;
    }
}
