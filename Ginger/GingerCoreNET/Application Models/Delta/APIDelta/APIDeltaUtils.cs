using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GingerCoreNET.Application_Models
{
    public class APIDeltaUtils
    {
        public static ObservableList<DeltaAPIModel> DoAPIModelsCompare(ObservableList<ApplicationAPIModel> learnedAPIModelsList, ObservableList<ApplicationAPIModel> existingAPIModels = null)
        {
            ObservableList<DeltaAPIModel> deltaAPIModelsList = new ObservableList<DeltaAPIModel>();

            ObservableList<ApplicationAPIModel> existingAPIModelsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>();

            //for (int i = 0; i < learnedAPIModelsList.Count; i++)
            foreach (ApplicationAPIModel apiModelLearned in learnedAPIModelsList)
            {
                //ApplicationAPIModel apiModelLearned = learnedAPIModelsList[i];

                DeltaAPIModel apiModelDelta = new DeltaAPIModel();
                apiModelDelta.learnedAPI = apiModelLearned;
                List<ApplicationAPIModel> matchingAPIModels;
                if (existingAPIModels == null)
                {
                    if (apiModelLearned.APIType == ApplicationAPIUtils.eWebApiType.SOAP)
                        matchingAPIModels = existingAPIModelsList.Where(m => m.EndpointURL.Equals(apiModelLearned.EndpointURL, StringComparison.OrdinalIgnoreCase) && m.APIType == apiModelLearned.APIType && m.SOAPAction.Equals(apiModelLearned.SOAPAction)).ToList();
                    else
                        matchingAPIModels = existingAPIModelsList.Where(m => m.EndpointURL.Equals(apiModelLearned.EndpointURL, StringComparison.OrdinalIgnoreCase) && m.APIType == apiModelLearned.APIType && m.RequestType == apiModelLearned.RequestType).ToList();
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

            deltaAPIModelsList = new ObservableList<DeltaAPIModel>(deltaAPIModelsList.OrderBy(d => d.comparisonStatus));

            return deltaAPIModelsList;
        }

        public static ApplicationAPIModel CreateAPIModelObject(ApplicationAPIModel sourceAPIModel)
        {
            AutoMapper.MapperConfiguration automapAPIModel = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<ApplicationAPIModel, ApplicationAPIModel>(); });
            ApplicationAPIModel DuplicateAPIModel = automapAPIModel.CreateMapper().Map<ApplicationAPIModel, ApplicationAPIModel>(sourceAPIModel);

            return DuplicateAPIModel;
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
                existingAPIs = existingAPIs.Where(m => m.ContentType == learnedModel.ContentType).ToList();
            }

            // Filter matching APIs based on URL Domain
            existingAPIs = existingAPIs.Where(m => m.URLDomain == learnedModel.URLDomain).ToList();

            // Filter matching APIs based on HTTP Headers
            //existingAPIs = existingAPIs.Where(m => m.HttpHeaders.Equals(learnedModel.HttpHeaders)).ToList();

            for (int i = existingAPIs.Count - 1; i >= 0; i--)
            {
                ApplicationAPIModel apiMod = existingAPIs[i];
                foreach (APIModelKeyValue headerPair in apiMod.HttpHeaders)
                {
                    if (learnedModel.HttpHeaders.Contains(headerPair))
                        continue;
                    else
                    {
                        existingAPIs[i] = null;
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
    }
}
