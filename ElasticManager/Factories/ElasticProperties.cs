namespace ElasticManager.Repository.ElasticSearch
{
    public class ElasticProperties
    {
        public string[] ElasticNodes { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool SecurityEnabled { get; set; }
    }
}
