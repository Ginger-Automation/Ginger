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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository.PlugInsLib;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.BusinessFlowPages.AddActionMenu;
using Ginger.UserControls;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for ActionsLibraryNavAction.xaml
    /// </summary>
    public partial class ActionsLibraryNavPage : Page, INavPanelPage
    {
        //GenericWindow pageGenericWin = null;
        ObservableList<IAct> mActionsList;
        // bool IsPlugInAvailable = false;
        Context mContext;

        public ActionsLibraryNavPage(Context context)
        {
            InitializeComponent();
            mContext = context;
            mActionsList = mContext.BusinessFlow.CurrentActivity.Acts;

            mContext.PropertyChanged += MContext_PropertyChanged;

            SetActionsGridsView();

            FillActionsList();

            Button addActionBtn = new Button();
            addActionBtn.Content = "Add Action";
            addActionBtn.Click += new RoutedEventHandler(AddActionButton_Click);
        }

        private void MContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.IsVisible && MainAddActionsNavigationPage.IsPanelExpanded)
            {
                if (e.PropertyName is nameof(mContext.Activity) || e.PropertyName is nameof(mContext.Target))
                {
                    FillActionsList();
                }
            }
        }

        private void FillActionsList()
        {
            LoadGridData();
            LoadPluginsActions();
            if (mContext.Activity != null)
            {
                mActionsList = mContext.Activity.Acts;
            }
            else
            {
                mActionsList = null;
            }
        }

        private void LoadPluginsActions()
        {
            ObservableList<PluginPackage> plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            ObservableList<Act> PlugInsActions = new ObservableList<Act>();
            foreach (PluginPackage pluginPackage in plugins)
            {
                try
                {
                    foreach (PluginServiceInfo pluginServiceInfo in pluginPackage.Services)
                    {
                        foreach (PluginServiceActionInfo pluginServiceAction in pluginServiceInfo.Actions)
                        {
                            ActPlugIn act = new ActPlugIn();
                            act.Description = pluginServiceAction.Description;
                            act.PluginId = pluginPackage.PluginId;
                            act.ServiceId = pluginServiceInfo.ServiceId;
                            act.ActionId = pluginServiceAction.ActionId;
                            foreach (var v in pluginServiceAction.InputValues)
                            {
                                if (v.Param == "GA") continue; // not needed
                                act.InputValues.Add(new ActInputValue() { Param = v.Param, ParamTypeEX = v.ParamTypeStr });
                            }
                            act.Active = true;
                            PlugInsActions.Add(act);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get the Action of the Plugin '" + pluginPackage.PluginId + "'", ex);
                }
            }

            PlugInsActionsGrid.DataSourceList = PlugInsActions;
        }

        private void LoadGridData()
        {
            ObservableList<Act> allActions = GetPlatformsActions();
            ObservableList<Act> platformActions = new ObservableList<Act>();
            ObservableList<Act> generalActions = new ObservableList<Act>();
            ObservableList<Act> LegacyActions = new ObservableList<Act>();

            if (allActions != null)
            {
                IEnumerable<Act> OrderedActions = allActions.OrderBy(x => x.Description);
                foreach (Act cA in OrderedActions)
                {
                    if (cA.LegacyActionPlatformsList.Intersect(WorkSpace.Instance.Solution.ApplicationPlatforms
                                                                    .Where(x => mContext.BusinessFlow.CurrentActivity.TargetApplication == x.AppName)
                                                                    .Select(x => x.Platform).ToList()).Any())
                    {
                        LegacyActions.Add(cA);
                    }
                    else if (cA.SupportedPlatforms == "All")
                    {
                        if ((cA is ActPlugIn) == false)
                            generalActions.Add(cA);
                    }
                    else
                    {
                        platformActions.Add(cA);
                    }
                }
            }

            //xPlatformActionsListView.DataSourceList = platformActions;
            PlatformActionsGrid.DataSourceList = platformActions;
            GeneralActionsGrid.DataSourceList = generalActions;
            LegacyActionsGrid.DataSourceList = LegacyActions;
        }

        private ObservableList<Act> GetPlatformsActions(bool ShowAll = false)
        {
            ObservableList<Act> Acts = new ObservableList<Act>();
            AppDomain.CurrentDomain.Load("GingerCore");
            AppDomain.CurrentDomain.Load("GingerCoreCommon");
            AppDomain.CurrentDomain.Load("GingerCoreNET");

            var ActTypes = new List<Type>();
            foreach (Assembly GC in AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.GetName().Name.Contains("GingerCore")))
            {
                var types = from type in GC.GetTypes() where type.IsSubclassOf(typeof(Act)) && type != typeof(ActWithoutDriver) select type;
                ActTypes.AddRange(types);
            }

            foreach (Type t in ActTypes)
            {
                Act a = (Act)Activator.CreateInstance(t);

                if (a.IsSelectableAction == false)
                    continue;

                if (mContext.BusinessFlow.CurrentActivity == null)
                {
                    return null;
                }
                TargetApplication TA = (TargetApplication)(from x in mContext.BusinessFlow.TargetApplications where x.Name == mContext.BusinessFlow.CurrentActivity.TargetApplication select x).FirstOrDefault();
                if (TA == null)
                {
                    if (mContext.BusinessFlow.TargetApplications.Count == 1)
                    {
                        TA = (TargetApplication)mContext.BusinessFlow.TargetApplications.FirstOrDefault();
                        mContext.BusinessFlow.CurrentActivity.TargetApplication = TA.AppName;
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.MissingActivityAppMapping);
                        return null;
                    }
                }
                ApplicationPlatform AP = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms where x.AppName == TA.AppName select x).FirstOrDefault();
                if (AP != null)
                {
                    if (a.Platforms.Contains(AP.Platform))
                    {
                        //DO Act.GetSampleAct in base
                        if ((Acts.Where(c => c.GetType() == a.GetType()).FirstOrDefault()) == null)
                        {
                            a.Description = a.ActionDescription;
                            a.Active = true;
                            Acts.Add(a);
                        }
                    }
                }
            }
            return Acts;
        }

        private void SetActionsGridsView()
        {
            //SetActionsListView(xPlatformActionsListView);
            SetActionsGridView(PlatformActionsGrid);
            SetActionsGridView(GeneralActionsGrid);
            SetActionsGridView(LegacyActionsGrid);
            SetActionsGridView(PlugInsActionsGrid);
        }

        private void SetActionsListView(UcListView xActionsListView)
        {
            xActionsListView.Title = "Actions";
            xActionsListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Action;

            ////TODO: move DataTemplate into ListView
            //DataTemplate dataTemp = new DataTemplate();
            //FrameworkElementFactory listItemFac = new FrameworkElementFactory(typeof(UcListViewItem));
            ////listItemFac.SetValue(UcListViewItem.ParentListProperty, xActionsListView);
            //listItemFac.SetBinding(UcListViewItem.ItemProperty, new Binding());
            //listItemFac.SetValue(UcListViewItem.ItemInfoProperty, new ActionListItemInfo(mContext));
            //dataTemp.VisualTree = listItemFac;
            //xActionsListView.List.ItemTemplate = dataTemp;

            xActionsListView.SetDefaultListDataTemplate(new ActionsListViewHelper(mContext, General.eRIPageViewMode.Automation));

            xActionsListView.DataSourceList = mContext.BusinessFlow.CurrentActivity.Acts;
            //xActionsListView.List.ItemsSource = mActivity.Acts;
        }

        private void SetActionsGridView(ucGrid actionsGrid)
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.Description, Header = "Action Type", AllowSorting = true, WidthWeight = 4 });

            if (actionsGrid == PlugInsActionsGrid)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(ActPlugIn.PluginId), Header = "Plugin ID", WidthWeight = 6, ReadOnly = true, BindingMode = BindingMode.OneWay });
                view.GridColsView.Add(new GridColView() { Field = nameof(ActPlugIn.ServiceId), Header = "Service ID", WidthWeight = 6, ReadOnly = true, BindingMode = BindingMode.OneWay });
            }
            else
            {
                view.GridColsView.Add(new GridColView() { Field = Act.Fields.SupportedPlatforms, Header = "Supported Platforms", WidthWeight = 6, ReadOnly = true });
            }
            actionsGrid.SetAllColumnsDefaultView(view);
            actionsGrid.InitViewItems();
            actionsGrid.grdMain.SelectionMode = DataGridSelectionMode.Extended;

            actionsGrid.AddToolbarTool(eImageType.GoBack, "Add to Actions", new RoutedEventHandler(AddMultipleActions));

            actionsGrid.RowDoubleClick += ActionsGrid_MouseDoubleClick;
        }

        private void AddMultipleActions(object sender, RoutedEventArgs e)
        {
            if (ActionsTabs.SelectedContent != null)
            {
                ucGrid actionsGrid = ((ucGrid) ActionsTabs.SelectedContent);
                if (actionsGrid.Grid.SelectedItems != null && actionsGrid.Grid.SelectedItems.Count > 0)
                {
                    foreach (Act selectedAct in actionsGrid.Grid.SelectedItems)
                    {
                        ActionsFactory.AddActionsHandler(selectedAct, mContext);
                    }
                }
                else
                    Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void AddAction()
        {
            if (ActionsTabs.SelectedContent != null && ((ucGrid)ActionsTabs.SelectedContent).CurrentItem != null)
            {
                Act selectedAction = ((ucGrid)ActionsTabs.SelectedContent).CurrentItem as Act;
                ActionsFactory.AddActionsHandler(selectedAction, mContext);
            }
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddAction();
            }
            catch (NullReferenceException)
            {
                //TODO: Enable adding new action in action grid after Run Flow
                //Fixes Bug 695. Prevents Ginger from crashing. 
            }
        }

        private void ActionsGrid_MouseDoubleClick(object sender, EventArgs e)
        {
            AddAction();
        }

        private void ActionsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            ShowSelectedActionDetails();
        }

        private void ActionsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ActionsTabs.SelectedItem != null)
                {
                    foreach (TabItem tab in ActionsTabs.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (ActionsTabs.SelectedItem == tab)
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
                Reporter.ToLog(eLogLevel.ERROR, "Error in PlugIn tabs style", ex);
            }
            ShowSelectedActionDetails();
        }

        private void Tab_GotFocus(object sender, RoutedEventArgs e)
        {
            ShowSelectedActionDetails();
        }

        private void ShowSelectedActionDetails()
        {
            ActDescriptionFrm.Content = null;
            if (ActionsTabs.SelectedContent != null)
            {
                if (ActionsTabs.SelectedContent.GetType() == typeof(UcListView))
                {
                    if (((UcListView)ActionsTabs.SelectedContent).CurrentItem != null)
                    {
                        Act a = (Act)(((UcListView)ActionsTabs.SelectedContent).CurrentItem);

                        ActDescriptionPage desPage = new ActDescriptionPage(a);
                        ActDescriptionFrm.Content = desPage;
                    }
                }
                else
                {
                    if (((ucGrid)ActionsTabs.SelectedContent).CurrentItem != null)
                    {
                        Act a = (Act)(((ucGrid)ActionsTabs.SelectedContent).CurrentItem);

                        ActDescriptionPage desPage = new ActDescriptionPage(a);
                        ActDescriptionFrm.Content = desPage;
                    }
                }
            }
        }

        static List<Type> AllActionType = null;
        List<Type> GetAllActionType()
        {
            if (AllActionType == null)
            {
                AllActionType = new List<Type>();
                List<Assembly> assemblies = new List<Assembly>();
                assemblies.Add(typeof(Act).Assembly); // add assembly of GingerCoreCommon
                assemblies.Add(typeof(RepositoryItem).Assembly); // add assembly of GingerCore
                                                                 // assemblies.Add(typeof(ActAgentManipulation).Assembly); // add assembly of GingerCoreNET  -- Getting laod exception

                var subclasses = from assembly in assemblies // not using AppDomain.CurrentDomain.GetAssemblies() because it checks in all assemblies and have load exception
                                 from type in assembly.GetTypes()
                                 where type.IsSubclassOf(typeof(Act)) && type != typeof(ActWithoutDriver) && type != typeof(ActPlugIn)
                                 select type;
                foreach (Type t in subclasses)
                {
                    AllActionType.Add(t);
                }

                // Adding manually from GingerCoreNET
                AllActionType.Add(typeof(ActAgentManipulation));
                AllActionType.Add(typeof(ActSetVariableValue));

                AllActionType = subclasses.ToList();
            }
            return AllActionType;
        }

        public void ReLoadPageItems()
        {
            FillActionsList();
        }
    }
}
