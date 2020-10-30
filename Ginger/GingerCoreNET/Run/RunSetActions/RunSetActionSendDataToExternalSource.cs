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
        
        private string mJsonOutput;
        public string JsonOutput { get { return mJsonOutput; } set { if (mJsonOutput != value) { mJsonOutput = value; OnPropertyChanged(nameof(JsonOutput)); } } }

        ValueExpression mValueExpression = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>());

        public override void Execute(ReportInfo RI)
        {
            JsonOutput = CreateJsonFromReportSourceConfig();
            SendData(JsonOutput);
        }

        public void SendData(string requestJsonBody)
        {
            Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, null, "Sending Execution data to " + EndPointUrl);
            string message = string.Format(" execution data to {0}", EndPointUrl);

            //string Host = new Uri(EndPoint).Host;
            //if (EndPoint.Contains("://"))
            //{
            //    string protocol = EndPoint.Split(new string[] { "://" }, 2, StringSplitOptions.None)[0];
            //    Host = protocol + "://" + Host;
            //}
            //RestClient restClient = new RestClient(Host);
            //RestRequest restRequest = (RestRequest)new RestRequest(new Uri(EndPoint).PathAndQuery, Method.POST);


            RestClient restClient = new RestClient(EndPointUrl);
            RestRequest restRequest = new RestRequest();
            restRequest.Method = Method.POST;
            restRequest.RequestFormat = RestSharp.DataFormat.Json;
            foreach (ActInputValue actInputValue in RequestHeaders)
            {
                mValueExpression.Value = actInputValue.Value;
                restRequest.AddHeader(actInputValue.Param, mValueExpression.ValueCalculated);
            }
            restRequest.AddJsonBody(requestJsonBody);
            try
            {
                IRestResponse response = restClient.Execute(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent"+ message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, "Failed to send"+ message + " Response: " + response.Content);
                    Errors = "Failed to send"+ message + " Response: " + response.Content;
                    Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
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
        public string CreateJsonFromReportSourceConfig()
        {
            //Latest Execution Details
            LiteDbRunSet liteDbRunSet = null;
            var loggerMode = WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod;
            if (loggerMode == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                WebReportGenerator webReporterRunner = new WebReportGenerator();
                liteDbRunSet = webReporterRunner.RunNewHtmlReport(null, null, false);
            }
            HTMLReportConfiguration currentTemplate = GetSelectedTemplate();

            //Full Json
            string json = JsonConvert.SerializeObject(liteDbRunSet, Formatting.Indented);
            JObject runSetObject = JObject.Parse(json);

            #region Generate JSON

            //Remove Fields from json which are not selected
            foreach (HTMLReportConfigFieldToSelect runsetFieldToRemove in currentTemplate.RunSetSourceFieldsToSelect.Where(x => x.IsSelected != true))
            {
                runSetObject.Property(runsetFieldToRemove.FieldKey).Remove();
            }

            //RunnersCollection
            HTMLReportConfigFieldToSelect runnerField = currentTemplate.RunSetSourceFieldsToSelect.Where(x => x.IsSelected == true && x.FieldKey == "RunnersColl").FirstOrDefault();
            if (runnerField != null)
            {
                if (currentTemplate.GingerRunnerSourceFieldsToSelect.Select(x => x.IsSelected == true).ToList().Count > 0)
                {
                    JArray runnerArray = (JArray)runSetObject[runnerField.FieldKey];
                    foreach (JObject jRunnerObject in runnerArray)
                    {
                        foreach (HTMLReportConfigFieldToSelect runnerFieldToRemove in currentTemplate.GingerRunnerSourceFieldsToSelect.Where(x => x.IsSelected != true))
                        {
                            jRunnerObject.Property(runnerFieldToRemove.FieldKey).Remove();
                        }
                        //BusinessFlowsCollection
                        HTMLReportConfigFieldToSelect bfField = currentTemplate.GingerRunnerSourceFieldsToSelect.Where(x => x.IsSelected == true && x.FieldKey == "BusinessFlowsColl").FirstOrDefault();
                        if (bfField != null)
                        {
                            if (currentTemplate.BusinessFlowSourceFieldsToSelect.Select(x => x.IsSelected == true).ToList().Count > 0)
                            {
                                JArray bfArray = (JArray)jRunnerObject[bfField.FieldKey];
                                foreach (JObject jBFObject in bfArray)
                                {
                                    foreach (HTMLReportConfigFieldToSelect bfFieldToRemove in currentTemplate.BusinessFlowSourceFieldsToSelect.Where(x => x.IsSelected != true))
                                    {
                                        jBFObject.Property(bfFieldToRemove.FieldKey).Remove();
                                    }
                                    //ActivitiesCollection
                                    HTMLReportConfigFieldToSelect activityField = currentTemplate.BusinessFlowSourceFieldsToSelect.Where(x => x.IsSelected == true && x.FieldKey == "ActivitiesColl").FirstOrDefault();
                                    if (activityField != null)
                                    {
                                        if (currentTemplate.ActivitySourceFieldsToSelect.Select(x => x.IsSelected == true).ToList().Count > 0)
                                        {
                                            JArray activityArray = (JArray)jBFObject[activityField.FieldKey];
                                            foreach (JObject jActivityObject in activityArray)
                                            {
                                                foreach (HTMLReportConfigFieldToSelect activityFieldToRemove in currentTemplate.ActivitySourceFieldsToSelect.Where(x => x.IsSelected != true))
                                                {
                                                    jActivityObject.Property(activityFieldToRemove.FieldKey).Remove();
                                                }
                                                //ActionsColl
                                                HTMLReportConfigFieldToSelect actionField = currentTemplate.ActivitySourceFieldsToSelect.Where(x => x.IsSelected == true && x.FieldKey == "ActionColl").FirstOrDefault();
                                                if (actionField != null)
                                                {
                                                    if (currentTemplate.ActionSourceFieldsToSelect.Select(x => x.IsSelected == true).ToList().Count > 0)
                                                    {
                                                        JArray actionArray = (JArray)jActivityObject[actionField.FieldKey];
                                                        foreach (JObject jActionObject in actionArray)
                                                        {
                                                            foreach (HTMLReportConfigFieldToSelect actionFieldToRemove in currentTemplate.ActionSourceFieldsToSelect.Where(x => x.IsSelected != true))
                                                            {
                                                                jActionObject.Property(actionFieldToRemove.FieldKey).Remove();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //ActivityGroupsCollection
                                    HTMLReportConfigFieldToSelect activityGroupField = currentTemplate.BusinessFlowSourceFieldsToSelect.Where(x => x.IsSelected == true && x.FieldKey == "ActivitiesGroupsColl").FirstOrDefault();
                                    if (activityGroupField != null)
                                    {
                                        if (currentTemplate.ActivityGroupSourceFieldsToSelect.Select(x => x.IsSelected == true).ToList().Count > 0)
                                        {
                                            JArray activityGroupArray = (JArray)jBFObject[activityGroupField.FieldKey];
                                            foreach (JObject jActivityGroupObject in activityGroupArray)
                                            {
                                                foreach (HTMLReportConfigFieldToSelect activityGroupFieldToRemove in currentTemplate.ActivityGroupSourceFieldsToSelect.Where(x => x.IsSelected != true))
                                                {
                                                    jActivityGroupObject.Property(activityGroupFieldToRemove.FieldKey).Remove();
                                                }
                                                //ActivitiesCollection
                                                HTMLReportConfigFieldToSelect activityFieldCheck = currentTemplate.ActivityGroupSourceFieldsToSelect.Where(x => x.IsSelected == true && x.FieldKey == "ActivitiesColl").FirstOrDefault();
                                                if (activityFieldCheck != null)
                                                {
                                                    if (currentTemplate.ActivitySourceFieldsToSelect.Select(x => x.IsSelected == true).ToList().Count > 0)
                                                    {
                                                        JArray activityArray = (JArray)jActivityGroupObject[activityFieldCheck.FieldKey];
                                                        foreach (JObject jActivityObject in activityArray)
                                                        {
                                                            foreach (HTMLReportConfigFieldToSelect activityFieldToRemove in currentTemplate.ActivitySourceFieldsToSelect.Where(x => x.IsSelected != true))
                                                            {
                                                                jActivityObject.Property(activityFieldToRemove.FieldKey).Remove();
                                                            }
                                                            //ActionsColl
                                                            HTMLReportConfigFieldToSelect actionField = currentTemplate.ActivitySourceFieldsToSelect.Where(x => x.IsSelected == true && x.FieldKey == "ActionColl").FirstOrDefault();
                                                            if (actionField != null)
                                                            {
                                                                if (currentTemplate.ActionSourceFieldsToSelect.Select(x => x.IsSelected == true).ToList().Count > 0)
                                                                {
                                                                    JArray actionArray = (JArray)jActivityObject[actionField.FieldKey];
                                                                    foreach (JObject jActionObject in actionArray)
                                                                    {
                                                                        foreach (HTMLReportConfigFieldToSelect actionFieldToRemove in currentTemplate.ActionSourceFieldsToSelect.Where(x => x.IsSelected != true))
                                                                        {
                                                                            jActionObject.Property(actionFieldToRemove.FieldKey).Remove();
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            json = runSetObject.ToString(Formatting.Indented);
            string jsonBody = string.Empty;

            if (!string.IsNullOrEmpty(RequestBodyJson))
            {
                jsonBody = RequestBodyJson.Replace("{ExecutionJsonData}", json);
            }
            else
            {
                jsonBody = RequestBodyWithParametersToJson();
                jsonBody = jsonBody.Replace("\"{ExecutionJsonData}\"", json);
            }

            return jsonBody;
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
                RequestBodyParams.Clear();
                Dictionary<string, string> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(RequestBodyJson);
                foreach (KeyValuePair<string, string> actInput in keyValuePairs)
                {
                    ActInputValue actInputValue = new ActInputValue();
                    actInputValue.Param = actInput.Key;
                    actInputValue.Value = actInput.Value;
                    RequestBodyParams.Add(actInputValue);
                }
            }
        }

        public override string Type { get { return "Send Data To External Source"; } }
    }
}
