#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore;
using System;
using System.IO;
using System.Reflection;

namespace Amdocs.Ginger
{
    // This Class is for Execution Log which will be uploaded and process to DB later as Telemetry
    // It will provide the statistics for usage, failures and more
    //    
    //
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // This Class is NOT for Application logging like errors/info etc...  - we will use LOG4NET to do it
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    public class AutoLogProxy
    {
        private static string mAccount;
        private static string mGingerVersion;

        public static void Init(string GingerVersion)
        {     
            mGingerVersion = GingerVersion;
        }

        public static void LogAppOpened(string ToolNameEx = "")
        {
         
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static string GetUserTimeZone()
        {
            DateTime currentDate = DateTime.Now;
            TimeZoneInfo localZone = TimeZoneInfo.Local;

            DateTime currentUTC = currentDate.ToUniversalTime();
            TimeSpan currentOffset = localZone.GetUtcOffset(currentDate);

            // const string dataFmt = "{0,-30}{1}";
            // const string timeFmt = "{0,-30}{1:yyyy-MM-dd HH:mm}";

            string s = currentUTC.ToString();
            s = s + " ";
            s = s + currentOffset.ToString();

            return s;
        }

        public static void LogAppClosed()
        {
            
        }

        public static void LogAction(BusinessFlow BF, IAct act)
        {
        
        }

        public static void LogActivity(BusinessFlow BF, Activity activity)
        {
        
        }

        public static void LogBusinessFlow(BusinessFlow BF)
        {
        
        }

        static string GetBusinessFlowPlatforms(BusinessFlow BF)
        {        
            return null;
        }

        public static void LogActionPublished(IAct act)
        {
         
        }

        public static void UserOperationStart(string TransactionName, string SolutionName = null, string EnvName = null)
        {
         
        }

        public static void UserOperationEnd()
        {
         
        }

        public static void SetAccount(string Account)
        {
            mAccount = Account;
        }
    }
}
