using Nest;

namespace ElasticManager.Model.Filters
{
    public abstract class Filtering
    {
        public string PropertyName { get; set; }
        public abstract Func<QueryContainerDescriptor<T>, QueryContainer> SetQueryContainer<T>(Filtering Filtering, string propertyName,
                                                                                               QueryContainerDescriptor<T> filterDescriptortest)
            where T : class;

    }

    public class OrFilter : Filtering
    {
        public string PropertyValue { get; set; }

        public override Func<QueryContainerDescriptor<T>, QueryContainer> SetQueryContainer<T>(Filtering Filtering, string propertyName,
                                                                                               QueryContainerDescriptor<T> filterDescriptortest
                                                                                               )
        {
            var orItem = (OrFilter)Filtering;
            Func<QueryContainerDescriptor<T>, QueryContainer> orSearch =
            search => search.Term(propertyName, orItem.PropertyValue);
            return orSearch;
        }
    }
    public class AndFilter : Filtering
    {
        public string PropertyValue { get; set; }

        public override Func<QueryContainerDescriptor<T>, QueryContainer> SetQueryContainer<T>
                                                            (Filtering Filtering, string propertyName,
                                                            QueryContainerDescriptor<T> filterDescriptortest)
        {
            var newFilter = new QueryContainerDescriptor<T>();
            var andItem = (AndFilter)Filtering;
            var newPartOfQuery = newFilter.Term(propertyName, andItem.PropertyValue);
            filterDescriptortest.Bool(x => x.Must(newPartOfQuery));
            Func<QueryContainerDescriptor<T>, QueryContainer>
            Search = Search => filterDescriptortest;
            return Search;
        }
    }

    public class AndNotFilter : Filtering
    {
        public string PropertyValue { get; set; }

        public override Func<QueryContainerDescriptor<T>, QueryContainer> SetQueryContainer<T>
                                                            (Filtering Filtering, string propertyName,
                                                            QueryContainerDescriptor<T> filterDescriptortest)
        {
            var newFilter = new QueryContainerDescriptor<T>();
            var andItem = (AndNotFilter)Filtering;
            var newPartOfQuery = newFilter.Term(propertyName, andItem.PropertyValue);
            filterDescriptortest.Bool(x => x.MustNot(newPartOfQuery));
            Func<QueryContainerDescriptor<T>, QueryContainer>
            Search = Search => filterDescriptortest;
            return Search;
        }
    }

    public class BetweenFilter : Filtering
    {
        public object LeftValue { get; set; }
        public object RightValue { get; set; }

        public override Func<QueryContainerDescriptor<T>, QueryContainer> SetQueryContainer<T>(Filtering Filtering, string propertyName,
                                                                                               QueryContainerDescriptor<T> filterDescriptortest)
        {
            var betweenItem = (BetweenFilter)Filtering;
            var newPartOfQuery= GenerateRangeFilter<T>(propertyName, filterDescriptortest,
                                                      betweenItem.LeftValue,
                                                      betweenItem.RightValue);

            filterDescriptortest.Bool(x => x.Must(newPartOfQuery));
            Func<QueryContainerDescriptor<T>, QueryContainer>
            Search = Search => filterDescriptortest;
            return Search;
        }

        /// <summary>
        /// Generate QueryContainer for range query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="leftValue"></param>
        /// <param name="rightValue"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static QueryContainer GenerateRangeFilter<T>(string fieldName, QueryContainerDescriptor<T> filterDescriptor,
                                                             object leftValue, object rightValue)
        where T : class
        {
            //if leftValue type is DateTime convert data to datetime and create QueryContainer and return it
            if (!DateRangeParamIsValid(leftValue, rightValue))
            {
                if (!NumberRangeParamIsValid(leftValue, rightValue))
                {
                    throw new ArgumentException("input param in data range query is not valid");
                }
                return filterDescriptor.Range(r => r
                          .Field(new Field(fieldName))
                          .LessThanOrEquals(Convert.ToDouble(rightValue))
                          .GreaterThanOrEquals(Convert.ToDouble(leftValue))
                          );
            }
            else
                return filterDescriptor.DateRange(r => r
                     .Field(new Field(fieldName))
                     .GreaterThanOrEquals((DateTime)leftValue)
                     .LessThanOrEquals((DateTime)rightValue)
                     );

            throw new NotImplementedException();
        }

        private static bool DateRangeParamIsValid(object leftValue, object rightValue)
        {
            if (leftValue is DateTime? && rightValue is DateTime?)
            {
                return true;
            }
            return false;
        }
        private static bool NumberRangeParamIsValid(object leftValue, object rightValue)
        {
            if (leftValue is decimal || leftValue is double || leftValue is long || leftValue is int &&
                     (rightValue is decimal || rightValue is double || rightValue is long || rightValue is int))
            {
                return true;
            }
            return false;
        }

    }
}
