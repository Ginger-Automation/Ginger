using System;
using System.Threading.Tasks;
using AccountReport.Contracts.GraphQL.ResponseModels;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQLClient.Extensions;

namespace GraphQLClient.Clients
{
    /// <summary>
    /// Handles GraphQL client operations for fetching data and managing pagination.
    /// </summary>
    public class GraphQlClient
    {
        private readonly GraphQLHttpClient client;

        public PageInfo PageInfo { get; private set; }
        public string EndCursor { get; private set; }
        public bool HasNextPage { get; private set; }
        public bool HasPreviousPage { get; private set; }
        public string StartCursor { get; private set; }
        public int TotalCount { get; set; }
        public int ItemsFetchedSoFar { get; set; }
        public int CurrentRecordCount { get; set; }

        public GraphQlClient(string endpoint)
        {
            try
            {
                client = new GraphQLHttpClient(endpoint, new NewtonsoftJsonSerializer());
                ResetPagination();
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException("Invalid URL format", nameof(endpoint), ex);
            }
        }

        /// <summary>
        /// Resets the pagination state.
        /// </summary>
        public void ResetPagination()
        {
            EndCursor = null;
            HasNextPage = true;
            ItemsFetchedSoFar = 0;
        }

        /// <summary>
        /// Decreases the count of fetched items, used when paginating backward.
        /// </summary>
        public void DecreaseFetchedItemsCount(int count)
        {
            ItemsFetchedSoFar -= count;
        }

        /// <summary>
        /// Fetches runsets data using the provided GraphQL query.
        /// </summary>
        public async Task<GraphQLResponse<GraphQLRunsetResponse>> GetRunsets(string query)
        {
            var request = new GraphQLRequest { Query = query };
            try
            {
                var response = await client.SendQueryAsync<GraphQLRunsetResponse>(request);
                var pageInfo = GraphQLClientExtensions.GetPageInfo(response);

                EndCursor = pageInfo.EndCursor;
                HasNextPage = pageInfo.HasNextPage;
                HasPreviousPage = pageInfo.HasPreviousPage;
                StartCursor = pageInfo.StartCursor;
                TotalCount = response.Data.Runsets.TotalCount;

                if (pageInfo.StartCursor == EndCursor)
                {
                    HasPreviousPage = false;
                    ItemsFetchedSoFar = 0;
                }
                var beforePage = query.IndexOf("before") >= 0;
                if (!beforePage)
                {
                    ItemsFetchedSoFar += response.Data.Runsets.Nodes.Count;
                }
                CurrentRecordCount = response.Data.Runsets.Nodes.Count;



                return response;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error fetching runsets", ex);
            }
        }
    }
}