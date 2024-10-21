using Amdocs.Ginger.Common;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.External.Katalon
{
    /// <summary>
    /// Interaction logic for SelectObjectRepositoryFolderWizardPage.xaml
    /// </summary>
    public partial class SelectObjectRepositoryFolderWizardPage : Page, IWizardPage
    {
        private readonly ImportKatalonObjectRepositoryWizard _wizard;

        public SelectObjectRepositoryFolderWizardPage(ImportKatalonObjectRepositoryWizard wizard)
        {
            InitializeComponent();

            _wizard = wizard;
            BindingHandler.ObjFieldBinding(DirectoryTextBox, TextBox.TextProperty, _wizard, nameof(ImportKatalonObjectRepositoryWizard.SelectedDirectory));
        }

        public void WizardEvent(WizardEventArgs e)
        {
            switch (e.EventType)
            {
                case EventType.LeavingForNextPage:
                    if (_wizard.SelectedDirectory == null || string.Equals(_wizard.SelectedDirectory.Trim(), string.Empty))
                    {
                        e.CancelEvent = true;
                        Reporter.ToUser(eUserMsgKey.InvalidKatalonObjectRepository, "Folder path is empty");
                    }
                    if (!Directory.Exists(_wizard.SelectedDirectory))
                    {
                        e.CancelEvent = true;
                        Reporter.ToUser(eUserMsgKey.InvalidKatalonObjectRepository, "Folder doesn't exist");
                    }
                    break;
            }
        }

        private void DirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new();
            string directory = General.SetupBrowseFolder(folderBrowserDialog);
            _wizard.SelectedDirectory = directory;
            _wizard.POMViewModels.ClearAll();
        }
    }
}
