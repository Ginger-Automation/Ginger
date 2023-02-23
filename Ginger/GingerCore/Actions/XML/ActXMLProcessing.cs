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
using System.Threading;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;
using amdocs.ginger.GingerCoreNET;

namespace GingerCore.Actions.XML
{
    public class ActXMLProcessing : ActWithoutDriver
    {
            public new static partial class Fields
            {
                public static string TemplateFileName = "TemplateFileName";
                public static string TargetFileName = "TargetFileName";
            }

            public override string ActionDescription { get { return "File Processing Action"; } }
            public override string ActionUserDescription { get { return "Performs File Processing action"; } }

            public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
            {
                TBH.AddText("Use this action in case you want to perform any File Processing.");
                TBH.AddLineBreak();
                TBH.AddLineBreak();
                TBH.AddText("To perform a File Processing action, enter File Template File and Target File Name.");
            }

            public override bool ObjectLocatorConfigsNeeded { get { return false; } }
            public override bool ValueConfigsNeeded { get { return false; } }
            public override string ActionEditPage { get { return "XML.ActXMLProcessingEditPage"; } }

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
                      
            public ActInputValue TemplateFileName { get { return GetOrCreateInputParam(Fields.TemplateFileName); } }
           
            public ActInputValue TargetFileName { get { return GetOrCreateInputParam(Fields.TargetFileName); } }
            
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
                    return "XML Processing";
                }
            }

            public override eImageType Image { get { return eImageType.File; } }

            public override void Execute()
            {
                // Steps
                // 1. Read Source File and replace Place Holders                
                // 2. Copy To target file
                // 3. Wait for processed file to exist
                // 4. read target file into Act.ReturnValues 

                string txt = PrepareFile();

                // Write to out file
                string fileName = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(TargetFileName.ValueForDriver);
                
                if(string.IsNullOrEmpty(fileName))
                {
                    Reporter.ToLog(eLogLevel.WARN, "Target file name can't be null.");
                }
                System.IO.File.WriteAllText(fileName, txt);

                ReadProcessedFile();
            }

            private void ReadProcessedFile()
            {
                string fileName = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(TargetFileName.ValueForDriver);

                string fileContents = System.IO.File.ReadAllText(fileName);
                XMLProcessor xmlProcessor = new XMLProcessor();
                if (System.IO.Path.GetExtension(fileName).ToLower().Equals(".xml"))
                {
                   xmlProcessor.ParseToReturnValues(fileContents, this);
                }
                else
                {
                   ParseFileToReturnValues(fileContents, this);
                }
            }
            private void ParseFileToReturnValues(string filecontent, Act act)
            {
                if (!string.IsNullOrEmpty(filecontent))
                {
                    act.AddOrUpdateReturnParamActualWithPath("Processed file", filecontent, null);
                }
            }

            private string PrepareFile()
            {
                string fileName = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(TemplateFileName.ValueForDriver);
                
                if(string.IsNullOrEmpty(fileName))
                {
                    Reporter.ToLog(eLogLevel.WARN, "Template file name can't be empty.");
                }
                string txt = System.IO.File.ReadAllText(fileName);
                foreach (ActInputValue AIV in DynamicElements)
                {
                    txt = txt.Replace(AIV.Param, AIV.ValueForDriver);
                }
                return txt;
            }
    }
}
