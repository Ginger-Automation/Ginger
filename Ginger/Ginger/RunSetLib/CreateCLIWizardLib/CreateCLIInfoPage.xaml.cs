using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Run;
using GingerCore.Environments;
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
    /// Interaction logic for CreateCLIInfoPage.xaml
    /// </summary>
    public partial class CreateCLIInfoPage : Page, IWizardPage
    {
        CreateCLIWizard mCreateCLIWizard;
        public CreateCLIInfoPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mCreateCLIWizard = (CreateCLIWizard)WizardEventArgs.Wizard;                    
                    break;
                case EventType.Active:
                    // Solution name
                    xSolutionLabel.Content = WorkSpace.Instance.Solution.Folder;

                    // RunSetCombo
                    var v = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>(); ;
                    //xRunSetComboBox.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                    //xRunSetComboBox.DisplayMemberPath = nameof(RunSetConfig.Name);
                    //xRunSetComboBox.SelectedValuePath = nameof(RunSetConfig.Name);
                    xRunSetComboBox.BindControl(mCreateCLIWizard, nameof(CreateCLIWizard.RunSetConfig),v, nameof(RunSetConfig.Name), "this");

                    // Set current selected
                    // xRunSetComboBox.SelectedValue = 

                    // Env
                    xEnvironmentComboBox.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
                    xEnvironmentComboBox.DisplayMemberPath = nameof(ProjEnvironment.Name);
                    //xEnvironmentComboBox.SelectedValuePath = nameof(ProjEnvironment.Name);
                    xEnvironmentComboBox.BindControl(mCreateCLIWizard, nameof(CreateCLIWizard.ProjEnvironment));

                    //TODO: We can diaply a label with run set summary like how many runners and BFs

                    break;
            }

        }

        private void XPluginIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
