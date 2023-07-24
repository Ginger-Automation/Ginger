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


using System;
using System.IO;

namespace Amdocs.Ginger.CoreNET.WorkSpaceLib
{
    public class ReportsInfo
    {
        private static string mEmailReportTempFolder = null;
        public string EmailReportTempFolder
        {
            get
            {
                if (mEmailReportTempFolder == null)
                {
                    mEmailReportTempFolder = Path.Combine(Path.GetDirectoryName(Path.GetTempFileName()), string.Format("Ginger_Email_Reports_{0}", DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss-fff")));
                }
                return mEmailReportTempFolder;
            }
        }
    }
}
