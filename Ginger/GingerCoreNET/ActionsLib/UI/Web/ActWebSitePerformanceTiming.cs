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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Actions
{
    public class ActWebSitePerformanceTiming :  Act
    {
        public new static partial class Fields
        {
            public static string CSVFileName = "CSVFileName";
            public static string Detail = "Detail";
        }

        public override string ActionDescription { get { return "WebSite Performance Timing"; } }
        public override string ActionUserDescription { get { return "The Navigation Timing Action provides data that can be used to measure the performance of a website."; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {            
            TBH.AddText("Use this action to log end-to-end latency of web site to CSV file later on you can analyze trend and much more of each parameter");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            
            TBH.AddBoldText("CSV File Name - if exist will append result to this file");
            TBH.AddLineBreak();
            TBH.AddBoldText("Details - Column for additional info like URL used, variable etc");
            TBH.AddLineBreak();

            TBH.AddBoldText("Calculated Return Values:");
            TBH.AddLineBreak();
            TBH.AddText("pageLoadTime = loadEventEnd - navigationStart");
            TBH.AddLineBreak();
            TBH.AddText("connectTime = responseEnd - requestStart");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddBoldText("Raw data Values");
            TBH.AddLineBreak();
            TBH.AddText("• navigationStart");
            TBH.AddLineBreak();
            TBH.AddText("• unloadEventStart");
            TBH.AddLineBreak();
            TBH.AddText("• unloadEventEnd");
            TBH.AddLineBreak();
            TBH.AddText("• redirectStart");
            TBH.AddLineBreak();
            TBH.AddText("• redirectEnd");
            TBH.AddLineBreak();
            TBH.AddText("• fetchStart");
            TBH.AddLineBreak();
            TBH.AddText("• domainLookupStart");
            TBH.AddLineBreak();
            TBH.AddText("• domainLookupEnd");
            TBH.AddLineBreak();
            TBH.AddText("• connectStart");
            TBH.AddLineBreak();
            TBH.AddText("• connectEnd");
            TBH.AddLineBreak();
            TBH.AddText("• secureConnectionStart");
            TBH.AddLineBreak();
            TBH.AddText("• requestStart");
            TBH.AddLineBreak();
            TBH.AddText("• responseStart");
            TBH.AddLineBreak();
            TBH.AddText("• responseEnd");
            TBH.AddLineBreak();
            TBH.AddText("• domLoading");
            TBH.AddLineBreak();
            TBH.AddText("• domInteractive");
            TBH.AddLineBreak();
            TBH.AddText("• domContentLoadedEventStart");
            TBH.AddLineBreak();
            TBH.AddText("• domContentLoadedEventEnd");
            TBH.AddLineBreak();
            TBH.AddText("• domComplete");
            TBH.AddLineBreak();
            TBH.AddText("• loadEventStart");
            TBH.AddLineBreak();
            TBH.AddText("• loadEventEnd");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddBoldText("Supported browser");
            TBH.AddLineBreak();
            TBH.AddText("Desktop: Chrome 6.0+ , Firefox 7+, Internet Explorer 9+, Opera 15.0+, Safari 8+");
            TBH.AddLineBreak();
            TBH.AddText("Mobile: Android 4.0+, Firefox 15+, IE Mobile 9+, Opera Mobile 15.0 +, Safari Mobile 8+");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("More info at:");
            TBH.AddLineBreak();
            TBH.AddBoldText("http://www.w3.org/TR/navigation-timing/");
        }

        public override string ActionEditPage { get { return "ActWebSitePerformanceTimingPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
                }
                return mPlatforms;
            }
        }

        public override String ToString()
        {
            return "Browser Performance Timing";
        }

        public override String ActionType
        {
            get
            {
                return "Browser Performance Timing";
            }
        }

        // TODO: make unique icon
        public override eImageType Image { get { return eImageType.ChartLine; } }
        
        public string CSVFileName
        {
            get
            {
                return GetInputParamValue(Fields.CSVFileName);
            }
            set
            {
                AddOrUpdateInputParamValue(Fields.CSVFileName, value);
            }
        }

        public string Detail
        {
            get
            {
                return GetInputParamValue(Fields.Detail);
            }
            set
            {
                AddOrUpdateInputParamValue(Fields.Detail, value);
            }
        }

        public void SetInfo()
        {
            AddCalculatedTimings();

            string FileName = GetInputParamValue(Fields.CSVFileName);

            if (string.IsNullOrEmpty(FileName)) return;

            if (!System.IO.File.Exists(FileName))
            {                
                string header = "Action Description, Details";
                foreach (ActReturnValue ARV in ReturnValues)
                {                    
                    header += ", " + ARV.Param;                    
                }
                System.IO.File.AppendAllText(FileName, header + Environment.NewLine);
            }

            string txt = Description;
            txt += ", " + GetInputParamCalculatedValue(Fields.Detail);

            foreach (ActReturnValue ARV in ReturnValues)
            {
                txt += ", " + ARV.Actual;
            }
            txt += Environment.NewLine;

            System.IO.File.AppendAllText(FileName, txt);
        }

        private void AddCalculatedTimings()
        {
            // pageLoadTime
            ulong loadEventEnd;
            ulong.TryParse(GetReturnParam("loadEventEnd"), out loadEventEnd);

            ulong navigationStart;
            ulong.TryParse(GetReturnParam("navigationStart"), out navigationStart);

            ulong pageLoadTime = loadEventEnd - navigationStart;
            AddOrUpdateReturnParamActual("pageLoadTime", pageLoadTime.ToString());

            // connectTime
            ulong responseEnd;
            ulong.TryParse(GetReturnParam("responseEnd"), out responseEnd);

            ulong requestStart;
            ulong.TryParse(GetReturnParam("requestStart"), out requestStart);

            ulong connectTime = responseEnd - requestStart;
            AddOrUpdateReturnParamActual("connectTime ", connectTime.ToString());
        }
    }
}
