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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCore.DataSource;
using GingerCore.GeneralLib;
using GingerWPF.ApplicationModelsLib.APIModelWizard;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Ginger.SolutionWindows.TreeViewItems;
using Amdocs.Ginger.Common.Repository;
using Ginger.UserControlsLib;

namespace GingerWPF.ApplicationModelsLib.ModelParams_Pages
{
    public partial class ModelsGlobalParamsPage : GingerUIPage
    {
        public ObservableList<GlobalAppModelParameter> mModelsGlobalParamsList;
        GenericWindow mGenericWindow = null;
        List<GlobalAppModelParameter> SelectedGlobalParamsFromDialogPage = new List<GlobalAppModelParameter>();
        List<GlobalAppModelParameter> GlobalParamsCopiedItemsList = new List<GlobalAppModelParameter>();
        string gridPlaceholderHeader = "Place Holder";
        bool mSelectionModePage;
        public ModelsGlobalParamsPage(bool selectionModePage = false)
        {
            InitializeComponent();
            mSelectionModePage = selectionModePage;
           
            InitApplicationModelsGlobalParamsGrid();
            if (!selectionModePage)
                SetGridRowStyle();
        }

        private void ShowGlobalAndLocalParams(ObservableList<AppModelParameter> MergedParamsList)
        {
            mModelsGlobalParamsList = new ObservableList<GlobalAppModelParameter>();

            foreach (var param in MergedParamsList)
                mModelsGlobalParamsList.Add(param as GlobalAppModelParameter);
        }

