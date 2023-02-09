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
using Amdocs.Ginger.Repository;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Xml;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;

namespace GingerCore.Actions.JSON
{
    public class ActJSONTagValidation : ActWithoutDriver
    {
        public new static partial class Fields
        {
            public static string JsonInput = "JsonInput";
            public static string ReqisFromFile = "ReqisFromFile";
        }

        public override bool IsSelectableAction { get { return false; } }
        public override string ActionDescription { get { return "JSON Tag Validation Action"; } }
        public override string ActionUserDescription { get { return "JSON Tag Validation Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
        }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }
        public override string ActionEditPage { get { return "JSON.ActJSONValidateTagsEditPage"; } }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        [IsSerializedForLocalRepository]
        public ActInputValue JsonInput
        {
            get
            {
                return GetOrCreateInputParam(Fields.JsonInput);
            }
        }

        public bool mReqisFromFile = true;
        [IsSerializedForLocalRepository(true)]
        public bool ReqisFromFile
        {
            get
            {
                return mReqisFromFile;
            }
            set
            {
                mReqisFromFile = value;
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> DynamicElements = new ObservableList<ActInputValue>();

        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = new List<ObservableList<ActInputValue>>();
            list.Add(DynamicElements);
            return list;
        }

        public override String ActionType
        {
            get
            {
                return "JSON Tag Validation";
            }
        }

        public override eImageType Image { get { return eImageType.Search; } }        // eImageType.Help = LifeRing in FontAwesomeIcon

        public override void Execute()
        {
            string jsonContent = String.Empty;
            ReqisFromFile = false; //temp WA
            if (ReqisFromFile == true)
            {
                String FilePath = JsonInput.ValueForDriver.ToString();
                if (FilePath == null || FilePath == String.Empty)
                {
                    throw new System.ArgumentException("Please provide a valid file name");
                }

                //if (FilePath.Contains("~\\"))
                //{
                //    FilePath = FilePath.Replace("~\\", SolutionFolder);
                //}
                FilePath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(FilePath);

                jsonContent = System.IO.File.ReadAllText(FilePath);
            }

            else
            {
                jsonContent = JsonInput.ValueForDriver.ToString();
            }

            XmlDocument doc = null;
            if (((jsonContent[0] == '[') && (jsonContent[jsonContent.Length - 1] == ']')))
            {
                doc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode("{\"root\":" + jsonContent + "}", "root");
            }
            else
            {
                doc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(jsonContent, "root");
            }

            XmlNode root = doc.DocumentElement;

            List<Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem> outputTagsList = new List<Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem>();
            outputTagsList = Amdocs.Ginger.Common.GeneralLib.General.GetXMLNodesItems(doc);
            foreach (Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem outputItem in outputTagsList)
            {
                foreach (ActInputValue aiv in DynamicElements)
                {
                    string calculatedValue = ValueExpression.Calculate(@aiv.Param);
                    if (outputItem.path == "/root/" + calculatedValue)
                    {
                        AddOrUpdateReturnParamActualWithPath(outputItem.param, outputItem.value.ToString(), calculatedValue);
                        if (aiv.Value == null || aiv.Value == String.Empty)
                        {
                        }
                    }
                }
            }
        }
    }
}