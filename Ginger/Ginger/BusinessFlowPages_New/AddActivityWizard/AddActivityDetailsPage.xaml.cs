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
                return mWizard.ParentActivitiesGroup.ActivitiesIdentifiers[0].IdentifiedActivity.TargetApplication;
            }
            else
            {
                if (mWizard.Context.BusinessFlow.TargetApplications.Count > 0)
                {
                    return mWizard.Context.BusinessFlow.TargetApplications[0].Name;
                }
            }
            return string.Empty;
        }
    }
}

