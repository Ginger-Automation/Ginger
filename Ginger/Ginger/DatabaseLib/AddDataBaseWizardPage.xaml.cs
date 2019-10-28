using amdocs.ginger.GingerCoreNET;
using GingerWPF.WizardLib;
using System.Windows.Controls;

namespace Ginger.DatabaseLib
{
    /// <summary>
    /// Interaction logic for AddDataBaseWizardPage.xaml
    /// </summary>
    public partial class AddDataBaseWizardPage : Page, IWizardPage
    {
        AddDatabaseWizard mWizard;

        public AddDataBaseWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            
            mWizard = (AddDatabaseWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    var databases = WorkSpace.Instance.PlugInsManager.GetDatabaseList();
                    xDatabaseList.ItemsSource = databases;
                    xDatabaseList.BindControl(mWizard, nameof(AddDatabaseWizard.ServiceID));


                    break;
            }
        }
    }
}
