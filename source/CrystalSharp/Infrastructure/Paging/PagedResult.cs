using System;
using System.Collections.Generic;

namespace CrystalSharp.Infrastructure.Paging
{
    public class PagedResult<T> : PagedData
    {
        public IEnumerable<T> Data { get; }

        public PagedResult(int skip, int take, long totalCount, IEnumerable<T> data)
        {
            double pageCount = (double)totalCount / take;
            int totalPages = (int)Math.Ceiling(pageCount);
            int page = (int)Math.Ceiling(double.Parse(((skip + take) / take).ToString()));
            PageCount = totalPages;
            CurrentPage = page;
            PageSize = take;
            RowCount = (int)totalCount;
            Data = data;
        }
    }
}
