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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.SourceControlLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
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
        private ResolveMergeConflictWizard? _wizard;

        public ConflictViewPage()
        {
            InitializeComponent();
            SetUI();
        }

        private void SetUI()
        {
            SetConflictButtonUI();
        }

        private void SetConflictButtonUI()
        {
            xPrevConflict.xButtonText.Visibility = Visibility.Collapsed;
            xPrevConflict.xButtonImage.Margin = new Thickness(left: 5, top: 0, right: 5, bottom: 0);
            xNextConflict.xButtonText.Visibility = Visibility.Collapsed;
            xNextConflict.xButtonImage.Margin = new Thickness(left: 5, top: 0, right: 5, bottom: 0);
        }

        public void WizardEvent(WizardEventArgs wizardEventArgs)
        {
            _wizard = wizardEventArgs.Wizard as ResolveMergeConflictWizard;

            if (_wizard == null)
            {
                throw new InvalidOperationException($"{nameof(ConflictViewPage)} must be used with {nameof(ResolveMergeConflictWizard)}.");
            }

            switch (wizardEventArgs.EventType)
            {
                case EventType.Init:
                    OnWizardPageInit();
                    break;
                case EventType.LeavingForNextPage:
                    OnWizardPageLeavingForNextPage(wizardEventArgs);
                    break;
                default:
                    break;
            }
        }

        private void OnWizardPageInit()
        {
            if (_wizard == null)
            {
                return;
            }

            Task.Run(() =>
            {
                ShowLoading();
                SetConflictStats(_wizard.Comparison);
                UpdateRemainingConflictCount();
                SetTreeItems(_wizard.Comparison);
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

        private void SetConflictStats(Comparison comparison)
        {
            BindComparisonSelectedProperty(comparison);
        }

        private void BindComparisonSelectedProperty(Comparison comparison)
        {
            if (comparison.State == Comparison.StateType.Unmodified)
            {
                return;
            }

            comparison.PropertyChanged += Comparison_Selected_Changed;

            if (comparison.State is Comparison.StateType.Added or Comparison.StateType.Deleted)
            {
                return;
            }

            foreach (Comparison childComparison in comparison.ChildComparisons)
            {
                BindComparisonSelectedProperty(childComparison);
            }
        }

        private void Comparison_Selected_Changed(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!string.Equals(e.PropertyName, nameof(Comparison.Selected)))
            {
                return;
            }

            UpdateRemainingConflictCount();
        }

        private void UpdateRemainingConflictCount()
        {
            if (_wizard != null)
            {
                int remainingComparisonCount = _wizard.Comparison.UnselectedComparisonCount();
                Dispatcher.Invoke(() => xRemainingConflictCount.Text = remainingComparisonCount.ToString());
            }
        }

        private void SetTreeItems(Comparison comparison)
        {
            Dispatcher.Invoke(() =>
            {
                Dictionary<Comparison, IList<ConflictComparisonTreeViewItem>> tviRepo = [];
                xLocalItemTree.AddItem(
                    new ConflictComparisonTreeViewItem(
                        comparison,
                        childrenStateFilter: new[]
                        {
                            Comparison.StateType.Unmodified,
                            Comparison.StateType.Modified,
                            Comparison.StateType.Deleted
                        },
                        tviRepo));
                xRemoteItemTree.AddItem(
                    new ConflictComparisonTreeViewItem(
                        comparison,
                        childrenStateFilter: new[]
                        {
                            Comparison.StateType.Unmodified,
                            Comparison.StateType.Modified,
                            Comparison.StateType.Added
                        },
                        tviRepo));
            });
        }

        private void OnWizardPageLeavingForNextPage(WizardEventArgs eventArgs)
        {
            if (_wizard == null)
            {
                return;
            }

            int unselectedComparisonCount = _wizard.Comparison.UnselectedComparisonCount();
            if (unselectedComparisonCount > 0)
            {
                Reporter.ToUser(eUserMsgKey.HandleConflictsBeforeMovingForward, messageArgs: unselectedComparisonCount);
                eventArgs.CancelEvent = true;
            }
        }

        private void ShowNavigatingToConflictLoader()
        {
            if (_wizard == null)
            {
                return;
            }

            _wizard.ProcessStarted();
        }

        private void HideNavigatingToConflictLoader()
        {
            if (_wizard == null)
            {
                return;
            }

            _wizard.ProcessEnded();
        }

        private void xPrevConflict_Click(object sender, RoutedEventArgs e)
        {
            _ = HighlighPrevConflictAsync();
        }

        private async Task HighlighPrevConflictAsync()
        {
            NextConflictFinder localNextConflictFinder = new();
            Task findNextLocalConflictTask = xLocalItemTree.IterateTreeViewItemsAsync(localNextConflictFinder.IterationConsumer, inReverseOrder: true);

            NextConflictFinder remoteNextConflictFinder = new();
            Task findNextRemoteConflictTask = xRemoteItemTree.IterateTreeViewItemsAsync(remoteNextConflictFinder.IterationConsumer, inReverseOrder: true);

            ShowNavigatingToConflictLoader();
            await Task.WhenAll(findNextLocalConflictTask, findNextRemoteConflictTask);
            HideNavigatingToConflictLoader();

            if (localNextConflictFinder.NextConflictTreeViewItem != null)
            {
                xLocalItemTree.FocusItem(localNextConflictFinder.NextConflictTreeViewItem);
            }

            if (remoteNextConflictFinder.NextConflictTreeViewItem != null)
            {
                xRemoteItemTree.FocusItem(remoteNextConflictFinder.NextConflictTreeViewItem);
            }
        }

        private void xNextConflict_Click(object sender, RoutedEventArgs e)
        {
            _ = HighlightNextConflictAsync();
        }

        private async Task HighlightNextConflictAsync()
        {
            NextConflictFinder localNextConflictFinder = new();
            Task findNextLocalConflictTask = xLocalItemTree.IterateTreeViewItemsAsync(localNextConflictFinder.IterationConsumer);

            NextConflictFinder remoteNextConflictFinder = new();
            Task findNextRemoteConflictTask = xRemoteItemTree.IterateTreeViewItemsAsync(remoteNextConflictFinder.IterationConsumer);

            ShowNavigatingToConflictLoader();
            await Task.WhenAll(findNextLocalConflictTask, findNextRemoteConflictTask);
            HideNavigatingToConflictLoader();

            if (localNextConflictFinder.NextConflictTreeViewItem != null)
            {
                xLocalItemTree.FocusItem(localNextConflictFinder.NextConflictTreeViewItem);
            }

            if (remoteNextConflictFinder.NextConflictTreeViewItem != null)
            {
                xRemoteItemTree.FocusItem(remoteNextConflictFinder.NextConflictTreeViewItem);
            }
        }

        private sealed class NextConflictFinder
        {
            public TreeViewItem? NextConflictTreeViewItem { get; private set; }

            public bool IterationConsumer(TreeViewItem currentTreeViewItem)
            {
                Comparison comparison = (Comparison)((ITreeViewItem)currentTreeViewItem.Tag).NodeObject();
                bool continueIteration = true;

                bool IsAddedOrDeleted = comparison.State is Comparison.StateType.Added or Comparison.StateType.Deleted;
                bool canBeSelected = comparison.IsSelectionEnabled;
                bool selfAndSiblingNotSelected = !comparison.Selected && (!comparison.HasSiblingComparison || !comparison.SiblingComparison.Selected);

                if (IsAddedOrDeleted && canBeSelected && selfAndSiblingNotSelected)
                {
                    NextConflictTreeViewItem = currentTreeViewItem;
                    continueIteration = false;
                }

                return continueIteration;
            }
        }

        private void xLocalItemTreeScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            xRemoteItemTreeScroller.ScrollToVerticalOffset(xLocalItemTreeScroller.VerticalOffset);
        }

        private void xRemoteItemTreeScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            xLocalItemTreeScroller.ScrollToVerticalOffset(xRemoteItemTreeScroller.VerticalOffset);
        }
    }
}
