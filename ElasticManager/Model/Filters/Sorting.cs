using ElasticManager.Enums;

namespace ElasticManager.Model.Filters
{
    public class Sorting
    {
        //property type 
        public SortType SortType { get; set; }

        //property name 
        public string SortProperty { get; set; }

        public Sorting(SortType sortType = SortType.desc, string sortProperty = "insertDateTime")
        {
            SortType = sortType;
            SortProperty = sortProperty;
        }

    }
}
