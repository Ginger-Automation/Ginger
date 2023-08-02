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
using System.Threading.Tasks;

namespace Ginger.ConflictResolve
{

    public class Conflict
    {
        public enum ResolutionType
        {
            [EnumValueDescription("Accept Server Changes")]
            AcceptServer,
            [EnumValueDescription("Keep Local Changes")]
            KeepLocal,
            [EnumValueDescription("Cherry Pick Changes")]
            CherryPick
        }

        private RepositoryItemBase _localItem;
        private RepositoryItemBase _remoteItem;
        private Comparison _comparison;
        private bool _isComparisonLoaded = false;

        public string Path { get; }

        public string RelativePath { get; }

        public ResolutionType Resolution { get; set; }

        public Comparison Comparison
        {
            get
            {
                if (!_isComparisonLoaded)
                {
                    LoadComparison();
                }

                return _comparison;
            }
        }

        public Conflict(string conflictPath)
        {
            Path = conflictPath;
            _localItem = null!;
            _remoteItem = null!;
            _comparison = null!;
            RelativePath = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(Path);
        }

        private void LoadComparison()
        {
            _isComparisonLoaded = true;

            _localItem = SourceControlIntegration.GetLocalItemFromConflict(WorkSpace.Instance.SourceControl, Path);
            _remoteItem = SourceControlIntegration.GetRemoteItemFromConflict(WorkSpace.Instance.SourceControl, Path);
            _comparison = SourceControlIntegration.CompareConflictedItems(_localItem, _remoteItem);
        }

        public RepositoryItemBase GetMergedItem()
        {
            return SourceControlIntegration.CreateMergedItemFromComparison(Comparison);
        }
    }
}
