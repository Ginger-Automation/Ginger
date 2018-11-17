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
using Amdocs.Ginger.Repository;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GingerCore.Actions.XML
{
    public class ActXMLProcessing : ActWithoutDriver
    {
            public new static partial class Fields
            {
                public static string TemplateFileName = "TemplateFileName";
                public static string TargetFileName = "TargetFileName";
                public static string ProcessedFileName = "ProcessedFileName";                                
            }

            public override string ActionDescription { get { return "XML Processing Action"; } }
            public override string ActionUserDescription { get { return "Performs XML Processing action"; } }

            public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
            {
                TBH.AddText("Use this action in case you want to perform any XML Processing.");
                TBH.AddLineBreak();
                TBH.AddLineBreak();
                TBH.AddText("To perform a XML Processing action, enter XML Template File,Target File Name and Processed File Name.");
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
            
            public ActInputValue ProcessedFileName { get { return GetOrCreateInputParam(Fields.ProcessedFileName); } }

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

            public override System.Drawing.Image Image { get { return Resources.console16x16; } }

            public override void Execute()
            {
                // Steps
                // 1. Read Source File and replace Place Holders                
                // 2. Copy To target file
                // 3. Wait for processed file to exist
                // 4. read target file into Act.ReturnValues 

                string txt = PrepareFile();

                // Write to out file
                string FileName = TargetFileName.ValueForDriver.Replace(@"~\", SolutionFolder);
                System.IO.File.WriteAllText(FileName, txt);

                WaitForProcessedFile();
                ReadProcessedFile();
            }

            private void ReadProcessedFile()
            {
                string FileName = ProcessedFileName.ValueForDriver.Replace(@"~\", SolutionFolder);
                string xml = System.IO.File.ReadAllText(FileName);
                XMLProcessor XMLP = new XMLProcessor();
                XMLP.ParseToReturnValues(xml, this);
            }

            private void WaitForProcessedFile()
            {
                string FileName = ProcessedFileName.ValueForDriver.Replace(@"~\", SolutionFolder);

                while (!System.IO.File.Exists(FileName))
                {
                    Thread.Sleep(100);
                    General.DoEvents();
                }
            }

            private string PrepareFile()
            {
                string FileName = TemplateFileName.ValueForDriver.Replace(@"~\", SolutionFolder);
                string txt = System.IO.File.ReadAllText(FileName);
                foreach (ActInputValue AIV in DynamicElements)
                {
                    txt = txt.Replace(AIV.Param, AIV.ValueForDriver);
                }
                return txt;
            }
    }
}
