using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace ElasticManager.Model.Filters
{
    [BindProperties]
    public class Filter
    {
        public Paging Paging { get; set; }
        public List<Sorting> Sorting { get; set; }
        public List<AndFilter>? AndFilters { get; set; }
        public List<AndNotFilter>? AndNotFilters { get; set; }
        public List<OrFilter>? OrFilters { get; set; }

        private List<BetweenFilter>? _betweenFilters;
        public List<BetweenFilter>? BetweenFilters { get { return _betweenFilters; } set { _betweenFilters = value; SetFiltering(); } }
        [JsonIgnore]
        public List<Filtering> Filters { get; set; }

        public Filter()
        {
            this.Paging = new Paging();
            this.Sorting = new List<Sorting>();
            this.Sorting.Add(new Sorting());
        }
        public void SetFiltering()
        {
            this.Filters = new List<Filtering>();
            if (AndNotFilters != null)
                foreach (var item in AndNotFilters)
                {
                    Filters.Add(item);
                }

            if (AndFilters != null)
                foreach (var item in AndFilters)
                {
                    Filters.Add(item);
                }

            if (BetweenFilters != null)
                foreach (var item in BetweenFilters)
                {
                    Filters.Add(item);
                }

            if (OrFilters != null)
                foreach (var item in OrFilters)
                {
                    Filters.Add(item);
                }
        }
    }
}
