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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.ApplicationModelsLib.APIModels.APIModelWizard
{
    /// <summary>
    /// Interaction logic for AddAPIModelExtraConfigsPage.xaml
    /// </summary>
    public partial class AddAPIModelExtraConfigsPage : Page, IWizardPage
    {
        public AddAPIModelExtraConfigsPage()
        {
            InitializeComponent();
            xTargetApplicationComboBox.ComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
        }

        private void FillTargetAppsComboBox()
        {
            //get key object 
            if (AddAPIModelWizard.TargetApplicationKey != null)
            {
                RepositoryItemKey key = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.Guid == AddAPIModelWizard.TargetApplicationKey.Guid).Select(x => x.Key).FirstOrDefault();
                if (key != null)
                {
                    AddAPIModelWizard.TargetApplicationKey = key;
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "The mapped " + AddAPIModelWizard.TargetApplicationKey.Key + " Target Application was not found, please select new Target Application");
                }
            }
            xTargetApplicationComboBox.ComboBox.ItemsSource = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.Platform == ePlatformType.WebServices).ToList();//.AsCollectionViewOrderBy(nameof(ApplicationPlatform.AppName));
            xTargetApplicationComboBox.ComboBox.SelectedValuePath = nameof(ApplicationPlatform.Key);
            xTargetApplicationComboBox.ComboBox.DisplayMemberPath = nameof(ApplicationPlatform.AppName);
        }

        public AddAPIModelWizard AddAPIModelWizard;

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                AddAPIModelWizard = ((AddAPIModelWizard)WizardEventArgs.Wizard);

                xTAlabel.Content = $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}:";
                FillTargetAppsComboBox();
                xTargetApplicationComboBox.Init(AddAPIModelWizard, nameof(AddAPIModelWizard.TargetApplicationKey));
                xTargetApplicationComboBox.ComboBox.SelectedIndex = 0;

                xTagsViewer.Init(AddAPIModelWizard.TagsKeys);
            }
            //else if (WizardEventArgs.EventType == EventType.Active)
            //{
            //    AddAPIModelWizard.FinishEnabled = true;
            //    AddAPIModelWizard.NextEnabled = false;
            //}
        }

        private void xWireMockMappingToggle_Checked(object sender, RoutedEventArgs e)
        {
            WireMockConfiguration mockConfiguration;
            mockConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<WireMockConfiguration>().Count == 0 ? new WireMockConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<WireMockConfiguration>();
            if (string.IsNullOrEmpty(mockConfiguration.WireMockUrl))
            {

                Reporter.ToUser(eUserMsgKey.WireMockConnectionFail);

            }
            else
            {
                AddAPIModelWizard.toCreateWireMock = true;
            }

        }
    }
}
