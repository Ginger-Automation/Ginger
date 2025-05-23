#region License
/*
Copyright © 2014-2025 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCoreNET.Application_Models
{
    public class APIDeltaUtils
    {
        public static ObservableList<DeltaAPIModel> DoAPIModelsCompare(ObservableList<ApplicationAPIModel> learnedAPIModelsList, ObservableList<ApplicationAPIModel> existingAPIModels = null)
        {
            ObservableList<DeltaAPIModel> deltaAPIModelsList = [];

            ObservableList<ApplicationAPIModel> existingAPIModelsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>();

            //for (int i = 0; i < learnedAPIModelsList.Count; i++)
            foreach (ApplicationAPIModel apiModelLearned in learnedAPIModelsList)
            {
                //ApplicationAPIModel apiModelLearned = learnedAPIModelsList[i];

                DeltaAPIModel apiModelDelta = new DeltaAPIModel
                {
                    learnedAPI = apiModelLearned
                };
                List<ApplicationAPIModel> matchingAPIModels;
                if (existingAPIModels == null)
                {
                    if (apiModelLearned.APIType == ApplicationAPIUtils.eWebApiType.SOAP)
                    {
                        matchingAPIModels = existingAPIModelsList.Where(m => (m.EndpointURL != null && apiModelLearned.EndpointURL != null && RemoveUrlVariables(m.EndpointURL).Equals(apiModelLearned.EndpointURL, StringComparison.OrdinalIgnoreCase)) && m.APIType == apiModelLearned.APIType && m.SOAPAction.Equals(apiModelLearned.SOAPAction)).ToList();
                    }
                    else
                    {
                        matchingAPIModels = existingAPIModelsList
                            .Where(m => (m.EndpointURL != null && apiModelLearned.EndpointURL != null &&
                                         RemoveUrlVariables(m.EndpointURL)
                                         .Equals(apiModelLearned.EndpointURL, StringComparison.OrdinalIgnoreCase))
                                        && m.APIType == apiModelLearned.APIType
                                        && m.RequestType == apiModelLearned.RequestType)
                            .ToList();
                    }
                }
                else
                {
                    matchingAPIModels = existingAPIModels.ToList();
                }

                if (matchingAPIModels != null && matchingAPIModels.Count > 0)
                {
                    ApplicationAPIModel matchingModel = matchingAPIModels[0];
                    matchingAPIModels = CompareAPIModels(apiModelLearned, matchingAPIModels);
                    if (matchingAPIModels.Count > 0)
                    {
                        apiModelDelta.comparisonStatus = DeltaAPIModel.eComparisonOutput.Unchanged;         // UNCHANGED
                        apiModelDelta.matchingAPIModel = matchingAPIModels[0];
                        apiModelDelta.IsSelected = false;
                    }
                    else
                    {
                        apiModelDelta.comparisonStatus = DeltaAPIModel.eComparisonOutput.Modified;          // MODIFIED
                        apiModelDelta.matchingAPIModel = matchingModel;
                        apiModelDelta.IsSelected = true;
                    }
                }
                else
                {
                    apiModelDelta.comparisonStatus = DeltaAPIModel.eComparisonOutput.New;                   // NEW API
                    apiModelDelta.IsSelected = true;
                }

                deltaAPIModelsList.Add(apiModelDelta);
            }

            return deltaAPIModelsList;
        }

        public static List<ApplicationAPIModel> CompareAPIModels(ApplicationAPIModel learnedModel, List<ApplicationAPIModel> existingAPIs)
        {
            if (learnedModel.APIType == ApplicationAPIUtils.eWebApiType.REST)
            {
                // Filter matching APIs based on Request Http Version
                existingAPIs = existingAPIs.Where(m => m.ReqHttpVersion == learnedModel.ReqHttpVersion).ToList();

                // Filter matching APIs based on Response Content Type
                existingAPIs = existingAPIs.Where(m => m.ResponseContentType == learnedModel.ResponseContentType).ToList();

                // Filter matching APIs based on Content Type
                existingAPIs = existingAPIs.Where(m => m.RequestContentType == learnedModel.RequestContentType).ToList();
            }

            // Filter matching APIs based on URL Domain
            existingAPIs = existingAPIs.Where(m => m.URLDomain == learnedModel.URLDomain).ToList();

            //Filter matching APIs based on EndPoint URL
            existingAPIs = existingAPIs.Where(m => m.EndpointURL != null && learnedModel.EndpointURL != null && RemoveUrlVariables(m.EndpointURL).Equals(learnedModel.EndpointURL, StringComparison.OrdinalIgnoreCase)).ToList();

            // Filter matching APIs based on HTTP Headers
            //existingAPIs = existingAPIs.Where(m => m.HttpHeaders.Equals(learnedModel.HttpHeaders)).ToList();

            for (int i = existingAPIs.Count - 1; i >= 0; i--)
            {
                ApplicationAPIModel apiMod = existingAPIs[i];
                foreach (APIModelKeyValue headerPair in apiMod.HttpHeaders)
                {
                    APIModelKeyValue apiHeaderExistings = learnedModel.HttpHeaders.FirstOrDefault(h => h.Param == headerPair.Param && h.Value == headerPair.Value);
                    if (apiHeaderExistings != null)
                    {
                        continue;
                    }
                    else
                    {
                        existingAPIs.RemoveAt(i);
                        break;
                    }
                }
            }

            // Filter matching APIs based on Request Body
            existingAPIs = existingAPIs.Where(m => m.RequestBody != null && m.RequestBody.Equals(learnedModel.RequestBody)).ToList();

            return existingAPIs;
        }

        public static void DeleteExistingItem(ApplicationAPIModel existingAPI)
        {
            RepositoryFolderBase repItemFolderBase = WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(existingAPI.ContainingFolderFullPath);
            repItemFolderBase.DeleteRepositoryItem(existingAPI);
        }

        private static string RemoveUrlVariables(string fullUrl)
        {
            if (string.IsNullOrWhiteSpace(fullUrl))
            {
                return fullUrl;
            }

            // URL is part of Model Global Param, we need to remove that only for comparison
            int startIndex = fullUrl.IndexOf('{');
            int endIndex = fullUrl.IndexOf('}');

            // If the curly braces are found in start and before end
            if (startIndex == 0 && endIndex != -1 && endIndex + 1 < fullUrl.Length)
            {
                // Removing the MGP of URL and replacing the actual endpoint
                fullUrl = fullUrl[(endIndex + 1)..].Trim();
            }

            return fullUrl;
        }

    }
}
