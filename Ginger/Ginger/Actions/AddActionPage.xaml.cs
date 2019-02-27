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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.PlugInsLib;
using Amdocs.Ginger.Common.Repository.TargetLib;
using Amdocs.Ginger.Repository;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for AddActionPage.xaml
    /// </summary>
    public partial class AddActionPage : Page
    {
        BusinessFlow mBusinessFlow;
        GenericWindow _pageGenericWin = null;
        ObservableList<IAct> mActionsList;
        // bool IsPlugInAvailable = false;
        Context mContext;
        
        public AddActionPage(Context context)
        {
            InitializeComponent();

            mContext = context;
            mBusinessFlow = context.BusinessFlow;

            SetActionsGridsView();
            LoadGridData();
            LoadPluginsActions();

            //if (IsPlugInAvailable == false)
            //{
            //    PlugInsActionsTab.Visibility = Visibility.Collapsed;
            //    LegacyActionsTab.Margin = new Thickness(9,0,-18,0);
            //}
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
                catch(Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get the Action of the Plugin '" + pluginPackage.PluginId + "'", ex);
                }
            }
          
            PlugInsActionsGrid.DataSourceList = PlugInsActions;
        }

        private void LoadGridData()
        {
            ObservableList<Act> allActions = GetPlatformsActions();
            ObservableList<Act> platformActions=new ObservableList<Act>();
            ObservableList<Act> generalActions = new ObservableList<Act>();
            ObservableList<Act> LegacyActions = new ObservableList<Act>();

            if (allActions != null)
            {
                IEnumerable<Act> OrderedActions = allActions.OrderBy(x => x.Description);
                foreach (Act cA in OrderedActions)
                {
                    if (cA.LegacyActionPlatformsList.Intersect( WorkSpace.UserProfile.Solution.ApplicationPlatforms
                                                                    .Where(x => mContext.Activity.TargetApplication == x.AppName)
                                                                    .Select(x => x.Platform).ToList()).Any())
                    {
                        LegacyActions.Add(cA);
                    }
                    else if (cA.SupportedPlatforms == "All")
                    {
                        if((cA is ActPlugIn) == false)
                            generalActions.Add(cA);
                    }
                    else
                    {
                        platformActions.Add(cA);
                    }
                }
            }

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

            ObservableList<TargetBase> targetApplications;
            if (mBusinessFlow != null)
            {
                targetApplications = mBusinessFlow.TargetApplications;
            }
            else
            {
                targetApplications = WorkSpace.UserProfile.Solution.GetSolutionTargetApplications();
            }
            TargetApplication targetApp = (TargetApplication)(from x in targetApplications where x.Name == mContext.Activity.TargetApplication select x).FirstOrDefault();
            if (targetApp == null)
            {
                if (targetApplications.Count == 1)
                {
                    targetApp = (TargetApplication)targetApplications.FirstOrDefault();
                    mContext.Activity.TargetApplication = targetApp.AppName;
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.MissingActivityAppMapping);
                    return null;
                }
            }
            ApplicationPlatform appPlatform = (from x in WorkSpace.UserProfile.Solution.ApplicationPlatforms where x.AppName == targetApp.AppName select x).FirstOrDefault();

            foreach (Type t in ActTypes)
            {
                Act action = (Act)Activator.CreateInstance(t);

                if (action.IsSelectableAction == false) 
                    continue;

                if (appPlatform != null)
                {
                    if (action.Platforms.Contains(appPlatform.Platform))
                    {
                        //DO Act.GetSampleAct in base
                        if ((Acts.Where(c => c.GetType() == action.GetType()).FirstOrDefault()) == null)
                        {
                            action.Description = action.ActionDescription;
                            action.Active = true;
                            Acts.Add(action);
                        }
                    }
                }
            }
            return Acts;
        }

        private void SetActionsGridsView()
        {
            SetActionsGridView(PlatformActionsGrid);
            SetActionsGridView(GeneralActionsGrid);
            SetActionsGridView(LegacyActionsGrid);
            SetActionsGridView(PlugInsActionsGrid);
        }

        private void SetActionsGridView(ucGrid actionsGrid)
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = Act.Fields.Description, Header = "Action Type",AllowSorting = true, WidthWeight = 4});

            if (actionsGrid == PlugInsActionsGrid)
            {
                view.GridColsView.Add(new GridColView() { Field = nameof(ActPlugIn.PluginId), Header = "Plugin ID", WidthWeight = 6, ReadOnly = true, BindingMode = BindingMode.OneWay });
                view.GridColsView.Add(new GridColView() { Field = nameof (ActPlugIn.ServiceId), Header = "Service ID", WidthWeight = 6, ReadOnly = true , BindingMode = BindingMode.OneWay});
            }
            else
            {
                view.GridColsView.Add(new GridColView() { Field = Act.Fields.SupportedPlatforms, Header = "Supported Platforms", WidthWeight = 6, ReadOnly = true });
            }
            actionsGrid.SetAllColumnsDefaultView(view);
            actionsGrid.InitViewItems();
            actionsGrid.grdMain.SelectionMode = DataGridSelectionMode.Single;

            actionsGrid.RowDoubleClick += ActionsGrid_MouseDoubleClick;
        }

        private void AddAction()
        {
            if(ActionsTabs.SelectedContent != null && ((ucGrid)ActionsTabs.SelectedContent).CurrentItem != null)
            {
                if(((Act)(((ucGrid)ActionsTabs.SelectedContent).CurrentItem)).AddActionWizardPage != null)
                {
                    _pageGenericWin.Close();
                    string classname = ((Act)(((ucGrid)ActionsTabs.SelectedContent).CurrentItem)).AddActionWizardPage;
                    Type t = Assembly.GetExecutingAssembly().GetType(classname);
                    if (t == null)
                    {
                        throw new Exception("Action edit page not found - " + classname);
                    }                    

                    WizardBase wizard = (GingerWPF.WizardLib.WizardBase)Activator.CreateInstance(t, mContext);
                    WizardWindow.ShowWizard(wizard);
                }
                else
                {
                    Act aNew = null;

                    if (ActionsTabs.SelectedContent != null && ((ucGrid)ActionsTabs.SelectedContent).CurrentItem != null)
                    {
                        Act selectedAction = (Act)(((ucGrid)ActionsTabs.SelectedContent).CurrentItem);
                        aNew = (Act)selectedAction.CreateCopy();
                        aNew.Context = mContext;
                        // copy param ex info
                        for (int i=0;i< selectedAction.InputValues.Count;i++)
                        {
                            aNew.InputValues[i].ParamTypeEX = selectedAction.InputValues[i].ParamTypeEX;
                        }
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                        return;
                    }
                    aNew.SolutionFolder =  WorkSpace.UserProfile.Solution.Folder.ToUpper();
                    
                    //adding the new act after the selected action in the grid  
                    //TODO: Add should be after the last, Insert should be in the middle...

                    

                    int selectedActIndex = -1;
                    if (mActionsList.CurrentItem != null)
                    {
                        selectedActIndex = mActionsList.IndexOf((IAct)mActionsList.CurrentItem);
                    }
                    mActionsList.Add(aNew);
                    if (selectedActIndex >= 0)
                    {
                        mActionsList.Move(mActionsList.Count - 1, selectedActIndex + 1);
                    }

                    _pageGenericWin.Close();

                    //allowing to edit the action
                    ActionEditPage actedit = new ActionEditPage(aNew);
                    actedit.ShowAsWindow();

                    if (aNew is ActPlugIn)
                    {
                        ActPlugIn p = (ActPlugIn)aNew;
                        // TODO: add per group or... !!!!!!!!!

                        //Check if target already exist else add it
                        // TODO: search only in targetplugin type
                        TargetPlugin targetPlugin = (TargetPlugin)(from x in mBusinessFlow.TargetApplications where x.Name == p.ServiceId select x).SingleOrDefault();
                        if (targetPlugin == null)
                        {
                            // check if interface add it
                            // App.BusinessFlow.TargetApplications.Add(new TargetPlugin() { AppName = p.ServiceId });

                            mBusinessFlow.TargetApplications.Add(new TargetPlugin() {PluginId = p.PluginId,  ServiceId = p.ServiceId });

                            //Search for default agent which match 
                            App.AutomateTabGingerRunner.UpdateApplicationAgents();
                            // TODO: update automate page target/agent

                            // if agent not found auto add or ask user 
                        }

                    }
                    
                }
            }
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddAction();
            }
            catch(NullReferenceException)
            {
                //TODO: Enable adding new action in action grid after Run Flow
                //Fixes Bug 695. Prevents Ginger from crashing. 
            }
        }

        private void ActionsGrid_MouseDoubleClick(object sender, EventArgs e)
        {
            AddAction();
        }

        /// <summary>
        /// Open window to user to select an action
        /// User will be able to Edit the action properties after clicking add
        /// Add the selected action to ActionsList
        /// </summary>
        /// <param name="ActionsList"></param>
        /// <param name="windowStyle"></param>
        public void ShowAsWindow(ObservableList<IAct> ActionsList, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            mActionsList = ActionsList;

            Button addActionBtn = new Button();
            addActionBtn.Content = "Add Action";
            addActionBtn.Click += new RoutedEventHandler(AddActionButton_Click);
            
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { addActionBtn });
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
                if (((ucGrid)ActionsTabs.SelectedContent).CurrentItem != null)
                {
                    Act a = (Act)(((ucGrid)ActionsTabs.SelectedContent).CurrentItem);

                    ActDescriptionPage desPage = new ActDescriptionPage(a);
                    ActDescriptionFrm.Content = desPage;
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
                foreach(Type t in subclasses)
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

    }
}
