using amdocs.ginger.GingerCoreNET;
using GingerCore.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal class BrowserHelper
    {
        ActBrowserElement _act;
        public BrowserHelper() {

        }

        public bool IsToMonitorAllUrls(ActBrowserElement act)
        {
            _act = act;
            return _act.GetOrCreateInputParam(nameof(ActBrowserElement.eMonitorUrl)).Value == nameof(ActBrowserElement.eMonitorUrl.AllUrl);
        }

        public bool IsToMonitorOnlySelectedUrls(ActBrowserElement act,string requestUrl)
        {
            return _act.GetOrCreateInputParam(nameof(ActBrowserElement.eMonitorUrl)).Value == nameof(ActBrowserElement.eMonitorUrl.SelectedUrl)
                && _act.UpdateOperationInputValues != null
                && _act.UpdateOperationInputValues.Any(x => !string.IsNullOrEmpty(x.ValueForDriver) && requestUrl.ToLower().Contains(x.ValueForDriver.ToLower()));
        }

        public string CreateNetworkLogFile(string Filename, List<Tuple<string, object>> networkLogList)
        {
            string FullFilePath = string.Empty;
            string FullDirectoryPath = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, "Documents", "NetworkLog");
            if (!System.IO.Directory.Exists(FullDirectoryPath))
            {
                System.IO.Directory.CreateDirectory(FullDirectoryPath);
            }

            FullFilePath = FullDirectoryPath + @"\" + Filename + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Millisecond.ToString() + ".har";
            if (!System.IO.File.Exists(FullFilePath))
            {
                string FileContent = JsonConvert.SerializeObject(networkLogList.Select(x => x.Item2).ToList());

                using (Stream fileStream = System.IO.File.Create(FullFilePath))
                {
                    fileStream.Close();
                }
                System.IO.File.WriteAllText(FullFilePath, FileContent);

            }
            return FullFilePath;
        }
    }
}
