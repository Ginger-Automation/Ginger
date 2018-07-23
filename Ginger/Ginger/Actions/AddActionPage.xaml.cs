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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using GingerCore;
using GingerCore.Actions;
using Ginger.UserControls;
using System.Reflection;
using GingerCore.Platforms;
using GingerCore.Actions.PlugIns;
using GingerPlugIns.ActionsLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for AddActionPage.xaml
    /// </summary>
    public partial class AddActionPage : Page
    {
        GenericWindow _pageGenericWin = null;
        ObservableList<Act> mActionsList;
        bool IsPlugInAvailable = false;

        public AddActionPage()
        {
            InitializeComponent();
            SetActionsGridsView();
            LoadGridData();
            LoadPlugInsActions();

            if (IsPlugInAvailable == false)
            {
                PlugInsActionsTab.Visibility = Visibility.Collapsed;
                LegacyActionsTab.Margin = new Thickness(9,0,-18,0);
            }
        }

        private void LoadPlugInsActions()
        {
            ObservableList<Act> PlugInsActions = new ObservableList<Act>();
            ObservableList<PlugInWrapper> PlugInsList = App.LocalRepository.GetSolutionPlugIns();
            foreach (PlugInWrapper PW in PlugInsList)
            {
                try
                {
                    if (PW.Actions != null && PW.Actions.Count > 0)
                        foreach (PlugInAction PIA in PW.Actions)
                        {
                            ActPlugIn act = new ActPlugIn();
                            act.Description = PIA.Description;
                            act.Active = true;
                            act.PlugInName = PW.Name;
                            act.GetOrCreateInputParam(ActPlugIn.Fields.PluginID, PW.ID);
                            act.GetOrCreateInputParam(ActPlugIn.Fields.PlugInActionID, PIA.ID);
                            act.GetOrCreateInputParam(ActPlugIn.Fields.PluginDescription, PIA.Description);
                            act.GetOrCreateInputParam(ActPlugIn.Fields.PluginUserDescription, PIA.UserDescription);
                            act.GetOrCreateInputParam(ActPlugIn.Fields.PluginUserRecommendedUseCase, PIA.UserRecommendedUseCase);

                            PlugInsActions.Add(act);
                        }
                }
                catch(Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get the Action of the Plugin '" + PW.Name + "'", ex);
                }
            }
            if (PlugInsActions.Count > 0)
            {
                IsPlugInAvailable = true;
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
                    if (cA.IsLegacyAction)
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

            var ActTypes =
                from type in typeof(Act).Assembly.GetTypes()
                where type.IsSubclassOf(typeof(Act))
                && type != typeof(ActWithoutDriver)
                select type;
            
            foreach (Type t in ActTypes)
            {
                Act a = (Act)Activator.CreateInstance(t);

                if (a.IsSelectableAction == false) 
                    continue;

                TargetApplication TA = (from x in App.BusinessFlow.TargetApplications where x.AppName == App.BusinessFlow.CurrentActivity.TargetApplication select x).FirstOrDefault();
                if (TA == null)
                {
                    if (App.BusinessFlow.TargetApplications.Count == 1)
                    {
                        TA = App.BusinessFlow.TargetApplications.FirstOrDefault();
                        App.BusinessFlow.CurrentActivity.TargetApplication = TA.AppName;
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKeys.MissingActivityAppMapping);
                        return null;
                    }
                }
                ApplicationPlatform AP = (from x in App.UserProfile.Solution.ApplicationPlatforms where x.AppName == TA.AppName select x).FirstOrDefault();
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
                view.GridColsView.Add(new GridColView() { Field = nameof (ActPlugIn.PlugInName), Header = "Owner PlugIn", WidthWeight = 6, ReadOnly = true , BindingMode = BindingMode.OneWay});
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

                    WizardBase wizard = (GingerWPF.WizardLib.WizardBase)Activator.CreateInstance(t);
                    WizardWindow.ShowWizard(wizard);
                }
                else
                {
                    Act aNew = null;

                    if (ActionsTabs.SelectedContent != null && ((ucGrid)ActionsTabs.SelectedContent).CurrentItem != null)
                    {
                        aNew = (Act)(((Act)(((ucGrid)ActionsTabs.SelectedContent).CurrentItem)).CreateCopy());
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
                        return;
                    }
                    aNew.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();

                    //adding the new act after the selected action in the grid  
                    //TODO: Add should be after the last, Insert should be in the middle...

                    

                    int selectedActIndex = -1;
                    if (mActionsList.CurrentItem != null)
                    {
                        selectedActIndex = mActionsList.IndexOf((Act)mActionsList.CurrentItem);
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
        public void ShowAsWindow(ObservableList<Act> ActionsList, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
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
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("@Skin1_ColorB");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("@Skin1_ColorA");

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
    }
}
