using amdocs.ginger.GingerCoreNET;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.RunSetLib.CreateCLIWizardLib
{
    /// <summary>
    /// Interaction logic for CreateCLIContentPage.xaml
    /// </summary>
    public partial class CreateCLILocationPage : Page, IWizardPage
    {

        CreateCLIWizard mCreateCLIWizard;

        public CreateCLILocationPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mCreateCLIWizard = (CreateCLIWizard)WizardEventArgs.Wizard;
                    xShortcutDescriptionTextBox.BindControl(mCreateCLIWizard, nameof(CreateCLIWizard.ShortcutDescription));
                    xDesktopRadioButton.IsChecked = true;
                    break;
                case EventType.Active:
                    if (string.IsNullOrEmpty(xShortcutDescriptionTextBox.Text))
                    {
                        string description = WorkSpace.Instance.Solution.Name + " " + WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + " " + WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name;
                        xShortcutDescriptionTextBox.Text = description;                        
                    }
                    xCLIFileName.Text = mCreateCLIWizard.CLIFileName;
                    break;
            }
        }

        private void XDesktopRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.SetCLIFolder();
        }

        private void XFolderRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mCreateCLIWizard.SetCLIFolder(xCLIFolderTextBox.Text);
        }

        private void XShortcutDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            xCLIFileName.Text = mCreateCLIWizard.CLIFileName;
        }
    }
}