        private void SetGridRowStyle()
        {
            //If row is dirty - show the row header (where line number) in different color
            Style st2 = xModelsGlobalParamsGrid.grdMain.RowHeaderStyle;
            DataTrigger DT2 = new DataTrigger();
            PropertyPath PT2 = new PropertyPath(nameof(GlobalAppModelParameter.DirtyStatus)); 
            DT2.Binding = new Binding{ Path = PT2 };
            DT2.Value = eDirtyStatus.Modified;
            DT2.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.LightPink));
            st2.Triggers.Add(DT2);
        }

        private void InitApplicationModelsGlobalParamsGrid()
        {            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.PlaceHolder), Header = gridPlaceholderHeader, WidthWeight = 30, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.Description), Header = "Description", WidthWeight = 30, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.Guid), Header = "ID", WidthWeight = 10, ReadOnly=true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.OptionalValuesString), Header = "Optional Values", WidthWeight = 30, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true });

            if (!mSelectionModePage)
            {
                xModelsGlobalParamsGrid.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.Parameter, "Applications Models Global Parameters", saveAllHandler: SaveAllGlobalParametersChanges, addHandler: AddGlobalParam,true);

                view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 8, MaxWidth=30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["OpenEditPossibleValuesPage"] });
                view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.CurrentValue), Header = "Current Value", WidthWeight = 20, AllowSorting = true });

                xModelsGlobalParamsGrid.btnSaveSelectedChanges.AddHandler(Button.ClickEvent, new RoutedEventHandler(SaveSelectedGlobalParametersChanges));                
                xModelsGlobalParamsGrid.SetbtnDeleteHandler(new RoutedEventHandler(DeleteSelectedEvent));
                xModelsGlobalParamsGrid.SetbtnClearAllHandler(DeleteAllEvent);
                xModelsGlobalParamsGrid.SetbtnCopyHandler(BtnCopyGlobalParamsClicked);
                xModelsGlobalParamsGrid.SetbtnPastHandler(BtnPastGlobalParamsClicked);

                xModelsGlobalParamsGrid.ShowSaveAllChanges = Visibility.Collapsed;
                xModelsGlobalParamsGrid.ShowSaveSelectedChanges = Visibility.Collapsed;
                xModelsGlobalParamsGrid.ShowEdit = Visibility.Collapsed;
                xModelsGlobalParamsGrid.ShowCopy = Visibility.Visible;
                xModelsGlobalParamsGrid.ShowPaste = Visibility.Visible;
                xModelsGlobalParamsGrid.ShowCut = Visibility.Collapsed;
                xModelsGlobalParamsGrid.ShowRefresh = Visibility.Collapsed;
                xModelsGlobalParamsGrid.ShowAdd = Visibility.Collapsed;
                xModelsGlobalParamsGrid.ShowDelete = Visibility.Visible;
                xModelsGlobalParamsGrid.ShowClearAll = Visibility.Visible;
                xModelsGlobalParamsGrid.ShowUpDown = Visibility.Collapsed;
                xModelsGlobalParamsGrid.Grid.CanUserDeleteRows = false;

                xModelsGlobalParamsGrid.Grid.BeginningEdit += grdMain_BeginningEdit;
                xModelsGlobalParamsGrid.Grid.CellEditEnding += grdMain_CellEditEndingAsync;
            }

            
            xModelsGlobalParamsGrid.ShowTitle = Visibility.Collapsed;
            xModelsGlobalParamsGrid.SetAllColumnsDefaultView(view);
            xModelsGlobalParamsGrid.InitViewItems();


            mModelsGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();          
            if (!mSelectionModePage)
            {
                foreach (GlobalAppModelParameter param in mModelsGlobalParamsList)
                {
                    StartTrackingModelGlobalParameter(param);
                }
            }
            xModelsGlobalParamsGrid.DataSourceList = mModelsGlobalParamsList;
            xModelsGlobalParamsGrid.AddToolbarTool("@Import_16x16.png", "Import Optional Values For Parameters", new RoutedEventHandler(ImportOptionalValuesForGlobalParameters));
            xModelsGlobalParamsGrid.AddToolbarTool(eImageType.ExcelFile, "Export Parameters to Excel File", new RoutedEventHandler(ExportOptionalValuesForParameters));
            xModelsGlobalParamsGrid.AddToolbarTool(eImageType.DataSource, "Export Parameters to DataSource", new RoutedEventHandler(ExportParametersToDataSource));
            xModelsGlobalParamsGrid.AddToolbarTool(Amdocs.Ginger.Common.Enums.eImageType.ID, "Copy selected item ID", CopySelectedItemID);
        }

        private void CopySelectedItemID(object sender, RoutedEventArgs e)
        {
            if (xModelsGlobalParamsGrid.Grid.SelectedItem != null)
            {
                Clipboard.SetText(((RepositoryItemBase)xModelsGlobalParamsGrid.Grid.SelectedItem).Guid.ToString());
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void ImportOptionalValuesForGlobalParameters(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new AddModelOptionalValuesWizard(mModelsGlobalParamsList));
            xModelsGlobalParamsGrid.DataSourceList = mModelsGlobalParamsList;
        }

        private void ExportOptionalValuesForParameters(object sender, RoutedEventArgs e)
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GlobalParameters.xlsx");
            bool overrideFile = true;
            if (File.Exists(fileName))
            {
                if (Reporter.ToUser(eUserMsgKey.FileAlreadyExistWarn) == eUserMsgSelection.Cancel)
                {
                    overrideFile = false;
                }
                else
                {
                    //In case File exists and user selects to overwrite the existing.
                    File.Delete(fileName);
                }
            }

            if (overrideFile)
            {
                ImportOptionalValuesForParameters im = new ImportOptionalValuesForParameters();
                List<AppParameters> parameters = GetParameterList();
                string filePath = im.ExportParametersToExcelFile(parameters, "GlobalParameters");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = filePath, UseShellExecute = true }); 
            }
        }

        private void ExportParametersToDataSource(object sender, RoutedEventArgs e)
        {
            try
            {
                Ginger.SolutionWindows.TreeViewItems.DataSourceFolderTreeItem dataSourcesRoot = new Ginger.SolutionWindows.TreeViewItems.DataSourceFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<DataSourceBase>(),DataSourceFolderTreeItem.eDataTableView.Customized);
                SingleItemTreeViewSelectionPage mDataSourceSelectionPage = new SingleItemTreeViewSelectionPage("DataSource - Customized Table", eImageType.DataSource, dataSourcesRoot, SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true);
                List<object> selectedRunSet = mDataSourceSelectionPage.ShowAsWindow();
                if (selectedRunSet != null && selectedRunSet.Count > 0)
                {
                    ImportOptionalValuesForParameters im = new ImportOptionalValuesForParameters();
                    DataSourceBase dataSource = (((DataSourceTable)selectedRunSet[0]).DSC);

                    string tableName = ((DataSourceTable)selectedRunSet[0]).FileName;
                    List<AppParameters> parameters = GetParameterList();
                    im.ExportSelectedParametersToDataSouce(parameters, dataSource, tableName);
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
            }
        }

        /// <summary>
        /// This method is used to Get Parameter List
        /// </summary>
        /// <param name="im"></param>
        /// <returns></returns>
        private List<AppParameters> GetParameterList()
        {
            ImportOptionalValuesForParameters im = new ImportOptionalValuesForParameters();
            List<AppParameters> parameters = new List<AppParameters>();
            try
            {
                foreach (var prms in mModelsGlobalParamsList)
                {
                    im.AddNewParameterToList(parameters, prms);
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
            }
            return parameters;
        }

        string PlaceholderBeforeEdit = string.Empty;

        private void grdMain_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            ((RepositoryItemBase)xModelsGlobalParamsGrid.Grid.SelectedItem).SaveBackup();
            if (e.Column.SortMemberPath == nameof(GlobalAppModelParameter.PlaceHolder))
            {
                GlobalAppModelParameter selectedGlobalAppModelParameter = (GlobalAppModelParameter)xModelsGlobalParamsGrid.CurrentItem;
                PlaceholderBeforeEdit = selectedGlobalAppModelParameter.PlaceHolder;
            }
        }

        private async void grdMain_CellEditEndingAsync(object sender, DataGridCellEditEndingEventArgs e)
        {
            GlobalAppModelParameter CurrentGAMP = null;
            if (e.Column.Header.ToString() == gridPlaceholderHeader)
            {
                CurrentGAMP = (GlobalAppModelParameter)xModelsGlobalParamsGrid.CurrentItem;
                if (IsParamPlaceholderNameConflict(CurrentGAMP))
                    return;
            }

            if (e.Column.SortMemberPath == nameof(GlobalAppModelParameter.PlaceHolder))
            {
                GlobalAppModelParameter selectedGlobalAppModelParameter = (GlobalAppModelParameter)xModelsGlobalParamsGrid.CurrentItem;
                string NameAfterEdit = selectedGlobalAppModelParameter.PlaceHolder;
                if (PlaceholderBeforeEdit != NameAfterEdit)
                {
                    char[] invalidChars = System.IO.Path.GetInvalidPathChars().Where(c => c != '<' && c != '>').ToArray<char>();
                    if (NameAfterEdit.IndexOfAny(invalidChars) > 0)
                    {

                        System.Text.StringBuilder builder = new System.Text.StringBuilder();
                        foreach (char value in invalidChars)
                        {
                            builder.Append(value);
                            builder.Append(" ");
                        }
                        Reporter.ToUser(eUserMsgKey.ValueIssue, "Value cannot contain characters like: " + builder);
                        selectedGlobalAppModelParameter.PlaceHolder = PlaceholderBeforeEdit;
                        return;
                    }

                    if (Reporter.ToUser(eUserMsgKey.ParameterUpdate, "The Global Parameter name may be used in Solution items Value Expression, Do you want to automatically update all those Value Expression instances with the parameter name change?") == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            xPageGrid.Visibility = Visibility.Collapsed;
                            xUpdatingItemsPnl.Visibility = Visibility.Visible;
                        });

                        await this.Dispatcher.Invoke(async () =>
                        {
                            await Task.Run(() =>
                            {
                                UpdateModelGlobalParamVeWithNameChange(PlaceholderBeforeEdit, NameAfterEdit);
                            });
                        });


                        this.Dispatcher.Invoke(() =>
                        {
                            xUpdatingItemsPnl.Visibility = Visibility.Collapsed;
                            xPageGrid.Visibility = Visibility.Visible;
                        });

                        Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Update finished successfully." + Environment.NewLine + "Please do not forget to save all modified " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows));
                    }
                }
            }
        }

        public void UpdateModelGlobalParamVeWithNameChange(string oldName, string updatedName)
        {
            ObservableList<RepositoryItemBase> allVESupportingItems = GetSolutionVEsupportedItems();

            foreach(var item in allVESupportingItems)
            {
                if (item is BusinessFlow)
                {
                    BusinessFlow bf = (BusinessFlow)item;
                    foreach (Activity activity in bf.Activities)
                        foreach (Act action in activity.Acts)
                        {
                            UpdateItemModelGlobalParamVeWithNameChange(action, oldName, updatedName);
                        }
                }
                else
                {
                    UpdateItemModelGlobalParamVeWithNameChange(item, oldName, updatedName);
                }
            }
        }

        static ObservableList<RepositoryItemBase> GetSolutionVEsupportedItems()
        {
            // CHECK FIXME !?!?
            ObservableList<RepositoryItemBase> supportedItems = new ObservableList<RepositoryItemBase>();
            ObservableList<BusinessFlow> BFs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            foreach (BusinessFlow bf in BFs)
            {
                supportedItems.Add(bf);
            }
            foreach (Agent agent in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>())
            {
                supportedItems.Add(agent);
            }
            foreach (ProjEnvironment env in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>())
            {
                supportedItems.Add(env);
            }


            return supportedItems;
        }

        public void UpdateItemModelGlobalParamVeWithNameChange(object item, string prevParamName, string newParamName)
        {
            if (item == null) return;

            string ValueToSearch = "{GlobalAppsModelsParam Name=" + prevParamName + "}";
            string ValueToReplace = "{GlobalAppsModelsParam Name=" + newParamName + "}";

            var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                try
                {
                    if (mi.Name == "mBackupDic" || mi.Name == nameof(RepositoryItemBase.FileName) || mi.Name == nameof(RepositoryItemBase.FilePath) ||
                        mi.Name == nameof(RepositoryItemBase.ObjFolderName) || mi.Name == nameof(RepositoryItemBase.ObjFileExt) ||
                        mi.Name == nameof(RepositoryItemBase.ContainingFolder) || mi.Name == nameof(RepositoryItemBase.ContainingFolderFullPath) ||
                        mi.Name == nameof(Act.ActInputValues) || mi.Name == nameof(Act.ActReturnValues) || mi.Name == nameof(Act.ActFlowControls) || mi.Name == nameof(Act.ItemNameField)) //needed?                   
                        continue;

                    //Get the attr value
                    PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                    dynamic value = null;
                    if (mi.MemberType == MemberTypes.Property)
                    {
                        value = PI.GetValue(item, null);
                    }
                    else if (mi.MemberType == MemberTypes.Field)
                        value = item.GetType().GetField(mi.Name).GetValue(item);

                    if (value is IObservableList)
                    {
                        List<dynamic> list = new List<dynamic>();
                        foreach (object o in value)
                            UpdateItemModelGlobalParamVeWithNameChange(o, prevParamName, newParamName);
                    }
                    else
                    {
                        if (value != null)
                        {
                            string stringValue = value.ToString();
                            if (string.IsNullOrEmpty(stringValue) == false && stringValue.Contains(ValueToSearch))
                            {
                                stringValue = stringValue.Replace(ValueToSearch, ValueToReplace);
                                PI.SetValue(item, stringValue);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to updated the Model Global Param name change for the property '{0}' in the item '{1}'", mi.Name, item.ToString()), ex);
                }
            }
        }

        private bool IsParamPlaceholderNameConflict(GlobalAppModelParameter CurrentGAMP)
        {
            foreach (AppModelParameter GAMP in mModelsGlobalParamsList)
            {
                if (GAMP != CurrentGAMP && GAMP.PlaceHolder == CurrentGAMP.PlaceHolder)
                {
                    CurrentGAMP.PlaceHolder = PlaceholderBeforeEdit;                    
                    Reporter.ToUser(eUserMsgKey.SpecifyUniqueValue);
                    return true;
                }
            }
            return false;
        }

        private void AddGlobalParam(object sender, RoutedEventArgs e)
        {
            GlobalAppModelParameter newModelGlobalParam = new GlobalAppModelParameter();
            SetUniquePlaceHolderName(newModelGlobalParam);

            string newParamPlaceholder = newModelGlobalParam.PlaceHolder;

            while(InputBoxWindow.GetInputWithValidation("Add Model Global Parameter", "Parameter Name:", ref newParamPlaceholder, System.IO.Path.GetInvalidPathChars().Where(c => c != '<' && c != '>').ToArray<char>()))
            {
                newModelGlobalParam.PlaceHolder = newParamPlaceholder;
                if (!IsParamPlaceholderNameConflict(newModelGlobalParam))
                {
                    newModelGlobalParam.OptionalValuesList.Add(new OptionalValue() { Value = GlobalAppModelParameter.CURRENT_VALUE, IsDefault = true });
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newModelGlobalParam);
                    StartTrackingModelGlobalParameter(newModelGlobalParam);
                    xModelsGlobalParamsGrid.Grid.SelectedIndex = xModelsGlobalParamsGrid.Grid.Items.Count-1;

                    //making sure rows numbers are ok
                    xModelsGlobalParamsGrid.Grid.UpdateLayout();
                    xModelsGlobalParamsGrid.Renum();
                    break;
                }
            }
        }

        private void BtnCopyGlobalParamsClicked(object sender, RoutedEventArgs e)
        {
            GlobalParamsCopiedItemsList.Clear();
            foreach (GlobalAppModelParameter param in xModelsGlobalParamsGrid.Grid.SelectedItems)
                GlobalParamsCopiedItemsList.Add(param);
        }

        private void BtnPastGlobalParamsClicked(object sender, RoutedEventArgs e)
        {
            foreach (GlobalAppModelParameter param in GlobalParamsCopiedItemsList)
            {

                GlobalAppModelParameter newCopyGlobalParam = (GlobalAppModelParameter)param.CreateCopy();
                SetUniquePlaceHolderName(newCopyGlobalParam, true);
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newCopyGlobalParam);
            }
        }

        public void SetUniquePlaceHolderName(GlobalAppModelParameter newModelGlobalParam, bool isCopy = false)
        {
            if (isCopy)
                newModelGlobalParam.PlaceHolder = newModelGlobalParam.PlaceHolder + "_Copy";
            else
                newModelGlobalParam.PlaceHolder = "{NewGlobalParameter}";

            if (mModelsGlobalParamsList.Where(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder).FirstOrDefault() == null) return;

            List<GlobalAppModelParameter> samePlaceHolderList = mModelsGlobalParamsList.Where(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder).ToList<GlobalAppModelParameter>();
            if (samePlaceHolderList.Count == 1 && samePlaceHolderList[0] == newModelGlobalParam) return; //Same internal object

            //Set unique name
            if (isCopy)
            {
                if ((mModelsGlobalParamsList.Where(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder).FirstOrDefault()) != null)
                {
                    int counter = 2;
                    while ((mModelsGlobalParamsList.Where(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder + counter).FirstOrDefault()) != null)
                        counter++;
                    newModelGlobalParam.PlaceHolder = newModelGlobalParam.PlaceHolder + counter;
                }
            }
            else
            {
                int counter = 2;
                while ((mModelsGlobalParamsList.Where(x => x.PlaceHolder == "{NewGlobalParameter_" + counter.ToString() + "}").FirstOrDefault()) != null)
                    counter++;
                newModelGlobalParam.PlaceHolder = "{NewGlobalParameter_" + counter.ToString() + "}";
            }
        }

        private void DeleteSelectedEvent(object sender, RoutedEventArgs e)
        {
            if (xModelsGlobalParamsGrid.Grid.SelectedItems.Count > 0)
            {
                string message = "After deletion there will be no way to restore deleted parameters.\nAre you sure that you want to delete the selected parameters?";
                if (Reporter.ToUser(eUserMsgKey.ParameterDelete, message) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    List<GlobalAppModelParameter> selectedItemsToDelete = new List<GlobalAppModelParameter>();
                    foreach (GlobalAppModelParameter selectedParam in xModelsGlobalParamsGrid.Grid.SelectedItems)

                    {
                        selectedItemsToDelete.Add(selectedParam);
                    }
                    foreach (GlobalAppModelParameter paramToDelete in selectedItemsToDelete)
                    {
                        DeleteGlobalParam(paramToDelete);
                    }
                    if (xModelsGlobalParamsGrid.Grid.SelectedItems.Count == 0)
                    {
                        WorkSpace.Instance.CurrentSelectedItem = null;
                    }
                }
            }
        }

        private void DeleteAllEvent(object sender, RoutedEventArgs e)
        {
            if (xModelsGlobalParamsGrid.Grid.Items.Count > 0)
            {
                string message = "After deletion there will be no way to restore deleted parameters.\nAre you sure that you want to delete All parameters?";
                if (Reporter.ToUser(eUserMsgKey.ParameterDelete, message) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    while (xModelsGlobalParamsGrid.Grid.SelectedItems.Count > 0)
                    {
                        DeleteGlobalParam((RepositoryItemBase)xModelsGlobalParamsGrid.Grid.SelectedItems[0]);
                    }
                    WorkSpace.Instance.CurrentSelectedItem = null;
                }
            }
        }

        private void DeleteGlobalParam(RepositoryItemBase globalParamToDelete)
        {
            WorkSpace.Instance.SolutionRepository.DeleteRepositoryItem(globalParamToDelete);
        }

        private void SaveAllGlobalParametersChanges(object sender, RoutedEventArgs e)
        {
            Save(true);
        }

        private void SaveSelectedGlobalParametersChanges(object sender, RoutedEventArgs e)
        {
            Save(false);
        }

        private void Save(bool saveAll)
        {
            int itemsSavedCount = 0;
            if (saveAll)
            {
                foreach (GlobalAppModelParameter globalParam in mModelsGlobalParamsList)
                    if (globalParam.DirtyStatus != Amdocs.Ginger.Common.Enums.eDirtyStatus.NoChange)
                    {
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(globalParam);
                        itemsSavedCount++;
                    }
            }
            else
            {
                foreach (GlobalAppModelParameter selectedGlobalParam in xModelsGlobalParamsGrid.Grid.SelectedItems)
                    if (selectedGlobalParam.DirtyStatus != Amdocs.Ginger.Common.Enums.eDirtyStatus.NoChange)
                    {
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(selectedGlobalParam);
                        itemsSavedCount++;
                    }
            }

            if (itemsSavedCount == 0)
            {
                if (saveAll)                    
                    Reporter.ToUser(eUserMsgKey.SaveAll, "Nothing found to Save.");
                else                    
                    Reporter.ToUser(eUserMsgKey.SaveSelected, "Nothing found to Save from the selected Global Parameters.");

            }
            else
            {
                if (saveAll)                    
                    Reporter.ToUser(eUserMsgKey.SaveAll, "All Global Parameters have been saved");
                else                    
                    Reporter.ToUser(eUserMsgKey.SaveSelected, "Selected Global Parameter/s have been saved");
            }
        }

        private void OpenEditPossibleValuesPageButton_Click(object sender, RoutedEventArgs e)
        {
            IParentOptionalValuesObject parObj = (IParentOptionalValuesObject)xModelsGlobalParamsGrid.CurrentItem;
            ModelOptionalValuesPage MDPVP = new ModelOptionalValuesPage(parObj);
            bool editWasDone = MDPVP.ShowAsWindow();

            if (editWasDone)
                ((GlobalAppModelParameter)xModelsGlobalParamsGrid.CurrentItem).SaveBackup();
        }

        public List<GlobalAppModelParameter> ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button selectBtn = new Button();
            selectBtn.Content = "Select";
            selectBtn.Click += new RoutedEventHandler(selectBtn_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(selectBtn);

            xModelsGlobalParamsGrid.ShowToolsBar = Visibility.Collapsed;
            xModelsGlobalParamsGrid.Grid.IsReadOnly = true;
            xModelsGlobalParamsGrid.Grid.MouseDoubleClick += selectBtn_Click;

            GenericWindow.LoadGenericWindow(ref mGenericWindow, null, windowStyle, "Add Global Parameter", this, winButtons, true, "Cancel", CloseWinClicked);

            return SelectedGlobalParamsFromDialogPage;
        }

        private void selectBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var GAMP in xModelsGlobalParamsGrid.Grid.SelectedItems)
                SelectedGlobalParamsFromDialogPage.Add((GlobalAppModelParameter)GAMP);

            if (mGenericWindow != null)
                mGenericWindow.Close();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            SelectedGlobalParamsFromDialogPage = null;
            mGenericWindow.Close();
        }
        private void StartTrackingModelGlobalParameter (GlobalAppModelParameter GAMP)
        {
            GAMP.StartDirtyTracking();
        }

        protected override void IsVisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (xModelsGlobalParamsGrid.Grid.Items.Count != 0 && xModelsGlobalParamsGrid.Grid.SelectedItems.Count != 0 && xModelsGlobalParamsGrid.Grid.SelectedItems[0] != null)
            {
                CurrentItemToSave = (RepositoryItemBase)xModelsGlobalParamsGrid.Grid.SelectedItems[0];
                base.IsVisibleChangedHandler(sender, e);
            }
            else
            {
                WorkSpace.Instance.CurrentSelectedItem = null;
            }
        }

        private void xModelsGlobalParamsGrid_SelectedItemChanged(object selectedItem)
        {
            if (selectedItem != null && selectedItem != WorkSpace.Instance.CurrentSelectedItem)
            {
                WorkSpace.Instance.CurrentSelectedItem = (Amdocs.Ginger.Repository.RepositoryItemBase)selectedItem;
            }
        }
    }
}
