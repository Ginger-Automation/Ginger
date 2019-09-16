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
using GingerCore;
using GingerCore.Activities;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowPages
{
    /// <summary>
    /// Interaction logic for AddActivityDetailsPage.xaml
    /// </summary>
    public partial class AddActivityDetailsPage : Page, IWizardPage
    {
        AddActivityWizard mWizard;

        public AddActivityDetailsPage()
        {
            InitializeComponent();

            xAddGroupBtn.ButtonText = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup, "New");
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddActivityWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    if (mWizard.ActivitiesGroupPreSet == false)
                    {
                        xGroupComboBox.ItemsSource = mWizard.Context.BusinessFlow.ActivitiesGroups;
                        xGroupComboBox.DisplayMemberPath = nameof(ActivitiesGroup.Name);
                        BindingHandler.ObjFieldBinding(xGroupComboBox, ComboBox.SelectedItemProperty, mWizard, nameof(AddActivityWizard.ParentActivitiesGroup));
                    }
                    else
                    {
                        xGroupPanel.Visibility = Visibility.Collapsed;
                    }
                    xRegularType.IsChecked = true;
                    break;
            }

        }

        private void XType_Checked(object sender, RoutedEventArgs e)
        {
            if (xRegularType.IsChecked == true)
            {
                if (mWizard.ActivityToAdd == null || (mWizard.ActivityToAdd is ErrorHandler))
                {
                    mWizard.ActivityToAdd = new Activity() { ActivityName = "New " + GingerDicser.GetTermResValue(eTermResKey.Activity), TargetApplication=SetTargetApp(), Active = true };
                }                
            }
            else if (xErrorHandlerType.IsChecked == true)
            {
                if (mWizard.ActivityToAdd == null || (mWizard.ActivityToAdd is ErrorHandler) == false)
                {
                    mWizard.ActivityToAdd = new ErrorHandler() { ActivityName = "New Error Handler" , TargetApplication = SetTargetApp()};
                }
            }
        }

        private void XAddGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            string groupName = string.Empty;
            if (InputBoxWindow.GetInputWithValidation("New Group", "New Group Name:", ref groupName))
            {
                if (!string.IsNullOrEmpty(groupName))
                {
                    if (mWizard.Context.BusinessFlow.ActivitiesGroups.Where(x => x.Name.Trim() == groupName.Trim()).FirstOrDefault() == null)
                    {
                        ActivitiesGroup activitiesGroup = new ActivitiesGroup() { Name = groupName.Trim() };
                        mWizard.Context.BusinessFlow.AddActivitiesGroup(activitiesGroup);
                        xGroupComboBox.SelectedItem = activitiesGroup;
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Group with same name already exist, please set unique name.");
                    }
                }
            }
        }

        private string SetTargetApp()
        {
            if (mWizard.ParentActivitiesGroup.ActivitiesIdentifiers.Count > 0)
            {
                ActivityIdentifiers activityIdnt = mWizard.ParentActivitiesGroup.ActivitiesIdentifiers.Where(x => string.IsNullOrEmpty(x.IdentifiedActivity.TargetApplication) == false).FirstOrDefault();
                if (activityIdnt != null)
                {
                    return activityIdnt.IdentifiedActivity.TargetApplication;
                }
            }

            if (mWizard.Context.BusinessFlow.TargetApplications.Count > 0)
            {
                return mWizard.Context.BusinessFlow.TargetApplications[0].Name;
            }

            return string.Empty;
        }
    }
}

