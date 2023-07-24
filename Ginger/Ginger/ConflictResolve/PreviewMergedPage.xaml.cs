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
        private Comparison? _dummyComparison;

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
            }
        }

        private void OnWizardPageActive(ResolveMergeConflictWizard wizard)
        {
            if (wizard.RootComparison == null)
                throw new InvalidOperationException("Cannot preview merge result before creating comparison.");

            Task.Run(() =>
            {
                ShowLoading();
                BusinessFlow mergedBF = MergeBusinessFlowComparison(wizard.RootComparison);
                Comparison dummyComparison = CompareAgainstNull(mergedBF);
                SetTreeItems(dummyComparison);
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
                xTree.AddItem(new ConflictMergeTreeViewItem(comparison));
            });
        }

        private BusinessFlow MergeBusinessFlowComparison(Comparison comparison)
        {
            ConflictResolver conflictResolver = new();
            BusinessFlow? mergedBF = conflictResolver.Merge<BusinessFlow>(comparison.ChildComparisons);
            if (mergedBF == null)
                throw new Exception("Merged business flow is null");
            return mergedBF;
        }

        private Comparison CompareAgainstNull(BusinessFlow bf)
        {
            ConflictResolver conflictResolver = new();
            ICollection<Comparison> businessFlowComparison = conflictResolver.Compare(bf, null, name: "[0]");
            State state;
            if (businessFlowComparison.All(c => c.State == State.Unmodified))
                state = State.Unmodified;
            else
                state = State.Modified;
            return new Comparison(name: "Business Flows", state, childComparisons: businessFlowComparison, dataType: null!);
        }
    }
}
//case EventType.Active:
//    ICollection<Comparison> comparison = RIBCompare.Compare("Business Flows", wizard.MergedBusinessFlow, null);
//    State state = comparison.All(c => c.State == State.Unmodified) ? State.Unmodified : State.Modified;
//    _dummyComparison = new Comparison("ROOT", state, childComparisons: comparison, dataType: null!);
//    xTree.Items.Clear();
//    xTree.Items.Add(_dummyComparison);
//    break;