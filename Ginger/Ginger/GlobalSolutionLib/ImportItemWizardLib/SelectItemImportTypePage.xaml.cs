using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GlobalSolutionLib;
using Amdocs.Ginger.CoreNET.GlobalSolutionLib;
using Ginger.Actions;
using Ginger.UserControls;
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

namespace Ginger.GlobalSolutionLib.ImportItemWizardLib
{
    /// <summary>
    /// Interaction logic for SelectItemImportTypePage.xaml
    /// </summary>
    public partial class SelectItemImportTypePage : Page, IWizardPage
    {
        ImportItemWizard wiz;

        public SelectItemImportTypePage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (ImportItemWizard)WizardEventArgs.Wizard;
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(EncryptionKeyTextBox, TextBox.TextProperty, wiz, nameof(ImportItemWizard.EncryptionKey));
                    xGlobalSolutionFolderUC.Init(null, wiz, nameof(ImportItemWizard.SolutionFolder), false, true, UCValueExpression.eBrowserType.Folder);
                    break;
                case EventType.LeavingForNextPage:
                    if (string.IsNullOrEmpty(wiz.SolutionFolder))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("Please select Solution Folder."));
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
                    else if (wiz.SolutionFolder == WorkSpace.Instance.SolutionRepository.SolutionFolder)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("Please select different Solution Folder."));
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
                    else if (string.IsNullOrEmpty(wiz.EncryptionKey))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("Please provide Solution Encryption Key."));
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }

                    GlobalSolutionUtils.Instance.EncryptionKey = wiz.EncryptionKey;
                    GlobalSolutionUtils.Instance.SolutionFolder = wiz.SolutionFolder;
                    if (!GlobalSolutionUtils.Instance.ValidateEncryptionKey())
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, string.Format("Loading Solution- Error: Encryption key validation failed."));
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }

                    break;
            }
        }

        private void ImportFromLocalFolderTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (wiz == null)
            {
                return;
            }
            wiz.ImportFromType = GlobalSolution.eImportFromType.LocalFolder;
            ImportFromLocalFolderPanel.Visibility = Visibility.Visible;
            ImportFromSourceControlPanel.Visibility = Visibility.Hidden;
        }

        private void ImportFromSourceControlTypeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (wiz == null)
            {
                return;
            }
            wiz.ImportFromType = GlobalSolution.eImportFromType.SourceControl;
            ImportFromSourceControlPanel.Visibility = Visibility.Visible;
            ImportFromLocalFolderPanel.Visibility = Visibility.Hidden;
        }
    }
}
