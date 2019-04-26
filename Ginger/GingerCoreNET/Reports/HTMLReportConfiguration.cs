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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
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
        public  static partial class Fields
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
                mIsDefualt = value;
                OnPropertyChanged(nameof(IsDefault));
            }
        }

        bool mShowAllIterationsElements;
        [IsSerializedForLocalRepository]
        public bool ShowAllIterationsElements { get { return mShowAllIterationsElements; } set { if (mShowAllIterationsElements != value) { mShowAllIterationsElements = value; OnPropertyChanged(nameof(ShowAllIterationsElements)); } } }

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
            EmailSummaryViewFieldsToSelect
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
        public HTMLReportConfiguration()
        {

        }
        public HTMLReportConfiguration(string name = "", bool isRunWithFlowOnly = false)
        {
            int configID = 1;
            if (!isRunWithFlowOnly)
            {
                configID = SetReportTemplateSequence(true);
                this.ID = configID;
            }
            if (Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetSolutionHTMLReportConfigurations().Count == 0)
                this.IsDefault = true;
            else
                this.IsDefault = false;
            this.Name = SetReportTempalteName(name, configID);
            SetHTMLReportConfigurationWithDefaultValues(this);
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
        public int SetReportTemplateSequence(bool isAddTemplate)
        {
            if(isAddTemplate)
            {
                return WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq + 1;
            }
            else
            {
                return WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq - 1;
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
        public void SetHTMLReportConfigurationWithDefaultValues(HTMLReportConfiguration reportConfiguraion)
        {
            reportConfiguraion.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.ActionLevel.ToString();
            reportConfiguraion.ShowAllIterationsElements = false;
            reportConfiguraion.UseLocalStoredStyling = false;
            reportConfiguraion.RunSetFieldsToSelect = GetReportLevelMembers(typeof(RunSetReport));
            reportConfiguraion.EmailSummaryViewFieldsToSelect = GetReportLevelMembers(typeof(RunSetReport));
            reportConfiguraion.GingerRunnerFieldsToSelect = GetReportLevelMembers(typeof(GingerReport));
            reportConfiguraion.BusinessFlowFieldsToSelect = GetReportLevelMembers(typeof(BusinessFlowReport));
            reportConfiguraion.ActivityGroupFieldsToSelect = GetReportLevelMembers(typeof(ActivityGroupReport));
            reportConfiguraion.ActivityFieldsToSelect = GetReportLevelMembers(typeof(ActivityReport));
            reportConfiguraion.ActionFieldsToSelect = GetReportLevelMembers(typeof(ActionReport));
            reportConfiguraion.Description = string.Empty;
            using (var ms = new MemoryStream())
            {
                string file = Ginger.Reports.GingerExecutionReport.ExtensionMethods.getGingerEXEFileName().Replace("Ginger.exe", @"Images\@amdocs_logo.jpg");
                Bitmap bitmap = new Bitmap(file);
                reportConfiguraion.LogoBase64Image = BitmapToBase64(bitmap);
            }
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

        public static HTMLReportConfiguration EnchancingLoadedFieldsWithDataAndValidating(HTMLReportConfiguration HTMLReportConfiguration)
        {
            HTMLReportConfiguration.RunSetFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.RunSetFieldsToSelect, typeof(RunSetReport), HTMLReportConfiguration);
            HTMLReportConfiguration.EmailSummaryViewFieldsToSelect =
               EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.EmailSummaryViewFieldsToSelect, typeof(RunSetReport), HTMLReportConfiguration);
            HTMLReportConfiguration.GingerRunnerFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.GingerRunnerFieldsToSelect, typeof(GingerReport), HTMLReportConfiguration);
            HTMLReportConfiguration.BusinessFlowFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.BusinessFlowFieldsToSelect, typeof(BusinessFlowReport), HTMLReportConfiguration);
            HTMLReportConfiguration.ActivityGroupFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.ActivityGroupFieldsToSelect, typeof(ActivityGroupReport), HTMLReportConfiguration);
            HTMLReportConfiguration.ActivityFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.ActivityFieldsToSelect, typeof(ActivityReport), HTMLReportConfiguration);
            HTMLReportConfiguration.ActionFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.ActionFieldsToSelect, typeof(ActionReport), HTMLReportConfiguration);

            if (HTMLReportConfiguration.ReportLowerLevelToShow == null)
            {
                HTMLReportConfiguration.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.ActionLevel.ToString();
            }

            return HTMLReportConfiguration;
        }




        public static ObservableList<HTMLReportConfigFieldToSelect> EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames fieldsToSelectListName, Type reportType, HTMLReportConfiguration HTMLReportConfiguration)
        {
            ObservableList<HTMLReportConfigFieldToSelect> savedFieldSelections = (ObservableList<HTMLReportConfigFieldToSelect>)HTMLReportConfiguration.GetType().GetField(fieldsToSelectListName.ToString()).GetValue(HTMLReportConfiguration);
            ObservableList<HTMLReportConfigFieldToSelect> referenceFieldSelections = GetReportLevelMembers(reportType);
            // swap should be done between two below lists. Previose saved selection should be performed on the referenceFieldSelections
            foreach (var saved_item in savedFieldSelections)
            {
                var savedref_item = referenceFieldSelections.Where(x => x.FieldKey == saved_item.FieldKey).FirstOrDefault();
                if (savedref_item != null)
                {
                    if (!savedref_item.IsNotMandatory)     // if field is mandatory
                    {                                       // select it anyway
                        saved_item.IsSelected = true;
                    }
                    saved_item.FieldName = savedref_item.FieldName;
                    saved_item.FieldType = savedref_item.FieldType;
                    saved_item.IsNotMandatory = savedref_item.IsNotMandatory;
                }
            }
            //adding missing fields
            foreach (var reference_item in referenceFieldSelections)
            {
                var savedref_item = savedFieldSelections.Where(x => x.FieldKey == reference_item.FieldKey).FirstOrDefault();
                if (savedref_item == null)
                {
                    savedFieldSelections.Add(reference_item);
                }
            }
            return savedFieldSelections;
        }
        public static ObservableList<HTMLReportConfigFieldToSelect> GetReportLevelMembers(Type reportLevelType)
        {
            ObservableList<HTMLReportConfigFieldToSelect> fieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();
            MemberInfo[] members = reportLevelType.GetMembers();
            FieldParams token = null;

            foreach (MemberInfo mi in members)
            {
                token = Attribute.GetCustomAttribute(mi, typeof(FieldParams), false) as FieldParams;

                if (token == null)
                    continue;

                fieldsToSelect.Add(new HTMLReportConfigFieldToSelect(mi.Name.ToString(),
                                                                     (Attribute.GetCustomAttribute(mi, typeof(FieldParamsNameCaption), false) as FieldParamsNameCaption).NameCaption,
                                                                     (Attribute.GetCustomAttribute(mi, typeof(FieldParamsIsSelected), false) as FieldParamsIsSelected).IsSelected,
                                                                     (Attribute.GetCustomAttribute(mi, typeof(FieldParamsIsNotMandatory), false) as FieldParamsIsNotMandatory).IsNotMandatory,
                                                                     (Attribute.GetCustomAttribute(mi, typeof(FieldParamsFieldType), false) as FieldParamsFieldType).FieldType.ToString(),
                                                                     false));
            }
            return fieldsToSelect;
        }
    }

    public class HTMLReportConfigFieldToSelect : RepositoryItemBase
    {
        public  static class Fields
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
