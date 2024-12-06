#region License
/*
Copyright © 2014-2024 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
