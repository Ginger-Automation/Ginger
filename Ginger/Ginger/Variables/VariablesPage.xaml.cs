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

using GingerWPF.DragDropLib;
using Ginger.Environments;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ginger.BusinessFlowFolder;
using Amdocs.Ginger.Common;
using System.Linq;
using Amdocs.Ginger.Repository;

namespace Ginger.Variables
{
    public enum eVariablesPageViewStyle
    {
        Design = 0,
    }
    
    /// <summary>
    /// Interaction logic for VariablesPage.xaml
    /// </summary>
    public partial class VariablesPage : Page
    {
        private eVariablesLevel mVariablesLevel;
        private object mVariablesParentObj;
        private bool mVariablesParentObjIsStatic;
        
        public eVariablesLevel VariablesLevel
        {
            get { return mVariablesLevel; }
            set { mVariablesLevel = value; }
        }
        
        /// <summary>
        /// Business Flow or Activity Variables
        /// </summary>
        /// <param name="variablesLevel">Type of Variables parent object</param>
        /// <param name="variablesParentObj">Actual Variables parent object, if not provided then the Current Business Flow / Activity will be used</param>       
        public VariablesPage(eVariablesLevel variablesLevel, object variablesParentObj = null, General.RepositoryItemPageViewMode editMode = General.RepositoryItemPageViewMode.Automation)
        {
            InitializeComponent();

            mVariablesLevel = variablesLevel;
            mVariablesParentObj = variablesParentObj;
            if (variablesParentObj == null)
            {
                mVariablesParentObjIsStatic = false;
            }
            else
                mVariablesParentObjIsStatic = true;
            App.PropertyChanged += App_PropertyChanged; //Hook to catch current Business Flow changes
            SetVariablesParentObj();            
            SetVariablesGridView();
            LoadGridData();
            if (editMode == General.RepositoryItemPageViewMode.View)
            {
                SetViewMode();
            }
        }

