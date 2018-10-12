#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Repository;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;


namespace GingerCore.Actions.XML
{
    public class ActXMLTagValidation : ActWithoutDriver
    {

        public enum eDocumentType
        {
            XML, JSON
        }
        public new static partial class Fields
        {
            public static string InputFile = "InputFile";
            public static string ReqisFromFile = "ReqisFromFile";
            public static string DocumentType = "DocumentType";
        }


        public override string ActionDescription { get { return "XML/JSON Tag Validation Action"; } }
        public override string ActionUserDescription { get { return string.Empty; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Validate tags in XML/JSON documents by path");
            TBH.AddLineBreak();
          
        }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public override string ActionEditPage { get { return "XML.ActXMLValidateTagsEditPage"; } }

        // return the list of platforms this action is supported on
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

       
        public ActInputValue InputFile
        {
            get
            {
                return GetOrCreateInputParam(Fields.InputFile);

            }
        }


        
        [IsSerializedForLocalRepository(true)]
        public bool ReqisFromFile
        {
            get;set;
        }
        private eDocumentType mDocumentType;
        [IsSerializedForLocalRepository]
        public eDocumentType DocumentType
        {
            get
            {
                return  mDocumentType;
            }
            set
            {
                mDocumentType = value;
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
                return "XML Tag Validation";
            }
        }


        public override System.Drawing.Image Image { get { return Resources.console16x16; } }



        public override void Execute()
        {
            string docTxt = String.Empty;

            if (ReqisFromFile == true)
            {
                String FilePath = InputFile.ValueForDriver.ToString();
                if (FilePath == null || FilePath == String.Empty)
                {
                    throw new System.ArgumentException("Please provide a valid file name");

                }

                if (FilePath.Contains("~\\"))
                {
                    FilePath = FilePath.Replace("~\\", SolutionFolder);
                }

                docTxt = System.IO.File.ReadAllText(FilePath);
            }


            else
            {
                docTxt = InputFile.ValueForDriver.ToString();
            }

            if (DocumentType == eDocumentType.XML)
                XMLValidation(docTxt);
            else if (DocumentType == eDocumentType.JSON)
                JSONValidation(docTxt);
        }

        private void JSONValidation(string json)
        {
            JToken jo = null;
            try
            {
                jo = JObject.Parse(json);
            }
            catch
            {
                jo = JArray.Parse(json);
            }

            foreach (ActInputValue aiv in DynamicElements)
            {
                ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
                VE.Value = @aiv.Param;
                if(string.IsNullOrEmpty(VE.ValueCalculated))
                    {
                    continue;
                }
                JToken Tokenfound = jo.SelectToken(VE.ValueCalculated);
                if (Tokenfound != null)
                {
                    AddOrUpdateReturnParamActualWithPath("InnerText", Tokenfound.ToString(), VE.ValueCalculated);
                    if (Tokenfound.Children().Count() > 0)
                    {
                        JsonExtended JE = new JsonExtended(Tokenfound.ToString());
                        foreach (JsonExtended item in JE.GetEndingNodes())
                        {
                            AddOrUpdateReturnParamActualWithPath(item.Name, item.JsonString, item.Path);
                        }
                    }
                }
            }
        }

        private void XMLValidation(string xml)
        {
            XmlDocument xmlReqDoc = new XmlDocument();
            xmlReqDoc.LoadXml(xml);

            // Steps
            // 1. Read Source File and replace Place Holders                
            // 2. Copy To target file
            // 3. Wait for processed file to exist
            // 4. read target file into Act.ReturnValues 
            try
            {
                foreach (ActInputValue aiv in DynamicElements)
                {
                    ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow,DSList);
                    VE.Value = @aiv.Param;
                    // var.Value = VE.ValueCalculated;

                    XmlNode node = xmlReqDoc.SelectSingleNode(VE.ValueCalculated);
                    if (node.InnerText != null)
                    {
                        AddOrUpdateReturnParamActualWithPath("InnerText", node.InnerText.ToString(), VE.ValueCalculated);
                    }

                    if (aiv.Value == null || aiv.Value == String.Empty)
                    {
                        foreach (XmlAttribute XA in node.Attributes)
                        {
                            ActReturnValue rv= ReturnValues.Where(x => x.Path == XA.Name).FirstOrDefault();
                            if (rv==null)
                                AddOrUpdateReturnParamActualWithPath(aiv.Param, XA.Value.ToString(), XA.Name);
                            else
                                rv.Actual= XA.Value.ToString();

                        }
                    }
                    else
                    {
                        if (node.Attributes != null)
                        {
                            var nameAttribute = node.Attributes[@aiv.Value];
                            ActReturnValue rv = ReturnValues.Where(x => x.Path == aiv.Value).FirstOrDefault();
                            if (rv == null)
                                AddOrUpdateReturnParamActualWithPath(aiv.Param, nameAttribute.Value.ToString(), aiv.Value);
                            else
                                rv.Actual = nameAttribute.Value.ToString();


                        }
                    }
                    
                }
            }

            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}");
                throw new System.ArgumentException("Node not found at provided path");
            }
        }
    }
}
