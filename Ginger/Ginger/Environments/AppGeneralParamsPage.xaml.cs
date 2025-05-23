#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.Environments
{
    /// <summary>
    /// Interaction logic for AppGeneralParamsWindow.xaml
    /// </summary>
    public partial class AppGeneralParamsPage : Page
    {
        public EnvApplication AppOwner { get; set; }

        public AppGeneralParamsPage(EnvApplication applicationOwner)
        {
            InitializeComponent();
            AppOwner = applicationOwner;
            //Set grid look and data
            SetGridView();
            SetGridData();
            //Added for Encryption
            if (grdAppParams.grdMain != null)
            {
                grdAppParams.grdMain.CellEditEnding += grdMain_CellEditEnding;
                grdAppParams.grdMain.PreparingCellForEdit += grdMain_PreparingCellForEdit;
            }
        }

        private void grdMain_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() == GeneralParam.Fields.Name)
            {
                GeneralParam selectedVarb = (GeneralParam)grdAppParams.CurrentItem;
                selectedVarb.NameBeforeEdit = selectedVarb.Name;
            }
        }

        private void grdMain_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == GeneralParam.Fields.Name)
            {
                GeneralParam changedParam = (GeneralParam)grdAppParams.CurrentItem;

                if (changedParam != null && !changedParam.Name.Equals(changedParam.NameBeforeEdit))
                {
                    if (string.IsNullOrWhiteSpace(changedParam.Name))
                    {
                        Reporter.ToUser(eUserMsgKey.EnvParamNameEmpty);
                        RestoreVariableName(changedParam);
                    }
                    else if (IsParamNameAlreadyExists(changedParam.Name, true))
                    {
                        Reporter.ToUser(eUserMsgKey.EnvParamNameExists);
                        RestoreVariableName(changedParam);
                    }
                    else if (IsParameterBeingUsed(changedParam.NameBeforeEdit))
                    {
                        if (Reporter.ToUser(eUserMsgKey.ChangingEnvironmentParameterValue) == eUserMsgSelection.Yes)
                        {
                            UpdateVariableNameChange(changedParam);
                        }
                        else
                        {
                            RestoreVariableName(changedParam);
                        }
                    }
                }
            }
            else if (e.Column.Header.ToString() == GeneralParam.Fields.Value)
            {
                GeneralParam selectedEnvParam = (GeneralParam)grdAppParams.CurrentItem;

                string intialValue = selectedEnvParam.Value;

                if (!string.IsNullOrEmpty(intialValue))
                {
                    if (selectedEnvParam.Encrypt == true)
                    {
                        //UpdateVariableNameChange(selectedEnvParam); // why is that needed here?

                        if (!EncryptionHandler.IsStringEncrypted(intialValue))
                        {
                            selectedEnvParam.Value = EncryptionHandler.EncryptwithKey(intialValue);
                            if (string.IsNullOrEmpty(selectedEnvParam.Value))
                            {
                                selectedEnvParam.Value = string.Empty;
                            }
                        }
                    }
                    else
                    {
                        if (EncryptionHandler.IsStringEncrypted(intialValue))
                        {
                            selectedEnvParam.Value = null;
                        }
                    }
                }
            }
        }

        private static void RestoreVariableName(GeneralParam changedParam)
        {
            changedParam.Name = string.IsNullOrWhiteSpace(changedParam.NameBeforeEdit) ? string.Empty : changedParam.NameBeforeEdit;
        }

        private bool IsParamNameAlreadyExists(string name, bool ignoreCurrentSelectedItem)
        {
            foreach (var item in grdAppParams.DataSourceList.ListItems)
            {
                if (ignoreCurrentSelectedItem && ((GeneralParam)item).Guid.Equals(((GeneralParam)grdAppParams.CurrentItem).Guid))
                {
                    continue;
                }

                if (((GeneralParam)item).Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateVariableNameChange(GeneralParam parameter)
        {
            if (parameter == null)
            {
                return;
            }
            else
            {
                ObservableList<BusinessFlow> bfs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

                foreach (BusinessFlow bf in bfs)
                {
                    foreach (Activity activity in bf.Activities)
                    {
                        foreach (Act action in activity.Acts)
                        {
                            bool changedwasDone = false;
                            GeneralParam.UpdateNameChangeInItem(action, AppOwner.Name, parameter.NameBeforeEdit, parameter.Name, ref changedwasDone);
                        }
                    }
                }
            }
            parameter.NameBeforeEdit = parameter.Name;
        }
        private bool IsParameterBeingUsed(string paramName)
        {
            try
            {
                ObservableList<BusinessFlow> bfs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                Mouse.OverrideCursor = Cursors.Wait;

                foreach (BusinessFlow bf in bfs)
                {
                    foreach (var activity in bf.Activities)
                    {
                        foreach (var action in activity.Acts)
                        {
                            if (GeneralParam.IsParamBeingUsedInBFs(action, AppOwner.Name, paramName))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #region Events
        private void AddParam(object sender, RoutedEventArgs e)
        {
            GeneralParam param = new GeneralParam() { Name = GenerateParamName(AppOwner.GeneralParams.Count) };
            param.PropertyChanged += param_PropertyChanged;

            AppOwner.GeneralParams.Add(param);
        }

        private string GenerateParamName(int count)
        {
            while (IsParamNameAlreadyExists($"Parameter {++count}", false))
            {
                continue;
            }

            return $"Parameter {count}";
        }
        #endregion Events

        #region Functions
        private void SetGridView()
        {
            //Set the Tool Bar look
            grdAppParams.ShowEdit = Visibility.Collapsed;
            grdAppParams.ShowUpDown = Visibility.Collapsed;
            grdAppParams.ShowUndo = Visibility.Visible;
            grdAppParams.ShowHeader = Visibility.Collapsed;
            grdAppParams.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddParam));
            grdAppParams.AddToolbarTool("@Share_16x16.png", "Add Selected Parameters to All Environments", new RoutedEventHandler(AddParamsToOtherEnvironmentsApps));
            //Set the Data Grid columns
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = GeneralParam.Fields.Name, WidthWeight = 40 },
                new GridColView() { Field = GeneralParam.Fields.Value, WidthWeight = 30 },
                new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.appGenParamsPageGrid.Resources["ParamValueExpressionButton"] },
                new GridColView() { Field = GeneralParam.Fields.Encrypt, WidthWeight = 5, MaxWidth = 100, StyleType = GridColView.eGridColStyleType.CheckBox },
                new GridColView() { Field = GeneralParam.Fields.Description, WidthWeight = 25 },
            ]
            };

            grdAppParams.SetAllColumnsDefaultView(view);
            grdAppParams.InitViewItems();
        }

        private void SetGridData()
        {
            foreach (GeneralParam param in AppOwner.GeneralParams)
            {
                param.PropertyChanged -= param_PropertyChanged;
                param.PropertyChanged += param_PropertyChanged;
            }
            grdAppParams.DataSourceList = AppOwner.GeneralParams;
        }

        private void param_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == GeneralParam.Fields.Encrypt)
            {
                GeneralParam param = (GeneralParam)sender;
                String intialValue = param.Value;
                bool res = false;
                if (!string.IsNullOrEmpty(intialValue) && param.Encrypt)
                {
                    if (!EncryptionHandler.IsStringEncrypted(intialValue))
                    {
                        param.Value = EncryptionHandler.EncryptwithKey(intialValue);
                        if (string.IsNullOrEmpty(param.Value))
                        {
                            param.Value = string.Empty;
                        }
                    }
                }
                else
                {
                    if (EncryptionHandler.IsStringEncrypted(intialValue))
                    {
                        param.Value = null;
                    }
                }
            }
        }

        private void AddParamsToOtherEnvironmentsApps(object sender, RoutedEventArgs e)
        {
            bool paramsWereAdded = false;

            if (grdAppParams.Grid.SelectedItems.Count > 0)
            {
                foreach (object obj in grdAppParams.Grid.SelectedItems)
                {
                    ObservableList<ProjEnvironment> envs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
                    foreach (ProjEnvironment env in envs)
                    {
                        EnvApplication matchingApp = env.Applications.FirstOrDefault(x => x.Name == AppOwner.Name);
                        if (matchingApp != null && matchingApp != AppOwner)
                        {
                            if (matchingApp.GeneralParams.FirstOrDefault(x => x.Name == ((GeneralParam)obj).Name) == null)
                            {
                                GeneralParam param = (GeneralParam)(((RepositoryItemBase)obj).CreateCopy());
                                matchingApp.GeneralParams.Add(param);
                                paramsWereAdded = true;
                            }
                        }
                    }
                }

                if (paramsWereAdded)
                {
                    Reporter.ToUser(eUserMsgKey.ShareEnvAppParamWithAllEnvs);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }
        #endregion Functions

        private void ParamsGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            GeneralParam selectedVarb = (GeneralParam)grdAppParams.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(selectedVarb, GeneralParam.Fields.Value, null);
            VEEW.ShowAsWindow();
        }
    }
}