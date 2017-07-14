using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Newtonsoft.Json;
using Xunit;

namespace Pagination.Elasticsearch.Tests
{
    public class PagedRequestBuilderTests
    {
        [Fact]
        public void Deserialize_TermExistForTerms_Success()
        {
            var request = JsonConvert.DeserializeObject<Terms>(
@"{
    ""field"": ""value""
}");
            QueryContainer[] queries = request.GetQuery().ToArray();
            IQueryContainer rootQuery = Assert.Single(queries);

            ITermQuery termQuery = rootQuery.Term;
            Assert.Equal("field", termQuery.Field);
            Assert.Equal("value", termQuery.Value);
        }

        [Fact]
        public void Deserialize_TermsExistForTerms_Success()
        {
            var request = JsonConvert.DeserializeObject<Terms>(
@"{
    ""field"": [ ""value1"", ""value2"" ]
}");
            QueryContainer[] queries = request.GetQuery().ToArray();
            IQueryContainer rootQuery = Assert.Single(queries);

            ITermsQuery termsQuery = rootQuery.Terms;
            Assert.Equal("field", termsQuery.Field);
            Assert.Equal(new string[] { "value1", "value2" }, termsQuery.Terms);
        }

        [Fact]
        public void Deserialize_TermsEmptyForTerms_Success()
        {
            var request = JsonConvert.DeserializeObject<Terms>(
@"{
    ""field"": []
}");
            QueryContainer[] queries = request.GetQuery().ToArray();
            IQueryContainer rootQuery = Assert.Single(queries);
            IQueryContainer mustNotQuery = Assert.Single(rootQuery.Bool.MustNot);

            IExistsQuery existsQuery = mustNotQuery.Exists;
            Assert.Equal("field", existsQuery.Field);
        }

        [Fact]
        public void Deserialize_NoTermsExistForTerms_Success()
        {
            var request = JsonConvert.DeserializeObject<Terms>(
@"{
    ""otherField"": [ ""value1"", ""value2"" ]
}");
            QueryContainer[] queries = request.GetQuery().ToArray();
            Assert.Empty(queries);
        }

        [Fact]
        public void Deserialize_TermsSubClass_Success()
        {
            var request = JsonConvert.DeserializeObject<SubTerms>(
@"{
    ""field"": [ ""value1"", ""value2"" ],
    ""since"": ""2017-07-14T06:43:33Z"",
    ""until"": ""2017-07-16T06:43:33Z""
}");
            Assert.Equal(new DateTime(2017, 07, 14, 06, 43, 33, DateTimeKind.Utc), request.Since);
            Assert.Equal(new DateTime(2017, 07, 16, 06, 43, 33, DateTimeKind.Utc), request.Until);

            QueryContainer[] queries = request.GetQuery().ToArray();
            Assert.Equal(2, queries.Length);

            IQueryContainer firstQuery = queries[0];
            ITermsQuery termsQuery = firstQuery.Terms;
            Assert.Equal("field", termsQuery.Field);
            Assert.Equal(new string[] { "value1", "value2" }, termsQuery.Terms);

            IQueryContainer secondQuery = queries[1];
            IDateRangeQuery rangeQuery = Assert.IsType<DateRangeQuery>(secondQuery.Range);
            Assert.Equal("range", rangeQuery.Field);
        }

        public class Terms : PagedRequestBuilder
        {
            protected override IEnumerable<RequestFieldDescriptor> AllowedTerms => new RequestFieldDescriptor[] { "field" };
        }

        public class SubTerms : Terms
        {
            public DateTime Since { get; set; }
            public DateTime Until { get; set; }

            public override IEnumerable<QueryContainer> GetQuery()
            {
                return base.GetQuery().Append(new DateRangeQuery
                {
                    Field = "range",
                    LessThan = Until,
                    GreaterThan = Since
                });
            }
        }
    }
}
