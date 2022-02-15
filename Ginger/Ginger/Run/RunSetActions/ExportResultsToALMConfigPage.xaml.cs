#region License
/*
Copyright © 2014-2021 European Support Limited

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
            if (runSetActionPublishToQC.VariableForTCRunName == null)
            {
                runSetActionPublishToQC.VariableForTCRunName = "GingerRun_{CS Exp=DateTime.Now}";
            }
            mPublishToALMConfig.ActionGuid = runSetActionPublishToQC.Guid;
            VariableForTCRunName.Init(null, runSetActionPublishToQC, nameof(RunSetActionPublishToQC.VariableForTCRunName));
            BindingHandler.ObjFieldBinding(VariableForTCRunName, TextBox.TextProperty, runSetActionPublishToQC, nameof(RunSetActionPublishToQC.VariableForTCRunName));
            BindingHandler.ObjFieldBinding(UseVariableInTCRunNameCbx, CheckBox.IsCheckedProperty, runSetActionPublishToQC, nameof(RunSetActionPublishToQC.isVariableInTCRunUsed));
            BindingHandler.ObjFieldBinding(AttachActivitiesGroupReportCbx, CheckBox.IsCheckedProperty, runSetActionPublishToQC, nameof(RunSetActionPublishToQC.toAttachActivitiesGroupReport));
            xFilterByStatusDroplist.BindControl(runSetActionPublishToQC, nameof(RunSetActionPublishToQC.FilterStatus));
            xALMTypeCbx.Init(runSetActionPublishToQC, nameof(RunSetActionPublishToQC.PublishALMType), GetTypeListForCbx(typeof(eALMType), new List<ComboEnumItem>() { new ComboEnumItem() { text = RunSetActionPublishToQC.AlmTypeDefault, Value = RunSetActionPublishToQC.AlmTypeDefault } }), ComboBox.TextProperty);
            xALMTestSetLevelCbx.Init(runSetActionPublishToQC, nameof(RunSetActionPublishToQC.ALMTestSetLevel), Enum.GetValues(typeof(eALMTestSetLevel)).Cast<eALMTestSetLevel>().ToList(), ComboBox.SelectedValueProperty);
            xALMTestSetLevelCbx.ComboBox.SelectionChanged += xALMTestSetLevelCbx_SelectionChanged;
            xExportTypeCbx.Init(runSetActionPublishToQC, nameof(RunSetActionPublishToQC.ExportType), Enum.GetValues(typeof(eExportType)).Cast<eExportType>().ToList(), ComboBox.SelectedValueProperty);
            xExportTypeCbx.ComboBox.SelectionChanged += xExportTypeCbx_SelectionChanged;
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

        static ExportResultsToALMConfigPage mInstance= null;
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

        public void Init(ObservableList<BusinessFlow> bfs, ValueExpression VE)
        {
            this.Title = "Export Results To ALM";
            mBfs = bfs;
            mVE = VE;
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
            await Task.Run(() => {
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
        private List<ComboEnumItem> GetTypeListForCbx(Type listType, List<ComboEnumItem> comboEnumItemsValues = null)
        {
            List<ComboEnumItem> comboEnumItemsList = new List<ComboEnumItem>();
            if (comboEnumItemsValues is not null && comboEnumItemsValues.Count > 0)
            {
                comboEnumItemsList.AddRange(comboEnumItemsValues);
            }
            comboEnumItemsList.AddRange(GingerCore.General.GetEnumValuesForCombo(listType));
            return comboEnumItemsList;
        }
        
        private void xChangeTestSetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xALMTypeCbx.ComboBoxSelectedValue.ToString().Equals(RunSetActionPublishToQC.AlmTypeDefault))
            {
                ALMIntegration.Instance.OpenALMItemsFieldsPage(eALMConfigType.MainMenu, ALMIntegration.Instance.GetALMType(), WorkSpace.Instance.Solution.ExternalItemsFields);
            }
            else
            {
                GingerCoreNET.ALMLib.ALMConfig AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(alm => alm.AlmType.ToString().Equals(xALMTypeCbx.ComboBoxSelectedValue.ToString())).FirstOrDefault();
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
                    //ObservableList<ExternalItemFieldBase> latestALMFields = ALMIntegration.Instance.AlmCore.GetALMItemFields(null, true);
                    ALMIntegration.Instance.UpdateALMType(AlmConfig.AlmType, true);
                    ObservableList<ExternalItemFieldBase> almItemFields = ALMIntegration.Instance.GetALMItemFieldsREST(true, ALM_Common.DataContracts.ResourceType.ALL, null);
                    ALMIntegration.Instance.OpenALMItemsFieldsPage(eALMConfigType.Operation, AlmConfig.AlmType, almItemFields, this.mPublishToALMConfig.ActionGuid);
                }
            }
        }
        private void xALMTestSetLevelCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xALMTestSetLevelCbx.ComboBoxSelectedValue is not null && xALMTestSetLevelCbx.ComboBoxSelectedValue.ToString().Equals(eALMTestSetLevel.BusinessFlow.ToString()))
            {
                xExportTypePanel.Visibility = Visibility.Collapsed;
                return;
            }
            xExportTypePanel.Visibility = Visibility.Visible;
            xExportTypeCbx_SelectionChanged(this, null);
        }
        private void xExportTypeCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xExportTypeCbx.ComboBoxSelectedValue is not null && xExportTypeCbx.ComboBoxSelectedValue.ToString().Equals(eExportType.ResultsOnly.ToString()))
            {
                xExportDestinationFolder.Visibility = Visibility.Collapsed;
                return;
            }
            xExportDestinationFolder.Visibility = Visibility.Visible;
        }

    }
}
