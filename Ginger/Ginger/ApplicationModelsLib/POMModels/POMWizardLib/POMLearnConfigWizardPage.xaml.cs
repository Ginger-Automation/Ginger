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
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Actions.Locators.ASCF;
using Ginger.Agents;
using Ginger.Drivers.PowerBuilder;
using Ginger.Drivers.Windows;
using Ginger.UserControls;
using Ginger.WindowExplorer;
using Ginger.WindowExplorer.Android;
using Ginger.WindowExplorer.Appium;
using Ginger.WindowExplorer.HTMLCommon;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.Mainframe;
using GingerCore;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Platforms;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using static GingerCore.Agent;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for LearnWizardPage.xaml
    /// </summary>
    public partial class POMLearnConfigWizardPage : Page, IWizardPage
    {
        private AddPOMWizard mWizard;
        ObservableList<UIElementFilter> FilteringCreteriaList = new ObservableList<UIElementFilter>();

        public POMLearnConfigWizardPage()
        {
            InitializeComponent();           
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    ObservableList<Agent> optionalAgentsList = GingerCore.General.ConvertListToObservableList((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Platform == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web select x).ToList());
                    xAgentControlUC.Init(optionalAgentsList);
                    App.ObjFieldBinding(xAgentControlUC, ucAgentControl.SelectedAgentProperty, mWizard, nameof(mWizard.Agent));


                    SetControlsGridView();
                    xFilterElementsGrid.DataSourceList = FilteringCreteriaList;                                 
                    break;

                case EventType.LeavingForNextPage:
                    mWizard.CheckedFilteringCreteriaList = GingerCore.General.ConvertListToObservableList(FilteringCreteriaList.Where(x=>x.Selected).ToList());                   
                    break;
            }
        }

        private void SetControlsGridView()
        {
            xFilterElementsGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Elements", new RoutedEventHandler(CheckUnCheckAllElements));
            xFilterElementsGrid.ShowTitle = Visibility.Collapsed;

            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.Selected), Header = "Selected", WidthWeight = 10, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.CheckBox});           
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementType), Header = "Element Type", WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(UIElementFilter.ElementExtraInfo), Header = "Element Extra Info", WidthWeight = 100 });

            xFilterElementsGrid.SetAllColumnsDefaultView(view);
            xFilterElementsGrid.InitViewItems();
        }

        private void CheckUnCheckAllElements(object sender, RoutedEventArgs e)
        {
            IObservableList filteringCriteriaList = xFilterElementsGrid.DataSourceList;
            int selectedItems = CountSelectedItems();
            if (selectedItems < xFilterElementsGrid.DataSourceList.Count)
                foreach (UIElementFilter UIEFActual in filteringCriteriaList)
                    UIEFActual.Selected = true;
            else if (selectedItems == xFilterElementsGrid.DataSourceList.Count)
                foreach (UIElementFilter UIEFActual in filteringCriteriaList)
                    UIEFActual.Selected = false;
            xFilterElementsGrid.DataSourceList = filteringCriteriaList;
        }

        private int CountSelectedItems()
        {
            int counter = 0;
            foreach (UIElementFilter UIEFActual in xFilterElementsGrid.DataSourceList)
            {
                if (UIEFActual.Selected)
                    counter++;
            }
            return counter;
        }
    }
}
