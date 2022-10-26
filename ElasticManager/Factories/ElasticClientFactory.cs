using Microsoft.Extensions.Logging;
using Nest;


namespace ElasticManager.Repository.ElasticSearch
{
    public class ElasticClientFactory
    {
        private readonly ElasticProperties _elasticProperties;

        public ElasticClientFactory(ElasticProperties elasticProperties)
        {
            _elasticProperties = elasticProperties;
        }

        public IElasticClient CreateElasticClient()
        {
            //add elastic node
            var uris = new List<Uri>();
            foreach (var elasticNode in _elasticProperties.ElasticNodes)
            {
                uris.Add(new Uri(elasticNode));
            }

            //set connectionstring
            var connectionSetting = new ConnectionSettings(new Uri(uris[0].ToString()));

            //if does not enable scurity
            if (!_elasticProperties.SecurityEnabled)
            {
                //generate general ElasticClient
                var generalClient = new ElasticClient(connectionSetting);

                //check Health general ElasticClient
                TestConnection(generalClient);

                //return ElasticClient
                return generalClient;
            }

            //if username or password elastic is null or with space return throw
            if (string.IsNullOrWhiteSpace(_elasticProperties.UserName) || string.IsNullOrWhiteSpace(_elasticProperties.Password))
                throw new Exception("elastic user or password not found");

            //check authentivation with username , password
            connectionSetting.BasicAuthentication(_elasticProperties.UserName, _elasticProperties.Password);

            //generate private ElasticClient
            var privateClient = new ElasticClient(connectionSetting);

            //check Health general ElasticClient
            TestConnection(privateClient);

            return privateClient;
        }

        /// <summary>
        /// test Elastic Client healthy
        /// </summary>
        /// <param name="client"></param>
        private void TestConnection(IElasticClient client)
        {
            try
            {
                var response = client.Cluster.Health();
                Console.WriteLine($"Elastic cluster state is: {response.Status}");
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
