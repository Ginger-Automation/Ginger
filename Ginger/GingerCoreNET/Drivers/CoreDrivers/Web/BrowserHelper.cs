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
using GingerCore.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal class BrowserHelper
    {
        private readonly ActBrowserElement _act;
        public BrowserHelper(ActBrowserElement act) {
            _act = act;
        }

        public bool ShouldMonitorAllUrls()
        {
            return _act.GetOrCreateInputParam(nameof(ActBrowserElement.eMonitorUrl)).Value == nameof(ActBrowserElement.eMonitorUrl.AllUrl);
        }

        public bool ShouldMonitorUrl(string requestUrl)
        {
            return _act.GetOrCreateInputParam(nameof(ActBrowserElement.eMonitorUrl)).Value == nameof(ActBrowserElement.eMonitorUrl.SelectedUrl)
                && _act.UpdateOperationInputValues != null
                && _act.UpdateOperationInputValues.Any(x => ( !string.IsNullOrEmpty(x.ValueForDriver) ? requestUrl.ToLower().Contains(x.ValueForDriver.ToLower()) : (!string.IsNullOrEmpty(x.Value) && requestUrl.ToLower().Contains(x.Value.ToLower()))));
        }

        public void ProcessNetworkLogs(ActBrowserElement act, List<Tuple<string, object>> networkResponseLogList, List<Tuple<string, object>> networkRequestLogList)
        {
            var parsedRequestObjects = networkRequestLogList.Select(x => x.Item2 is string str ? JsonConvert.DeserializeObject<object>(str) : x.Item2).ToList();
            var parsedResponseObjects = networkResponseLogList.Select(x => x.Item2 is string str ? JsonConvert.DeserializeObject<object>(str) : x.Item2).ToList();

            string formattedRequests = JsonConvert.SerializeObject(parsedRequestObjects, Formatting.Indented);
            string formattedResponses = JsonConvert.SerializeObject(parsedResponseObjects, Formatting.Indented);

            act.AddOrUpdateReturnParamActual("Raw Request", formattedRequests);
            act.AddOrUpdateReturnParamActual("Raw Response", formattedResponses);

            for (int i = 0; i < networkRequestLogList.Count; i++)
            {
                act.AddOrUpdateReturnParamActual($"{act.ControlAction} {networkRequestLogList[i].Item1}", JsonConvert.SerializeObject(parsedRequestObjects[i], Formatting.Indented));
            }

            for (int i = 0; i < networkResponseLogList.Count; i++)
            {
                act.AddOrUpdateReturnParamActual($"{act.ControlAction} {networkResponseLogList[i].Item1}", JsonConvert.SerializeObject(parsedResponseObjects[i], Formatting.Indented));
            }

            if ((act.ControlAction==ActBrowserElement.eControlAction.GetNetworkLog && act.SaveLogToFile) || act.ControlAction==ActBrowserElement.eControlAction.StopMonitoringNetworkLog)
            {
                var parsedRequestTuples = networkRequestLogList.Select((x, i) => Tuple.Create(x.Item1, parsedRequestObjects[i])).ToList();
                var parsedResponseTuples = networkResponseLogList.Select((x, i) => Tuple.Create(x.Item1, parsedResponseObjects[i])).ToList();

                string requestParamValue = act.GetInputParamCalculatedValue(ActBrowserElement.Fields.RequestFileName);
                string responseParamValue = act.GetInputParamCalculatedValue(ActBrowserElement.Fields.ResponseFileName);
                string RequestFileName = string.IsNullOrWhiteSpace(requestParamValue) ? "NetworklogRequest" : requestParamValue;
                string ResponseFileName = string.IsNullOrWhiteSpace(responseParamValue) ? "NetworklogResponse" : responseParamValue;

                string requestPath = CreateNetworkLogFile(RequestFileName, parsedRequestTuples);
                string responsePath = CreateNetworkLogFile(ResponseFileName, parsedResponseTuples);

                act.ExInfo = $"RequestFile : {requestPath}\nResponseFile : {responsePath}\n";
                act.AddOrUpdateReturnParamActual("RequestFile", requestPath);
                act.AddOrUpdateReturnParamActual("ResponseFile", responsePath);

                Act.AddArtifactToAction(Path.GetFileName(requestPath), act, requestPath);
                Act.AddArtifactToAction(Path.GetFileName(responsePath), act, responsePath);
            }
        }
        public string CreateNetworkLogFile(string Filename, List<Tuple<string, object>> networkLogList)
        {
            if (string.IsNullOrEmpty(Filename) || networkLogList == null)
            {
                Reporter.ToLog(eLogLevel.INFO, $"Method - {MethodBase.GetCurrentMethod().Name}, Filename or networkLogList is invalid");
                return string.Empty;
            }

            string FullFilePath = string.Empty;
            try
            {
                string FullDirectoryPath = Path.Combine(WorkSpace.Instance.Solution.Folder, "Documents", "NetworkLog");
                if (!Directory.Exists(FullDirectoryPath))
                {
                    Directory.CreateDirectory(FullDirectoryPath);
                }

                FullFilePath = $"{FullDirectoryPath}{Path.DirectorySeparatorChar}{Filename}_{DateTime.Now:dd_MM_yyyy_HHmmssfff}.har";

                string FileContent = JsonConvert.SerializeObject(networkLogList.Select(x => x.Item2).ToList(), Formatting.Indented);
                File.WriteAllText(FullFilePath, FileContent);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name} Error: {ex.Message}", ex);
            }

            return FullFilePath;
        }

    }
}
