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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.UserControls;
using Ginger.Variables;
using GingerCore;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for BusinessFlowRunVariablesPage.xaml
    /// </summary>
    public partial class BusinessFlowRunConfigurationsPage : Page
    {        
        private enum eWindowMode
        {
            Configuration,SummaryView
        }
        private eWindowMode mWindowMode;
        
        private BusinessFlow mBusinessFlow;
                
        private BusinessFlowExecutionSummary mBusinessFlowExecSummary;

        GenericWindow _pageGenericWin = null;

        ObservableList<BusinessFlow> mPrevBusinessFlowsInFlow;

        GingerRunner mGingerRunner;


        public BusinessFlowRunConfigurationsPage(GingerRunner mRunner, BusinessFlow businessFlow, ObservableList<BusinessFlow> prevBusinessFlowsInFlow)
        {
            InitializeComponent();

            mWindowMode = eWindowMode.Configuration;

            mGingerRunner = mRunner;
            mBusinessFlow = businessFlow;
            mPrevBusinessFlowsInFlow = prevBusinessFlowsInFlow;
            
            mBusinessFlow.SaveBackup();

			App.ObjFieldBinding(MandatoryBusinessFlowCB, CheckBox.IsCheckedProperty, businessFlow, BusinessFlow.Fields.Mandatory, BindingMode.TwoWay);

			RunDescritpion.Init(businessFlow, BusinessFlow.Fields.RunDescription);

            grdVariables.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditVar));
            grdVariables.AddToolbarTool("@Undo_16x16.png", "Reset " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " to Original Configurations", new RoutedEventHandler(ResetBusFlowVariables));
            grdVariables.AddToolbarTool("@Share_16x16.png", "Share Selected " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Value to all Similar Variables in Run Set", new RoutedEventHandler(CopyBusFlowVariables));
            grdVariables.RowDoubleClick += VariablesGrid_grdMain_MouseDoubleClick;

            SetVariablesGridView();
            LoadGridData();
            UpdateEditVariablesTabVisual();
            
            LoadBusinessFlowcontrols(businessFlow);
            UpdateFlowControlTabVisual();
            mBusinessFlow.BFFlowControls.CollectionChanged += BFFlowControls_CollectionChanged;
        }
        
        private void LoadBusinessFlowcontrols(BusinessFlow businessFlow)
        {
            FlowControlFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
            BusinessFlowRunFlowControlPage BFCP = new BusinessFlowRunFlowControlPage(mGingerRunner, businessFlow);
            FlowControlFrame.Content = BFCP;

        }
        public BusinessFlowRunConfigurationsPage(BusinessFlowExecutionSummary businessFlowExecSummary)
        {
            InitializeComponent();

            mWindowMode = eWindowMode.SummaryView;
            btnAutoCreateDescription.Visibility = System.Windows.Visibility.Collapsed;

            SetVariablesGridView();
            LoadGridData();
        }

        private void SetVariablesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Image, Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, MaxWidth = 20 });
            view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.ParentType, Header = "Level", WidthWeight = 10, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.ParentName, Header = "Parent Name", WidthWeight = 15, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Name, WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Description, WidthWeight = 20, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Formula, WidthWeight = 10, BindingMode = BindingMode.OneWay, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Value, Header = "Initial Value", WidthWeight = 10, BindingMode = BindingMode.OneWay, ReadOnly = true });           
            if (mWindowMode == eWindowMode.Configuration)
            {
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.MappedOutputValue, Header = "Mapped Runtime Value", StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetStoreToTemplate(VariableBase.Fields.MappedOutputType, VariableBase.Fields.MappedOutputValue,null, VariableBase.Fields.PossibleOutputVariables, VariableBase.Fields.SupportSetValue, "Output Variable", null), WidthWeight = 40 });
            }                
            else if (mWindowMode == eWindowMode.SummaryView)
            {
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.MappedOutputValue, Header = "Mapped Runtime Value", BindingMode = BindingMode.OneWay, ReadOnly = true, WidthWeight = 40});
            }
                
            view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.DiffrentFromOrigin, Header = "Different From Origin", WidthWeight = 15, BindingMode = BindingMode.OneWay, ReadOnly = true });
            grdVariables.SetAllColumnsDefaultView(view);
            grdVariables.InitViewItems();

            if (mWindowMode == eWindowMode.SummaryView)
                grdVariables.ShowEdit = System.Windows.Visibility.Collapsed;
        }

        private void LoadGridData()
        {
            //TODO: check if Summary view is used...
            switch (mWindowMode)
            {
                case eWindowMode.Configuration:                              
                    grdVariables.Title = "'" + mBusinessFlow.Name + "' Run " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                    ObservableList<VariableBase> bfInputVariables = mBusinessFlow.GetBFandActivitiesVariabeles(true,true);

                    //set the Output vars can be used
                    ObservableList<string> optionalOutputVars = new ObservableList<string>();
                    optionalOutputVars.Add(string.Empty);//default value for clear selection
                    //solution vars
                    if (BusinessFlow.SolutionVariables != null)
                        foreach (VariableBase var in BusinessFlow.SolutionVariables)
                            optionalOutputVars.Add(var.Name);
                    //prev bf's output vars
                    foreach (BusinessFlow bf in mPrevBusinessFlowsInFlow)
                    {                       
                        foreach (VariableBase var in bf.GetBFandActivitiesVariabeles(true, false, true))
                            optionalOutputVars.Add(var.Name);
                    }

                    //allow setting output vars options only to variables types which supports setting value
                    foreach (VariableBase inputVar in bfInputVariables)
                    {
                        if (inputVar.SupportSetValue)                        
                            inputVar.PossibleOutputVariables = optionalOutputVars;                        
                    }

                    grdVariables.DataSourceList = bfInputVariables;
                    break;

                case eWindowMode.SummaryView:
                    grdVariables.Title = "'" + mBusinessFlowExecSummary.BusinessFlowName + "' Run " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                    grdVariables.DataSourceList = mBusinessFlowExecSummary.ExecutionVariabeles;
                    break;
            }
        }

        private void CopyBusFlowVariables(object sender, RoutedEventArgs e)
        {
            if (grdVariables.CurrentItem != null)
            {
                if (Reporter.ToUser(eUserMsgKeys.AskIfShareVaribalesInRunner) == MessageBoxResult.Yes)
                {
                    int countMatchingBfs = 0;
                    
                    List<VariableBase> selectedVars = grdVariables.Grid.SelectedItems.Cast<VariableBase>().ToList();
                    foreach (GingerRunner runner in App.RunsetExecutor.Runners)
                    {
                        List<BusinessFlow> matchingBfs = runner.BusinessFlows.Where(x => x.Guid == mBusinessFlow.Guid).ToList();
                        countMatchingBfs += matchingBfs.Count;
                        foreach (BusinessFlow bf in matchingBfs)
                        {
                            foreach (VariableBase selectedVar in selectedVars)
                            {

                                VariableBase matchingVar = bf.GetBFandActivitiesVariabeles(true).Where(x => x.Guid == selectedVar.Guid).FirstOrDefault();
                                if (matchingVar != null)
                                {
                                    String originalValue = matchingVar.Value;
                                    String originalFormula = matchingVar.Formula;

                                    VariableBase copiedVar = (VariableBase)selectedVar.CreateCopy(); ;
                                    if (selectedVar.ParentType == "Business Flow")
                                    {
                                        int indexSelected = bf.Variables.IndexOf(matchingVar);
                                        bf.Variables.Remove(matchingVar);
                                        bf.Variables.Insert(indexSelected, copiedVar);
                                    }
                                    if (selectedVar.ParentType == "Activity")
                                    {
                                        Activity a = bf.GetActivity(selectedVar.ParentGuid);
                                        int indexSelected = a.Variables.IndexOf(matchingVar);
                                        a.Variables.Remove(matchingVar);
                                        a.Variables.Insert(indexSelected, copiedVar);
                                    }
                                    if (copiedVar.Formula != originalFormula || copiedVar.Value != originalValue) //variable was changed
                                    {
                                        copiedVar.VarValChanged = true;
                                        copiedVar.DiffrentFromOrigin = true;
                                    }
                                }
                            }
                        }
                    }
                    Reporter.ToUser(eUserMsgKeys.CopiedVariableSuccessfully, countMatchingBfs - 1);
                }

            }
            else
                Reporter.ToUser(eUserMsgKeys.ShareVariableNotSelected);
        }
        private void ResetBusFlowVariables(object sender, RoutedEventArgs e)
        {
            try
            {            
                BusinessFlow originalBF = (from bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>() where bf.Guid == mBusinessFlow.Guid select bf).FirstOrDefault();
                if (originalBF == null)
                    originalBF = (from bf in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>() where bf.Name == mBusinessFlow.Name select bf).FirstOrDefault();             
                if (originalBF == null)
                {
                    Reporter.ToUser(eUserMsgKeys.ResetBusinessFlowRunVariablesFailed, "Original " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " was not found");
                    return;
                }
                else
                {
                    BusinessFlow originalBFCopy = (BusinessFlow)originalBF.CreateCopy(false);
                    mBusinessFlow.Variables = originalBFCopy.Variables;
                    mBusinessFlow.Activities = originalBFCopy.Activities; 
                    LoadGridData();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.ResetBusinessFlowRunVariablesFailed, ex.Message);
            }
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            grdVariables.grdMain.ItemsSource = null;
            grdVariables.grdMain.ItemsSource = grdVariables.DataSourceList;
        }

        private void EditVar(object sender, RoutedEventArgs e)
        {
            if (grdVariables.CurrentItem != null && grdVariables.CurrentItem.ToString() != "{NewItemPlaceholder}")
            {
                EditVar();
                RefreshGrid(sender, e);
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.AskToSelectVariable);
            }
        }

        private void VariablesGrid_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditVar();
        }

        private void EditVar()
        {
            VariableBase varToEdit = (VariableBase)grdVariables.CurrentItem;
            string originalFormula = varToEdit.Formula;
            string originalValue= varToEdit.Value;            
            VariableEditPage w = new VariableEditPage(varToEdit, true);
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            if (varToEdit.Formula != originalFormula || varToEdit.Value != originalValue)//variable was changed
            {
                varToEdit.VarValChanged = true;
                varToEdit.DiffrentFromOrigin = true;
            }
            UpdateEditVariablesTabVisual();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            switch (mWindowMode)
            {
                case eWindowMode.Configuration:
                    Button okBtn = new Button();
                    okBtn.Content = "Ok";
                    okBtn.Click += new RoutedEventHandler(okBtn_Click);
                    Button undoBtn = new Button();
                    undoBtn.Content = "Undo & Close";
                    undoBtn.Click += new RoutedEventHandler(undoBtn_Click);
                    ObservableList<Button> winButtons = new ObservableList<Button>();
                    winButtons.Add(okBtn);
                    winButtons.Add(undoBtn);

                    GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Edit " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Run Configurations", this, winButtons, false, "Undo & Close", CloseWinClicked);
                    break;

                case eWindowMode.SummaryView:
                    GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Run Configurations", this, null, true, "Close");
                    break;
            }        
        }

        private void UndoChangesAndClose()
        {
            Mouse.OverrideCursor = Cursors.Wait;            
            mBusinessFlow.RestoreFromBackup(true);
            Mouse.OverrideCursor = null;

            _pageGenericWin.Close();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.AskIfToUndoChanges) == MessageBoxResult.Yes)
            {
                UndoChangesAndClose();
            }
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            _pageGenericWin.Close();
        }

        private void btnAutoCreateDescription_Click(object sender, RoutedEventArgs e)
        {
            SetAutoDescription();
        }

        private void SetAutoDescription()
        {
            string autoDesc = string.Empty;
            List<VariableBase> bfVariables = (mBusinessFlow.GetBFandActivitiesVariabeles(true)).Where(var => var.GetType() == typeof(VariableSelectionList) || var.DiffrentFromOrigin == true || string.IsNullOrEmpty(var.MappedOutputVariable)==false || string.IsNullOrEmpty(var.MappedOutputValue) == false).ToList(); ;
            if (bfVariables != null && bfVariables.Count > 0)
            {
                autoDesc = "Running Configurations: ";
                foreach (VariableBase var in bfVariables)
                {
                    if (string.IsNullOrEmpty(var.MappedOutputVariable) == false)
                        autoDesc += "'" + var.Name + "' variable value mapped to '" + var.Value + "', ";
                    else
                        autoDesc += "'" + var.Name + "' variable value = '" + var.Value + "', ";
                }
                    
                autoDesc = autoDesc.TrimEnd(new char[] { ',', ' ' });
            }
            else
            {
                autoDesc = "Default Run";
            }

            RunDescritpion.ValueTextBox.Text = autoDesc;
        }

        private void grdVariables_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void RunDescritpion_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void BusinessFlowTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //set the selected tab text style
            try
            {
                if (BusinessFlowTab.SelectedItem != null)
                {
                    foreach (TabItem tab in BusinessFlowTab.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (BusinessFlowTab.SelectedItem == tab)
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$Color_DarkBlue");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error in Action Edit Page tabs style", ex);
            }

            
        }

        private void BFFlowControls_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateFlowControlTabVisual();
        }
        private void UpdateFlowControlTabVisual()
        {
            bool b = mBusinessFlow.BFFlowControls.Count() > 0;
            SetTabOnOffSign(FlowControlTab, b);
            if (b)
            {
                FlowControlCountLabel.Text = "(" + mBusinessFlow.BFFlowControls.Count() + ")";
            }
            else
            {
                FlowControlCountLabel.Text = "";
            }
        }
        void UpdateEditVariablesTabVisual()
        {
            int count = 0;
            foreach (VariableBase var in mBusinessFlow.GetBFandActivitiesVariabeles(true,true))
                if (var.DiffrentFromOrigin == true)
                    count++;

            if (count > 0)
            {
                SetTabOnOffSign(EditVariablesTab, true);
                EditVariablesCountLabel.Text = "(" + count + ")";
            }
            else
            {
                SetTabOnOffSign(EditVariablesTab, false);
                EditVariablesCountLabel.Text = "";
            }
        }
        
        void SetTabOnOffSign(TabItem tab, bool indicatorToShow)
        {
            try
            {
                //set the selected tab text style
                if (tab != null)
                {
                    foreach (object ctrl in ((StackPanel)(tab.Header)).Children)
                        if (ctrl.GetType() == typeof(System.Windows.Controls.Image))
                        {
                            System.Windows.Controls.Image img = (System.Windows.Controls.Image)ctrl;
                            if (img.Tag != null)
                            {
                                if (img.Tag.ToString() == "OffSignImage")
                                    if (indicatorToShow)
                                        img.Visibility = Visibility.Collapsed;
                                    else
                                        img.Visibility = Visibility.Visible;
                                else if (img.Tag.ToString() == "OnSignImage")
                                    if (indicatorToShow)
                                        img.Visibility = Visibility.Visible;
                                    else
                                        img.Visibility = Visibility.Collapsed;
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error in Business Flow Configuration Page tabs style", ex);
            }
        }
    }
}
