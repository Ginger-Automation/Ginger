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
                Name: "CompareAndSelect",
                Title: "Compare and Select Conflicts",
                SubTitle: "Compare and Select Conflicts",
                new ConflictViewPage());

            AddPage(
                Name: "PreviewMergedResult",
                Title: "Preview Merged Result",
                SubTitle: "Preview Merged Result",
                new PreviewMergedPage());
        }

        public RepositoryItemBase GetMergedItem()
        {
            return _conflictResolve.GetMergedItem();
        }

        public override void Finish()
        {
            //no action required
        }
    }
}
