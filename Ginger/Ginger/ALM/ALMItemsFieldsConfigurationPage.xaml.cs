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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.UserControls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger.ALM
{
    /// <summary>
    /// Interaction logic for QCItemsFieldsConfigurationPage.xaml
    /// </summary>
    public partial class ALMItemsFieldsConfigurationPage : Page
    {
        ObservableList<ExternalItemFieldBase> mItemsFields;

        GenericWindow genWin = null;
        ImageMakerControl loaderElement;
        BackgroundWorker fieldsWorker = new BackgroundWorker();
        bool isReferFields = true;
        bool isExceptionToGetFields = false;
        public static string LoadFieldsState = "aa";
        eALMConfigType mAlmConfigType = eALMConfigType.MainMenu;

        public ALMItemsFieldsConfigurationPage(eALMConfigType configType, eALMType type, ObservableList<ExternalItemFieldBase> selectedItemsFields)
        {
            InitializeComponent();
            mAlmConfigType = configType;
            mItemsFields = WorkSpace.Instance.Solution.ExternalItemsFields;

            if (!mAlmConfigType.ToString().Equals(eALMConfigType.MainMenu.ToString()))
            {
                grdQCFields.DataSourceList = selectedItemsFields;
                SetFieldsGrid();
            }

        }

        private void SetFieldsGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = ExternalItemFieldBase.Fields.Name, Header = "Field Name", WidthWeight = 20, ReadOnly = true, AllowSorting = true },
                new GridColView() { Field = ExternalItemFieldBase.Fields.Mandatory, WidthWeight = 15, ReadOnly = true, AllowSorting = true },
                new GridColView() { Field = ExternalItemFieldBase.Fields.SelectedValue, Header = "Selected Value", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(ExternalItemFieldBase.Fields.PossibleValues, ExternalItemFieldBase.Fields.SelectedValue, true), WidthWeight = 20 },
                new GridColView() { Field = ExternalItemFieldBase.Fields.ItemType, Header = "Field Type", WidthWeight = 15, ReadOnly = true, AllowSorting = true },
            ]
            };
            grdQCFields.SetAllColumnsDefaultView(view);
            grdQCFields.InitViewItems();

            grdQCFields.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(Refresh));
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(ALMIntegration.Instance.GetDownloadPossibleValuesMessage()) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                if (ALMIntegration.Instance.TestALMProjectConn(eALMConnectType.Silence))
                {
                    RunWorker(true);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Error in Connection to ALM, Please check ALM Connection Settings");
                }
            }
        }

        public void ShowAsWindow(bool refreshFields = true, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            isReferFields = refreshFields;
            loaderElement = new ImageMakerControl
            {
                Name = "xProcessingImage",
                Height = 30,
                Width = 30,
                ImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing,
                Visibility = Visibility.Collapsed
            };
            if (mAlmConfigType == eALMConfigType.MainMenu)
            {
                Button saveButton = new Button
                {
                    Content = "Save",
                    ToolTip = "Save 'To Update' fields"
                };
                saveButton.Click += new RoutedEventHandler(Save);
                GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, [saveButton], true, "Close", null, false, loaderElement);
            }
            else
            {
                GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, null, true, "Close", null, false, loaderElement);
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            // TODO Rearrange save function to keep old fields value.
            ObservableList<ExternalItemFieldBase> tempItemList = ALMIntegration.Instance.GetUpdatedFields(mItemsFields, false);
            if (tempItemList.Any())
            {
                WorkSpace.Instance.Solution.ExternalItemsFields = tempItemList;
            }
            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.ALMSettings);
            genWin.Close();
        }
        #region BackgroundWorker Thread
        public void RunWorker(Boolean refreshFlag)
        {
            this.ShowLoader();
            fieldsWorker.WorkerReportsProgress = true;
            fieldsWorker.DoWork += new DoWorkEventHandler(fieldsWorker_DoWork);
            fieldsWorker.ProgressChanged += new ProgressChangedEventHandler(FieldsWorker_ProgressChanged);
            fieldsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(fieldsWorker_Completed);

            LoadFieldsStatusLbl.Visibility = Visibility.Visible;
            grdQCFields.Visibility = Visibility.Collapsed;

            fieldsWorker.RunWorkerAsync(refreshFlag);
            this.HideLoader();
        }

        private void fieldsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Boolean refreshFlag = (Boolean)e.Argument;

            try
            {
                ALMIntegration.Instance.RefreshALMItemFields(mItemsFields, true, fieldsWorker);
            }
            catch (Exception ex)
            {
                isExceptionToGetFields = true;
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }

            fieldsWorker.ReportProgress(GingerCore.ALM.RQM.ImportFromRQM.totalValues);
            e.Result = GingerCore.ALM.RQM.ImportFromRQM.totalValues;
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
            if (isExceptionToGetFields)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "ALM item fields population failed");
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "ALM item fields population completed");
            }

        }
        #endregion

        private void ShowLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                loaderElement.Visibility = Visibility.Visible;
            });
        }

        private void HideLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                loaderElement.Visibility = Visibility.Collapsed;
            });
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (mAlmConfigType == eALMConfigType.MainMenu)
            {
                if (isReferFields)
                {
                    if (mItemsFields.Count == 0 && Reporter.ToUser(ALMIntegration.Instance.GetDownloadPossibleValuesMessage()) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                    {
                        RunWorker(true);
                    }

                    grdQCFields.DataSourceList = mItemsFields;
                    SetFieldsGrid();
                }
            }
        }
    }
}
