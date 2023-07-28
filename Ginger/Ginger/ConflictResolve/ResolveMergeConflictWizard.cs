using Amdocs.Ginger.Common.SourceControlLib;
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
    public class ResolveMergeConflictWizard : WizardBase
    {
        public override string Title => "Resolve Merge Conflict";

        public Comparison Comparison { get; }

        public BusinessFlow? MergedBusinessFlow { get; set; }

        public ResolveMergeConflictWizard(Comparison comparison)
        {
            Comparison = comparison;

            AddPage(
                Name: "Name - Compare And Select",
                Title: "Title - Compare and Select",
                SubTitle: "SubTitle - Compare and Select",
                new ConflictViewPage());

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
