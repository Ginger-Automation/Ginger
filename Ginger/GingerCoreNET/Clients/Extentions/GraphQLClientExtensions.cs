#region License
/*
Copyright © 2014-2026 European Support Limited

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
