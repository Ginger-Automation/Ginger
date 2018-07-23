#region License
/*
Copyright © 2014-2018 European Support Limited

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
using GingerCore;

namespace Ginger.Reports
{
    public class HTMLReportsConfiguration : RepositoryItemBase
    {
        public override bool UseNewRepositorySerializer { get { return true; } }

        public enum AutomationTabContext
        {
            None,
            ActionRun,
            ActivityRun,
            BussinessFlowRun
        }

        public  static class Fields
        {
            public static string Name = "Name";
            public static string IsSelected = "IsSelected";
            public static string HTMLReportsFolder = "HTMLReportsFolder";
            public static string HTMLReportsAutomaticProdIsEnabled = "HTMLReportsAutomaticProdIsEnabled";
        } 

        [IsSerializedForLocalRepository]
        public long Seq { get; set; }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public bool LimitReportFolderSize { get; set; }

        [IsSerializedForLocalRepository]
        public bool IsSelected { get; set; }
        
        [IsSerializedForLocalRepository]
        public string HTMLReportsFolder { get; set; }

        [IsSerializedForLocalRepository]
        public bool HTMLReportsAutomaticProdIsEnabled { get; set; }

        [IsSerializedForLocalRepository]
        public long HTMLReportConfigurationMaximalFolderSize { get; set; }

        [IsSerializedForLocalRepository]
        public int HTMLReportTemplatesSeq { get; set; }

        private string _HTMLReportsConfigurationSetName = string.Empty;

        public override string ItemName
        {
            get
            {
                return _HTMLReportsConfigurationSetName;
            }
            set
            {
                _HTMLReportsConfigurationSetName = value;
            }
        }

        #region General

        public static void ExecutionResultsConfigurationPage()
        {
            ExecutionResultsConfiguration.Instance.ShowAsWindow();
        }

        #endregion
    }
}