#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Repository;
using Ginger.SourceControl;
using GingerCore;
using GingerTest.WizardLib;
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
        private readonly Conflict _conflict;

        public override string Title => "Resolve Merge Conflict";

        public Comparison Comparison => _conflict.Comparison;

        public ResolveMergeConflictWizard(Conflict conflict)
        {
            _conflict = conflict;
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

        public bool TryGetOrCreateMergedItem(out RepositoryItemBase? mergedItem)
        {
            bool hasMergedItem = _conflict.TryGetMergedItem(out mergedItem);
            if (hasMergedItem)
            {
                return true;
            }
            else
            {
                bool wasCreated = _conflict.TryCreateAndSetMergedItemFromComparison(out mergedItem);
                return wasCreated;
            }
        }

        public override void Finish()
        {
            int unselectedComparisonCount = _conflict.Comparison.UnselectedComparisonCount();
            if (unselectedComparisonCount > 0)
            {
                //since the currenct state of this conflict has unhandled comparisons, discard the previously created merged item
                _conflict.DiscardMergedItem();
                Reporter.ToUser(eUserMsgKey.HasUnhandledConflicts, unselectedComparisonCount);
            }
            else
            {
                //if merged item is not already created, then create it
                if (!_conflict.TryGetMergedItem(out RepositoryItemBase? _))
                {
                    _conflict.TryCreateAndSetMergedItemFromComparison();
                }
            }
        }
    }
}
