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

        public string CreateNetworkLogFile(string Filename, List<Tuple<string, object>> networkLogList)
        {
            if (string.IsNullOrEmpty(Filename))
            {
                Reporter.ToLog(eLogLevel.INFO, $"Method - {MethodBase.GetCurrentMethod().Name}, Filename should not be empty");
            }

            if (networkLogList == null)
            {
                Reporter.ToLog(eLogLevel.INFO, $"Method - {MethodBase.GetCurrentMethod().Name}, networkLogList should not be empty");
            }
            string FullFilePath = string.Empty;
            try
            {
                string FullDirectoryPath = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, "Documents", "NetworkLog");
                if (!System.IO.Directory.Exists(FullDirectoryPath))
                {
                    System.IO.Directory.CreateDirectory(FullDirectoryPath);
                }

                FullFilePath = $"{FullDirectoryPath}{Path.DirectorySeparatorChar}{Filename}_{DateTime.Now.Day.ToString() }_{ DateTime.Now.Month.ToString() }_{ DateTime.Now.Year.ToString() }_{DateTime.Now.Millisecond.ToString()}.har";
                if (!System.IO.File.Exists(FullFilePath))
                {
                    string FileContent = JsonConvert.SerializeObject(networkLogList.Select(x => x.Item2).ToList());

                    using (Stream fileStream = System.IO.File.Create(FullFilePath))
                    {
                        fileStream.Close();
                    }
                    System.IO.File.WriteAllText(FullFilePath, FileContent);
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name} Error: {ex.Message}", ex);
            }
            return FullFilePath;


        }
    }
}
