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

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.Reports;
using Ginger.UserControls;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionSedDataToExternalSource.xaml
    /// </summary>
    public partial class RunSetActionSendDataToExternalSourceEditPage : Page
    {
        private RunSetActionSendDataToExternalSource runSetActionSendData;
        private Context mContext = new Context();

        public RunSetActionSendDataToExternalSourceEditPage(RunSetActionSendDataToExternalSource RunSetActionSendData)
        {
            InitializeComponent();
            this.runSetActionSendData = RunSetActionSendData;
            mContext.RunsetAction = runSetActionSendData;
            mContext.Environment = WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment;
            
            xEndPointURLTextBox.Init(mContext, runSetActionSendData, nameof(RunSetActionSendDataToExternalSource.EndPointUrl));
            xJsonBodyTextBox.Init(mContext, runSetActionSendData, nameof(RunSetActionSendDataToExternalSource.RequestBodyJson));
            xJsonBodyTextBox.AdjustHight(100);
            CurrentTemplatePickerCbx_Binding(); 

            SetHeadersGridView();
            SetBodyGridView();

            grdRequestHeaders.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddHeaderParamButton));
            grdRequestBody.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddBodyParamButton));
        }

        public void CurrentTemplatePickerCbx_Binding()
        {
            CurrentTemplatePickerCbx.ItemsSource = null;

            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            if ((WorkSpace.Instance.Solution != null) && (HTMLReportConfigurations.Count > 0))
            {
                CurrentTemplatePickerCbx.ItemsSource = HTMLReportConfigurations;
                CurrentTemplatePickerCbx.DisplayMemberPath = HTMLReportConfiguration.Fields.Name;
                CurrentTemplatePickerCbx.SelectedValuePath = HTMLReportConfiguration.Fields.ID;
                if ((runSetActionSendData.selectedHTMLReportTemplateID != 0))
                {
                    CurrentTemplatePickerCbx.SelectedIndex = CurrentTemplatePickerCbx.Items.IndexOf(HTMLReportConfigurations.Where(x => (x.ID == runSetActionSendData.selectedHTMLReportTemplateID)).FirstOrDefault());
                    if (CurrentTemplatePickerCbx.SelectedIndex == -1)
                    {
                        CurrentTemplatePickerCbx.SelectedIndex = CurrentTemplatePickerCbx.Items.IndexOf(HTMLReportConfigurations.Where(x => x.IsDefault).FirstOrDefault());
                    }
                }
                else
                {
                    CurrentTemplatePickerCbx.SelectedIndex = CurrentTemplatePickerCbx.Items.IndexOf(HTMLReportConfigurations.Where(x => x.IsDefault).FirstOrDefault());
                }
            }
        }
        private void SetHeadersGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = "Parameter Name", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = "Parameter Value", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.RequestHeadersGrid.Resources["HeaderValueExpressionButton"] });

            grdRequestHeaders.SetAllColumnsDefaultView(defView);
            grdRequestHeaders.InitViewItems();

            grdRequestHeaders.DataSourceList = runSetActionSendData.RequestHeaders;

        }
        private void SetBodyGridView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Param), Header = "Parameter Name", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(ActInputValue.Value), Header = "Parameter Value", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.RequestBodyGrid.Resources["BodyValueExpressionButton"] });

            grdRequestBody.SetAllColumnsDefaultView(defView);
            grdRequestBody.InitViewItems();

            grdRequestBody.DataSourceList = runSetActionSendData.RequestBodyParams;

        }

        private void CurrentTemplatePickerCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            runSetActionSendData.selectedHTMLReportTemplateID = ((HTMLReportConfiguration)CurrentTemplatePickerCbx.SelectedItem).ID;
        }

        private void AddHeaderParamButton(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = new ActInputValue();
            runSetActionSendData.RequestHeaders.Add(AIV);
        }
        private void AddBodyParamButton(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = new ActInputValue();
            runSetActionSendData.RequestBodyParams.Add(AIV);
        }
        private void HeaderGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)grdRequestHeaders.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), mContext);
            VEEW.ShowAsWindow();
        }
        private void BodyGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            ActInputValue AIV = (ActInputValue)grdRequestBody.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AIV, nameof(ActInputValue.Value), mContext);
            VEEW.ShowAsWindow();
        }

        private void tabRequestBody_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)e.Source).Name != "tabRequestBody")
            {
                return;
            }
            if (tabListView.IsSelected)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    runSetActionSendData.RefreshBodyParamsPreview();
                }));
            }
            else if(tabJsonView.IsSelected)
            {
                runSetActionSendData.RefreshJsonPreview();
            }
        }
    }
}
