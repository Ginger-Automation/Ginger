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
                && _act.UpdateOperationInputValues.Any(x => !string.IsNullOrEmpty(x.ValueForDriver) && requestUrl.ToLower().Contains(x.ValueForDriver.ToLower()));
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
