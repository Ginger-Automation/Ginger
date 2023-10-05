#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Common.SourceControlLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for ConflictViewPage.xaml
    /// </summary>
    public partial class ConflictViewPage : Page, IWizardPage
    {
        public ConflictViewPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs wizardEventArgs)
        {
            ResolveMergeConflictWizard? wizard = wizardEventArgs.Wizard as ResolveMergeConflictWizard;

            if (wizard == null)
            {
                throw new InvalidOperationException($"{nameof(ConflictViewPage)} must be used with {nameof(ResolveMergeConflictWizard)}.");
            }

            switch (wizardEventArgs.EventType)
            {
                case EventType.Init:
                    OnWizardPageInit(wizard);
                    break;
                case EventType.LeavingForNextPage:
                    OnWizardPageLeavingForNextPage(wizard, wizardEventArgs);
                    break;
                default:
                    break;
            }
        }

        private void OnWizardPageInit(ResolveMergeConflictWizard wizard)
        {
            Task.Run(() =>
            {
                ShowLoading();
                SetTreeItems(wizard.Comparison);
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
                xLocalItemTree.AddItem(
                    new ConflictComparisonTreeViewItem(
                        comparison, 
                        childrenStateFilter: new[] 
                        { 
                            Comparison.StateType.Unmodified, 
                            Comparison.StateType.Modified, 
                            Comparison.StateType.Deleted 
                        }));
                xRemoteItemTree.AddItem(
                    new ConflictComparisonTreeViewItem(
                        comparison, 
                        childrenStateFilter: new[] 
                        { 
                            Comparison.StateType.Unmodified, 
                            Comparison.StateType.Modified, 
                            Comparison.StateType.Added 
                        }));
            });
        }

        private void OnWizardPageLeavingForNextPage(ResolveMergeConflictWizard wizard, WizardEventArgs eventArgs)
        {
            int unselectedComparisonCount = wizard.Comparison.UnselectedComparisonCount();
            if (unselectedComparisonCount > 0)
            {
                Reporter.ToUser(eUserMsgKey.HandleConflictsBeforeMovingForward, messageArgs: unselectedComparisonCount);
                eventArgs.CancelEvent = true;
            }
        }

        private void xPrevConflict_Click(object sender, RoutedEventArgs e)
        {
            _ = HighlighPrevConflictAsync();
        }

        private async Task HighlighPrevConflictAsync()
        {
            NextConflictFinder nextConflictFinder = new();

            Task findNextLocalConflictTask = xLocalItemTree.IterateTreeViewItemsAsync(nextConflictFinder.IterationConsumer, inReverseOrder: true);
            if (nextConflictFinder.NextConflictTreeViewItem != null)
            {
                xLocalItemTree.FocusItem(nextConflictFinder.NextConflictTreeViewItem);
            }

            Task findNextRemoteConflictTask = xRemoteItemTree.IterateTreeViewItemsAsync(nextConflictFinder.IterationConsumer, inReverseOrder: true);
            if (nextConflictFinder.NextConflictTreeViewItem != null)
            {
                xRemoteItemTree.FocusItem(nextConflictFinder.NextConflictTreeViewItem);
            }

            await Task.WhenAll(findNextLocalConflictTask, findNextRemoteConflictTask);
        }

        private void xNextConflict_Click(object sender, RoutedEventArgs e)
        {
            _ = HighlightNextConflictAsync();
        }

        private async Task HighlightNextConflictAsync()
        {
            NextConflictFinder nextConflictFinder = new();

            Task findNextLocalConflictTask = xLocalItemTree.IterateTreeViewItemsAsync(nextConflictFinder.IterationConsumer);
            if (nextConflictFinder.NextConflictTreeViewItem != null)
            {
                xLocalItemTree.FocusItem(nextConflictFinder.NextConflictTreeViewItem);
            }

            Task findNextRemoteConflictTask = xRemoteItemTree.IterateTreeViewItemsAsync(nextConflictFinder.IterationConsumer);
            if (nextConflictFinder.NextConflictTreeViewItem != null)
            {
                xRemoteItemTree.FocusItem(nextConflictFinder.NextConflictTreeViewItem);
            }

            await Task.WhenAll(findNextLocalConflictTask, findNextRemoteConflictTask);
        }

        private sealed class NextConflictFinder
        {
            public TreeViewItem? NextConflictTreeViewItem { get; private set; }

            public bool IterationConsumer(TreeViewItem currentTreeViewItem)
            {
                Comparison comparison = (Comparison)((ITreeViewItem)currentTreeViewItem.Tag).NodeObject();
                bool continueIteration = true;

                bool IsAddedOrDeleted = comparison.State == Comparison.StateType.Added || comparison.State == Comparison.StateType.Deleted;
                bool selfAndSiblingNotSelected = !comparison.Selected && (!comparison.HasSiblingComparison || !comparison.SiblingComparison.Selected);

                if (IsAddedOrDeleted && selfAndSiblingNotSelected)
                {
                    NextConflictTreeViewItem = currentTreeViewItem;
                    continueIteration = false;
                }

                return continueIteration;
            }
        }
    }
}
