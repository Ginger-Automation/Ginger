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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.Common
{
    /// <summary>
    /// Interaction logic for MenuControlActionsPage.xaml
    /// </summary>
    public partial class MenuControlActionsPage : Page
    {
        ObservableList<Act> mActions;
        AutomationElement mAE;

        public MenuControlActionsPage(ObservableList<Act> Actions, AutomationElement AE)
        {
            InitializeComponent();
            mActions = Actions;
            mAE = AE;
            InitGrid();
            LoadMenuItems();
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            Act act = (Act)((Act)(mActions.CurrentItem)).CreateCopy();
            act.Active = true;
            //App.BusinessFlow.AddAct(act);
            ActionEditPage AEP = new ActionEditPage(act);
            AEP.ShowAsWindow();
        }

        private void InitGrid()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = Act.Fields.Description, WidthWeight = 10 });
            defView.GridColsView.Add(new GridColView() { Field = Act.Fields.LocateBy, WidthWeight = 10 });
            defView.GridColsView.Add(new GridColView() { Field = Act.Fields.LocateValue, WidthWeight = 10 });
            defView.GridColsView.Add(new GridColView() { Field = "Value", WidthWeight = 10 });

            AvailableControlActionsGrid.SetAllColumnsDefaultView(defView);
            AvailableControlActionsGrid.InitViewItems();

            AvailableControlActionsGrid.DataSourceList = mActions;
        }

        private void LoadMenuItems()
        {
            AutomationElementCollection menuList = null;
            try
            {
                CollapseMenu();
                Thread thread1 = new Thread(new ThreadStart(ExpandMenu));
                thread1.Start();
                System.Threading.Thread.Sleep(1500);

                PropertyCondition menuMatch = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "menu item");
                menuList = mAE.FindAll(TreeScope.Descendants, menuMatch);

                foreach (AutomationElement menuElement in menuList)
                {
                    MenuItemComboBox.Items.Add(menuElement.Current.Name);
                }
                MenuItemComboBox.SelectedIndex = 0;
                CollapseMenu();
            }
            catch (InvalidOperationException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                return;
            }
        }

        private void ExpandMenu()
        {
            try
            {
                ExpandCollapsePattern fileECPattern = mAE.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
                fileECPattern.Expand();
            }
            catch (InvalidOperationException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                return;
            }
        }

        private void CollapseMenu()
        {
            ExpandCollapsePattern fileECPattern = mAE.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
            fileECPattern.Collapse();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Act act = (Act)((Act)(mActions.CurrentItem)).CreateCopy();
            act.LocateValueCalculated = act.LocateValue;
            act.ValueForDriver = act.Value;
            act.Active = true;
            //TODO: remove hard coded selecting first agent
            //((Agent)App.AutomateTabGingerRunner.ApplicationAgents[0].Agent).RunAction(act);
        }

        private void MenuItemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string menuName = MenuItemComboBox.SelectedItem.ToString();
            if (!(String.IsNullOrEmpty(menuName)))
            {
                ActMenuItem actMenuAction_Click = new ActMenuItem()
                {
                    Description = "Click " + menuName + " Menu",
                    LocateBy = eLocateBy.ByName,
                    LocateValue = mAE.Current.Name + "|" + menuName,
                    MenuAction = ActMenuItem.eMenuAction.Click,
                };

                foreach (ActMenuItem act in mActions)
                {
                    if (act.MenuAction.Equals(ActMenuItem.eMenuAction.Click))
                    {
                        mActions.Remove(act);
                        break;
                    }
                }

                mActions.Add(actMenuAction_Click);
                AvailableControlActionsGrid.DataSourceList = mActions;
            }
        }
    }
}
