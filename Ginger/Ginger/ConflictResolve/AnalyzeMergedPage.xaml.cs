using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Repository;
using Ginger.AnalyzerLib;
using Ginger.Run;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for AnalyeMergedPage.xaml
    /// </summary>
    public partial class AnalyeMergedPage : Page, IWizardPage
    {
        private readonly AnalyzerPage _analyzerPage;

        public AnalyeMergedPage()
        {
            InitializeComponent();
            _analyzerPage = new();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            ResolveMergeConflictWizard wizard = (ResolveMergeConflictWizard)WizardEventArgs.Wizard;

            switch(WizardEventArgs.EventType)
            {
                case EventType.Active:
                    OnWizardActivePage(wizard);
                    break;
            }
        }

        private void OnWizardActivePage(ResolveMergeConflictWizard wizard)
        {
            Task.Run(() =>
            {
                ShowLoading();
                bool hasMergedItem = wizard.TryGetOrCreateMergedItem(out RepositoryItemBase? mergedItem);
                if (hasMergedItem && mergedItem != null)
                {
                    if (mergedItem is BusinessFlow mergedBusinessFlow)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            _analyzerPage.Init(mergedBusinessFlow, WorkSpace.Instance.AutomateTabSelfHealingConfiguration.AutoFixAnalyzerIssue);
                            xAnalyzerPageFrame.ClearAndSetContent(_analyzerPage);
                        });
                        _analyzerPage.AnalyzeWithUI().Wait();
                    }
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
    }
}
