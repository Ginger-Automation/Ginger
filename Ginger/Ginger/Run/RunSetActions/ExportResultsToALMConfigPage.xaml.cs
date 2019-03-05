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

//# Status=Cleaned; Comment=Cleaned on 05/11/18
using Amdocs.Ginger.Common;
using Ginger.ALM;
using System.Windows;
using System.Windows.Controls;
using System;
using GingerCore.ALM;
using GingerCore;
using System.Threading.Tasks;
using Ginger.Run.RunSetActions;


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
                runSetActionPublishToQC.VariableForTCRunName = "GingerRun_{VBS Eval=now()}";
            }

            VariableForTCRunName.Init(null, runSetActionPublishToQC, RunSetActionPublishToQC.Fields.VariableForTCRunName);

            App.ObjFieldBinding(VariableForTCRunName, TextBox.TextProperty, runSetActionPublishToQC, RunSetActionPublishToQC.Fields.VariableForTCRunName);
            App.ObjFieldBinding(UseVariableInTCRunNameCbx, CheckBox.IsCheckedProperty, runSetActionPublishToQC, RunSetActionPublishToQC.Fields.isVariableInTCRunUsed);
            App.ObjFieldBinding(AttachActivitiesGroupReportCbx, CheckBox.IsCheckedProperty, runSetActionPublishToQC, RunSetActionPublishToQC.Fields.toAttachActivitiesGroupReport);
            xFilterByStatusDroplist.BindControl(runSetActionPublishToQC, nameof(RunSetActionPublishToQC.FilterStatus));            
        }
        private ExportResultsToALMConfigPage()
        {
            InitializeComponent();

            if (VariableForTCRunName == null)
            {
                VariableForTCRunName.Content = "GingerRun_{VBS Eval=now()}";
            }
            VariableForTCRunName.Init(null, mPublishToALMConfig, nameof(PublishToALMConfig.VariableForTCRunName));
            App.ObjFieldBinding(VariableForTCRunName, TextBox.TextProperty, mPublishToALMConfig, nameof(PublishToALMConfig.VariableForTCRunName));
            App.ObjFieldBinding(UseVariableInTCRunNameCbx, CheckBox.IsCheckedProperty, mPublishToALMConfig, nameof(PublishToALMConfig.IsVariableInTCRunUsed));
            App.ObjFieldBinding(AttachActivitiesGroupReportCbx, CheckBox.IsCheckedProperty, mPublishToALMConfig, nameof(PublishToALMConfig.ToAttachActivitiesGroupReport));
            xFilterByStatusDroplist.BindControl(mPublishToALMConfig, nameof(PublishToALMConfig.FilterStatus));
        }

        static ExportResultsToALMConfigPage mInstance= null;
        public static ExportResultsToALMConfigPage Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new ExportResultsToALMConfigPage();
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
                 ALMIntegration.Instance.ExportBusinessFlowsResultToALM(mBfs, ref result, mPublishToALMConfig, ALMIntegration.eALMConnectType.Auto, true);
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
    }
}
