namespace ElasticManager.Model
{
    public class PagedResponse<T> : Response<T>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public long TotalRecords { get; set; }
        public bool HasPervious { get; set; }
        public bool HasNext { get; set; }

        public PagedResponse()
        { }
        public PagedResponse(List<T> data, int currentPage, int pageSize, long totalRecords)
        {
            this.CurrentPage = (int)Math.Round((double)currentPage / pageSize) + 1;
            this.PageSize = pageSize;
            this.TotalRecords = totalRecords;
            double totalpage = (totalRecords / pageSize);
            double totalpages = totalpage == 0 ? totalpage : totalpage + 1;
            this.TotalPages = (int)Math.Round(totalpages);
            this.HasPervious = this.CurrentPage == 1 ? false : true;
            this.HasNext = this.CurrentPage == this.TotalPages ? false : true;
            this.Data = data;
            this.ErrorMessage = null;
            this.Succeeded = true;
            this.Error = null;
        }

        public PagedResponse(List<T> data, int currentPage, int pageSize, long totalRecords,
                             int totalPages, bool hasPervious, bool hasNext)
        {
            this.Data = data;
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
            this.TotalRecords = totalRecords;
            this.TotalPages = totalPages;
            this.HasPervious = hasPervious;
            this.HasNext = hasNext;
            this.Data = data;
            this.ErrorMessage = null;
            this.Succeeded = true;
            this.Error = null;
        }
    }
}
