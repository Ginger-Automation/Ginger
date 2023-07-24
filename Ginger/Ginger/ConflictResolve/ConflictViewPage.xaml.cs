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
                    OnWizardPageLeavingForNextPage(wizard);
                    break;
            }
        }

        private void OnWizardPageInit(ResolveMergeConflictWizard wizard)
        {
            Task.Run(() =>
            {
                ShowLoading();
                wizard.RootComparison = CompareBusinessFlows(wizard.LocalBusinessFlow, wizard.RemoteBusinessFlow);
                SetTreeItems(wizard.RootComparison);
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

        private Comparison CompareBusinessFlows(BusinessFlow localBF, BusinessFlow remoteBF)
        {
            ConflictResolver conflictResolver = new();
            ICollection<Comparison> businessFlowComparison = conflictResolver.Compare(localBF, remoteBF, name: "[0]");
            State state;
            if (businessFlowComparison.All(c => c.State == State.Unmodified))
                state = State.Unmodified;
            else
                state = State.Modified;
            return new Comparison(name: "Business Flows", state, childComparisons: businessFlowComparison, dataType: null!);
        }

        private void SetTreeItems(Comparison comparison)
        {
            Dispatcher.Invoke(() =>
            {
                xLocalItemTree.AddItem(new ConflictComparisonTreeViewItem(comparison, new[] { State.Unmodified, State.Modified, State.Deleted }));
                xRemoteItemTree.AddItem(new ConflictComparisonTreeViewItem(comparison, new[] { State.Unmodified, State.Modified, State.Added }));
            });
        }

        private void OnWizardPageActive(ResolveMergeConflictWizard wizard)
        {

        }

        private void OnWizardPageLeavingForNextPage(ResolveMergeConflictWizard wizard)
        {

        }
    }
}
