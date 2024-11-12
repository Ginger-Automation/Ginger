using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.WizardLib;
using GingerWPF.WizardLib;
using System.ComponentModel;

namespace Ginger.External.Katalon
{
    public sealed class ImportKatalonObjectRepositoryWizard : WizardBase, INotifyPropertyChanged
    {
        private readonly RepositoryFolder<ApplicationPOMModel> _importTargetDirectory;

        private string _selectedDirectory;

        public ObservableList<KatalonConvertedPOMViewModel> POMViewModels { get; }


        public string SelectedDirectory
        {
            get => _selectedDirectory;
            set
            {
                _selectedDirectory = value;
                PropertyChanged?.Invoke(sender: this, new PropertyChangedEventArgs(propertyName: nameof(SelectedDirectory)));
            }
        }

        public override string Title => "Import POM From Katalon Object-Repository";

        public event PropertyChangedEventHandler? PropertyChanged;

        internal ImportKatalonObjectRepositoryWizard(RepositoryFolder<ApplicationPOMModel> importTargetDirectory)
        {
            _importTargetDirectory = importTargetDirectory;
            POMViewModels = [];
            _selectedDirectory = string.Empty;
            AddPages();
        }

        private void AddPages()
        {
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Katalon Object Repository Import Introduction", Page: new WizardIntroPage("/External/Katalon/ImportKatalonObjectRepositoryIntro.md"));
            AddPage(Name: "SelectFolder", Title: "Select Folder", SubTitle: "Select Object-Repository folder", Page: new SelectObjectRepositoryFolderWizardPage(wizard: this));
            AddPage(Name: "ImportPOM", Title: "Import POM", SubTitle: "View imported POM list", new ImportPOMFromObjectRepositoryWizardPage(wizard: this));
        }

        public override void Finish()
        {
            foreach (KatalonConvertedPOMViewModel pomViewModel in POMViewModels)
            {
                if (!pomViewModel.Active)
                {
                    continue;
                }
                pomViewModel.CommitChanges();
                _importTargetDirectory.AddRepositoryItem(pomViewModel.POM);
            }
        }
    }
}
