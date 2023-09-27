using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Repository;
using Ginger.SourceControl;
using GingerCore;
using GingerWPF.WizardLib;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

            switch(WizardEventArgs.EventType)
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
                bool hasMergedItem = wizard.TryGetOrCreateMergedItem(out RepositoryItemBase ? mergedItem);
                if (hasMergedItem)
                {
                    Comparison mergedItemComparison = SourceControlIntegration.CompareConflictedItems(mergedItem, null);
                    SetTreeItems(mergedItemComparison);
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
    }
}