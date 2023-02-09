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
using System.Text;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Actions;

namespace Ginger.Export
{
    class GingerToCSV
    {
        static string csvFileName;
        public static void BrowseForFilename()
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                csvFileName = dlg.SelectedPath;
            }
        }

        public static void BusinessFlowToCSV(BusinessFlow BF)
        {
            try
            {
                csvFileName = csvFileName + "\\" + BF.Name+".csv";
                System.IO.File.WriteAllText(csvFileName, GenerateCSVfromBusinessFlow(BF), Encoding.UTF8);
            }
            catch(Exception e) 
            {
                Reporter.ToUser(eUserMsgKey.FailedToExportBF, e.Message);
            }
        }

        private static string GenerateCSVfromBusinessFlow(BusinessFlow BF)
        {            
            StringBuilder Output = new StringBuilder();

            Output.AppendLine(BF.Name.Replace(",", " ") + ",,,,,");

            foreach (Activity a in BF.Activities)
            {
                Output.AppendLine("," + a.Active.ToString() + "," + a.ActivityName.Replace(",", " ") + ",,,");
                foreach (Act act in a.Acts)
                {
                    string inputParam=act.GetInputParamValue("Value") == null ? "" : act.GetInputParamValue("Value").Replace(",", " ").Replace(Environment.NewLine, " ");
                    Output.AppendLine(",,," + act.Active.ToString() + "," + act.Description.Replace(",", " ") + "," + inputParam);
                    
                }
            }
            Output.AppendLine("EOBF,,,,,");
            return Output.ToString();
         }
    }
}