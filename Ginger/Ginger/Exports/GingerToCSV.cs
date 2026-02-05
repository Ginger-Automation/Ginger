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

using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Text;

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
                csvFileName = csvFileName + "\\" + BF.Name + ".csv";
                System.IO.File.WriteAllText(csvFileName, GenerateCSVfromBusinessFlow(BF), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKey.FailedToExportBF, e.Message);
            }
        }

        private static string GenerateCSVfromBusinessFlow(BusinessFlow businessFlow)
        {
            StringBuilder Output = new StringBuilder();

            string bfName = businessFlow.Name.Replace(",", " ");
            string bfNameCheck = bfName.TrimStart();
            if (!string.IsNullOrEmpty(bfNameCheck) && "=+-@".Contains(bfNameCheck[0])) bfName = "'" + bfName;
            Output.AppendLine(bfName + ",,,,,");

            foreach (Activity activity in businessFlow.Activities)
            {
                string actName = activity.ActivityName.Replace(",", " ");
                string actNameCheck = actName.TrimStart();
                if (!string.IsNullOrEmpty(actNameCheck) && "=+-@".Contains(actNameCheck[0])) actName = "'" + actName;
                Output.AppendLine("," + activity.Active.ToString() + "," + actName + ",,,");
                foreach (Act act in activity.Acts)
                {
                    string inputParam = act.GetInputParamValue("Value") == null ? "" : act.GetInputParamValue("Value").Replace(",", " ").Replace(Environment.NewLine, " ");
                    string inputParamCheck = inputParam.TrimStart();
                    if (!string.IsNullOrEmpty(inputParamCheck) && "=+-@".Contains(inputParamCheck[0])) inputParam = "'" + inputParam;

                    string desc = (act.Description ?? string.Empty).Replace(",", " ");
                    string descCheck = desc.TrimStart();
                    if (!string.IsNullOrEmpty(descCheck) && "=+-@".Contains(descCheck[0])) desc = "'" + desc;

                    Output.AppendLine(",,," + act.Active.ToString() + "," + desc + "," + inputParam);
                }
            }
            Output.AppendLine("EOBF,,,,,");
            return Output.ToString();

        }
    }
}