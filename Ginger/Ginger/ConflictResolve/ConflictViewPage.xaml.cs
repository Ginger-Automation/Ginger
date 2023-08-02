using Amdocs.Ginger.Common.SourceControlLib;
using DocumentFormat.OpenXml.Bibliography;
using GingerCore;
using GingerTest.WizardLib;
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
                throw new InvalidOperationException($"{nameof(ConflictViewPage)} must be used with {nameof(ResolveMergeConflictWizard)}.");

            switch (wizardEventArgs.EventType)
            {
                case EventType.Init:
                    OnWizardPageInit(wizard);
                    break;
                case EventType.Active:
                    OnWizardPageActive(wizard);
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
            Dispatcher.Invoke((Action)(() =>
            {
                xLocalItemTree.AddItem(new ConflictComparisonTreeViewItem(comparison, (Comparison.StateType[])(new[] { Comparison.StateType.Unmodified, Comparison.StateType.Modified, Comparison.StateType.Deleted })));
                xRemoteItemTree.AddItem(new ConflictComparisonTreeViewItem(comparison, (Comparison.StateType[])(new[] { Comparison.StateType.Unmodified, Comparison.StateType.Modified, Comparison.StateType.Added })));
            }));
        }

        private void OnWizardPageActive(ResolveMergeConflictWizard wizard)
        {

        }

        private void OnWizardPageLeavingForNextPage(ResolveMergeConflictWizard wizard, WizardEventArgs eventArgs)
        {
            if (!wizard.Comparison.CanBeMerged())
            {
                eventArgs.CancelEvent = true;
            }
        }
    }
}
