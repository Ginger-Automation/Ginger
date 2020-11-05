#region License
/*
Copyright © 2014-2020 European Support Limited

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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Ginger.Reports;
using Amdocs.Ginger;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore;
using GingerCore.DataSource;
using Ginger.Reports.GingerExecutionReport;
using Amdocs.Ginger.CoreNET.Logger;
using System.IO;
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Newtonsoft.Json;
using System.Text;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Dynamic;
using RestSharp;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSendDataToExternalSource : RunSetActionBase
    {
        public new static class Fields
        {
            public static string EndPoint = "EndPoint";
            public static string selectedHTMLReportTemplateID = "selectedHTMLReportTemplateID";
        }

        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
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
        public ObservableList<ActInputValue> RequestHeaders = new ObservableList<ActInputValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> RequestBodyParams = new ObservableList<ActInputValue>();

        private string mRequestBodyJson;
        [IsSerializedForLocalRepository]
        public string RequestBodyJson { get { return mRequestBodyJson; } set { if (mRequestBodyJson != value) { mRequestBodyJson = value; OnPropertyChanged(nameof(RequestBodyJson)); } } }
        
        //private string mJsonOutput;
        //public string JsonOutput { get { return mJsonOutput; } set { if (mJsonOutput != value) { mJsonOutput = value; OnPropertyChanged(nameof(JsonOutput)); } } }

        ValueExpression mValueExpression = null;

        public override void Execute(ReportInfo RI)
        {
            Context mContext = new Context();
            mContext.RunsetAction = this;
            mValueExpression = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, mContext, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>());

            if (!string.IsNullOrEmpty(RequestBodyJson))
            {
                mValueExpression.Value = RequestBodyJson;
            }
            else
            {
                string jsonBody = RequestBodyWithParametersToJson();
                mValueExpression.Value = jsonBody.Replace("\"{ExecutionJsonData}\"", "{ExecutionJsonData}");
            }
            string JsonOutput = mValueExpression.ValueCalculated;

            Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, null, "Sending Execution data to " + EndPointUrl);
            string message = string.Format(" execution data to {0}", EndPointUrl);

            RestClient restClient = new RestClient(EndPointUrl);
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            RestRequest restRequest = new RestRequest();
            restRequest.Method = Method.POST;
            restRequest.RequestFormat = RestSharp.DataFormat.Json;
            foreach (ActInputValue actInputValue in RequestHeaders)
            {
                mValueExpression.Value = actInputValue.Value;
                restRequest.AddHeader(actInputValue.Param, mValueExpression.ValueCalculated);
            }
            restRequest.AddJsonBody(JsonOutput);
            
            try
            {
                IRestResponse response = restClient.Execute(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent" + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send" + message + " Response: " + response.ErrorMessage);
                    Errors = "Failed to send" + message + " Response: " + response.ErrorMessage;
                    Status = eRunSetActionStatus.Failed;
                    return;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }

        public HTMLReportConfiguration GetSelectedTemplate()
        {
            HTMLReportConfiguration currentTemplate = new HTMLReportConfiguration();
            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            currentTemplate = HTMLReportConfigurations.Where(x => (x.ID == selectedHTMLReportTemplateID)).FirstOrDefault();
            if (currentTemplate == null && selectedHTMLReportTemplateID == 100)
            {
                currentTemplate = HTMLReportConfigurations.Where(x => (x.IsDefault == true)).FirstOrDefault();
            }
            return currentTemplate;
        }
        private string RequestBodyWithParametersToJson()
        {
            Dictionary<string, string> requestBodyParams = new Dictionary<string, string>();

            foreach (ActInputValue AIV in RequestBodyParams)
            {
                mValueExpression.Value = AIV.Value;
                requestBodyParams.Add(AIV.Param, mValueExpression.ValueCalculated);
            }
            return JsonConvert.SerializeObject(requestBodyParams,Formatting.Indented);
        }
        public override string GetEditPage()
        {
            return "RunSetActionSendDataToExternalSourceEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public void RefreshJsonPreview()
        {
            if (RequestBodyParams.Count > 0)
            {
                JObject obj = new JObject();
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
                        ActInputValue actInputValue = new ActInputValue();
                        actInputValue.Param = actInput.Key;
                        actInputValue.Value = actInput.Value;
                        RequestBodyParams.Add(actInputValue);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to convert JSON to List View Params", ex);
                }
            }
        }

        public override string Type { get { return "Send Execution JSON Data To External Source"; } }
    }
}
