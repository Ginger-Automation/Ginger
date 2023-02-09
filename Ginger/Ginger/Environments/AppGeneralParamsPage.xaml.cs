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
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ginger.UserControls;
using GingerCore.Environments;
using GingerCore;
using GingerCore.Actions;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

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
                if (changedParam.Name != changedParam.NameBeforeEdit)
                {
                    //ask user if want us to update the parameter name in all BF's
                    if (Reporter.ToUser(eUserMsgKey.ChangingEnvironmentParameterValue) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                    {
                        UpdateVariableNameChange(changedParam);
                    }
                }
            }
            else if (e.Column.Header.ToString() == GeneralParam.Fields.Value)
            {
                GeneralParam selectedEnvParam = (GeneralParam)grdAppParams.CurrentItem;

                String intialValue = selectedEnvParam.Value;

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

        public void UpdateVariableNameChange(GeneralParam parameter)
        {
            if (parameter == null) return;

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

        #region Events
        private void AddParam(object sender, RoutedEventArgs e)
        {
            GeneralParam param = new GeneralParam() { Name = "Parameter " + AppOwner.GeneralParams.Count };
            param.PropertyChanged += param_PropertyChanged;

            AppOwner.GeneralParams.Add(param);
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
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = GeneralParam.Fields.Name, WidthWeight = 40 });
            view.GridColsView.Add(new GridColView() { Field = GeneralParam.Fields.Value, WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 30, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.appGenParamsPageGrid.Resources["ParamValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = GeneralParam.Fields.Encrypt, WidthWeight = 5, MaxWidth = 100, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = GeneralParam.Fields.Description, WidthWeight = 25 });

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
                        EnvApplication matchingApp = env.Applications.Where(x => x.Name == AppOwner.Name).FirstOrDefault();
                        if (matchingApp != null && matchingApp != AppOwner)
                        {
                            if (matchingApp.GeneralParams.Where(x => x.Name == ((GeneralParam)obj).Name).FirstOrDefault() == null)
                            {
                                GeneralParam param = (GeneralParam)(((RepositoryItemBase)obj).CreateCopy());
                                matchingApp.GeneralParams.Add(param);
                                paramsWereAdded = true;
                            }
                        }
                    }
                }

                if (paramsWereAdded)
                    Reporter.ToUser(eUserMsgKey.ShareEnvAppParamWithAllEnvs);
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
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
