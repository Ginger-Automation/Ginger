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
                    break;
                case EventType.Active:
                    if (string.IsNullOrEmpty(xShortcutDescriptionTextBox.Text))
                    {
                        string description = "Ginger Solution=" + WorkSpace.Instance.Solution.Name + ", RunSet=" + "!!!!," + " Env=" + "!!!!!!!!!!!!!!!!!!!!!!!!!!!";
                        xShortcutDescriptionTextBox.Text = description;                        
                    }
                    break;
            }

        }

       
    }
}
