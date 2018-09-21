#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.WindowExplorer;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Ginger.ExtensionMethods;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for SelectAppFolderWizardPage.xaml
    /// </summary>
    public partial class POMGeneralDetailsWizardPage : Page, IWizardPage
    {
        AddPOMWizard mWizard;

        public POMGeneralDetailsWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
                    xNameTextBox.BindControl(mWizard.POM, nameof(ApplicationPOMModel.Name));
                    xNameTextBox.AddValidationRule(new POMNameValidationRule());
                    xNameTextBox.Focus();

                    xDescriptionTextBox.BindControl(mWizard.POM, nameof(ApplicationPOMModel.Description));
                    xTagsViewer.Init(mWizard.POM.TagsKeys);

                    ObservableList<ApplicationPlatform> TargetApplications = GingerCore.General.ConvertListToObservableList(App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.Platform == ePlatformType.Web).ToList());
                    xTargetApplicationComboBox.BindControl<ApplicationPlatform>(mWizard.POM, nameof(ApplicationPOMModel.TargetApplicationKey), TargetApplications, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));
                    xTargetApplicationComboBox.AddValidationRule(new POMTAValidationRule());

                    if (xTargetApplicationComboBox.Items != null && xTargetApplicationComboBox.Items.Count > 0)
                    {
                        xTargetApplicationComboBox.SelectedIndex = 0;
                    }
                    break;
            }
        }

        /// <summary>
        /// This method is used to cehck whether alternate page is required to load
        /// </summary>
        /// <returns></returns>
        public bool IsAlternatePageToLoad()
        {
            return false;
        }
    }
}
