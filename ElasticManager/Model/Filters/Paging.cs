namespace ElasticManager.Model.Filters
{
    public class Paging
    {
        public Paging(int pageNumber = 0, int pageSize = 10)
        {
            this.PageNumber = pageNumber < 1 ? 0 : pageNumber;
            this.PageSize = pageSize > 50 ? 50 : pageSize < 10 ? 10 : pageSize;
        }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
