using Nest;
using ElasticManager.Model;
using Filter = ElasticManager.Model.Filters.Filter;

namespace ElasticManager.Repository.ElasticSearch
{
    public interface IElasticServiceClient
    {
        Task BulkInsert<T, Key>(string indexName, IEnumerable<T> tepixList) where T : BaseEntity<Key>;
        Task<bool> SingleInsert<T, Key>(string indexName, T item) where T : BaseEntity<Key>;
        Task<bool> UpdateAsync<T, Key>(string indexName, T item) where T : BaseEntity<Key>;

        Task DeleteAll<T>() where T : class;
        Task<bool> IsIndexExisting(string index);
        void CreateIndex(ICreateIndexRequest indexRequest);
        Task<T?> GetNameById<T, Key>(long letterTypeId, string indexName) where T : BaseEntity<Key>;


        Task<PagedResponse<T>> SearchByFilter<T>(Filter filter, string indexname) where T : class;

        Task<PagedResponse<T>> SearchByText<T>(string text, int pageSize, int pageNumber, string indexname) where T : class;


    }
}
