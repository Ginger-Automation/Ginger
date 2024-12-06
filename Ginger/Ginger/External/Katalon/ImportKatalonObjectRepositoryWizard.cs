using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.WizardLib;
using GingerWPF.WizardLib;
using System;
using System.ComponentModel;

namespace Ginger.External.Katalon
{
    public sealed class ImportKatalonObjectRepositoryWizard : WizardBase, INotifyPropertyChanged
    {
        private readonly RepositoryFolder<ApplicationPOMModel> _importTargetDirectory;

        private string _selectedDirectory;
        private int _importedPOMCount;

        public ObservableList<KatalonConvertedPOMViewModel> POMViewModels { get; }

        public int ImportedPOMCount
        {
            get => _importedPOMCount;
            private set
            {
                _importedPOMCount = value;
                PropertyChanged?.Invoke(sender: this, new(nameof(ImportedPOMCount)));
            }
        }

        public string ImportTargetDirectoryPath => _importTargetDirectory.FolderFullPath;

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
            DisableBackBtnOnLastPage = true;
            AddPages();
        }

        private void AddPages()
        {
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Katalon Object Repository Import Introduction", Page: new WizardIntroPage("/External/Katalon/ImportKatalonObjectRepositoryIntro.md"));
            AddPage(Name: "SelectFolder", Title: "Select Folder", SubTitle: "Select Object-Repository folder", Page: new SelectObjectRepositoryFolderWizardPage(wizard: this));
            AddPage(Name: "ImportPOM", Title: "Import POM", SubTitle: "View imported POM list", new ImportPOMFromObjectRepositoryWizardPage(wizard: this));
            AddPage(Name: "ImportSummary", Title: "Summary", SubTitle: "Summary", new ImportPOMSummaryWizardPage(wizard: this));
        }

        public void AddPOMs()
        {
            if (POMViewModels.Count == 0)
            {
                return;
            }

            try
            {
                ImportedPOMCount = 0;
                ProcessStarted();
                foreach (KatalonConvertedPOMViewModel pomViewModel in POMViewModels.ItemsAsEnumerable())
                {
                    try
                    {
                        if (!pomViewModel.Active || !pomViewModel.IsValid())
                        {
                            continue;
                        }
                        pomViewModel.CommitChanges();
                        _importTargetDirectory.AddRepositoryItem(pomViewModel.POM);
                        ImportedPOMCount++;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error while adding imported POM to solution", ex);
                    }
                }

                //clear so that when the Finish method is called, it won't add POMs again
                POMViewModels.Clear();
            }
            finally
            {
                ProcessEnded();
            }
        }

        public override void Finish()
        {
            AddPOMs();
        }
    }
}
