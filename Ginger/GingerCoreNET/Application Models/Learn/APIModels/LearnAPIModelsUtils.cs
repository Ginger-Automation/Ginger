//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.Repository;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Amdocs.Ginger.CoreNET.Application_Models
//{
//    public class LearnAPIModelsUtils
//    {
//        public static void ImportAPIModels(ObservableList<ApplicationAPIModel> SelectedAAMList, bool IsComparisonDone)
//        {
//            foreach (ApplicationAPIModel apiModel in SelectedAAMList)
//            {
//                Dictionary<System.Tuple<string, string>, List<string>> OptionalValuesPerParameterDict = new Dictionary<Tuple<string, string>, List<string>>();

//                ImportOptionalValuesForParameters ImportOptionalValues = new ImportOptionalValuesForParameters();
//                ImportOptionalValues.GetAllOptionalValuesFromExamplesFiles(apiModel, OptionalValuesPerParameterDict);
//                ImportOptionalValues.PopulateOptionalValuesForAPIParameters(apiModel, OptionalValuesPerParameterDict);

//                DeltaAPIModel deltaModel = null;

//                if (IsComparisonDone)
//                {
//                    deltaModel = DeltaModelsList.Where(d => d.learnedAPI == apiModel || d.MergedAPIModel == apiModel).FirstOrDefault();
//                }

//                if (deltaModel == null || deltaModel.DefaultOperationEnum == DeltaAPIModel.eHandlingOperations.Add)
//                {
//                    apiModel.ContainingFolder = APIModelFolder.FolderFullPath;

//                    if (TargetApplicationKey != null)
//                    {
//                        apiModel.TargetApplicationKey = TargetApplicationKey;
//                    }
//                    if (TagsKeys != null)
//                    {
//                        foreach (RepositoryItemKey tagKey in TagsKeys)
//                        {
//                            apiModel.TagsKeys.Add(tagKey);
//                        }
//                    }
//                    APIModelFolder.AddRepositoryItem(apiModel);
//                }
//                else
//                {
//                    apiModel.ContainingFolderFullPath = deltaModel.matchingAPIModel.ContainingFolderFullPath;

//                    apiModel.TargetApplicationKey = deltaModel.matchingAPIModel.TargetApplicationKey;

//                    foreach (RepositoryItemKey tagKey in deltaModel.matchingAPIModel.TagsKeys)
//                    {
//                        apiModel.TagsKeys.Add(tagKey);
//                    }

//                    APIModelFolder.DeleteRepositoryItem(deltaModel.matchingAPIModel);
//                    APIModelFolder.AddRepositoryItem(apiModel);
//                }

//            }
//        }
//    }
//}
