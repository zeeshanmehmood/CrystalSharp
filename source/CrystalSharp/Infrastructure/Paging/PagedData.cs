namespace CrystalSharp.Infrastructure.Paging
{
    public abstract class PagedData
    {
        public int PageCount { get; protected set; }
        public int CurrentPage { get; protected set; }
        public int PageSize { get; protected set; }
        public int RowCount { get; protected set; }
    }
}
