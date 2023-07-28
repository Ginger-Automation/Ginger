using Amdocs.Ginger.Common.SourceControlLib;
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
            Task.Run(() =>
            {
                ShowLoading();
                Comparison dummyComparison = SourceControlIntegration.PreviewManualConflictResolve(wizard.Comparison);
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
    }
}
//case EventType.Active:
//    ICollection<Comparison> comparison = RIBCompare.Compare("Business Flows", wizard.MergedBusinessFlow, null);
//    State state = comparison.All(c => c.State == State.Unmodified) ? State.Unmodified : State.Modified;
//    _dummyComparison = new Comparison("ROOT", state, childComparisons: comparison, dataType: null!);
//    xTree.Items.Clear();
//    xTree.Items.Add(_dummyComparison);
//    break;