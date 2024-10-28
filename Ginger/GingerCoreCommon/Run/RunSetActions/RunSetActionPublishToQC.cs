#region License
/*
Copyright © 2014-2024 European Support Limited

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

using System.Collections.Generic;
using System.ComponentModel;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore.ALM;
using static GingerCore.ALM.PublishToALMConfig;

namespace Ginger.Run.RunSetActions
{
    //Name of the class should be RunSetActionPublishToQC 
    //If we change the name, run set xml fails to find it because it look for name RunSetActionPublishToQC
    public class RunSetActionPublishToQC : RunSetActionBase, INotifyPropertyChanged
    {
        public IRunSetActionPublishToQCOperations RunSetActionPublishToQCOperations;
        public static readonly string AlmTypeDefault = "Default";
        private string mVariableForTCRunName;
        [IsSerializedForLocalRepository]
        public string VariableForTCRunName { get { return mVariableForTCRunName; } set { if (mVariableForTCRunName != value) { mVariableForTCRunName = value; OnPropertyChanged(nameof(VariableForTCRunName)); } } }

        private bool mIsVariableInTCRunUsed;
        [IsSerializedForLocalRepository]
        public bool isVariableInTCRunUsed { get { return mIsVariableInTCRunUsed; } set { if (mIsVariableInTCRunUsed != value) { mIsVariableInTCRunUsed = value; OnPropertyChanged(nameof(isVariableInTCRunUsed)); } } }

        private bool mtoAttachActivitiesGroupReport;
        [IsSerializedForLocalRepository]
        public bool toAttachActivitiesGroupReport { get { return mtoAttachActivitiesGroupReport; } set { if (mtoAttachActivitiesGroupReport != value) { mtoAttachActivitiesGroupReport = value; OnPropertyChanged(nameof(toAttachActivitiesGroupReport)); } } }


        private bool mToExportReportLink;
        [IsSerializedForLocalRepository]
        public bool ToExportReportLink
        {
            get
            {
                return mToExportReportLink;
            }
            set
            {
                if (mToExportReportLink != value)
                {
                    mToExportReportLink = value;
                    OnPropertyChanged(nameof(ToExportReportLink));
                }
            }
        }

        private FilterByStatus mFilterStatus;
        [IsSerializedForLocalRepository]
        public FilterByStatus FilterStatus
        {
            get { return mFilterStatus; }
            set { mFilterStatus = value; }
        }
        private string mPublishALMType = AlmTypeDefault;
        [IsSerializedForLocalRepository]
        public string PublishALMType
        {
            get
            {
                return mPublishALMType;
            }
            set
            {
                if (mPublishALMType != value)
                {
                    mPublishALMType = value;
                    AlmFields.Clear();
                    OnPropertyChanged(nameof(RunSetActionPublishToQC.ALMTestSetLevel));
                }
            }
        }
        private eALMTestSetLevel mALMTestSetLevel;
        [IsSerializedForLocalRepository]
        public eALMTestSetLevel ALMTestSetLevel
        {
            get
            {
                return mALMTestSetLevel;
            }
            set
            {
                if (mALMTestSetLevel != value)
                {
                    mALMTestSetLevel = value;
                    OnPropertyChanged(nameof(RunSetActionPublishToQC.ALMTestSetLevel));
                }
            }
        }
        private eExportType mExportType;
        [IsSerializedForLocalRepository]
        public eExportType ExportType
        {
            get
            {
                return mExportType;
            }
            set
            {
                mExportType = value;
                OnPropertyChanged(nameof(RunSetActionPublishToQC.ExportType));
            }
        }

        private ObservableList<ExternalItemFieldBase> mAlmFields = [];
        [IsSerializedForLocalRepository]
        public ObservableList<ExternalItemFieldBase> AlmFields
        {
            get
            {
                return mAlmFields;
            }
            set
            {
                if (mAlmFields != value)
                {
                    mAlmFields = value;
                    OnPropertyChanged(nameof(AlmFields));
                }
            }
        }

        private string mTestSetFolderDestination;
        [IsSerializedForLocalRepository]
        public string TestSetFolderDestination
        {
            get
            {
                return mTestSetFolderDestination;
            }
            set
            {
                mTestSetFolderDestination = value;
                OnPropertyChanged(nameof(TestSetFolderDestination));
            }
        }

        private string mTestCaseFolderDestination;
        [IsSerializedForLocalRepository]
        public string TestCaseFolderDestination
        {
            get
            {
                return mTestCaseFolderDestination;
            }
            set
            {
                mTestCaseFolderDestination = value;
                OnPropertyChanged(nameof(TestCaseFolderDestination));
            }
        }
        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = [RunSetActionBase.eRunAt.ExecutionEnd, RunSetActionBase.eRunAt.DuringExecution];
            return list;
        }

        public override bool SupportRunOnConfig
        {
            get { return false; }
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            RunSetActionPublishToQCOperations.PrepareDuringExecAction(Gingers);
        }

        public override void Execute(IReportInfo RI)
        {
            if (!string.IsNullOrEmpty(PublishALMType))
            {
                Reporter.AddFeatureUsage(FeatureId.ALM, new TelemetryMetadata()
                {
                    { "Type", PublishALMType },
                    { "Operation", "Publish" },
                });
            }
            RunSetActionPublishToQCOperations.Execute(RI);
        }

        public override string GetEditPage()
        {
            //return new ExportResultsToALMConfigPage(this);
            return "ExportResultsToALMConfigPage";
        }



        public override string Type { get { return "Publish Execution Results to ALM"; } }

        public override eALMTestSetLevel GetAlMTestSetLevel()
        {
            return ALMTestSetLevel;
        }

        private bool mSearchALMEntityByName;
        [IsSerializedForLocalRepository]
        public bool SearchALMEntityByName { get { return mSearchALMEntityByName; } set { if (mSearchALMEntityByName != value) { mSearchALMEntityByName = value; OnPropertyChanged(nameof(SearchALMEntityByName)); } } }


    }
}
