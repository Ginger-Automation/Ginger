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

using System;
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.FlowControlLib;
using Ginger.Run;

namespace Ginger.Actions.UserControls
{
    /// <summary>
    /// Interaction logic for UCFlowControlAction.xaml
    /// </summary>
    public partial class UCFlowControlAction : UserControl
    {
        FlowControl FC;
        BusinessFlow mActParentBusinessFlow=null;
        Activity mActParentActivity=null;
        GingerRunner mBfParentRunner = null;

        public UCFlowControlAction()
        {
            InitializeComponent();
            this.DataContextChanged += UCFlowControlAction_DataContextChanged;
        }

        public static DependencyProperty ActParentBusinessFlowProperty =
            DependencyProperty.Register("mActParentBusinessFlow", typeof(BusinessFlow), typeof(UCFlowControlAction), new PropertyMetadata(OnActParentBusinessFlowPropertyChanged));
        private static void OnActParentBusinessFlowPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCFlowControlAction;
            if (control != null)
                control.OnActParentBusinessFlowChanged((BusinessFlow)args.NewValue);            
        }

        private void OnActParentBusinessFlowChanged(BusinessFlow bf)
        {
            mActParentBusinessFlow = bf;
        }

        public static DependencyProperty ActParentActivityProperty =
            DependencyProperty.Register("mActParentActivity", typeof(Activity), typeof(UCFlowControlAction), new PropertyMetadata(OnActParentActivityPropertyChanged));
        private static void OnActParentActivityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCFlowControlAction;
            if (control != null)
                control.OnActParentActivityChanged((Activity)args.NewValue);
        }

        private void OnActParentActivityChanged(Activity activity)
        {
            mActParentActivity = activity;
        }

        public static DependencyProperty BfParentRunnerProperty =
            DependencyProperty.Register("gingerRunner", typeof(GingerRunner), typeof(UCFlowControlAction), new PropertyMetadata(OnBfParentRunnerPropertyChanged));

        private static void OnBfParentRunnerPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as UCFlowControlAction;
            if (control != null)
                control.OnBfParentRunnerChanged((GingerRunner)args.NewValue);
        }

        private void OnBfParentRunnerChanged(GingerRunner mRunner)
        {
            mBfParentRunner = mRunner;
        }

        private void UCFlowControlAction_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FC = (FlowControl)e.NewValue;
                        

            if (mBfParentRunner != null)
            {
                App.FillComboFromEnumVal(ActionComboBox, FC.BusinessFlowControlAction);
                App.ObjFieldBinding(ActionComboBox, ComboBox.SelectedValueProperty, FC, FlowControl.Fields.BusinessFlowControlAction);
            }
            else
            {
                App.FillComboFromEnumVal(ActionComboBox, FC.FlowControlAction);

                App.ObjFieldBinding(ActionComboBox, ComboBox.SelectedValueProperty, FC, FlowControl.Fields.FlowControlAction);
            }                               

            App.ObjFieldBinding(ActionValueTextBox, TextBox.TextProperty, FC, FlowControl.Fields.Value);
            ActionValueTextBox.Init(FC, FlowControl.Fields.Value);
            ActionValueTextBox.ValueTextBox.Text = FC.Value;

            SetActionValueComboData();
            ActionValueComboBox.SelectionChanged += ActionValueComboBox_SelectionChanged;
            ActionComboBox.SelectionChanged += ActionComboBox_SelectionChanged;
        }

        private void ActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FC.FlowControlAction = (FlowControl.eFlowControlAction)ActionComboBox.SelectedValue;
            SetActionValueComboData();
        }

        private void ActionValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // We keep the GUID of the action or activity
            GingerCore.General.ComboEnumItem CEI = (GingerCore.General.ComboEnumItem)ActionValueComboBox.SelectedItem;
            if (CEI != null)
            {
                FC.Value = CEI.Value.ToString();
            }
            else
            {
                FC.Value = null;
            }
        }
      

        private void SetActionValueComboData()
        {
            ActionValueComboBox.Items.Clear();
            ActionValueComboBox.Visibility = System.Windows.Visibility.Collapsed;
            ActionValueTextBox.Visibility = System.Windows.Visibility.Collapsed;

            // For Business flow Control in Run Set
            if (mBfParentRunner != null)
            {
                switch (FC.BusinessFlowControlAction)
                {
                    case FlowControl.eBusinessFlowControlAction.GoToBusinessFlow:
                        {
                            foreach (BusinessFlow bf in mBfParentRunner.BusinessFlows)
                            {
                                if (App.MainWindow.SelectedSolutionTab == MainWindow.eSolutionTabType.Run && mActParentBusinessFlow == bf)//TODO: do better condition 
                                    continue;
                                
                                GingerCore.General.ComboEnumItem CEI = new GingerCore.General.ComboEnumItem();
                                CEI.Value = bf.InstanceGuid + FC.GUID_NAME_SEPERATOR + bf.Name;//adding also name as second option search to be used when pulling the activity from Shared Repository
                                CEI.text = bf.Name;
                                ActionValueComboBox.Items.Add(CEI);

                                if (ActionValueComboBox.SelectedItem == null || (ActionValueComboBox.SelectedItem != null && bf.Active))
                                {
                                    if (FC.GetGuidFromValue(true) == bf.InstanceGuid)
                                    {
                                        ActionValueComboBox.SelectedItem = CEI;
                                    }
                                    else if (FC.GetGuidFromValue(true) == Guid.Empty && FC.GetNameFromValue(true) == bf.RunDescription)
                                    {
                                        ActionValueComboBox.SelectedItem = CEI;
                                    }

                                }
                            }
                            ActionValueComboBox.Visibility = System.Windows.Visibility.Visible;
                            ActionValueTextBox.Visibility = System.Windows.Visibility.Hidden;

                            if (FC.Value != null && ActionValueComboBox.SelectedItem == null)
                            {
                                Reporter.ToUser(eUserMsgKeys.ActivityIDNotFound, FC.Value);
                            }
                            break;
                        }

                    case FlowControl.eBusinessFlowControlAction.SetVariableValue:
                        {
                            ActionValueTextBox.Visibility = System.Windows.Visibility.Visible;
                            ActionValueComboBox.Visibility = System.Windows.Visibility.Hidden;
                            break;
                        }
                }
            }
            else
            {
                switch (FC.FlowControlAction)
                {

                    case FlowControl.eFlowControlAction.GoToAction:
                        {
                            foreach (Act a in mActParentActivity.Acts)
                            {
                                //avoid current Action
                                if (App.MainWindow.SelectedSolutionTab == MainWindow.eSolutionTabType.BusinessFlows && App.BusinessFlow.CurrentActivity.Acts.CurrentItem == a)//TODO: do better condition 
                                {
                                    continue;
                                }

                                GingerCore.General.ComboEnumItem CEI = new GingerCore.General.ComboEnumItem();
                                CEI.Value = a.Guid + FC.GUID_NAME_SEPERATOR + a.Description;//adding also name as second option search to be used when pulling the actions from Shared Repository
                                CEI.text = a.Description;
                                ActionValueComboBox.Items.Add(CEI);

                                if (ActionValueComboBox.SelectedItem == null ||
                                    ((ActionValueComboBox.SelectedItem != null && a.Active)))
                                {
                                    if (FC.GetGuidFromValue(true) == a.Guid)//we leeting it run each time becase in Convertion mechinsem we have 2 actions with same GUID
                                    {
                                        ActionValueComboBox.SelectedItem = CEI;
                                    }
                                    else if (FC.GetGuidFromValue(true) == Guid.Empty && FC.GetNameFromValue(true) == a.Description)
                                    {
                                        ActionValueComboBox.SelectedItem = CEI;
                                    }
                                }
                            }
                            ActionValueComboBox.Visibility = System.Windows.Visibility.Visible;
                            ActionValueTextBox.Visibility = System.Windows.Visibility.Hidden;

                            if (FC.Value != null && ActionValueComboBox.SelectedItem == null)
                            {
                                Reporter.ToUser(eUserMsgKeys.ActionIDNotFound, FC.Value);
                            }
                        }
                        break;

                    case FlowControl.eFlowControlAction.GoToActivity:
                        {
                            foreach (Activity a in mActParentBusinessFlow.Activities)
                            {
                                if (App.MainWindow.SelectedSolutionTab == MainWindow.eSolutionTabType.BusinessFlows && App.BusinessFlow.CurrentActivity == a)//TODO: do better condition 
                                {
                                    continue;
                                }

                                GingerCore.General.ComboEnumItem CEI = new GingerCore.General.ComboEnumItem();
                                CEI.Value = a.Guid + FC.GUID_NAME_SEPERATOR + a.ActivityName;//adding also name as second option search to be used when pulling the activity from Shared Repository
                                CEI.text = a.ActivityName;
                                ActionValueComboBox.Items.Add(CEI);

                                if (ActionValueComboBox.SelectedItem == null ||
                                   (ActionValueComboBox.SelectedItem != null && a.Active))
                                {
                                    if (FC.GetGuidFromValue(true) == a.Guid)
                                    {
                                        ActionValueComboBox.SelectedItem = CEI;
                                    }
                                    else if (FC.GetGuidFromValue(true) == Guid.Empty && FC.GetNameFromValue(true) == a.Description)
                                    {
                                        ActionValueComboBox.SelectedItem = CEI;
                                    }

                                }
                            }
                            ActionValueComboBox.Visibility = System.Windows.Visibility.Visible;
                            ActionValueTextBox.Visibility = System.Windows.Visibility.Hidden;

                            if (FC.Value != null && ActionValueComboBox.SelectedItem == null)
                            {
                                Reporter.ToUser(eUserMsgKeys.ActivityIDNotFound, FC.Value);
                            }
                            break;
                        }

                    case FlowControl.eFlowControlAction.MessageBox:
                        {
                            ActionValueTextBox.Visibility = System.Windows.Visibility.Visible;
                            ActionValueComboBox.Visibility = System.Windows.Visibility.Hidden;
                            break;
                        }

                    case FlowControl.eFlowControlAction.SetVariableValue:
                        {
                            ActionValueTextBox.Visibility = System.Windows.Visibility.Visible;
                            ActionValueComboBox.Visibility = System.Windows.Visibility.Hidden;
                            break;
                        }
                    case FlowControl.eFlowControlAction.RunSharedRepositoryActivity:
                    case FlowControl.eFlowControlAction.GoToActivityByName:
                        {
                            ActionValueTextBox.Visibility = System.Windows.Visibility.Visible;
                            ActionValueComboBox.Visibility = System.Windows.Visibility.Hidden;
                            break;
                        }
                }
            }
        }      
    }
}
