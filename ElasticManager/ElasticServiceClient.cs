using Elasticsearch.Net;
using Nest;
using ElasticManager.Model;
using Filter = ElasticManager.Model.Filters.Filter;

namespace ElasticManager.Repository.ElasticSearch
{
    public class ElasticServiceClient : IElasticServiceClient
    {
        private readonly IElasticClient _elasticClient;
        private ElasticDynamicSearch _elasticDynamicSearch;
        public ElasticServiceClient(ElasticClientFactory elasticClientFactory)
        {
            _elasticClient = elasticClientFactory.CreateElasticClient();
            _elasticDynamicSearch = new ElasticDynamicSearch(_elasticClient);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Key"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task BulkInsert<T, Key>(string indexName, IEnumerable<T> list) where T : BaseEntity<Key>
        {
            try
            {
                var items = list.ToList();
                var bulk = new BulkRequest(indexName) { Operations = new List<IBulkOperation>() };
                items.ForEach(item =>
                {
                    var bulkIndex = new BulkIndexOperation<T>(item)
                    {
                        Id = item.Id as Id
                    };
                    bulk.Operations.Add(bulkIndex);
                });
                var response = await _elasticClient.BulkAsync(bulk);
                EndorseElasticResponse(response);

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// single insert item in elasticsearch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="Key"></typeparam>
        /// <param name="indexName"></param>
        /// <param name="item"></param>
        public async Task<bool> SingleInsert<T, Key>(string indexName, T item) where T : BaseEntity<Key>
        {
            try
            {
                var result = await _elasticClient.IndexAsync<T>(item, i => i.Index(indexName));

                if (!result.IsValid)
                {
                    return false;
                }

                return true;
            }
            catch (global::System.Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync<T, Key>(string indexName, T item) where T : BaseEntity<Key>
        {
            try
            {
                var result = await _elasticClient.UpdateAsync<T>(item,
                    i => i.Index(indexName)
                    .Doc(item)

                );

                if (!result.IsValid)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task BulkUpdateAsync<T, Key>(string indexName, IEnumerable<T> list) where T : BaseEntity<Key>
        {
            var descriptor = new BulkDescriptor(indexName);

            foreach (var eachDoc in list)
            {
                var doc = eachDoc;
                descriptor.Update<T>(i => i
                   .Id(doc.Id as Id)
                   .Doc(doc)
                   .DocAsUpsert(true));
            }

            var response = await _elasticClient.BulkAsync(descriptor);
        }

        /// <summary>
        /// Delete all T in elasticsearch with query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task DeleteAll<T>() where T : class
        {
            await _elasticClient.DeleteByQueryAsync<T>(s => s.AllIndices().Query(q => q.QueryString(qs => qs.Query("*"))));
        }

        /// <summary>
        /// Endorse Elastic Response
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        private static void EndorseElasticResponse<T>(T request) where T : class
        {
            //if type of T not equal IElasticsearchResponse return throw
            if (!typeof(IElasticsearchResponse).IsAssignableFrom(typeof(T)))
                throw new ArgumentException();

            //cast response to IElasticsearchResponse
            var elasticResponse = (IElasticsearchResponse)request;

            //if elastic response not equal success return throw
            if (!elasticResponse.ApiCall.Success)
            {
                throw new Exception();///TODO add Metric for this exception
            }
        }

        /// <summary>
        /// if elastic index (check index name) is exist return true
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task<bool> IsIndexExisting(string index)
        {
            var existResponse = await _elasticClient.Indices.ExistsAsync(index);
            return existResponse.Exists;
        }

        /// <summary>
        /// create new index in elastic
        /// </summary>
        /// <param name="indexRequest"></param>
        /// <exception cref="Exception"></exception>
        public void CreateIndex(ICreateIndexRequest indexRequest)
        {
            var response = _elasticClient.Indices.Create(indexRequest);
            if (!response.ApiCall.Success)
            {
                var split = response.OriginalException?.Message.Split(' ');
                if (split != null)
                {
                    if (split[13] == "resource_already_exists_exception")
                        return;
                }
            }
            EndorseElasticResponse(response);
            if (!response.Acknowledged || !response.ShardsAcknowledged)
            {
                throw new Exception("New shard Not Acknowledged");
            }
        }

        #region Codal

        /// <summary>
        /// get by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public async Task<T?> GetNameById<T, Key>(long id, string indexName) where T : BaseEntity<Key>
        {
            //query in elasticsearch with letterid
            var searchResponse = await _elasticClient.SearchAsync<T>(ms => ms
                                .Index(indexName)
                                    .Query(q => q
                                        .Match(m => m
                                            .Field(f => f.Id)
                                            .Query(id.ToString())
                                        )
                                    )
                                );

            // get the search responses for one of the searches by name
            var response = searchResponse.Documents.FirstOrDefault();

            return response;
        }

        public async Task<PagedResponse<T>> SearchByFilter<T>(Filter filter, string indexname) where T : class
        {
            return await _elasticDynamicSearch.SearchByFilter<T>(filter, indexname);
        }

        public async Task<PagedResponse<T>> SearchByText<T>(string text, int pageSize, int pageNumber, string indexname) where T : class
        {
            return await _elasticDynamicSearch.SearchByText<T>(text, pageSize, pageNumber, indexname);
        }
        #endregion
    }
}
