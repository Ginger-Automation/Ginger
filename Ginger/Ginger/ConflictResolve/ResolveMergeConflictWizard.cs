#region License
/*
Copyright © 2014-2024 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Repository;
using Ginger.WizardLib;
using GingerWPF.WizardLib;
using System;

namespace Ginger.ConflictResolve
{
    public class ResolveMergeConflictWizard : WizardBase
    {
        private readonly Conflict _conflict;
        private AnalyeMergedPage? _analyzeMergedPage;

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
                Name: "Introduction",
                Title: "Introduction",
                SubTitle: "Conflict Resolve Introduction",
                Page: new WizardIntroPage("/ConflictResolve/ConflictResolveIntro.md"));

            AddPage(
                Name: "CompareAndSelect",
                Title: "Compare and Select Conflicts",
                SubTitle: "Compare and Select Conflicts",
                new ConflictViewPage());

            Type? conflictedItemType = GetConflictedItemType();
            if (conflictedItemType != null && AnalyeMergedPage.IsTypeSupportedForIsolatedAnalyzation(conflictedItemType))
            {
                _analyzeMergedPage = new();
                AddPage(
                Name: "Analyze",
                Title: "Analyze",
                SubTitle: "Analyze Merged Item",
                Page: _analyzeMergedPage);
            }

            AddPage(
                Name: "PreviewMergedEntity",
                Title: "Preview Merged Entity",
                SubTitle: "Preview Merged Entity",
                new PreviewMergedPage());
        }

        private Type? GetConflictedItemType()
        {
            RepositoryItemBase? localItem = _conflict.GetLocalItem();
            if (localItem != null)
            {
                return localItem.GetType();
            }
            RepositoryItemBase? remoteItem = _conflict.GetRemoteItem();
            if (remoteItem != null)
            {
                return remoteItem.GetType();
            }
            return null;
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
            bool hasUnselectedComparison = unselectedComparisonCount > 0;

            int unhandledMandatoryIssueCount = _analyzeMergedPage != null ? _analyzeMergedPage.GetUnhandledMandatoryIssueCount() : 0;
            bool hasUnhandledMandatoryIssues = unhandledMandatoryIssueCount > 0;
            if (hasUnselectedComparison || hasUnhandledMandatoryIssues)
            {
                _conflict.DiscardMergedItem();
                if (hasUnselectedComparison)
                {
                    Reporter.ToUser(eUserMsgKey.HasUnhandledConflicts, unselectedComparisonCount);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.HasUnhandledMandatoryIssues, unhandledMandatoryIssueCount);
                }
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

        public override void Cancel()
        {
            int unselectedComparisonCount = _conflict.Comparison.UnselectedComparisonCount();
            bool hasUnselectedComparison = unselectedComparisonCount > 0;

            int unhandledMandatoryIssueCount = _analyzeMergedPage != null ? _analyzeMergedPage.GetUnhandledMandatoryIssueCount() : 0;
            bool hasUnhandledMandatoryIssues = unhandledMandatoryIssueCount > 0;
            if (hasUnselectedComparison || hasUnhandledMandatoryIssues)
            {
                _conflict.DiscardMergedItem();
            }
            base.Cancel();
        }
    }
}
