using AccountReport.Contracts.GraphQL.ResponseModels;
using GraphQL;

namespace GraphQLClient.Extensions
{
    public static class GraphQLClientExtensions
    {
        /// <summary>
        /// Extracts pagination information from the GraphQL response.
        /// </summary>
        public static PageInfo GetPageInfo(GraphQLResponse<GraphQLRunsetResponse> response)
        {
            return new PageInfo
            {
                EndCursor = response.Data.Runsets.PageInfo.EndCursor,
                HasNextPage = response.Data.Runsets.PageInfo.HasNextPage,
                HasPreviousPage = response.Data.Runsets.PageInfo.HasPreviousPage,
                StartCursor = response.Data.Runsets.PageInfo.StartCursor
            };
        }
    }
}
