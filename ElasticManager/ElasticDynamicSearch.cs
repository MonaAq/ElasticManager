using ElasticManager.Model;
using ElasticManager.Model.Filters;
using Nest;
using Filter = ElasticManager.Model.Filters.Filter;

namespace ElasticManager
{
    public class ElasticDynamicSearch
    {
        private readonly IElasticClient _elasticClient;

        public ElasticDynamicSearch(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        /// <summary>
        /// search in elastic client with dynamic model and filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="indexname"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<PagedResponse<T>> SearchByFilter<T>(Filter filter, string indexname) where T : class
        {
            try
            {
                //if filter is null return throw
                if (filter == null)
                    throw new ArgumentNullException(nameof(filter));

                //set sorting with filter.sorting
                SortDescriptor<T> sortDescriptor = SetSorting<T>(filter);

                //search in elasticsearch 
                var searchResponse = await _elasticClient.SearchAsync<T>(s => s
                            .Index(indexname)
                            .From(filter.Paging.PageNumber)
                            .Query(q => SetQueryContainer<T>(filter.Filters))
                            .Sort(sort => sortDescriptor)
                        );

                //set totalcount
                var totalCount = searchResponse.Total;

                //set result with take searchResponse Documents 
                var result = searchResponse.Documents.Take(filter.Paging.PageSize);

                //create PagedResponse and set it  to result
                var response = new PagedResponse<T>(result.ToList(), filter.Paging.PageNumber, filter.Paging.PageSize, totalCount);

                //return result 
                return response;
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<PagedResponse<T>> SearchByText<T>(string text, int pageSize, int pageNumber, string indexname) where T : class
        {
            var searchResponse = await _elasticClient.SearchAsync<T>(s => s
                           .Index(indexname)
            .From(pageNumber * pageSize)
                .Query(q => q.QueryString(qs => qs.Query(text + "*"))));

            //set totalcount
            var totalCount = searchResponse.Total;

            //set result with take searchResponse Documents 
            var result = searchResponse.Documents.Take(pageSize);

            //create PagedResponse and set it  to result
            var response = new PagedResponse<T>(result.ToList(), pageNumber, pageSize, totalCount);
            return response;
        }

        /// <summary>
        /// set query Container 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static QueryContainer SetQueryContainer<T>(List<Filtering> filters) where T : class
        {
            QueryContainerDescriptor<T> filterDescriptortest = new QueryContainerDescriptor<T>();
            var searchTerms = new List<Func<QueryContainerDescriptor<T>, QueryContainer>> { };

            foreach (var item in filters)
            {
                var search = item.SetQueryContainer<T>(item, item.PropertyName, filterDescriptortest);

                searchTerms.Add(search);
            }

            return new QueryContainerDescriptor<T>().Bool(
               b => b.Should((IEnumerable<Func<QueryContainerDescriptor<T>, QueryContainer>>)searchTerms.ToArray()));
        }

        /// <summary>
        /// check filter sorting and set defult value 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static SortDescriptor<T> SetSorting<T>(Filter filter) where T : class
        {
            var sortDescriptor = new SortDescriptor<T>();

            //if sorting is null fill with defult value and reurn it
            if (filter.Sorting == null)
            {
                return sortDescriptor.Field(new Field("InsertDateTime"), SortOrder.Descending);
            }
            else
            {
                //fill sortDescriptor with Sorting filter and return it
                foreach (var sort in filter.Sorting)
                {
                    sortDescriptor.Field(sort.SortProperty, (SortOrder)sort.SortType);
                }

                return sortDescriptor;
            }
        }
    }
}
