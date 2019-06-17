using Ginger.BusinessFlowWindows;
using GingerCore;
using GingerWPF.WizardLib;
using System.Windows.Controls;

namespace Ginger.BusinessFlowPages
{
    /// <summary>
    /// Interaction logic for AddActivityConfigsPage.xaml
    /// </summary>
    public partial class AddActivityConfigsPage : Page, IWizardPage
    {
        AddActivityWizard mWizard;
        Activity mLastActivity;

        public AddActivityConfigsPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddActivityWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    break;
                case EventType.Active:
                    if (mLastActivity != mWizard.ActivityToAdd)
                    {
                        mLastActivity = mWizard.ActivityToAdd;
                        ActivityEditPage editPage = new ActivityEditPage(mWizard.ActivityToAdd, General.RepositoryItemPageViewMode.Automation, context: mWizard.Context);
                        xFrame.Content = editPage;
                    }
                    break;
            }
        }
    }
}
