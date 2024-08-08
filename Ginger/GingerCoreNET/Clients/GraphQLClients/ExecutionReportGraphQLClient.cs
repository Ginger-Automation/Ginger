using AccountReport.Contracts.GraphQL.Helpers;
using AccountReport.Contracts.GraphQL.RequestModels;
using AccountReport.Contracts.GraphQL.ResponseModels;
using GraphQL;
using GraphQLClient.Clients;
using System;
using System.Collections.Generic;

using System.Threading.Tasks;

/// <summary>
/// Handles execution of GraphQL queries for execution reports.
/// </summary>
public class ExecutionReportGraphQLClient
{
    private readonly GraphQlClient graphQlClient;

    public ExecutionReportGraphQLClient(GraphQlClient graphQlClient)
    {
        this.graphQlClient = graphQlClient;
    }

    /// <summary>
    /// Executes the report query with specified parameters.
    /// </summary>
    public async Task<GraphQLResponse<GraphQLRunsetResponse>> ExecuteReportQuery(int recordLimit, Guid solutionId, Guid? runSetId = null, string endCursor = null, string startCursor = null, bool firstPage = false, bool lastPage = false, bool afterOrBefore = true)
    {
        const string paraList = "executionId, entityId, name, description, sourceApplication, sourceApplicationUser, startTime, endTime, elapsedEndTimeStamp, status";

        if (recordLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recordLimit), "Record limit must be greater than zero.");
        }
        if (solutionId == Guid.Empty)
        {
            throw new ArgumentException("Solution ID cannot be empty.", nameof(solutionId));
        }

        var queryInfo = new GraphQLQueryInfo
        {
            PageInfo = new PageRequestInfo
            {
                FirstPage = firstPage,
                LastPage = lastPage,
                StartCursor = startCursor,
                EndCursor = endCursor,
                RecordLimit = recordLimit,
                RequestNextPage = afterOrBefore
            },
            SortingInfo = new SortColumn
            {
                Field = "startTime",
                Order = "DESC"
            },
            Filters = new List<Filter>
            {
                new Filter
                {
                    Field = "solutionId",
                    Operator = "eq",
                    Value = solutionId
                }
            }
        };

        if (runSetId.HasValue)
        {
            queryInfo.Filters.Add(new Filter
            {
                Field = "entityId",
                Operator = "eq",
                Value = runSetId.Value
            });
        }

        var generatedQuery = RunsetQueryBuilder.CreateQuery(queryInfo, paraList);
        try
        {
            return await graphQlClient.GetRunsets(generatedQuery);
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Error executing the GraphQL query.", ex);
        }

    }
}
