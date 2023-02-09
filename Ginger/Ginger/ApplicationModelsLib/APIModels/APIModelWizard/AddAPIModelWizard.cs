#region License
/*
Copyright © 2014-2023 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.APIModels;
using Ginger.ApplicationModelsLib.APIModels.APIModelWizard;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.WizardLib;
using GingerCore;
using GingerCoreNET.Application_Models;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public enum eAPITypeTemp
        {
            [EnumValueDescription("WSDL")]
            WSDL
        }

        public eAPIType APIType { get; set; }

        public RepositoryFolder<ApplicationAPIModel> APIModelFolder;

        public string URL { get; set; }

        public ObservableList<TemplateFile> XTFList = new ObservableList<TemplateFile>();

        public ObservableList<ApplicationAPIModel> LearnedAPIModelsList { get; set; }
        public ObservableList<DeltaAPIModel> DeltaModelsList { get; set; }

        public bool IsParsingWasDone { get; set; }

        public WSDLParser mWSDLParser { get; set; }
        //public ObservableList<ApplicationAPIModel> SelectedAAMList { get; set; }

        public override string Title { get { return "API Model Import Wizard"; } }

        public RepositoryItemKey TargetApplicationKey { get; set; }

        public ObservableList<RepositoryItemKey> TagsKeys = new ObservableList<RepositoryItemKey>();

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
            if(DeltaModelsList != null && DeltaModelsList.Count > 0)
            {
                foreach(DeltaAPIModel deltaAPI in DeltaModelsList.Where(d => d.SelectedOperationEnum == DeltaAPIModel.eHandlingOperations.MergeChanges || d.SelectedOperationEnum == DeltaAPIModel.eHandlingOperations.ReplaceExisting).GroupBy(d => d.matchingAPIModel).Select(d => d.First()))     // (DeltaAPIModel.matchingAPIModel)))          //.Where(d => d.IsSelected))
                {
                    if(deltaAPI.SelectedOperationEnum == DeltaAPIModel.eHandlingOperations.MergeChanges 
                        || deltaAPI.SelectedOperationEnum == DeltaAPIModel.eHandlingOperations.ReplaceExisting)
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

            ImportAPIModels(General.ConvertListToObservableList(LearnedAPIModelsList.Where(x => x.IsSelected == true).ToList()));
        }

        private void ImportAPIModels(ObservableList<ApplicationAPIModel> SelectedAAMList)
        {
            foreach (ApplicationAPIModel apiModel in SelectedAAMList)
            {
                Dictionary<System.Tuple<string, string>, List<string>> OptionalValuesPerParameterDict = new Dictionary<Tuple<string, string>, List<string>>();

                ImportOptionalValuesForParameters ImportOptionalValues = new ImportOptionalValuesForParameters();
                ImportOptionalValues.GetAllOptionalValuesFromExamplesFiles(apiModel, OptionalValuesPerParameterDict);
                ImportOptionalValues.PopulateOptionalValuesForAPIParameters(apiModel, OptionalValuesPerParameterDict);

                if (string.IsNullOrEmpty(apiModel.ContainingFolder))
                {
                    apiModel.ContainingFolder = APIModelFolder.FolderFullPath;
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

                if (APIModelFolder.FolderFullPath == apiModel.ContainingFolder)
                {
                    APIModelFolder.AddRepositoryItem(apiModel);
                }
                else
                {
                    RepositoryFolderBase rfFolderBase = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(apiModel.ContainingFolder);
                    rfFolderBase.AddRepositoryItem(apiModel);
                }

            }
        }
    }
}