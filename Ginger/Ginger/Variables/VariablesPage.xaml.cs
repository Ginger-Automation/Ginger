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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Repository;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Variables;
using GingerWPF.DragDropLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
       
        readonly General.eRIPageViewMode mEditMode;
        Context mContext = new Context();

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
        public VariablesPage(eVariablesLevel variablesLevel, object variablesParentObj, Context context, General.eRIPageViewMode editMode = General.eRIPageViewMode.Automation)
        {
            InitializeComponent();

            mVariablesLevel = variablesLevel;
            mVariablesParentObj = variablesParentObj;
            mEditMode = editMode;
            mContext = context;
            SetVariablesParentObj(variablesParentObj);                      
            SetVariablesGridView();            

            if (mEditMode == General.eRIPageViewMode.View)
            {
                grdVariables.ShowToolsBar = Visibility.Collapsed;
                grdVariables.ToolsTray.Visibility = Visibility.Collapsed;
                grdVariables.RowDoubleClick -= VariablesGrid_grdMain_MouseDoubleClick;
                grdVariables.DisableGridColoumns();
            }
        }

        private void grdVariables_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(VariableBase)))
            {
                // OK to drop
                DragDrop2.SetDragIcon(true);
            }
            else
            {
                // Do Not Drop
                DragDrop2.SetDragIcon(false);
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
            ObservableList<VariableBase> actsList = mContext.BusinessFlow.Variables;
            if (actsList.CurrentItem != null)
            {
                selectedActIndex = actsList.IndexOf((VariableBase)actsList.CurrentItem);
            }
            if (selectedActIndex >= 0)
            {
                actsList.Move(actsList.Count - 1, selectedActIndex + 1);
            }
        }

        private void SetVariablesParentObj(object variablesParentObj)
        {
            switch (mVariablesLevel)
            {
                case eVariablesLevel.Solution:
                    if (variablesParentObj != null)
                    {
                        mVariablesParentObj = variablesParentObj;
                        ((Solution)mVariablesParentObj).PropertyChanged -= Solution_PropertyChanged;
                        ((Solution)mVariablesParentObj).PropertyChanged += Solution_PropertyChanged;//Hook to catch Solution Variables changes
                        LoadGridData();
                    }
                    break;

                case eVariablesLevel.BusinessFlow:
                    if (variablesParentObj != null)
                    {
                        UpdateBusinessFlow((BusinessFlow)variablesParentObj);
                    }
                    break;

                case eVariablesLevel.Activity:
                    if (variablesParentObj != null)
                    {
                        UpdateActivity((Activity)variablesParentObj);
                    }
                    break;
            }
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            mVariablesParentObj = bf;
            mContext.BusinessFlow = (BusinessFlow)mVariablesParentObj;
            if (mVariablesParentObj != null)
            {
                LoadGridData();
            }
            grdVariables.ClearFilters();
        }

        public void UpdateActivity(Activity activity)
        {
            mVariablesParentObj = activity;
            if (mVariablesParentObj != null)
            {
                ((Activity)mVariablesParentObj).PropertyChanged -= Activity_PropertyChanged;
                ((Activity)mVariablesParentObj).PropertyChanged += Activity_PropertyChanged;//Hook to catch Activity Variables changes
                LoadGridData();
            }
        }

        private void LoadGridData()
        {            
            if (mVariablesParentObj != null)
            {
                ObservableList<VariableBase> variables = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
                switch (mVariablesLevel)
                {
                    case eVariablesLevel.Solution:                        
                        grdVariables.DataSourceList = ((Solution)mVariablesParentObj).Variables;
                        break;

                    case eVariablesLevel.BusinessFlow:
                        if (mEditMode != General.eRIPageViewMode.Automation)
                            grdVariables.Title = GingerDicser.GetTermResValue(eTermResKey.Variables);
                        else
                            grdVariables.Title = "'" + ((BusinessFlow)mVariablesParentObj).Name + "' - " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                        SharedRepositoryOperations.MarkSharedRepositoryItems((IEnumerable<object>)((BusinessFlow)mVariablesParentObj).Variables, (IEnumerable<object>)variables);
                        grdVariables.DataSourceList = ((BusinessFlow)mVariablesParentObj).Variables;
                        break;

                    case eVariablesLevel.Activity:
                        if (mEditMode != General.eRIPageViewMode.Automation)
                            grdVariables.Title = GingerDicser.GetTermResValue(eTermResKey.Variables);
                        else
                            grdVariables.Title = "'" + ((Activity)mVariablesParentObj).ActivityName + "' - " + GingerDicser.GetTermResValue(eTermResKey.Variables);
                        SharedRepositoryOperations.MarkSharedRepositoryItems((IEnumerable<object>)((Activity)mVariablesParentObj).Variables, (IEnumerable<object>)variables);
                        grdVariables.DataSourceList = ((Activity)mVariablesParentObj).Variables;
                        break;
                }

                if (grdVariables.DataSourceList != null)
                {
                    grdVariables.DataSourceList.CollectionChanged -= VariablesPage_CollectionChanged;
                    grdVariables.DataSourceList.CollectionChanged += VariablesPage_CollectionChanged;
                }
            }
        }

        private void SetVariablesGridView()
        {
            //Columns View
            if (mVariablesLevel == eVariablesLevel.BusinessFlow || mVariablesLevel == eVariablesLevel.Activity)
            {
                GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
                view.GridColsView = new ObservableList<GridColView>();
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.Image), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, MaxWidth = 20 });
                view.GridColsView.Add(new GridColView() { Field = nameof(RepositoryItemBase.SharedRepoInstanceImage), Header = "S.R.", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, MaxWidth = 20 });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.Name), WidthWeight = 20, AllowSorting = true });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.Description), WidthWeight = 15 });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.VariableUIType), Header = "Type", WidthWeight = 10, BindingMode = BindingMode.OneWay });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.Formula), WidthWeight = 20, BindingMode = BindingMode.OneWay, ReadOnly = true });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.SetAsInputValue), Header = "Set as Input Value", WidthWeight = 10, MaxWidth = 200, StyleType = GridColView.eGridColStyleType.CheckBox });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.SetAsOutputValue), Header = "Set as Output Value", WidthWeight = 10, MaxWidth = 200, StyleType = GridColView.eGridColStyleType.CheckBox });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.LinkedVariableName), Header = "Linked " + GingerDicser.GetTermResValue(eTermResKey.Variable) , WidthWeight = 10, BindingMode = BindingMode.OneWay, ReadOnly = true });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.Value), Header = "Current Value", WidthWeight = 20, BindingMode = BindingMode.TwoWay, ReadOnly = true });
                grdVariables.SetAllColumnsDefaultView(view);
            }
            else//Solution Global Variables 
            {
                GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
                view.GridColsView = new ObservableList<GridColView>();
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.Image), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, MaxWidth = 20 });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.Name), WidthWeight = 20, AllowSorting = true });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.Description), WidthWeight = 20 });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.Formula), WidthWeight = 20, BindingMode = BindingMode.OneWay, ReadOnly = true });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.LinkedVariableName), Header = "Linked " + GingerDicser.GetTermResValue(eTermResKey.Variable), WidthWeight = 15, BindingMode = BindingMode.OneWay, ReadOnly = true });
                view.GridColsView.Add(new GridColView() { Field = nameof(VariableBase.Value), Header = "Current Value", WidthWeight = 15, BindingMode = BindingMode.OneWay, ReadOnly = true });
                grdVariables.SetAllColumnsDefaultView(view);

                grdVariables.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.Variable, GingerDicser.GetTermResValue(eTermResKey.Variables, "Global "), saveAllHandler: SaveSolutionConfigurations, addHandler: AddVar);
                grdVariables.ShowAdd = Visibility.Collapsed;
                grdVariables.ShowRefresh = Visibility.Collapsed;
            }

            grdVariables.InitViewItems();

            //Tool Bar
            grdVariables.btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditVar));
            
            grdVariables.ShowCopyCutPast = System.Windows.Visibility.Visible;
            grdVariables.ShowTagsFilter = Visibility.Visible;


            grdVariables.AddToolbarTool("@Reset_16x16.png", "Reset Selected " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Value", new RoutedEventHandler(ResetVariablesValue));
            grdVariables.AddToolbarTool("@A_16x16.png", "Generate Auto Value to Selected " + GingerDicser.GetTermResValue(eTermResKey.Variables) + "", new RoutedEventHandler(GenerateVariablesValue));
            grdVariables.AddFloatingImageButton("@Reset_16x16.png", "Reset Value", new RoutedEventHandler(ResetVariablesValue), 2);
            grdVariables.AddFloatingImageButton("@A_16x16.png", "Generate Auto Value", new RoutedEventHandler(GenerateVariablesValue), 2);
            if (mVariablesLevel != eVariablesLevel.Solution)
            {
                grdVariables.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddVar));               
                grdVariables.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshVarsGrid));                
                grdVariables.AddToolbarTool("@UploadStar_16x16.png", "Add to Shared Repository", new RoutedEventHandler(AddToRepository));                
            }

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

        private void Solution_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Solution.Variables) && mVariablesLevel == eVariablesLevel.Solution)
            {
                if ((Solution)mVariablesParentObj ==  WorkSpace.Instance.Solution)
                {
                    LoadGridData();
                }
            }
        }


        private void Activity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Activity.Variables) && mVariablesLevel == eVariablesLevel.Activity)
            {
                LoadGridData();
            }
        }

        private void AddToRepository(object sender, RoutedEventArgs e)
        {          
          (new Repository.SharedRepositoryOperations()).AddItemsToRepository(mContext, grdVariables.Grid.SelectedItems.Cast<RepositoryItemBase>().ToList());         
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
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            else
                foreach (object var in grdVariables.Grid.SelectedItems)
                    ((VariableBase)var).ResetValue();
        }

        private void GenerateVariablesValue(object sender, RoutedEventArgs e)
        {
            if (grdVariables == null) return;

            if (grdVariables.Grid.SelectedItems.Count == 0)
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            else
                foreach (object var in grdVariables.Grid.SelectedItems)
                    ((VariableBase)var).GenerateAutoValue();
        }

        private void EditVar(object sender, RoutedEventArgs e)
        {
            if (grdVariables.CurrentItem != null && grdVariables.CurrentItem.ToString() != "{NewItemPlaceholder}")
            {
                VariableBase selectedVarb = (VariableBase)grdVariables.CurrentItem;
                selectedVarb.NameBeforeEdit = selectedVarb.Name;
                VariableEditPage.eEditMode mode = VariableEditPage.eEditMode.Default;
                switch (mVariablesLevel)// variable page is generic so need to change editmode as per type of variable.
                {
                    case eVariablesLevel.BusinessFlow:
                    case eVariablesLevel.Activity:
                        mode = VariableEditPage.eEditMode.Default;
                        break;

                    case eVariablesLevel.Solution:
                        mode = VariableEditPage.eEditMode.Global;
                        break;
                }            
                VariableEditPage w = new VariableEditPage(selectedVarb, mContext, false, mode);
                w.ShowAsWindow(eWindowShowStyle.Dialog);
                RefreshGrid(sender, e);

                if (selectedVarb.NameBeforeEdit != selectedVarb.Name)
                    UpdateVariableNameChange(selectedVarb);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectVariable);
            }
        }

        private void VariablesGrid_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            VariableBase selectedVarb = (VariableBase)grdVariables.CurrentItem;
            selectedVarb.NameBeforeEdit = selectedVarb.Name;
            VariableEditPage.eEditMode editMode = VariableEditPage.eEditMode.Default;
            if (mContext!=null && mContext.BusinessFlow == null)
            {
                editMode = VariableEditPage.eEditMode.SharedRepository;
            }
            if (mVariablesLevel == eVariablesLevel.Solution)
            {
                editMode = VariableEditPage.eEditMode.Global;
            }
            VariableEditPage w = new VariableEditPage(selectedVarb, mContext, false, editMode);
            w.ShowAsWindow(eWindowShowStyle.Dialog);

            if (selectedVarb.NameBeforeEdit != selectedVarb.Name)
                UpdateVariableNameChange(selectedVarb);
        }

        private void SaveSolutionConfigurations(object sender, RoutedEventArgs e)
        {
            ((Solution)mVariablesParentObj).SaveSolution(true, Solution.eSolutionItemToSave.GlobalVariabels);
        }
        
        private void AddVar(object sender, RoutedEventArgs e)
        {
            AddVariablePage addVarPage = new AddVariablePage(mVariablesLevel, (RepositoryItemBase)mVariablesParentObj,mContext);
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
            if (mEditMode != General.eRIPageViewMode.Automation
                && mVariablesLevel == eVariablesLevel.Activity && mVariablesParentObj != null)
            {
                ((Activity)mVariablesParentObj).RefreshVariablesNames();
            }
        }

        private void grdMain_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() == nameof(VariableBase.Name))
            {
                VariableBase selectedVarb = (VariableBase)grdVariables.CurrentItem;
                selectedVarb.NameBeforeEdit = selectedVarb.Name;
            }
        }

        private void grdMain_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == nameof(VariableBase.Name))
            {
                VariableBase selectedVarb = (VariableBase)grdVariables.CurrentItem;
                if (selectedVarb.Name != selectedVarb.NameBeforeEdit)
                    UpdateVariableNameChange(selectedVarb);
            }
        }

        public void UpdateVariableNameChange(VariableBase variable)
        {
            if (variable == null) return;

            Reporter.ToStatus(eStatusMsgKey.RenameItem, null, variable.NameBeforeEdit,variable.Name);
            try
            {
                switch (mVariablesLevel)
                {
                    case eVariablesLevel.Solution:
                        ObservableList<BusinessFlow> allBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                        foreach (BusinessFlow bfl in allBF)
                        {
                            bfl.SetUniqueVariableName(variable);
                            foreach (Activity activity in bfl.Activities)
                                foreach (Act action in activity.Acts)
                                {
                                    bool changedwasDone = false;
                                    VariableBase.UpdateVariableNameChangeInItem(action, variable.NameBeforeEdit, variable.Name, ref changedwasDone);
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
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occurred while renaming variable name", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }
    }
}
