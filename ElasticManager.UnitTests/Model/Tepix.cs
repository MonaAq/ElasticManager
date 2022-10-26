using ElasticManager.Model;
using System.Text;

namespace ElasticManager.UnitTests.Model
{
    public class Tepix : BaseEntity<string>
    {
        public Tepix(DateTime datetime, String indexValue)
        {
            Id = CreateId(datetime);
            IndexValue = indexValue;
            DateTime = datetime;
        }
        public DateTime DateTime { get; set; }
        public string IndexValue { get; set; }
        private static string CreateId(DateTime date)
        {
            return new StringBuilder()
            .Append(date.Year)
            .Append('-')
            .Append(date.Month)
            .Append('-')
            .Append(date.Day)
            .Append('-')
            .Append(date.Hour)
            .Append('-')
            .Append(date.Minute)
            .Append('-')
            .Append(date.Second)
            .ToString();
        }

    }
}
