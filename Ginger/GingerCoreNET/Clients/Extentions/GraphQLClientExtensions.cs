using AccountReport.Contracts.GraphQL.ResponseModels;
using GraphQL;
using System;

namespace GraphQLClient.Extensions
{
    public static class GraphQLClientExtensions
    {
        /// <summary>
        /// Extracts pagination information from the GraphQL response.
        /// </summary>
        public static PageInfo GetPageInfo(GraphQLResponse<GraphQLRunsetResponse> response)
        {
            if (response?.Data?.Runsets?.PageInfo == null)
            {
                throw new ArgumentNullException(nameof(response), "The response or its PageInfo is null.");
            }
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
