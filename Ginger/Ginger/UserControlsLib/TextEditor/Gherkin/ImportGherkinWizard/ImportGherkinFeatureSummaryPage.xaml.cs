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

using GingerWPF.WizardLib;
using System.Windows.Controls;
using Ginger.SolutionWindows.TreeViewItems;

namespace Ginger.UserControlsLib.TextEditor.Gherkin
{
    /// <summary>
    /// Interaction logic for ImportGherkinFeatureSummaryPage.xaml
    /// </summary>
    public partial class ImportGherkinFeatureSummaryPage : Page, IWizardPage
    {
        public ImportGherkinFeatureSummaryPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Active:
                    ImportGherkinFeatureWizard wiz = (ImportGherkinFeatureWizard)WizardEventArgs.Wizard; 
                    if(wiz.featureTargetFolder != null)
                    { 
                        TargetFeatureFolderLabel.Content = ((DocumentsFolderTreeItem)wiz.featureTargetFolder).NodePath();
                    }
                    if (wiz.bizFlowTargetFolder != null)
                    {
                        TargetBFFolderLabel.Content = ((BusinessFlowsFolderTreeItem)wiz.bizFlowTargetFolder).NodePath();
                    }
                    break;
            }
        }
    }
}
