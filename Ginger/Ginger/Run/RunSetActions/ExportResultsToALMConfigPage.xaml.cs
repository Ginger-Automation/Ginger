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

//# Status=Cleaned; Comment=Cleaned on 05/11/18
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.ALM;
using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.ALM;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static GingerCore.ALM.PublishToALMConfig;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for RunSetActionPublishToQCEditPage.xaml
    /// </summary>
    public partial class ExportResultsToALMConfigPage : Page
    {
        GenericWindow genWin = null;
        ObservableList<BusinessFlow> mBfs = new ObservableList<BusinessFlow>();
        PublishToALMConfig mPublishToALMConfig = new PublishToALMConfig();
        public bool IsProcessing = false;
        ValueExpression mVE = null;
        public ExportResultsToALMConfigPage(RunSetActionPublishToQC runSetActionPublishToQC)
        {
            InitializeComponent();
            Context context = new Context();
            if (runSetActionPublishToQC.VariableForTCRunName == null)
            {
                runSetActionPublishToQC.VariableForTCRunName = "GingerRun_{CS Exp=DateTime.Now}";
            }
            mPublishToALMConfig.AlmFields = runSetActionPublishToQC.AlmFields;
            VariableForTCRunName.Init(null, runSetActionPublishToQC, nameof(RunSetActionPublishToQC.VariableForTCRunName));
            BindingHandler.ObjFieldBinding(VariableForTCRunName, TextBox.TextProperty, runSetActionPublishToQC, nameof(RunSetActionPublishToQC.VariableForTCRunName));
            BindingHandler.ObjFieldBinding(UseVariableInTCRunNameCbx, CheckBox.IsCheckedProperty, runSetActionPublishToQC, nameof(RunSetActionPublishToQC.isVariableInTCRunUsed));
            BindingHandler.ObjFieldBinding(AttachActivitiesGroupReportCbx, CheckBox.IsCheckedProperty, runSetActionPublishToQC, nameof(RunSetActionPublishToQC.toAttachActivitiesGroupReport));
            xFilterByStatusDroplist.BindControl(runSetActionPublishToQC, nameof(RunSetActionPublishToQC.FilterStatus));
            xALMTypeCbx.Init(runSetActionPublishToQC, nameof(RunSetActionPublishToQC.PublishALMType),
                GingerCore.General.GetEnumValuesForComboAndAddExtraValues(typeof(eALMType), new List<ComboEnumItem>() { new ComboEnumItem() { text = RunSetActionPublishToQC.AlmTypeDefault, Value = RunSetActionPublishToQC.AlmTypeDefault } }), ComboBox.TextProperty);
            xALMTestSetLevelCbx.Init(runSetActionPublishToQC, nameof(RunSetActionPublishToQC.ALMTestSetLevel), Enum.GetValues(typeof(eALMTestSetLevel)).Cast<eALMTestSetLevel>().ToList(), ComboBox.SelectedValueProperty);
            xALMTestSetLevelCbx.ComboBox.SelectionChanged += xALMTestSetLevelCbx_SelectionChanged;
            xExportTypeCbx.Init(runSetActionPublishToQC, nameof(RunSetActionPublishToQC.ExportType), Enum.GetValues(typeof(eExportType)).Cast<eExportType>().ToList(), ComboBox.SelectedValueProperty);
            xTestSetFolderDestination.Init(context, runSetActionPublishToQC, nameof(RunSetActionPublishToQC.TestSetFolderDestination));
            xTestCaseFolderDestination.Init(context, runSetActionPublishToQC, nameof(RunSetActionPublishToQC.TestCaseFolderDestination));
            xExportTypeCbx.ComboBox.SelectionChanged += xExportTypeCbx_SelectionChanged;
            xALMTypeCbx.ComboBox.SelectionChanged += xALMTypeCbx_SelectionChanged;
            xALMTestSetLevelCbx_SelectionChanged(this, null);
            xALMTypeCbx_SelectionChanged(this, null);
        }

        private ExportResultsToALMConfigPage()
        {
            InitializeComponent();

            if (VariableForTCRunName == null)
            {
                VariableForTCRunName.Content = "GingerRun_{CS Exp=DateTime.Now}";
            }
            VariableForTCRunName.Init(null, mPublishToALMConfig, nameof(PublishToALMConfig.VariableForTCRunName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(VariableForTCRunName, TextBox.TextProperty, mPublishToALMConfig, nameof(PublishToALMConfig.VariableForTCRunName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(UseVariableInTCRunNameCbx, CheckBox.IsCheckedProperty, mPublishToALMConfig, nameof(PublishToALMConfig.IsVariableInTCRunUsed));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AttachActivitiesGroupReportCbx, CheckBox.IsCheckedProperty, mPublishToALMConfig, nameof(PublishToALMConfig.ToAttachActivitiesGroupReport));
            xFilterByStatusDroplist.BindControl(mPublishToALMConfig, nameof(PublishToALMConfig.FilterStatus));
        }

        static ExportResultsToALMConfigPage mInstance = null;
        public static ExportResultsToALMConfigPage Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ExportResultsToALMConfigPage();
                }
                return mInstance;
            }
        }

        public bool Init(ObservableList<BusinessFlow> bfs, ValueExpression VE)
        {
            this.Title = "Export Results To ALM";
            GingerCoreNET.ALMLib.ALMConfig AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.DefaultAlm).FirstOrDefault();
            if (AlmConfig != null)
            {
                xALMTypeCbx.Init(AlmConfig.AlmType, nameof(RunSetActionPublishToQC.PublishALMType), Enum.GetValues(typeof(eALMType)).Cast<eALMType>().ToList(), ComboBox.SelectedValueProperty);
                xALMTestSetLevelCbx.Init(PublishToALMConfig.eALMTestSetLevel.BusinessFlow, nameof(RunSetActionPublishToQC.ALMTestSetLevel), Enum.GetValues(typeof(eALMTestSetLevel)).Cast<eALMTestSetLevel>().ToList(), ComboBox.SelectedValueProperty);
                xALMTypeCbx.ComboBox.SelectedValue = AlmConfig.AlmType;
                xALMTestSetLevelCbx.ComboBox.SelectedValue = PublishToALMConfig.eALMTestSetLevel.BusinessFlow;
                xALMTestSetLevelCbx.IsEnabled = false;
                xALMTypeCbx.IsEnabled = false;
                mBfs = bfs;
                mVE = VE;
                return true;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Please configure ALM Settings");
                return false;
            }
        }
        internal void ShowAsWindow()
        {
            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button SaveAllButton = new Button();
            SaveAllButton.Content = "Export To ALM";
            SaveAllButton.Click += new RoutedEventHandler(xExportToALMBtn_Click);
            winButtons.Add(SaveAllButton);
            this.Width = 500;
            this.Height = 180;
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, eWindowShowStyle.Dialog, this.Title, this, winButtons, true);
        }

        private async void xExportToALMBtn_Click(object sender, RoutedEventArgs e)
        {
            string result = string.Empty;
            xExportToALMLoadingIcon.Visibility = Visibility.Visible;
            IsProcessing = true;
            mPublishToALMConfig.CalculateTCRunName(mVE);
            await Task.Run(() =>
            {
                ALMIntegration.Instance.ExportBusinessFlowsResultToALM(mBfs, ref result, mPublishToALMConfig, eALMConnectType.Auto, true);
            });
            IsProcessing = false;
            xExportToALMLoadingIcon.Visibility = Visibility.Collapsed;
            Reporter.ToUser(eUserMsgKey.ExportedExecDetailsToALM, result);
        }

        private void UseVariableInTCRunNameCbx_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)UseVariableInTCRunNameCbx.IsChecked)
            {
                VariableForTCRunNamePanel.IsEnabled = false;
            }
        }

        private void UseVariableInTCRunNameCbx_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)UseVariableInTCRunNameCbx.IsChecked)
            {
                VariableForTCRunNamePanel.IsEnabled = true;
            }
        }
        private void AttachActivitiesGroupReportCbx_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void AttachActivitiesGroupReportCbx_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void xSetFieldsBtn_Click(object sender, RoutedEventArgs e)
        {
            GingerCoreNET.ALMLib.ALMConfig AlmConfig = null;
            if (xALMTypeCbx.ComboBoxSelectedValue.ToString().Equals(RunSetActionPublishToQC.AlmTypeDefault))
            {
                AlmConfig = ALMIntegration.Instance.GetDefaultAlmConfig();
            }
            else
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(alm => alm.AlmType.ToString().Equals(xALMTypeCbx.ComboBoxSelectedValue.ToString())).FirstOrDefault();
            }
            if (AlmConfig is null)
            {
                ALMConnectionPage almConnPage = new ALMConnectionPage(eALMConnectType.Auto, true);
                almConnPage.ShowAsWindow();
                if (ALMIntegration.Instance.TestALMProjectConn(eALMConnectType.Auto))
                {
                    return;
                }
            }
            else
            {

                try
                {
                    ALMIntegration.Instance.UpdateALMType(AlmConfig.AlmType, true);
                    ObservableList<ExternalItemFieldBase> almItemFields = ALMIntegration.Instance.GetALMItemFieldsREST(true, AlmDataContractsStd.Enums.ResourceType.ALL, null);
                    ObservableList<ExternalItemFieldBase> operationItemFields = new ObservableList<ExternalItemFieldBase>();
                    foreach (ExternalItemFieldBase field in mPublishToALMConfig.AlmFields)
                    {
                        operationItemFields.Add(field);
                    }
                    foreach (ExternalItemFieldBase field in operationItemFields)
                    {
                        mPublishToALMConfig.AlmFields.Remove(field);
                    }
                    if (almItemFields is not null)
                    {
                        almItemFields = ALMIntegration.Instance.AlmCore.RefreshALMItemFields(operationItemFields, almItemFields);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.ALMOperationFailed, "Failed get ALM items fields");
                        return;
                    }
                    ALMIntegration.Instance.OpenALMItemsFieldsPage(eALMConfigType.Operation, AlmConfig.AlmType, almItemFields);
                    operationItemFields = ALMIntegration.Instance.GetUpdatedFields(almItemFields, false);
                    foreach (ExternalItemFieldBase field in operationItemFields)
                    {
                        mPublishToALMConfig.AlmFields.Add(field);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while Fetching Fields", ex);
                }
                finally
                {
                    ALMIntegration.Instance.UpdateALMType(ALMCore.GetDefaultAlmConfig().AlmType);
                }
            }
        }
        private void xALMTestSetLevelCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xALMTestSetLevelCbx.ComboBoxSelectedValue is not null && xALMTestSetLevelCbx.ComboBoxSelectedValue.ToString().Equals(eALMTestSetLevel.BusinessFlow.ToString()))
            {
                xExportTypePanel.Visibility = Visibility.Collapsed;
                xExportDestinationFolder.Visibility = Visibility.Collapsed;
                return;
            }
            xExportTypePanel.Visibility = Visibility.Visible;
            xExportTypeCbx_SelectionChanged(this, null);
        }
        private void xExportTypeCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xExportTypeCbx.ComboBoxSelectedValue is null) //&& xExportTypeCbx.ComboBoxSelectedValue.ToString().Equals(eExportType.ResultsOnly.ToString()))
            {
                xExportDestinationFolder.Visibility = Visibility.Collapsed;
                return;
            }
            xExportDestinationFolder.Visibility = Visibility.Visible;
            xExportTypeCbx.ComboBox.SelectedValue = eExportType.EntitiesAndResults;
            xExportTypeCbx.IsEnabled = false;
        }
        private void xALMTypeCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xALMTypeCbx.ComboBoxSelectedValue is not null && xALMTypeCbx.ComboBoxSelectedValue.ToString().Equals(RunSetActionPublishToQC.AlmTypeDefault))
            {
                xSetFieldsBtn.Visibility = Visibility.Collapsed;
                return;
            }
            xSetFieldsBtn.Visibility = Visibility.Visible;
        }
    }
}
