using GingerCore;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ConflictResolve
{
    public class ResolveMergeConflictWizard : WizardBase, INotifyPropertyChanged
    {
        public override string Title => "Resolve Merge Conflict";

        public BusinessFlow LocalBusinessFlow { get; }
        public BusinessFlow RemoteBusinessFlow { get; }

        private Comparison? _rootComparison;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Comparison? RootComparison 
        {
            get => _rootComparison;
            set
            {
                if(_rootComparison != value)
                {
                    _rootComparison = value;
                    PropertyChanged?.Invoke(sender: this, new PropertyChangedEventArgs(nameof(RootComparison)));
                }
            }
        }

        public BusinessFlow? MergedBusinessFlow { get; set; }

        public ResolveMergeConflictWizard(BusinessFlow localBF, BusinessFlow remoteBF)
        {
            LocalBusinessFlow = localBF;
            RemoteBusinessFlow = remoteBF;

            //ComparisonResult = RIBCompare.Compare("Business Flows", localBF, remoteBF);

            AddPage(
                Name: "Name - Compare And Select",
                Title: "Title - Compare and Select",
                SubTitle: "SubTitle - Compare and Select",
                new ConflictViewPage());

            //AddPage(
            //    Name: "Name - Compare And Select",
            //    Title: "Title - Compare and Select",
            //    SubTitle: "SubTitle - Compare and Select",
            //    new TreeViewComparisonPage());

            AddPage(
                Name: "Name - Preview Merged",
                Title: "Title - Preview Merged", 
                SubTitle: "SubTitle - Preview Merged",
                new PreviewMergedPage());
        }

        public override void Finish()
        {
            throw new NotImplementedException();
        }
    }
}
