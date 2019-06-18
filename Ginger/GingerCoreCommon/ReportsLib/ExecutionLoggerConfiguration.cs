﻿#region License
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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using GingerCore;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.Reports
{
    public class ExecutionLoggerConfiguration : RepositoryItemBase
    {
        public static partial class Fields
        {
            public static string Parameter = "Parameter";
            public static string Value = "Value";
            public static string Description = "Description";
        }
        public enum AutomationTabContext
        {
            None,
            ActionRun,
            ActivityRun,
            BussinessFlowRun,
            ContinueRun,
            Reset
        }
        public enum DataRepositoryMethod
        {
            TextFile,
            LiteDB
        }

        // Why we serialzie!!?

        [IsSerializedForLocalRepository]
        public long Seq { get; set; }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public bool IsSelected { get; set; }

        [IsSerializedForLocalRepository]
        public bool ExecutionLoggerConfigurationIsEnabled { get; set; }

        [IsSerializedForLocalRepository]
        public string ExecutionLoggerConfigurationExecResultsFolder { get; set; }

        [IsSerializedForLocalRepository]
        public long ExecutionLoggerConfigurationMaximalFolderSize { get; set; }

        public string ExecutionLoggerConfigurationHTMLReportsFolder { get; set; }

        public bool ExecutionLoggerHTMLReportsAutomaticProdIsEnabled { get; set; }

        public ObservableList<IHTMLReportConfiguration> temporaryPlacedHTMLReportConfigurationList = new ObservableList<IHTMLReportConfiguration>();

        public AutomationTabContext ExecutionLoggerAutomationTabContext { get; set; }

        private string _ExecutionLoggerConfigurationSetName = string.Empty;
        private DataRepositoryMethod mDataRepositoryMethod;
        [IsSerializedForLocalRepository]
        public DataRepositoryMethod SelectedDataRepositoryMethod
        {
            get { return mDataRepositoryMethod; }
            set
            {
                if (mDataRepositoryMethod != value)
                {
                    mDataRepositoryMethod = value;
                    OnPropertyChanged(nameof(mDataRepositoryMethod));
                }
            }
        }
        public override string ItemName
        {
            get
            {
                return _ExecutionLoggerConfigurationSetName;
            }
            set
            {
                _ExecutionLoggerConfigurationSetName = value;
            }
        }

        #region General

        #endregion
    }
}
