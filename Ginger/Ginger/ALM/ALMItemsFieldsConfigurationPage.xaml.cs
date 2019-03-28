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

using Amdocs.Ginger.Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using Ginger.UserControls;
using System.ComponentModel;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.ALM
{
    /// <summary>
    /// Interaction logic for QCItemsFieldsConfigurationPage.xaml
    /// </summary>
    public partial class ALMItemsFieldsConfigurationPage : Page
    {
        ObservableList<ExternalItemFieldBase> mItemsFields;

        GenericWindow genWin = null;

        BackgroundWorker fieldsWorker = new BackgroundWorker();

        public static string LoadFieldsState = "aa";

        public ALMItemsFieldsConfigurationPage()
        {
            InitializeComponent();

            mItemsFields =  WorkSpace.Instance.Solution.ExternalItemsFields;
            ALMIntegration.Instance.RefreshALMItemFields(mItemsFields, false, null);
            if (mItemsFields.Count == 0 && Reporter.ToUser(ALMIntegration.Instance.GetDownloadPossibleValuesMessage()) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                RunWorker(true);
            }

            grdQCFields.DataSourceList = mItemsFields;
            SetFieldsGrid();
        }

        private void SetFieldsGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = ExternalItemFieldBase.Fields.Name, Header = "Field Name", WidthWeight = 20, ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = ExternalItemFieldBase.Fields.Mandatory, WidthWeight = 15, ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = ExternalItemFieldBase.Fields.SelectedValue, Header = "Selected Value", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(ExternalItemFieldBase.Fields.PossibleValues, ExternalItemFieldBase.Fields.SelectedValue, true), WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = ExternalItemFieldBase.Fields.ItemType, Header = "Field Type", WidthWeight = 15, ReadOnly = true, AllowSorting = true });
            grdQCFields.SetAllColumnsDefaultView(view);
            grdQCFields.InitViewItems();

            grdQCFields.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(Refresh));
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(ALMIntegration.Instance.GetDownloadPossibleValuesMessage()) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                RunWorker(true);
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button saveButton = new Button();
            saveButton.Content = "Save";
            saveButton.ToolTip = "Save 'To Update' fields";
            saveButton.Click += new RoutedEventHandler(Save);
            grdQCFields.DataSourceList = WorkSpace.Instance.Solution.ExternalItemsFields;
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { saveButton });
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            // TODO Rearrange save function to keep old fields value.
            ObservableList<ExternalItemFieldBase> tempItemList = new ObservableList<ExternalItemFieldBase>();
            WorkSpace.Instance.Solution.ExternalItemsFields = ALMIntegration.Instance.GetUpdatedFields(mItemsFields, false);
            WorkSpace.Instance.Solution.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.ALMSettings);
            WorkSpace.Instance.Solution.ExternalItemsFields = mItemsFields;
            genWin.Close();

        }

        #region BackgroundWorker Thread
        public void RunWorker(Boolean refreshFlag)
        {
            fieldsWorker.WorkerReportsProgress = true;
            fieldsWorker.DoWork += new DoWorkEventHandler(fieldsWorker_DoWork);
            fieldsWorker.ProgressChanged += new ProgressChangedEventHandler(FieldsWorker_ProgressChanged);
            fieldsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(fieldsWorker_Completed);

            LoadFieldsStatusLbl.Visibility = Visibility.Visible;
            grdQCFields.Visibility = Visibility.Collapsed;

            fieldsWorker.RunWorkerAsync(refreshFlag);
        }

        private void fieldsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Boolean refreshFlag = (Boolean)e.Argument;

            ALMIntegration.Instance.RefreshALMItemFields(mItemsFields, true, fieldsWorker);

            fieldsWorker.ReportProgress(GingerCore.ALM.RQM.ImportFromRQM.totalValues);
            e.Result = GingerCore.ALM.RQM.ImportFromRQM.totalValues;
            System.Diagnostics.Debug.WriteLine("values = " + e.Result);
        }

        private void FieldsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            LoadFieldsStatusLbl.Dispatcher.Invoke(() =>
            {
                LoadFieldsStatusLbl.Content = GingerCore.ALM.RQM.ImportFromRQM.populatedValue;

            });

            LoadFieldsStatusLbl.Refresh();
        }

        private void fieldsWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadFieldsStatusLbl.Visibility = Visibility.Collapsed;
            grdQCFields.Visibility = Visibility.Visible;
            grdQCFields.DataSourceList = mItemsFields;
            Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "ALM Item Fields population is complete");
        }
        #endregion
    }
}
