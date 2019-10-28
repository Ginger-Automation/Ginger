using GingerWPF.WizardLib;
using System.Windows.Controls;

namespace Ginger.DatabaseLib
{
    /// <summary>
    /// Interaction logic for AddDatabaseConnWizardPage.xaml
    /// </summary>
    public partial class AddDatabaseConnWizardPage : Page, IWizardPage
    {
        AddDatabaseWizard mWizard;

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddDatabaseWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    xConnectionStringTextBox.BindControl(mWizard, nameof(AddDatabaseWizard.ConnectionString));
                    break;
            }
        }
    

        public AddDatabaseConnWizardPage()
        {
            InitializeComponent();
        }



    }
}
