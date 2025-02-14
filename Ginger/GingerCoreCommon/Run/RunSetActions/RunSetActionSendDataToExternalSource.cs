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

using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore.ALM;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSendDataToExternalSource : RunSetActionBase
    {
        public IRunSetActionSendDataToExternalSourceOperations RunSetActionSendDataToExternalSourceOperations;
        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = [RunSetActionBase.eRunAt.ExecutionEnd];
            return list;
        }

        public override bool SupportRunOnConfig
        {
            get { return true; }
        }

        [IsSerializedForLocalRepository]
        public int selectedHTMLReportTemplateID { get; set; }

        private string mEndPointUrl;
        [IsSerializedForLocalRepository]
        public string EndPointUrl { get { return mEndPointUrl; } set { if (mEndPointUrl != value) { mEndPointUrl = value; OnPropertyChanged(nameof(EndPointUrl)); } } }

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> RequestHeaders = [];

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> RequestBodyParams = [];

        private string mRequestBodyJson;
        [IsSerializedForLocalRepository]
        public string RequestBodyJson { get { return mRequestBodyJson; } set { if (mRequestBodyJson != value) { mRequestBodyJson = value; OnPropertyChanged(nameof(RequestBodyJson)); } } }


        public override void Execute(IReportInfo RI)
        {
            RunSetActionSendDataToExternalSourceOperations.Execute(RI);
        }


        public override string GetEditPage()
        {
            return "RunSetActionSendDataToExternalSourceEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Send Execution JSON Data To External Source"; } }

        public void RefreshJsonPreview()
        {
            if (RequestBodyParams.Count > 0)
            {
                JObject obj = [];
                foreach (ActInputValue AIV in RequestBodyParams)
                {
                    if (AIV.IntValue != 0)
                    {
                        obj.Add(AIV.Param, AIV.IntValue);
                    }
                    else
                    {
                        obj.Add(AIV.Param, AIV.Value);
                    }
                }
                RequestBodyJson = obj.ToString(Formatting.Indented);
            }
        }
        public void RefreshBodyParamsPreview()
        {
            if (!string.IsNullOrEmpty(RequestBodyJson))
            {
                RequestBodyParams.ClearAll();
                try
                {
                    Dictionary<string, string> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(RequestBodyJson);
                    foreach (KeyValuePair<string, string> actInput in keyValuePairs)
                    {
                        ActInputValue actInputValue = new ActInputValue
                        {
                            Param = actInput.Key,
                            Value = actInput.Value
                        };
                        RequestBodyParams.Add(actInputValue);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to convert JSON to List View Params", ex);
                }
            }
        }

        public override PublishToALMConfig.eALMTestSetLevel GetAlMTestSetLevel()
        {
            throw new NotImplementedException();
        }
    }
}
