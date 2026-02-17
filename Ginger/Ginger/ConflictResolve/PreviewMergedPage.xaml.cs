#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.Agents;
using Ginger.ApplicationModelsLib.POMModels;
using Ginger.Run;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerWPF.ApplicationModelsLib.APIModels;
using GingerWPF.BusinessFlowsLib;
using GingerWPF.WizardLib;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for PreviewMergedPage.xaml
    /// </summary>
    public partial class PreviewMergedPage : Page, IWizardPage
    {
        public PreviewMergedPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            ResolveMergeConflictWizard wizard = (ResolveMergeConflictWizard)WizardEventArgs.Wizard;

            switch (WizardEventArgs.EventType)
            {
                case EventType.Active:
                    OnWizardPageActive(wizard);
                    break;
                default:
                    break;
            }
        }

        private void OnWizardPageActive(ResolveMergeConflictWizard wizard)
        {
            Task.Run(() =>
            {
                ShowLoading();
                bool hasMergedItem = wizard.TryGetOrCreateMergedItem(out RepositoryItemBase? mergedItem);
                if (hasMergedItem && mergedItem != null)
                {
                    Comparison mergedItemComparison = SourceControlIntegration.CompareConflictedItems(mergedItem, null);
                    SetTreeItems(mergedItemComparison);
                    SetPageContent(mergedItem);
                }
                HideLoading();
            });
        }

        private void ShowLoading()
        {
            Dispatcher.Invoke(() =>
            {
                xContentGrid.Visibility = Visibility.Collapsed;
                if (xLoadingFrame.Content == null)
                {
                    xLoadingFrame.Content = new LoadingPage();
                }
                xLoadingFrame.Visibility = Visibility.Visible;
            });
        }

        private void HideLoading()
        {
            Dispatcher.Invoke(() =>
            {
                xLoadingFrame.Visibility = Visibility.Collapsed;
                xContentGrid.Visibility = Visibility.Visible;
            });
        }

        private void SetTreeItems(Comparison comparison)
        {
            Dispatcher.Invoke(() =>
            {
                xTree.ClearTreeItems();
                xTree.AddItem(new ConflictMergeTreeViewItem(comparison));
            });
        }

        private void SetPageContent(RepositoryItemBase mergedItem)
        {
            if (mergedItem is BusinessFlow mergedBusinessFlow)
            {
                Dispatcher.Invoke(() =>
                {
                    xPageViewTabItem.IsEnabled = true;
                    xPageFrame.ClearAndSetContent(new BusinessFlowViewPage(mergedBusinessFlow, context: null!, General.eRIPageViewMode.View, ignoreValidationRules: true));
                });
            }
            else if (mergedItem is RunSetConfig mergedRunSetConfig)
            {
                Dispatcher.Invoke(() =>
                {
                    xPageViewTabItem.IsEnabled = true;
                    xPageFrame.ClearAndSetContent(new NewRunSetPage(mergedRunSetConfig, NewRunSetPage.eEditMode.View, ignoreValidationRules: true));
                });
            }
            else if (mergedItem is Agent mergedAgent)
            {
                Dispatcher.Invoke(() =>
                {
                    xPageViewTabItem.IsEnabled = true;
                    xPageFrame.ClearAndSetContent(new AgentEditPage(mergedAgent, isReadOnly: true, ignoreValidationRules: true, General.eRIPageViewMode.View));
                });
            }
            else if (mergedItem is Activity mergedActivity)
            {
                Dispatcher.Invoke(() =>
                {
                    xPageViewTabItem.IsEnabled = true;
                    xPageFrame.ClearAndSetContent(new ActivityPage(mergedActivity, new Amdocs.Ginger.Common.Context() { Activity = mergedActivity }, General.eRIPageViewMode.View));
                });
            }
            else if (mergedItem is Act mergedAction)
            {
                Dispatcher.Invoke(() =>
                {
                    xPageViewTabItem.IsEnabled = true;
                    xPageFrame.ClearAndSetContent(new ActionEditPage(mergedAction, General.eRIPageViewMode.View));
                });
            }
            else if (mergedItem is ApplicationAPIModel mergedApplicationAPIModel)
            {
                Dispatcher.Invoke(() =>
                {
                    xPageViewTabItem.IsEnabled = true;
                    xPageFrame.ClearAndSetContent(new APIModelPage(mergedApplicationAPIModel, General.eRIPageViewMode.View));
                });
            }
            else if (mergedItem is ApplicationPOMModel mergedApplicationPOMModel)
            {
                Dispatcher.Invoke(() =>
                {
                    xPageViewTabItem.IsEnabled = true;
                    xPageFrame.ClearAndSetContent(new POMEditPage(mergedApplicationPOMModel, General.eRIPageViewMode.View, ignoreValidationRules: true));
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    xPageViewTabItem.IsEnabled = false;
                    xPageFrame.ClearAndSetContent(null);
                });
            }
        }
    }
}
