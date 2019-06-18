using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.SolutionGeneral;
using Ginger.UserControlsLib.UCListView;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Variables;
using GingerWPF.DragDropLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowPages
{
    /// <summary>
    /// Interaction logic for VariabelsListViewPage.xaml
    /// </summary>
    public partial class VariabelsListViewPage : Page
    {
        RepositoryItemBase mVariabelsParent;
        Context mContext;

        VariablesListHelper mVariabelListItemInfo;
        UcListView mVariabelsListView;
        VariableEditPage mVariabelEditPage;
        VariableBase mVarBeenEdit;

        public UcListView ListView
        {
            get { return mVariabelsListView; }
        }

        public VariabelsListViewPage(RepositoryItemBase variabelsParent, Context context)
        {
            InitializeComponent();

            mVariabelsParent = variabelsParent;
            mContext = context;

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

        private void ShowHideEditPage(VariableBase variabelToEdit)
        {
            if (variabelToEdit != null)
            {
                xBackToListPnl.Visibility = Visibility.Visible;
                mVarBeenEdit = variabelToEdit;
                mVarBeenEdit.NameBeforeEdit = mVarBeenEdit.Name;
                if (mVariabelsParent is Solution)
                {
                    mVariabelEditPage = new VariableEditPage(mVarBeenEdit, mContext, false, VariableEditPage.eEditMode.Global);
                }
                else if (mVariabelsParent is BusinessFlow)
                {
                    mVariabelEditPage = new VariableEditPage(mVarBeenEdit, mContext, false, VariableEditPage.eEditMode.BusinessFlow);
                }
                else if (mVariabelsParent is Activity)
                {
                    mVariabelEditPage = new VariableEditPage(mVarBeenEdit, mContext, false, VariableEditPage.eEditMode.Activity);
                }
                
                xMainFrame.Content = mVariabelEditPage;
            }
            else
            {
                xBackToListPnl.Visibility = Visibility.Collapsed;
                mVariabelEditPage = null;
                xMainFrame.Content = mVariabelsListView;
            }
        }

        private void SetListView()
        {
            mVariabelsListView = new UcListView();
            mVariabelsListView.Title = GingerDicser.GetTermResValue(eTermResKey.Variables);
            mVariabelsListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Variable;

            mVariabelListItemInfo = new VariablesListHelper(mContext);
            mVariabelListItemInfo.VariabelListItemEvent += MVariabelListItemInfo_VariabelListItemEvent;
            mVariabelsListView.SetDefaultListDataTemplate(mVariabelListItemInfo);

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
                if (mVariabelsParent != null)
                {
                    mVariabelsListView.DataSourceList = GetVariablesList();
                }
                else
                {
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
                UpdateVariableNameChange(mVarBeenEdit);

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
    }
}
