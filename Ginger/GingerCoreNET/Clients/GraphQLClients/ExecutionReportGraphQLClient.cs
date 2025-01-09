#region License
/*
Copyright © 2014-2024 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using AccountReport.Contracts.GraphQL.Helpers;
using AccountReport.Contracts.GraphQL.RequestModels;
using AccountReport.Contracts.GraphQL.ResponseModels;
using GraphQL;
using GraphQLClient.Clients;
using System;

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
            Filters =
            [
                new Filter
                {
                    Field = "solutionId",
                    Operator = "eq",
                    Value = solutionId
                }
            ]
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
    /// <summary>
    /// Fetches data by matching the solution ID and execution ID.
    /// </summary>
    public async Task<GraphQLResponse<GraphQLRunsetResponse>> FetchDataBySolutionAndExecutionId(Guid solutionId, Guid executionId)
    {
        if (solutionId == Guid.Empty)
        {
            throw new ArgumentException("Solution ID cannot be empty.", nameof(solutionId));
        }
        if (executionId == Guid.Empty)
        {
            throw new ArgumentException("Execution ID cannot be empty.", nameof(executionId));
        }

        const string paraList = "executionId, entityId, name";

        var queryInfo = new GraphQLQueryInfo
        {
            Filters =
            [
                new Filter
                {
                    Field = "solutionId",
                    Operator = "eq",
                    Value = solutionId
                },
                new Filter
                {
                    Field = "executionId",
                    Operator = "eq",
                    Value = executionId
                }
            ]
        };

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
