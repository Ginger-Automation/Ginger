using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Repository;
using Ginger.SourceControl;
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
        private readonly Conflict _conflictResolve;

        public override string Title => "Resolve Merge Conflict";

        public Comparison Comparison => _conflictResolve.Comparison;

        public ResolveMergeConflictWizard(Conflict conflictResolve)
        {
            _conflictResolve = conflictResolve;
            AddPages();
        }

        private void AddPages()
        {
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

        public RepositoryItemBase GetMergedItem()
        {
            return _conflictResolve.GetMergedItem();
        }

        public override void Finish()
        {

        }
    }
}
