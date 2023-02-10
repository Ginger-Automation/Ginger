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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.Repository;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ginger.Reports
{
    public class HTMLReportConfigurationOperations : IHTMLReportConfigurationOperations
    {
        public HTMLReportConfiguration HTMLReportConfiguration;
        public HTMLReportConfigurationOperations(HTMLReportConfiguration HTMLReportConfiguration)
        {
            this.HTMLReportConfiguration = HTMLReportConfiguration;
            this.HTMLReportConfiguration.HTMLReportConfigurationOperations = this;
        }

        public bool CheckIsDefault()
        {
            if (Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetSolutionHTMLReportConfigurations().Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public int SetReportTemplateSequence(bool isAddTemplate)
        {
            if (isAddTemplate)
            {
                return WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq + 1;
            }
            else
            {
                return WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().HTMLReportTemplatesSeq - 1;
            }
        }
        public void SetHTMLReportConfigurationWithDefaultValues(HTMLReportConfiguration reportConfiguraion)
        {
            reportConfiguraion.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.ActionLevel.ToString();
            reportConfiguraion.ExecutionJsonDataLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.ActionLevel.ToString();
            reportConfiguraion.ShowAllIterationsElements = false;
            reportConfiguraion.UseLocalStoredStyling = false;
            reportConfiguraion.RunSetFieldsToSelect = GetReportLevelMembers(typeof(RunSetReport));
            reportConfiguraion.EmailSummaryViewFieldsToSelect = GetReportLevelMembers(typeof(RunSetReport));
            reportConfiguraion.GingerRunnerFieldsToSelect = GetReportLevelMembers(typeof(GingerReport));
            reportConfiguraion.BusinessFlowFieldsToSelect = GetReportLevelMembers(typeof(BusinessFlowReport));
            reportConfiguraion.ActivityGroupFieldsToSelect = GetReportLevelMembers(typeof(ActivityGroupReport));
            reportConfiguraion.ActivityFieldsToSelect = GetReportLevelMembers(typeof(ActivityReport));
            reportConfiguraion.ActionFieldsToSelect = GetReportLevelMembers(typeof(ActionReport));

            reportConfiguraion.RunSetSourceFieldsToSelect = GetReportLevelMembers(typeof(LiteDbRunSet), true);
            reportConfiguraion.GingerRunnerSourceFieldsToSelect = GetReportLevelMembers(typeof(LiteDbRunner), true);
            reportConfiguraion.BusinessFlowSourceFieldsToSelect = GetReportLevelMembers(typeof(LiteDbBusinessFlow), true);
            reportConfiguraion.ActivityGroupSourceFieldsToSelect = GetReportLevelMembers(typeof(LiteDbActivityGroup), true);
            reportConfiguraion.ActivitySourceFieldsToSelect = GetReportLevelMembers(typeof(LiteDbActivity), true);
            reportConfiguraion.ActionSourceFieldsToSelect = GetReportLevelMembers(typeof(LiteDbAction), true);

            reportConfiguraion.Description = string.Empty;
            using (var ms = new MemoryStream())
            {
                string file = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Images", "@amdocs_logo.jpg");
                Bitmap bitmap = new Bitmap(file);
                reportConfiguraion.LogoBase64Image = HTMLReportConfiguration.BitmapToBase64(bitmap);
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
            //
            HTMLReportConfiguration.RunSetSourceFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.RunSetSourceFieldsToSelect, typeof(LiteDbRunSet), HTMLReportConfiguration, true);
            HTMLReportConfiguration.GingerRunnerSourceFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.GingerRunnerSourceFieldsToSelect, typeof(LiteDbRunner), HTMLReportConfiguration, true);
            HTMLReportConfiguration.BusinessFlowSourceFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.BusinessFlowSourceFieldsToSelect, typeof(LiteDbBusinessFlow), HTMLReportConfiguration, true);
            HTMLReportConfiguration.ActivityGroupSourceFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.ActivityGroupSourceFieldsToSelect, typeof(LiteDbActivityGroup), HTMLReportConfiguration, true);
            HTMLReportConfiguration.ActivitySourceFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.ActivitySourceFieldsToSelect, typeof(LiteDbActivity), HTMLReportConfiguration, true);
            HTMLReportConfiguration.ActionSourceFieldsToSelect =
                EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames.ActionSourceFieldsToSelect, typeof(LiteDbAction), HTMLReportConfiguration, true);


            if (HTMLReportConfiguration.ReportLowerLevelToShow == null)
            {
                HTMLReportConfiguration.ReportLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.ActionLevel.ToString();
            }

            if (HTMLReportConfiguration.ExecutionJsonDataLowerLevelToShow == null)
            {
                HTMLReportConfiguration.ExecutionJsonDataLowerLevelToShow = HTMLReportConfiguration.ReportsLevel.ActionLevel.ToString();
            }

            return HTMLReportConfiguration;
        }

        public static ObservableList<HTMLReportConfigFieldToSelect> EnchancingLoadedFieldsWithDataAndValidatingPerLevel(HTMLReportConfiguration.FieldsToSelectListsNames fieldsToSelectListName, Type reportType, HTMLReportConfiguration HTMLReportConfiguration, bool isSourceFeild = false)
        {
            ObservableList<HTMLReportConfigFieldToSelect> savedFieldSelections = (ObservableList<HTMLReportConfigFieldToSelect>)HTMLReportConfiguration.GetType().GetField(fieldsToSelectListName.ToString()).GetValue(HTMLReportConfiguration);
            ObservableList<HTMLReportConfigFieldToSelect> referenceFieldSelections = GetReportLevelMembers(reportType, isSourceFeild);
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

        public static ObservableList<HTMLReportConfigFieldToSelect> GetReportLevelMembers(Type reportLevelType, bool isSourceFeild = false)
        {
            ObservableList<HTMLReportConfigFieldToSelect> fieldsToSelect = new ObservableList<HTMLReportConfigFieldToSelect>();
            MemberInfo[] members = reportLevelType.GetMembers();
            FieldParams token = null;

            foreach (MemberInfo mi in members)
            {
                token = Attribute.GetCustomAttribute(mi, typeof(FieldParams), false) as FieldParams;

                if (token == null)
                {
                    continue;
                }
                if (isSourceFeild && (Attribute.GetCustomAttribute(mi, typeof(FieldParamsFieldType), false) as FieldParamsFieldType).FieldType.ToString() == FieldsType.Section.ToString())
                {
                    continue;
                }
                else
                {
                    fieldsToSelect.Add(new HTMLReportConfigFieldToSelect(mi.Name.ToString(),
                                                                         (Attribute.GetCustomAttribute(mi, typeof(FieldParamsNameCaption), false) as FieldParamsNameCaption).NameCaption,
                                                                         (Attribute.GetCustomAttribute(mi, typeof(FieldParamsIsSelected), false) as FieldParamsIsSelected).IsSelected,
                                                                         (Attribute.GetCustomAttribute(mi, typeof(FieldParamsIsNotMandatory), false) as FieldParamsIsNotMandatory).IsNotMandatory,
                                                                         (Attribute.GetCustomAttribute(mi, typeof(FieldParamsFieldType), false) as FieldParamsFieldType).FieldType.ToString(),
                                                                         false));
                }
            }
            return fieldsToSelect;
        }
    }


}
