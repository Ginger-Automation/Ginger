using GingerCore;
using GingerWPF.WizardLib;
using Microsoft.Office.Interop.Outlook;
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
using static System.Windows.Forms.AxHost;

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for TreeViewComparisonPage.xaml
    /// </summary>
    public partial class TreeViewComparisonPage : Page, IWizardPage
    {
        private ICollection<Comparison>? _comparisonResult;
        private Comparison? _rootComparison;

        public TreeViewComparisonPage()
        {
            InitializeComponent();
            
            //xTree.Items.Add(CreateTreeViewItemForComparisonResult(comparisonResult));
        }

        private TreeViewItem CreateTreeViewItemForComparisonResult(Comparison comparisonResult)
        {
            TreeViewItem treeViewItem = null!;

            if(comparisonResult.HasChildComparisons)
            {
                treeViewItem = new()
                {
                    Header = comparisonResult.Name,
                    Background = GetColorByState(comparisonResult.State),
                };
                foreach (Comparison childComparisonResult in comparisonResult.ChildComparisons)
                    treeViewItem.Items.Add(CreateTreeViewItemForComparisonResult(childComparisonResult));
                return treeViewItem;
            }
            else if(comparisonResult.HasData)
            {
                treeViewItem = new TreeViewItem()
                {
                    Header = $"{comparisonResult.Name}: {comparisonResult.Data}",
                    Background = GetColorByState(comparisonResult.State),
                };
            }

            return treeViewItem;
        }

        public SolidColorBrush GetColorByState(State state)
        {
            switch(state)
            {
                case State.Unmodified:
                    return Brushes.Transparent;
                case State.Modified:
                    return Brushes.Yellow;
                case State.Added:
                    return Brushes.PaleGreen;
                case State.Deleted:
                    return Brushes.PaleVioletRed;
                default:
                    throw new NotImplementedException();
            }
        }

        //public void WizardEvent(WizardEventArgs WizardEventArgs)
        //{
        //    ResolveMergeConflictWizard wizard = (ResolveMergeConflictWizard)WizardEventArgs.Wizard;
        //    switch (WizardEventArgs.EventType)
        //    {
        //        case EventType.Init:
        //            if (_rootComparison == null)
        //            {
        //                _comparisonResult = RIBCompare.Compare("Business Flows", wizard.LocalBusinessFlow, wizard.RemoteBusinessFlow);
        //                wizard.ComparisonResult = _comparisonResult;
        //                State state = _comparisonResult.All(c => c.State == State.Unmodified) ? State.Unmodified : State.Modified;
        //                _rootComparison = new Comparison("ROOT", state, childComparisons: _comparisonResult, dataType: null!);
        //            }
        //            xTree.Items.Clear();
        //            xTree.Items.Add(_rootComparison);
        //            break;
        //        case EventType.Active:
        //            xTree.Items.Clear();
        //            xTree.Items.Add(_comparisonResult);
        //            break;
        //        case EventType.LeavingForNextPage:
        //            if (_comparisonResult == null)
        //                throw new InvalidOperationException("Cannot merge, since Comparison Results are null.");

        //            wizard.MergedBusinessFlow = RIBMerge.Merge<BusinessFlow>(_comparisonResult);
        //            break;
        //    }
        //}


        private ConflictComparisonTreeViewItem treeItem;

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            ResolveMergeConflictWizard wizard = (ResolveMergeConflictWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {
                //case EventType.Init:
                //    wizard.RootComparison = new ConflictResolver().Compare(wizard.LocalBusinessFlow, wizard.RemoteBusinessFlow);
                //    State state = wizard.ComparisonResult.All(c => c.State == State.Unmodified) ? State.Unmodified : State.Modified;
                //    _rootComparison = new Comparison("Business Flows", state, childComparisons: wizard.ComparisonResult, dataType: null!);
                //    treeItem = new ConflictResolutionTreeViewItem(_rootComparison);
                //    xUCTree.AddItem(treeItem);
                //    break;
                //case EventType.Active:
                //    xUCTree.ClearTreeItems();
                //    xUCTree.AddItem(treeItem);
                //    break;
                //case EventType.LeavingForNextPage:
                //    if (_comparisonResult == null)
                //        throw new InvalidOperationException("Cannot merge, since Comparison Results are null.");

                //    wizard.MergedBusinessFlow = RIBMerge.Merge<BusinessFlow>(_comparisonResult);
                //    break;
            }
        }
    }
}
