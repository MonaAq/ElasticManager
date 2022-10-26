using ElasticManager.Repository.ElasticSearch;
using Microsoft.Extensions.Configuration;
using Nest;

namespace ElasticManager.UnitTests
{
    internal class ElasticServiceClientTests : ElasticServiceClient
    {
        private readonly ElasticProperties _elasticProperties;
        public readonly IConfigurationRoot _configuration;
        private readonly IElasticClient _elasticClient;
        public ElasticServiceClientTests(ElasticClientFactory elasticClientFactory) : base(elasticClientFactory)
        {
            _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.UnitTest.json", true).Build();
            _elasticProperties = _configuration.GetSection("ElasticProperties").Get<ElasticProperties>();
            _elasticClient = elasticClientFactory.CreateElasticClient();
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
            catch (Exception)
            {
                throw;
            }
        }
    }
}
