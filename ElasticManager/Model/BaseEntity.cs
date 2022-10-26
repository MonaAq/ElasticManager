namespace ElasticManager.Model
{
    public class BaseEntity<Key>
    {
        public DateTime TimeStamp { get; set; }

        public Key Id { get; set; }
    }
}