        private void grdVariables_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(VariableBase)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = DragInfo.eDragIcon.Copy;
            }
        }

        private void grdVariables_ItemDropped(object sender, EventArgs e)
        {
            VariableBase v = (VariableBase)((DragInfo)sender).Data;
            VariableBase instance = (VariableBase)v.CreateInstance(true);
            switch (mVariablesLevel)
            {
                case eVariablesLevel.BusinessFlow:
                    ((BusinessFlow)mVariablesParentObj).AddVariable(instance);
                    break;
                case eVariablesLevel.Activity:
                    ((Activity)mVariablesParentObj).AddVariable(instance);
                    break;
            }

            int selectedActIndex = -1;
            ObservableList<VariableBase> actsList = App.BusinessFlow.Variables;
            if (actsList.CurrentItem != null)
            {
                selectedActIndex = actsList.IndexOf((VariableBase)actsList.CurrentItem);
            }
            if (selectedActIndex >= 0)
            {
                actsList.Move(actsList.Count - 1, selectedActIndex + 1);
            }
        }

        private void SetVariablesParentObj()
        {
            switch (mVariablesLevel)
            {
                case eVariablesLevel.Solution:
                    if (mVariablesParentObjIsStatic == false)
                    {
                        if (App.UserProfile.Solution != null)
                        {
                            mVariablesParentObj = App.UserProfile.Solution;
                            ((Solution)mVariablesParentObj).PropertyChanged += Solution_PropertyChanged;//Hook to catch Solution Variables changes
                        }
                        else
                            mVariablesParentObj = new Solution();//to avoid crashing
                    }
                    break;
                case eVariablesLevel.BusinessFlow:
                    if (mVariablesParentObjIsStatic == false)
                    {
                        if (App.BusinessFlow != null)
                        {
                            mVariablesParentObj = App.BusinessFlow;
                            ((BusinessFlow)mVariablesParentObj).PropertyChanged += BusinessFlow_PropertyChanged;//Hook to catch Business Flow Variables changes
                        }
                        else
                            mVariablesParentObj = new BusinessFlow();//to avoid crashing
                    }
                    break;
                case eVariablesLevel.Activity:
                    if (mVariablesParentObjIsStatic == false)
                    {
                        if (App.BusinessFlow != null && App.BusinessFlow.CurrentActivity != null)
                        {
                            mVariablesParentObj = App.BusinessFlow.CurrentActivity;
                            App.BusinessFlow.PropertyChanged += BusinessFlow_PropertyChanged;//Hook to catch Current Activity changes                           
                        }
                        else
                            mVariablesParentObj = new Activity();//to avoid crashing
                    }
                    if (mVariablesParentObj != null)
                        ((Activity)mVariablesParentObj).PropertyChanged += Activity_PropertyChanged;//Hook to catch Activity Variables changes
                    break;
            }
        }

        private void LoadGridData()
        {
            if (mVariablesParentObj != null)
            {
                switch (mVariablesLevel)
                {
                    case eVariablesLevel.Solution:
                        if (mVariablesParentObjIsStatic)
                            grdVariables.Title = GingerDicser.GetTermResValue(eTermResKey.Variables);
                        App.LocalRepository.MarkSharedRepositoryItems((IEnumerable<object>)((Solution)mVariablesParentObj).Variables, (IEnumerable<object>)App.LocalRepository.GetSolutionRepoVariables());
                        grdVariables.DataSourceList = ((Solution)mVariablesParentObj).Variables;
                        break;
                    case eVariablesLevel.BusinessFlow:
                        if (mVariablesParentObjIsStatic)
                            grdVariables.Title = GingerDicser.GetTermResValue(eTermResKey.Variables);
                        else
                            grdVariables.Title = "'" + ((BusinessFlow)mVariablesParentObj).Name + "' - " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                        App.LocalRepository.MarkSharedRepositoryItems((IEnumerable<object>)((BusinessFlow)mVariablesParentObj).Variables, (IEnumerable<object>)App.LocalRepository.GetSolutionRepoVariables());
                        grdVariables.DataSourceList = ((BusinessFlow)mVariablesParentObj).Variables;
                        break;
                    case eVariablesLevel.Activity:
                        if (mVariablesParentObjIsStatic)
                            grdVariables.Title = GingerDicser.GetTermResValue(eTermResKey.Variables);
                        else
                            grdVariables.Title = "'" + ((Activity)mVariablesParentObj).ActivityName + "' - " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                        App.LocalRepository.MarkSharedRepositoryItems((IEnumerable<object>)((Activity)mVariablesParentObj).Variables, (IEnumerable<object>)App.LocalRepository.GetSolutionRepoVariables());
                        grdVariables.DataSourceList = ((Activity)mVariablesParentObj).Variables;
                        break;
                }

                if (grdVariables.DataSourceList != null)
                {
                    grdVariables.DataSourceList.CollectionChanged += VariablesPage_CollectionChanged;
                }
            }
        }
        public void SetViewMode()
        {           
                grdVariables.ShowToolsBar = Visibility.Collapsed;             
                grdVariables.ToolsTray.Visibility = Visibility.Collapsed;
                grdVariables.RowDoubleClick -= VariablesGrid_grdMain_MouseDoubleClick;
                grdVariables.DisableGridColoumns();
        }
        private void SetVariablesGridView()
        {
            //Columns View
            if (mVariablesLevel == eVariablesLevel.BusinessFlow || mVariablesLevel == eVariablesLevel.Activity)
            {
                GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
                view.GridColsView = new ObservableList<GridColView>();
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Image, Header = " ", StyleType = GridColView.eGridColStyleType.Image, WidthWeight = 2.5, MaxWidth = 20 });
                view.GridColsView.Add(new GridColView() { Field = RepositoryItem.Fields.SharedRepoInstanceImage, Header = "S.R.", StyleType = GridColView.eGridColStyleType.Image, WidthWeight = 2.5, MaxWidth = 20 });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Name, WidthWeight = 20, AllowSorting = true });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Description, WidthWeight = 15 });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.VariableUIType, Header="Type", WidthWeight = 10, BindingMode = BindingMode.OneWay });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Formula, WidthWeight = 20, BindingMode = BindingMode.OneWay, ReadOnly = true });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.SetAsInputValue, Header = "Set as Input Value", WidthWeight = 10, MaxWidth = 200, StyleType = GridColView.eGridColStyleType.CheckBox });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.SetAsOutputValue, Header = "Set as Output Value", WidthWeight = 10, MaxWidth = 200, StyleType = GridColView.eGridColStyleType.CheckBox });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.LinkedVariableName, Header="Linked Variable", WidthWeight = 10, BindingMode = BindingMode.OneWay, ReadOnly = true });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Value, Header = "Current Value", WidthWeight = 20, BindingMode = BindingMode.TwoWay, ReadOnly = true });                                                              
                grdVariables.SetAllColumnsDefaultView(view);
            }
            else//Global Variables 
            {
                GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
                view.GridColsView = new ObservableList<GridColView>();
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Image, Header = " ", StyleType = GridColView.eGridColStyleType.Image, WidthWeight = 2.5, MaxWidth = 20 });                
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Name, WidthWeight = 20 , AllowSorting = true });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Description, WidthWeight = 20 });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Formula, WidthWeight = 20, BindingMode = BindingMode.OneWay, ReadOnly = true });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.LinkedVariableName, Header = "Linked Variable", WidthWeight = 15, BindingMode = BindingMode.OneWay, ReadOnly = true });
                view.GridColsView.Add(new GridColView() { Field = VariableBase.Fields.Value, Header = "Current Value", WidthWeight = 15, BindingMode = BindingMode.OneWay, ReadOnly = true });               
                grdVariables.SetAllColumnsDefaultView(view);
            }

            grdVariables.InitViewItems();

            //Tool Bar
            grdVariables.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditVar));
            grdVariables.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddVar));
            grdVariables.ShowCopyCutPast = System.Windows.Visibility.Visible;
            grdVariables.ShowTagsFilter = Visibility.Visible;
            grdVariables.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshVarsGrid));
            grdVariables.AddToolbarTool("@Reset_16x16.png", "Reset Selected " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Value", new RoutedEventHandler(ResetVariablesValue));
            grdVariables.AddToolbarTool("@A_16x16.png", "Generate Auto Value to Selected " + GingerDicser.GetTermResValue(eTermResKey.Variables) + "", new RoutedEventHandler(GenerateVariablesValue));
            grdVariables.AddFloatingImageButton("@Reset_16x16.png", "Reset Value", new RoutedEventHandler(ResetVariablesValue), 2);           
            grdVariables.AddFloatingImageButton("@A_16x16.png", "Generate Auto Value", new RoutedEventHandler(GenerateVariablesValue), 2);
            if (mVariablesLevel != eVariablesLevel.Solution)
                grdVariables.AddToolbarTool("@UploadStar_16x16.png", "Add to Shared Repository", new RoutedEventHandler(AddToRepository));

            //Events
            grdVariables.RowDoubleClick += VariablesGrid_grdMain_MouseDoubleClick;
            if (grdVariables.grdMain != null)
            {
                grdVariables.grdMain.RowEditEnding += grdMain_RowEditEnding;
                grdVariables.grdMain.PreparingCellForEdit += grdMain_PreparingCellForEdit;
                grdVariables.grdMain.CellEditEnding += grdMain_CellEditEnding;
            }
            grdVariables.ItemDropped += grdVariables_ItemDropped;
            grdVariables.PreviewDragItem += grdVariables_PreviewDragItem;
        }

        private void App_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BusinessFlow")
            {
                if (App.BusinessFlow != null)
                {
                    if (mVariablesLevel == eVariablesLevel.BusinessFlow && (BusinessFlow)mVariablesParentObj != App.BusinessFlow)
                    {
                        SetVariablesParentObj();
                        LoadGridData();
                    }
                    else if (mVariablesLevel == eVariablesLevel.Activity && (Activity)mVariablesParentObj != App.BusinessFlow.CurrentActivity)
                    {
                        SetVariablesParentObj();
                        LoadGridData();
                    }
                }
                else
                {
                    //TODO: ???
                }
            }
        }

        private void Solution_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Variables" && mVariablesLevel == eVariablesLevel.Solution)
            {
                if ((Solution)mVariablesParentObj == App.UserProfile.Solution)
                {
                    LoadGridData();
                }
            }
        }

        private void BusinessFlow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentActivity" && mVariablesLevel == eVariablesLevel.Activity)
            {
                if ((Activity)mVariablesParentObj != App.BusinessFlow.CurrentActivity)
                {
                    SetVariablesParentObj();
                    LoadGridData();
                }
            }
            else if (e.PropertyName == "Variables" && mVariablesLevel == eVariablesLevel.BusinessFlow)
            {
                if ((BusinessFlow)mVariablesParentObj == App.BusinessFlow)
                {
                    LoadGridData();
                }
            }
        }

        private void Activity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Variables" && mVariablesLevel == eVariablesLevel.Activity)
            {
                if ((Activity)mVariablesParentObj == App.BusinessFlow.CurrentActivity)
                {
                    LoadGridData();
                }
            }
        }

        private void AddToRepository(object sender, RoutedEventArgs e)
        {          
            Repository.SharedRepositoryOperations.AddItemsToRepository(grdVariables.Grid.SelectedItems.Cast<RepositoryItemBase>().ToList());
         
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            grdVariables.grdMain.ItemsSource = null;
            grdVariables.grdMain.ItemsSource = grdVariables.DataSourceList;
        }

        private void RefreshVarsGrid(object sender, RoutedEventArgs e)
        {
            LoadGridData();
        }
        
        private void ResetVariablesValue(object sender, RoutedEventArgs e)
        {
            if (grdVariables == null) return;
            if (grdVariables.Grid.SelectedItems.Count == 0)
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
            else
                foreach (object var in grdVariables.Grid.SelectedItems)
                    ((VariableBase)var).ResetValue();
        }

        private void GenerateVariablesValue(object sender, RoutedEventArgs e)
        {
            if (grdVariables == null) return;

            if (grdVariables.Grid.SelectedItems.Count == 0)
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
            else
                foreach (object var in grdVariables.Grid.SelectedItems)
                    ((VariableBase)var).GenerateAutoValue();
        }

        private void EditVar(object sender, RoutedEventArgs e)
        {
            if (grdVariables.CurrentItem != null && grdVariables.CurrentItem.ToString() != "{NewItemPlaceholder}")
            {
                VariableBase selectedVarb = (VariableBase)grdVariables.CurrentItem;
                VariableEditPage.eEditMode  mode=VariableEditPage.eEditMode.BusinessFlow;
                selectedVarb.NameBeforeEdit = selectedVarb.Name;    
                switch(mVariablesLevel)// variable page is generic so need to change editmode as per type of variable.
                {
                    case eVariablesLevel.Activity:
                        mode = VariableEditPage.eEditMode.Activity;
                        break;
                    case eVariablesLevel.BusinessFlow:
                        mode = VariableEditPage.eEditMode.BusinessFlow;
                        break;
                    case eVariablesLevel.Solution:
                        mode = VariableEditPage.eEditMode.Global;
                        break;
                }            
                VariableEditPage w = new VariableEditPage(selectedVarb, false,mode);
                w.ShowAsWindow(eWindowShowStyle.Dialog);
                RefreshGrid(sender, e);

                if (selectedVarb.NameBeforeEdit != selectedVarb.Name)
                    UpdateVariableNameChange(selectedVarb);
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.AskToSelectVariable);
            }
        }

        private void VariablesGrid_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            VariableBase selectedVarb = (VariableBase)grdVariables.CurrentItem;
            selectedVarb.NameBeforeEdit = selectedVarb.Name;

            VariableEditPage w = new VariableEditPage(selectedVarb, false);
            w.ShowAsWindow(eWindowShowStyle.Dialog);

            if (selectedVarb.NameBeforeEdit != selectedVarb.Name)
                UpdateVariableNameChange(selectedVarb);
        }

        private void AddVar(object sender, RoutedEventArgs e)
        {
            AddVariablePage addVarPage = new AddVariablePage(mVariablesLevel, mVariablesParentObj);
            addVarPage.ShowAsWindow();

            RefreshGrid(sender, e);
        }
        
        private void VariablesPage_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshVariablesNames();
        }

        private void grdMain_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            RefreshVariablesNames();

            VariableBase selectedVarb = (VariableBase)grdVariables.CurrentItem;
            if (selectedVarb.Name != selectedVarb.NameBeforeEdit)
                UpdateVariableNameChange(selectedVarb);
        }

        private void RefreshVariablesNames()
        {
            if (mVariablesParentObjIsStatic == false
                && mVariablesLevel == eVariablesLevel.Activity && mVariablesParentObj != null)
            {
                ((Activity)mVariablesParentObj).RefreshVariablesNames();
            }
        }

        private void grdMain_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() == VariableBase.Fields.Name)
            {
                VariableBase selectedVarb = (VariableBase)grdVariables.CurrentItem;
                selectedVarb.NameBeforeEdit = selectedVarb.Name;
            }
        }

        private void grdMain_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == VariableBase.Fields.Name)
            {
                VariableBase selectedVarb = (VariableBase)grdVariables.CurrentItem;
                if (selectedVarb.Name != selectedVarb.NameBeforeEdit)
                    UpdateVariableNameChange(selectedVarb);
            }
        }

        public void UpdateVariableNameChange(VariableBase variable)
        {
            if (variable == null) return;

            switch (mVariablesLevel)
            {
                case eVariablesLevel.Solution:
                    ObservableList<BusinessFlow> allBF = App.LocalRepository.GetSolutionBusinessFlows();
                    foreach(BusinessFlow bfl in allBF)
                    {
                        bfl.SetUniqueVariableName(variable);
                        foreach (Activity activity in bfl.Activities)
                            foreach (Act action in activity.Acts)
                            {
                                bool changedwasDone = false;
                                VariableBase.UpdateVariableNameChangeInItem(action, variable.NameBeforeEdit, variable.Name,ref changedwasDone);
                                if (changedwasDone == true && bfl.IsDirty == false)
                                    bfl.SaveBackup();
                            }
                    }
                    break;

                case eVariablesLevel.BusinessFlow:
                    BusinessFlow bf = (BusinessFlow)mVariablesParentObj;
                    bf.SetUniqueVariableName(variable);
                    foreach (Activity activity in bf.Activities)
                        foreach (Act action in activity.Acts)
                        {
                            bool changedwasDone = false;
                            VariableBase.UpdateVariableNameChangeInItem(action, variable.NameBeforeEdit, variable.Name, ref changedwasDone);
                            if (changedwasDone == true && bf.IsDirty == false)
                                bf.SaveBackup();
                        }
                    break;

                case eVariablesLevel.Activity:
                    Activity activ = (Activity)mVariablesParentObj;
                    activ.SetUniqueVariableName(variable);
                    foreach (Act action in activ.Acts)
                    {
                        bool changedwasDone = false;
                        VariableBase.UpdateVariableNameChangeInItem(action, variable.NameBeforeEdit, variable.Name, ref changedwasDone);
                    }
                    break;
            }

            variable.NameBeforeEdit = variable.Name;
        }
    }
}
