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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.CoreNET.External.WireMock;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.APIModels.APIModelWizard;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.WizardLib;
using GingerCore;
using GingerCoreNET.Application_Models;
using GingerWPF.WizardLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GingerWPF.ApplicationModelsLib.APIModels.APIModelWizard
{
    public class AddAPIModelWizard : WizardBase
    {
        public enum eAPIType
        {
            [EnumValueDescription("WSDL")]
            WSDL,
            [EnumValueDescription("XML Templates")]
            XMLTemplates,
            [EnumValueDescription("JSON Templates")]
            JsonTemplate,
            [EnumValueDescription("Swagger(Open API) Document")]
            Swagger
        }

        public eAPIType APIType { get; set; }

        public RepositoryFolder<ApplicationAPIModel> APIModelFolder;


        /// <summary>
        /// Gets or sets a value indicating whether WireMock mappings should be created during the finish process.
        /// </summary>
        public bool ToCreateWireMock { get; set; }

        public string URL { get; set; }

        public string InfoTitle { get; set; }

        public ObservableList<TemplateFile> XTFList = [];

        public ObservableList<ApplicationAPIModel> LearnedAPIModelsList { get; set; }
        public ObservableList<DeltaAPIModel> DeltaModelsList { get; set; }

        public bool IsParsingWasDone { get; set; }

        public WSDLParser mWSDLParser { get; set; }
        //public ObservableList<ApplicationAPIModel> SelectedAAMList { get; set; }

        public override string Title { get { return "API Model Import Wizard"; } }

        public RepositoryItemKey TargetApplicationKey { get; set; }

        public ObservableList<RepositoryItemKey> TagsKeys = [];

        public bool AvoidDuplicatesNodes { get; set; }

        public AddAPIModelWizard(RepositoryFolder<ApplicationAPIModel> APIModelsFolder)
        {
            APIModelFolder = APIModelsFolder;

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "API Model Introduction", Page: new WizardIntroPage("/ApplicationModelsLib/APIModels/APIModelWizard/AddAPIModelIntro.md"));

            AddPage(Name: "Select Document", Title: "API's Import Source", SubTitle: "Set API's Import Source/s", Page: new AddAPIModelSelectTypePage());

            AddPage(Name: "Select API", Title: "Select API's", SubTitle: "Select Desired API's to Add", Page: new ScanAPIModelWizardPage());

            AddPage(Name: "Learn Optional Parameters Values", Title: "Learn Optional Parameters Values", SubTitle: "Learn Optional Parameters Values from Sample Request Files", Page: new AdAPIModelMappingPage());

            AddPage(Name: "Extra Configurations", Title: "API's Extra Configurations", SubTitle: "Set API's Extra Configurations", Page: new AddAPIModelExtraConfigsPage());
        }

        public override void Finish()
        {
            //ExportAPIFiles(SelectedAAMList);
            if (DeltaModelsList != null && DeltaModelsList.Count > 0)
            {
                foreach (DeltaAPIModel deltaAPI in DeltaModelsList.Where(d => d.SelectedOperationEnum is DeltaAPIModel.eHandlingOperations.MergeChanges or DeltaAPIModel.eHandlingOperations.ReplaceExisting).GroupBy(d => d.matchingAPIModel).Select(d => d.First()))     // (DeltaAPIModel.matchingAPIModel)))          //.Where(d => d.IsSelected))
                {
                    if (deltaAPI.SelectedOperationEnum is DeltaAPIModel.eHandlingOperations.MergeChanges
                        or DeltaAPIModel.eHandlingOperations.ReplaceExisting)
                    {
                        if (deltaAPI.SelectedOperationEnum == DeltaAPIModel.eHandlingOperations.MergeChanges)
                        {
                            deltaAPI.MergedAPIModel.Guid = deltaAPI.matchingAPIModel.Guid;
                        }
                        else
                        {
                            deltaAPI.learnedAPI.Guid = deltaAPI.matchingAPIModel.Guid;
                        }
                        APIDeltaUtils.DeleteExistingItem(deltaAPI.matchingAPIModel);
                    }
                }
            }

            if (ToCreateWireMock)
            {
                CreateWireMockMappingsAsync(General.ConvertListToObservableList(LearnedAPIModelsList.Where(x => x.IsSelected).ToList()));
            }

            ImportAPIModels(General.ConvertListToObservableList(LearnedAPIModelsList.Where(x => x.IsSelected == true).ToList()));
        }
        private GlobalAppModelParameter AddGlobalParam(string customurl, string placehold)
        {
            GlobalAppModelParameter newModelGlobalParam = new GlobalAppModelParameter
            {
                PlaceHolder = "{" + placehold + "}"
            };
            var GlobalParams = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();
            var existingMatchingParam = GlobalParams.FirstOrDefault(g => !string.IsNullOrEmpty(g.PlaceHolder)
            && g.PlaceHolder.Equals(newModelGlobalParam.PlaceHolder, System.StringComparison.InvariantCultureIgnoreCase)
            && g.OptionalValuesList.Any(f => !string.IsNullOrEmpty(f.Value)
                 && f.Value.Equals(customurl, System.StringComparison.InvariantCultureIgnoreCase)
                 && f.IsDefault));

            if (existingMatchingParam != null)
            {
                return existingMatchingParam;
            }

            if (GlobalParams.Any(x => x.PlaceHolder.Equals(newModelGlobalParam.PlaceHolder)))
            {
                newModelGlobalParam.PlaceHolder = "{" + (!string.IsNullOrEmpty(newModelGlobalParam.PlaceHolder) ? newModelGlobalParam.PlaceHolder.Replace("{", "").Replace("}", "") : newModelGlobalParam.PlaceHolder) + "_Copy}";
            }
            ModelParamUtils.SetUniquePlaceHolderName(newModelGlobalParam);
            newModelGlobalParam.OptionalValuesList.Add(new OptionalValue() { Value = customurl, IsDefault = true });
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newModelGlobalParam);
            return newModelGlobalParam;
        }

        private async Task CreateWireMockMappingsAsync(ObservableList<ApplicationAPIModel> SelectedAAMList)
        {
            foreach (ApplicationAPIModel appmodel in SelectedAAMList)
            {
                WireMockMappingGenerator.CreateWireMockMapping(appmodel);
            }
        }

        private void ImportAPIModels(ObservableList<ApplicationAPIModel> SelectedAAMList)
        {
            GlobalAppModelParameter globalAppModelParameterForUrl = null;
            string? customUrl = string.Empty;
            if (APIType == eAPIType.Swagger)
            {
                customUrl = SelectedAAMList.FirstOrDefault()?.URLDomain;
                globalAppModelParameterForUrl = AddGlobalParam(customUrl, this.InfoTitle);
                GetOrAddRootFolder();
            }


            foreach (ApplicationAPIModel apiModel in SelectedAAMList)
            {
                if (APIType == eAPIType.Swagger)
                {
                    apiModel.EndpointURL = globalAppModelParameterForUrl?.PlaceHolder + apiModel.EndpointURL;
                    apiModel.GlobalAppModelParameters = [
                        new GlobalAppModelParameter
                        {
                            Guid = globalAppModelParameterForUrl.Guid,
                            PlaceHolder = globalAppModelParameterForUrl.PlaceHolder,
                            OptionalValuesList = [                                
                                new OptionalValue
                                {
                                    Value = customUrl,
                                    IsDefault = true
                                }                                
                            ]
                        }
                    ];
                }

                Dictionary<System.Tuple<string, string>, List<string>> OptionalValuesPerParameterDict = [];

                ImportOptionalValuesForParameters ImportOptionalValues = new ImportOptionalValuesForParameters();
                ImportOptionalValues.GetAllOptionalValuesFromExamplesFiles(apiModel, OptionalValuesPerParameterDict);
                ImportOptionalValues.PopulateOptionalValuesForAPIParameters(apiModel, OptionalValuesPerParameterDict);

                if (string.IsNullOrEmpty(apiModel.ContainingFolder))
                {
                    apiModel.ContainingFolder = APIModelFolder?.FolderFullPath;
                    try
                    {
                        if (APIType == eAPIType.Swagger && apiModel.TagsKeys?.Count > 0)
                        {
                            var apiModelPath = Path.Combine(apiModel?.ContainingFolder, apiModel?.TagsKeys?.First()?.ItemName);
                            if (amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(apiModelPath) == null)
                            {
                                APIModelFolder?.AddSubFolder(apiModel.TagsKeys.First().ItemName);
                            }

                            apiModel.ContainingFolder = apiModelPath;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error while creating folder according to the tag", ex);
                    }
                }

                if (apiModel.TargetApplicationKey == null && TargetApplicationKey != null)
                {
                    apiModel.TargetApplicationKey = TargetApplicationKey;
                }

                if (apiModel.TagsKeys != null && TagsKeys != null)
                {
                    foreach (RepositoryItemKey tagKey in TagsKeys)
                    {
                        apiModel.TagsKeys.Add(tagKey);
                    }
                }

                if (APIModelFolder?.FolderFullPath == apiModel.ContainingFolder)
                {
                    APIModelFolder?.AddRepositoryItem(apiModel);
                }
                else
                {
                    RepositoryFolderBase rfFolderBase = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(apiModel.ContainingFolder);
                    rfFolderBase.AddRepositoryItem(apiModel);
                }
            }
        }

        private void GetOrAddRootFolder()
        {
            var apiModelPath = Path.Combine(APIModelFolder.FolderFullPath, this.InfoTitle);
            var apimodelFolder = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(apiModelPath);
            if (apimodelFolder == null)
            {
                APIModelFolder = APIModelFolder.AddSubFolder(folderName: this.InfoTitle) as RepositoryFolder<ApplicationAPIModel>;
            }
            else
            {
                APIModelFolder = apimodelFolder as RepositoryFolder<ApplicationAPIModel>;
            }
        }
    }
}