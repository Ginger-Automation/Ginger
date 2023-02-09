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

//using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
//using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.Repository;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ginger.Reports
{
    public class HTMLReportConfiguration : RepositoryItemBase, IHTMLReportConfiguration
    {
        public IHTMLReportConfigurationOperations HTMLReportConfigurationOperations;
        public static partial class Fields
        {
            public static string ID = "ID";
            public static string Name = "Name";
            public static string Description = "Description";
            public static string IsSelected = "IsSelected";
            public static string IsDefault = "IsDefault";
            public static string ShowAllIterationsElements = "ShowAllIterationsElements";
            public static string LogoBase64Image = "LogoBase64Image";
            public static string ActionFieldsToSelect = "ActionFieldsToSelect";
            public static string ActivityFieldsToSelect = "ActivityFieldsToSelect";
            public static string BusinessFlowFieldsToSelect = "BusinessFlowFieldsToSelect";
            public static string GingerRunnerFieldsToSelect = "GingerRunnerFieldsToSelect";
            public static string RunSetFieldsToSelect = "RunSetFieldsToSelect";
            public static string ReportLowerLevelToShow = "ReportLowerLevelToShow";
            public static string ExecutionStatisticsCountBy = "ExecutionStatisticsCountBy";
        }

        bool mIsSelected;
        [IsSerializedForLocalRepository]
        public bool IsSelected { get { return mIsSelected; } set { if (mIsSelected != value) { mIsSelected = value; OnPropertyChanged(nameof(IsSelected)); } } }

        bool mIsDefualt;
        [IsSerializedForLocalRepository]
        public bool IsDefault
        {
            get
            {
                return mIsDefualt;
            }
            set
            {
                if (mIsDefualt != value)
                {
                    mIsDefualt = value;
                    OnPropertyChanged(nameof(IsDefault));
                }
            }
        }

        bool mShowAllIterationsElements;
        [IsSerializedForLocalRepository]
        public bool ShowAllIterationsElements { get { return mShowAllIterationsElements; } set { if (mShowAllIterationsElements != value) { mShowAllIterationsElements = value; OnPropertyChanged(nameof(ShowAllIterationsElements)); } } }

        bool mIgnoreSkippedEntities;
        [IsSerializedForLocalRepository]
        public bool IgnoreSkippedEntities
        {
            get
            {
                return mIgnoreSkippedEntities;
            }
            set
            {
                if(mIgnoreSkippedEntities != value)
                {
                    mIgnoreSkippedEntities = value;
                    OnPropertyChanged(nameof(IgnoreSkippedEntities));
                }
            }
        }

        eExecutionStatisticsCountBy mExecutionStatisticsCountBy;
        [IsSerializedForLocalRepository]
        public eExecutionStatisticsCountBy ExecutionStatisticsCountBy { get { return mExecutionStatisticsCountBy; } set { if (mExecutionStatisticsCountBy != value) { mExecutionStatisticsCountBy = value; OnPropertyChanged(nameof(ExecutionStatisticsCountBy)); } } }

        bool mUseLocalStoredStyling;
        [IsSerializedForLocalRepository]
        public bool UseLocalStoredStyling { get { return mUseLocalStoredStyling; } set { if (mUseLocalStoredStyling != value) { mUseLocalStoredStyling = value; OnPropertyChanged(nameof(UseLocalStoredStyling)); } } }

        string mLogoBase64Image;
        [IsSerializedForLocalRepository]
        public string LogoBase64Image { get { return mLogoBase64Image; } set { if (mLogoBase64Image != value) { mLogoBase64Image = value; OnPropertyChanged(nameof(LogoBase64Image)); } } }

        int mID;
        [IsSerializedForLocalRepository]
        public int ID { get { return mID; } set { if (mID != value) { mID = value; OnPropertyChanged(nameof(ID)); } } }

        string mName;
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(nameof(Name)); } } }

        string mDescription;
        [IsSerializedForLocalRepository]
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(nameof(Description)); } } }

        string mReportLowerLevelToShow;
        [IsSerializedForLocalRepository]
        public string ReportLowerLevelToShow { get { return mReportLowerLevelToShow; } set { if (mReportLowerLevelToShow != value) { mReportLowerLevelToShow = value; OnPropertyChanged(nameof(ReportLowerLevelToShow)); } } }

        private bool bIsEnableReportActivityGroup;
        [IsSerializedForLocalRepository]
        public bool IsEnableReportActivityGroup { get { return bIsEnableReportActivityGroup; } set { if (bIsEnableReportActivityGroup != value) { bIsEnableReportActivityGroup = value; OnPropertyChanged(nameof(IsEnableReportActivityGroup)); } } }

        private string mExecutionJsonDataLowerLevelToShow;
        [IsSerializedForLocalRepository]
        public string ExecutionJsonDataLowerLevelToShow { get { return mExecutionJsonDataLowerLevelToShow; } set { if (mExecutionJsonDataLowerLevelToShow != value) { mExecutionJsonDataLowerLevelToShow = value; OnPropertyChanged(nameof(ExecutionJsonDataLowerLevelToShow)); } } }

        private bool bIsEnableExecutionJsonActivityGroup;
        [IsSerializedForLocalRepository]
        public bool IsEnableExecutionJsonActivityGroup { get { return bIsEnableExecutionJsonActivityGroup; } set { if (bIsEnableExecutionJsonActivityGroup != value) { bIsEnableExecutionJsonActivityGroup = value; OnPropertyChanged(nameof(IsEnableExecutionJsonActivityGroup)); } } } // we sent the property name ExecutionJsonDataLowerLevelToShow instead of IsEnableExecutionJsonActivityGroup

        public enum ReportsLevel
        {
            ActionLevel,
            ActivityLevel,
            ActivityGroupLevel,
            BusinessFlowLevel,
            GingerRunnerLevel,
            SummaryViewLevel
        }

        public enum FieldsToSelectListsNames
        {
            ActionFieldsToSelect,
            ActivityFieldsToSelect,
            BusinessFlowFieldsToSelect,
            ActivityGroupFieldsToSelect,
            GingerRunnerFieldsToSelect,
            RunSetFieldsToSelect,
            EmailSummaryViewFieldsToSelect,

            ActionSourceFieldsToSelect,
            ActivitySourceFieldsToSelect,
            BusinessFlowSourceFieldsToSelect,
            ActivityGroupSourceFieldsToSelect,
            GingerRunnerSourceFieldsToSelect,
            RunSetSourceFieldsToSelect
        }

        public enum eExecutionStatisticsCountBy
        {
            Actions = 1,
            Activities = 0
        }

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> ActionFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> ActivityFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> ActivityGroupFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> BusinessFlowFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> GingerRunnerFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> RunSetFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> EmailSummaryViewFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        //Source Feilds Config
        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> ActionSourceFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> ActivitySourceFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> ActivityGroupSourceFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> BusinessFlowSourceFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> GingerRunnerSourceFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportConfigFieldToSelect> RunSetSourceFieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();


        public override string GetNameForFileName()
        {
            return Name;
        }

        private string _HTMLReportConfiguration = string.Empty;
        public override string ItemName
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        public override string GetItemType()
        {
            return nameof(HTMLReportConfiguration);
        }
        public HTMLReportConfiguration()
        {

        }
        public HTMLReportConfiguration(string name, bool isRunWithFlowOnly = false, IHTMLReportConfigurationOperations HTMLReportConfigurationOperations = null)
        {
            int configID = 1;
            if (!isRunWithFlowOnly)
            {
                configID = HTMLReportConfigurationOperations.SetReportTemplateSequence(true);
                this.ID = configID;
            }
            if (HTMLReportConfigurationOperations.CheckIsDefault())
            {
                this.IsDefault = true;
            }
            else
            {
                this.IsDefault = false;
            }
            this.Name = SetReportTempalteName(name, configID);
            HTMLReportConfigurationOperations.SetHTMLReportConfigurationWithDefaultValues(this);
        }
        internal void Save()
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            throw new NotImplementedException();
        }

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.HtmlReport;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }
        
        private string SetReportTempalteName(string name, int configId)
        {
            if (name.Equals(string.Empty))
            {
                return "Template #" + configId;
            }
            return name;
        }


        public static string BitmapToBase64(Bitmap bImage)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] byteImage = ms.ToArray();
                return Convert.ToBase64String(byteImage); //Get Base64
            }
        }

        
    }

    public class HTMLReportConfigFieldToSelect : RepositoryItemBase
    {
        public static class Fields
        {
            public static string FieldKey = "FieldKey";
            public static string FieldName = "FieldName";
            public static string FieldType = "FieldType";
            public static string IsSelected = "IsSelected";
            public static string IsNotMandatory = "IsNotMandatory";
            public static string IsSectionCollapsed = "IsSectionCollapsed";
        }

        public string FieldName { get; set; }

        string mFieldKey;
        [IsSerializedForLocalRepository]
        public string FieldKey { get { return mFieldKey; } set { if (mFieldKey != value) { mFieldKey = value; OnPropertyChanged(nameof(FieldKey)); } } }

        bool mIsSelected;
        [IsSerializedForLocalRepository]
        public bool IsSelected { get { return mIsSelected; } set { if (mIsSelected != value) { mIsSelected = value; OnPropertyChanged(nameof(IsSelected)); } } }

        bool mIsSectionCollapsed;
        [IsSerializedForLocalRepository]
        public bool IsSectionCollapsed { get { return mIsSectionCollapsed; } set { if (mIsSectionCollapsed != value) { mIsSectionCollapsed = value; OnPropertyChanged(nameof(IsSectionCollapsed)); } } }

        public bool IsNotMandatory { get; set; }

        private bool mIsSection;
        public bool IsSection
        {
            get
            {
                if (FieldType == "Section")
                {
                    mIsSection = true;
                }
                return mIsSection;
            }
            set
            {
                mIsSection = value;
            }
        }
        public string FieldType { get; set; }

        public HTMLReportConfigFieldToSelect(string fieldKey, string fieldName, bool isSelected, bool isNotMandatory, string fieldType, bool isSectionCollapsed)
        {
            FieldKey = fieldKey;
            FieldName = fieldName;
            IsSelected = isSelected;
            IsSectionCollapsed = isSectionCollapsed;
            IsNotMandatory = isNotMandatory;
            FieldType = fieldType;
        }

        public HTMLReportConfigFieldToSelect()
        {

        }

        private string _HTMLReportConfigFieldToSelect = string.Empty;
        public override string ItemName
        {
            get
            {
                return _HTMLReportConfigFieldToSelect;
            }
            set
            {
                _HTMLReportConfigFieldToSelect = value;
            }
        }

    }
}
