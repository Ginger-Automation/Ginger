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
using Amdocs.Ginger.UserControls;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.Repository;
using Ginger.SolutionGeneral;
using Ginger.UserControlsLib.UCListView;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using GingerWPF.DragDropLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.BusinessFlowPages
{
    /// <summary>
    /// Interaction logic for VariabelsListViewPage.xaml
    /// </summary>
    public partial class VariabelsListViewPage : Page
    {
        RepositoryItemBase mVariabelsParent;
        eVariablesLevel mVariablesLevel;
        Context mContext;
        General.eRIPageViewMode mPageViewMode;

        VariablesListViewHelper mVariabelListHelper;
        UcListView mVariabelsListView;
        VariableEditPage mVariabelEditPage;
        VariableBase mVarBeenEdit;

        public UcListView ListView
        {
            get { return mVariabelsListView; }
        }

        public VariabelsListViewPage(RepositoryItemBase variabelsParent, Context context, General.eRIPageViewMode pageViewMode)
        {
            InitializeComponent();

            mVariabelsParent = variabelsParent;
            mVariablesLevel = GetVariablesLevel();
            mContext = context;
            mPageViewMode = pageViewMode;

            SetListView();
            ShowHideEditPage(null);
        }

        private ObservableList<VariableBase> GetVariablesList()
        {
            if (mVariabelsParent is Solution)
            {
                return ((Solution)mVariabelsParent).Variables;
            }
            else if (mVariabelsParent is BusinessFlow)
            {
                return ((BusinessFlow)mVariabelsParent).Variables;
            }
            else if (mVariabelsParent is Activity)
            {
                return ((Activity)mVariabelsParent).Variables;
            }
            else
            {
                return null; 
            }
        }

        private eVariablesLevel GetVariablesLevel()
        {
            if (mVariabelsParent is Solution)
            {
                return eVariablesLevel.Solution;
            }
            else if (mVariabelsParent is BusinessFlow)
            {
                return eVariablesLevel.BusinessFlow;
            }
            else if (mVariabelsParent is Activity)
            {
                return eVariablesLevel.Activity;
            }
            else
            {
                return eVariablesLevel.Activity; 
            }
        }

        private void ShowHideEditPage(VariableBase variabelToEdit)
        {
            if (variabelToEdit != null)
            {
                xBackToListGrid.Visibility = Visibility.Visible;
                mVarBeenEdit = variabelToEdit;
                BindingHandler.ObjFieldBinding(xSelectedItemTitleText, TextBlock.TextProperty, mVarBeenEdit, nameof(VariableBase.Name));
                BindingHandler.ObjFieldBinding(xSelectedItemTitleText, TextBlock.ToolTipProperty, mVarBeenEdit, nameof(VariableBase.Name));
                                
                bool showAsReadOnly = false;
                if (mPageViewMode == General.eRIPageViewMode.View)
                {
                    showAsReadOnly = true;
                    xEditAndValueChangeOperationsPnl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xEditAndValueChangeOperationsPnl.Visibility = Visibility.Visible;
                    BindingHandler.ObjFieldBinding(xResetValueBtn, ucButton.VisibilityProperty, mVarBeenEdit, nameof(VariableBase.SupportResetValue), bindingConvertor: new BooleanToVisibilityConverter(), BindingMode.OneWay);
                    BindingHandler.ObjFieldBinding(xAutoValueBtn, ucButton.VisibilityProperty, mVarBeenEdit, nameof(VariableBase.SupportAutoValue), bindingConvertor: new BooleanToVisibilityConverter(), BindingMode.OneWay);
                    mVarBeenEdit.NameBeforeEdit = mVarBeenEdit.Name;
                    mVarBeenEdit.SaveBackup();
                }

                if (mVariabelsParent is Solution)
                {
                    mVariabelEditPage = new VariableEditPage(mVarBeenEdit, mContext, showAsReadOnly, VariableEditPage.eEditMode.Global);
                }
                else if (mVariabelsParent is BusinessFlow)
                {
                    mVariabelEditPage = new VariableEditPage(mVarBeenEdit, mContext, showAsReadOnly, VariableEditPage.eEditMode.Default);
                }
                else if (mVariabelsParent is Activity)
                {
                    if(mPageViewMode== General.eRIPageViewMode.View)
                    {
                        mVariabelEditPage = new VariableEditPage(mVarBeenEdit, mContext, showAsReadOnly, VariableEditPage.eEditMode.View);
                    }
                    else
                    {
                        mVariabelEditPage = new VariableEditPage(mVarBeenEdit, mContext, showAsReadOnly, VariableEditPage.eEditMode.Default);
                    }
                }                
                xMainFrame.SetContent(mVariabelEditPage);
            }
            else
            {
                xBackToListGrid.Visibility = Visibility.Collapsed;
                mVariabelEditPage = null;
                xMainFrame.SetContent(mVariabelsListView);
            }
        }

        //private void ClearListViewBindings()
        //{
        //    if (mVariabelListHelper != null)
        //    {
        //        mVariabelListHelper.VariabelListItemEvent -= MVariabelListItemInfo_VariabelListItemEvent;
        //        mVariabelListHelper = null;
        //    }

        //    if (mVariabelsListView != null)
        //    {
        //        mVariabelsListView.PreviewDragItem -= ListVars_PreviewDragItem;
        //        mVariabelsListView.ItemDropped -= ListVars_ItemDropped;
        //        mVariabelsListView.List.MouseDoubleClick -= VariabelsListView_MouseDoubleClick;
        //        mVariabelsListView.ClearBindings();
        //        mVariabelsListView.DataSourceList = null;
        //        mVariabelsListView = null;
        //    }
        //}

        //public void ClearBindings()
        //{
        //    xMainFrame.Content = null;
        //    xMainFrame.NavigationService.RemoveBackEntry();

        //    ClearListViewBindings();

        //    BindingOperations.ClearAllBindings(xSelectedItemTitleText);
        //    BindingOperations.ClearAllBindings(xResetValueBtn);
        //    BindingOperations.ClearAllBindings(xAutoValueBtn);
        //    this.ClearControlsBindings();            
        //}

        private void SetListView()
        {
            if (mVariabelsListView == null)
            {
                mVariabelsListView = new UcListView();
                mVariabelsListView.Title = GingerDicser.GetTermResValue(eTermResKey.Variables);
                mVariabelsListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Variable;

                mVariabelListHelper = new VariablesListViewHelper(GetVariablesList(), mVariabelsParent, mVariablesLevel, mContext, mPageViewMode);
                mVariabelListHelper.VariabelListItemEvent += MVariabelListItemInfo_VariabelListItemEvent;
                mVariabelsListView.SetDefaultListDataTemplate(mVariabelListHelper);

                mVariabelsListView.ListSelectionMode = SelectionMode.Extended;
                
                mVariabelsListView.PreviewDragItem += ListVars_PreviewDragItem;
                mVariabelsListView.ItemDropped += ListVars_ItemDropped;

                mVariabelsListView.List.MouseDoubleClick += VariabelsListView_MouseDoubleClick;

                mVariabelsListView.List.SetValue(ScrollViewer.CanContentScrollProperty, true);

                if (mPageViewMode == Ginger.General.eRIPageViewMode.View)
                {
                    mVariabelsListView.IsDragDropCompatible = false;
                }
            }

            if (mVariabelsParent != null)
            {
                mVariabelListHelper.VariablesParent = mVariabelsParent;
                mVariabelListHelper.VariablesLevel = mVariablesLevel;
                mVariabelListHelper.Variables = GetVariablesList(); 
                mVariabelsListView.DataSourceList = GetVariablesList();
                if (mVariablesLevel != eVariablesLevel.Solution)
                {
                    SharedRepositoryOperations.MarkSharedRepositoryItems(GetVariablesList(), WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>());
                }
            }
            else
            {
                mVariabelListHelper.VariablesParent = null;
                mVariabelListHelper.Variables = null;
                mVariabelsListView.DataSourceList = null;
            }
        }

        private void MVariabelListItemInfo_VariabelListItemEvent(VariabelListItemEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
            {
                case VariabelListItemEventArgs.eEventType.ShowVariabelEditPage:
                    ShowHideEditPage((VariableBase)EventArgs.EventObject);
                    break;
            }
        }

        private void VariabelsListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (mVariabelsListView.CurrentItem != null)
            {
                ShowHideEditPage((VariableBase)mVariabelsListView.CurrentItem);
            }
        }

        public void UpdateParent(RepositoryItemBase parent)
        {
            if (mVariabelsParent != parent)
            {
                mVariabelsParent = parent;
                mVariablesLevel = GetVariablesLevel();                
                SetListView();                
                ShowHideEditPage(null);
            }
        }

        // Drag Drop handlers
        private void ListVars_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(VariableBase), true))
            {
                if(DragDrop2.DrgInfo.Data is ObservableList<RepositoryItemBase>)
                {
                    DragDrop2.SetDragIcon(true, true);
                }
                else
                {
                    DragDrop2.SetDragIcon(true);
                }
                // OK to drop
            }
            else
            {
                // Do Not Drop
                DragDrop2.SetDragIcon(false);
            }
        }

        private void ListVars_ItemDropped(object sender, EventArgs e)
        {
            object droppedItem = ((DragInfo)sender).Data as object;

            if (droppedItem != null)
            {
                VariableBase droppedAtVar = DragDrop2.GetRepositoryItemHit(ListView) as VariableBase;

                VariableBase varDropped = droppedItem as VariableBase;

                VariableBase instance = (VariableBase)varDropped.CreateInstance(true);

                if (droppedAtVar != null)
                {
                    int targetIndex = GetVariablesList().IndexOf(droppedAtVar);

                    GetVariablesList().Insert(targetIndex, instance);
                    ListView.xListView.SelectedItem = instance;
                }
                else
                {
                    GetVariablesList().Add(instance);

                    int selectedActIndex = -1;
                    if (GetVariablesList().CurrentItem != null)
                    {
                        selectedActIndex = GetVariablesList().IndexOf((VariableBase)GetVariablesList().CurrentItem);
                    }
                    if (selectedActIndex >= 0)
                    {
                        GetVariablesList().Move(GetVariablesList().Count - 1, selectedActIndex + 1);
                    }
                }
            }
        }

        private void xGoToList_Click(object sender, RoutedEventArgs e)
        {
            if (mVarBeenEdit.NameBeforeEdit != mVarBeenEdit.Name)
            {
                UpdateVariableNameChange(mVarBeenEdit);
            }

            ShowHideEditPage(null);
        }

        public void UpdateVariableNameChange(VariableBase variable)
        {
            if (variable == null) return;

            if (mVariabelsParent is Solution)
            {
                ObservableList<BusinessFlow> allBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                foreach (BusinessFlow bfl in allBF)
                {
                    bfl.SetUniqueVariableName(variable);
                    foreach (Activity activity in bfl.Activities)
                    {
                        foreach (Act action in activity.Acts)
                        {
                            bool changedwasDone = false;
                            VariableBase.UpdateVariableNameChangeInItem(action, variable.NameBeforeEdit, variable.Name, ref changedwasDone);
                        }
                    }
                }
            }
            else if (mVariabelsParent is BusinessFlow)
            {
                BusinessFlow bf = (BusinessFlow)mVariabelsParent;
                bf.SetUniqueVariableName(variable);
                foreach (Activity activity in bf.Activities)
                {
                    foreach (Act action in activity.Acts)
                    {
                        bool changedwasDone = false;
                        VariableBase.UpdateVariableNameChangeInItem(action, variable.NameBeforeEdit, variable.Name, ref changedwasDone);
                    }
                }
            }
            else if (mVariabelsParent is Activity)
            {
                Activity activ = (Activity)mVariabelsParent;
                activ.SetUniqueVariableName(variable);
                foreach (Act action in activ.Acts)
                {
                    bool changedwasDone = false;
                    VariableBase.UpdateVariableNameChangeInItem(action, variable.NameBeforeEdit, variable.Name, ref changedwasDone);
                }
            }
            variable.NameBeforeEdit = variable.Name;
        }

        private void xPreviousBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mVariabelsListView.List.Items.CurrentPosition >= 1)
            {
                mVariabelsListView.List.Items.MoveCurrentToPrevious();
                ShowHideEditPage((VariableBase)mVariabelsListView.List.Items.CurrentItem);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, string.Format( "No {0} to move to.", GingerDicser.GetTermResValue(eTermResKey.Variable)));
            }
        }

        private void xNextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mVariabelsListView.List.Items.CurrentPosition < mVariabelsListView.List.Items.Count - 1)
            {
                mVariabelsListView.List.Items.MoveCurrentToNext();
                ShowHideEditPage((VariableBase)mVariabelsListView.List.Items.CurrentItem);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, string.Format("No {0} to move to.", GingerDicser.GetTermResValue(eTermResKey.Variable)));
            }
        }

        private void XUndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Ginger.General.UndoChangesInRepositoryItem(mVarBeenEdit, true))
            {
                mVarBeenEdit.SaveBackup();
            }
        }

        private void xDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SureWantToDelete, mVarBeenEdit.Name) == eUserMsgSelection.Yes)
            {
                GetVariablesList().Remove(mVarBeenEdit);
                if (mVariabelsListView.List.Items.CurrentItem != null)
                {
                    ShowHideEditPage((VariableBase)mVariabelsListView.List.Items.CurrentItem);
                }
                else
                {
                    ShowHideEditPage(null);
                }
            }
        }

        private void xResetValueBtn_Click(object sender, RoutedEventArgs e)
        {
            mVarBeenEdit.ResetValue();
        }

        private void xAutoValueBtn_Click(object sender, RoutedEventArgs e)
        {
            mVarBeenEdit.GenerateAutoValue();
        }
    }
}
