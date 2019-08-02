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
                    mVariabelEditPage = new VariableEditPage(mVarBeenEdit, mContext, showAsReadOnly, VariableEditPage.eEditMode.BusinessFlow);
                }
                else if (mVariabelsParent is Activity)
                {
                    mVariabelEditPage = new VariableEditPage(mVarBeenEdit, mContext, showAsReadOnly, VariableEditPage.eEditMode.Activity);
                }                
                xMainFrame.Content = mVariabelEditPage;
            }
            else
            {
                xBackToListGrid.Visibility = Visibility.Collapsed;
                mVariabelEditPage = null;
                xMainFrame.Content = mVariabelsListView;
            }
        }

        private void SetListView()
        {
            mVariabelsListView = new UcListView();
            mVariabelsListView.Title = GingerDicser.GetTermResValue(eTermResKey.Variables);
            mVariabelsListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Variable;

            mVariabelListHelper = new VariablesListViewHelper(GetVariablesList(), mVariabelsParent, mVariablesLevel, mContext, mPageViewMode);
            mVariabelListHelper.VariabelListItemEvent += MVariabelListItemInfo_VariabelListItemEvent;
            mVariabelsListView.SetDefaultListDataTemplate(mVariabelListHelper);

            mVariabelsListView.ListSelectionMode = SelectionMode.Extended;
            mVariabelsListView.DataSourceList = GetVariablesList();

            mVariabelsListView.PreviewDragItem += ListVars_PreviewDragItem;
            mVariabelsListView.ItemDropped += ListVars_ItemDropped;

            mVariabelsListView.List.MouseDoubleClick += VariabelsListView_MouseDoubleClick;
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
                if (mVariabelsParent != null)
                {
                    mVariabelListHelper.VariablesParent = mVariabelsParent;
                    mVariabelListHelper.VariablesLevel = mVariablesLevel;
                    mVariabelListHelper.Variables = GetVariablesList();
                    mVariabelsListView.DataSourceList = GetVariablesList();
                }
                else
                {
                    mVariabelListHelper.VariablesParent = null;
                    mVariabelListHelper.Variables = null;
                    mVariabelsListView.DataSourceList = null;
                }
                ShowHideEditPage(null);
            }
        }

        // Drag Drop handlers
        private void ListVars_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(VariableBase)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }
        }

        private void ListVars_ItemDropped(object sender, EventArgs e)
        {
            VariableBase a = (VariableBase)((DragInfo)sender).Data;
            VariableBase instance = (VariableBase)a.CreateInstance(true);
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
