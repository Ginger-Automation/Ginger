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


using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionScriptEditPage.xaml
    /// </summary>
    public partial class RunSetActionGenerateTestNGReportEditPage : Page
    {
        RunSetActionGenerateTestNGReport mRunSetActionScript;
        public RunSetActionGenerateTestNGReportEditPage(RunSetActionGenerateTestNGReport TestNGReport)
        {
            InitializeComponent();

            mRunSetActionScript = TestNGReport;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(TargetTestNGReportFolderBox, TextBox.TextProperty, mRunSetActionScript, nameof(RunSetActionGenerateTestNGReport.SaveResultsInSolutionFolderName));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(sourceActivityRadioBtn, RadioButton.IsCheckedProperty, mRunSetActionScript, nameof(RunSetActionGenerateTestNGReport.IsStatusByActivity));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(sourceActivitiesRadioBtn, RadioButton.IsCheckedProperty, mRunSetActionScript, nameof(RunSetActionGenerateTestNGReport.IsStatusByActivitiesGroup));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xUseDynamicParameters, CheckBox.IsCheckedProperty, mRunSetActionScript,nameof(RunSetActionGenerateTestNGReport.ConfiguerDynamicParameters));

            SetParametersGridView();
            grdTestNGReportParameters.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddButton));
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFolder(new System.Windows.Forms.FolderBrowserDialog()) is string folderName)
            {
                TargetTestNGReportFolderBox.Text = folderName;
            }
        }
    private void useDynamicParameters_Checked(object sender, RoutedEventArgs e)
        {
            if (xUseDynamicParameters.IsChecked == true)
            {
                dynamicParametersPnl.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                dynamicParametersPnl.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void SetParametersGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = "Parameter Name", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = "Parameter Value", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ParamValueExpressionButton"] });

            grdTestNGReportParameters.SetAllColumnsDefaultView(defView);
            grdTestNGReportParameters.InitViewItems();

            grdTestNGReportParameters.DataSourceList = mRunSetActionScript.DynamicParameters;

        }
        private void AddButton(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = new ActInputValue();
            mRunSetActionScript.DynamicParameters.Add(AIV);
        }
        private void ParamsGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)grdTestNGReportParameters.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), null);
            VEEW.ShowAsWindow();
        }
    }
}
