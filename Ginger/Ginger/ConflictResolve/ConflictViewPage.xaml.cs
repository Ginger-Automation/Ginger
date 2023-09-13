using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.SourceControlLib;
using GingerCore;
using GingerTest.WizardLib;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.AxHost;

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for ConflictViewPage.xaml
    /// </summary>
    public partial class ConflictViewPage : Page, IWizardPage
    {
        private Comparison _wizardComparison;

        public ConflictViewPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs wizardEventArgs)
        {
            ResolveMergeConflictWizard? wizard = wizardEventArgs.Wizard as ResolveMergeConflictWizard;

            if (wizard == null)
                throw new InvalidOperationException($"{nameof(ConflictViewPage)} must be used with {nameof(ResolveMergeConflictWizard)}.");

            switch (wizardEventArgs.EventType)
            {
                case EventType.Init:
                    OnWizardPageInit(wizard);
                    break;
                case EventType.LeavingForNextPage:
                    OnWizardPageLeavingForNextPage(wizard, wizardEventArgs);
                    break;
            }
        }

        private void OnWizardPageInit(ResolveMergeConflictWizard wizard)
        {
            Task.Run(() =>
            {
                ShowLoading();
                _wizardComparison = wizard.Comparison;
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
                    xLoadingFrame.Content = new LoadingPage();
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
                Reporter.ToUser(eUserMsgKey.HasUnselectedConflicts, messageArgs: unselectedComparisonCount);
                eventArgs.CancelEvent = true;
            }
        }

        private void xPrevConflict_Click(object sender, RoutedEventArgs e)
        {
            ITreeViewItem? previousConflictTVI = null;
            Func<ITreeViewItem, bool> iterationConsumer = tvi =>
            {
                Comparison comparison = (Comparison)tvi.NodeObject();
                bool continueIteration = true;

                bool IsAddedOrDeleted = comparison.State == Comparison.StateType.Added || comparison.State == Comparison.StateType.Deleted;
                bool selfAndSiblingNotSelected = !comparison.Selected && (!comparison.HasSiblingComparison || !comparison.SiblingComparison.Selected);

                if (IsAddedOrDeleted && selfAndSiblingNotSelected)
                {
                    previousConflictTVI = tvi;
                    continueIteration = false;
                }

                return continueIteration;
            };

            xLocalItemTree.IterateTreeViewItems(iterationConsumer, inReverseOrder: true);
            if (previousConflictTVI != null)
            {
                xLocalItemTree.SelectItem(previousConflictTVI);
            }

            xRemoteItemTree.IterateTreeViewItems(iterationConsumer, inReverseOrder: true);
            if (previousConflictTVI != null)
            {
                xRemoteItemTree.SelectItem(previousConflictTVI);
            }
        }

        private void xNextConflict_Click(object sender, RoutedEventArgs e)
        {
            ITreeViewItem? nextConflictTVI = null;
            Func<ITreeViewItem, bool> iterationConsumer = tvi =>
            {
                Comparison comparison = (Comparison)tvi.NodeObject();
                bool continueIteration = true;

                bool IsAddedOrDeleted = comparison.State == Comparison.StateType.Added || comparison.State == Comparison.StateType.Deleted;
                bool selfAndSiblingNotSelected = !comparison.Selected && (!comparison.HasSiblingComparison || !comparison.SiblingComparison.Selected);

                if (IsAddedOrDeleted && selfAndSiblingNotSelected)
                {
                    nextConflictTVI = tvi;
                    continueIteration = false;
                }

                return continueIteration;
            };

            xLocalItemTree.IterateTreeViewItems(iterationConsumer);
            if (nextConflictTVI != null)
            {
                xLocalItemTree.SelectItem(nextConflictTVI);
            }

            xRemoteItemTree.IterateTreeViewItems(iterationConsumer);
            if (nextConflictTVI != null)
            {
                xRemoteItemTree.SelectItem(nextConflictTVI);
            }
        }
    }
}
