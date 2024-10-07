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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Repository;
using Ginger.SourceControl;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Ginger.ConflictResolve
{

    public class Conflict : INotifyPropertyChanged
    {
        public enum ResolutionType
        {
            [EnumValueDescription("Accept Server Changes")]
            AcceptServer,
            [EnumValueDescription("Keep Local Changes")]
            KeepLocal,
            [EnumValueDescription("Merge Conflicts")]
            CherryPick
        }

        private RepositoryItemBase? _localItem;
        private RepositoryItemBase? _remoteItem;
        private RepositoryItemBase? _mergedItem;
        private bool _isMergedItemSet = false;
        private Comparison _comparison;
        private bool _isComparisonLoaded = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Path { get; }

        public string RelativePath { get; }

        private bool _isSelectedForResolution;
        public bool IsSelectedForResolution
        {
            get => _isSelectedForResolution;
            set
            {
                if (_isSelectedForResolution != value)
                {
                    _isSelectedForResolution = value;
                    OnPropertyChanged(nameof(IsSelectedForResolution));
                }
            }
        }

        private ResolutionType _resolution;
        public ResolutionType Resolution
        {
            get => _resolution;
            set
            {
                if (_resolution != value)
                {
                    _resolution = value;
                    CanResolve = CalculateCanResolve();
                    OnPropertyChanged(nameof(Resolution));
                }
            }
        }

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

        private bool _canResolve;
        public bool CanResolve
        {
            get => _canResolve;
            private set
            {
                if (_canResolve != value)
                {
                    _canResolve = value;
                    OnPropertyChanged(nameof(CanResolve));
                }
            }
        }

        public IEnumerable<ResolutionType> PossibleResolutions { get; }

        public IEnumerable<ComboEnumItem> PossibleResolutionsComboEnumItems { get; }

        public Conflict(string conflictPath, IEnumerable<ResolutionType> possibleResolutions)
        {
            Path = conflictPath;
            _localItem = SourceControlIntegration.GetLocalItemFromConflict(WorkSpace.Instance.SourceControl, Path);
            _remoteItem = SourceControlIntegration.GetRemoteItemFromConflict(WorkSpace.Instance.SourceControl, Path);
            _comparison = null!;
            _canResolve = CalculateCanResolve();
            RelativePath = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(Path);
            PossibleResolutions = new List<ResolutionType>(possibleResolutions);

            List<ComboEnumItem> resolutionOptions = GingerCore.General.GetEnumValuesForCombo(typeof(ResolutionType));
            PossibleResolutionsComboEnumItems = resolutionOptions.Where(r => PossibleResolutions.Contains((ResolutionType)r.Value));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void LoadComparison()
        {
            _isComparisonLoaded = true;
            _comparison = SourceControlIntegration.CompareConflictedItems(_localItem, _remoteItem);
            _comparison.PropertyChanged += (_, args) =>
            {
                if (string.Equals(args.PropertyName, nameof(Comparison.ChildComparisons)))
                {
                    DiscardMergedItem();
                }
            };
        }

        public RepositoryItemBase? GetLocalItem()
        {
            return _localItem;
        }

        public RepositoryItemBase? GetRemoteItem()
        {
            return _remoteItem;
        }

        public bool TryGetMergedItem(out RepositoryItemBase? mergedItem)
        {
            if (_isMergedItemSet)
            {
                mergedItem = _mergedItem;
                return true;
            }
            else
            {
                mergedItem = null;
                return false;
            }
        }

        public bool TryCreateAndSetMergedItemFromComparison()
        {
            return TryCreateAndSetMergedItemFromComparison(out RepositoryItemBase? _);
        }

        public bool TryCreateAndSetMergedItemFromComparison(out RepositoryItemBase? mergedItem)
        {
            int unselectedComparisonCount = Comparison.UnselectedComparisonCount();
            if (unselectedComparisonCount > 0)
            {
                mergedItem = null;
                return false;
            }

            _mergedItem = SourceControlIntegration.CreateMergedItemFromComparison(Comparison);
            mergedItem = _mergedItem;
            _isMergedItemSet = true;
            CanResolve = CalculateCanResolve();
            return true;
        }

        public void DiscardMergedItem()
        {
            _mergedItem = null;
            _isMergedItemSet = false;
            CanResolve = CalculateCanResolve();
        }

        private bool CalculateCanResolve()
        {
            return
                _resolution == ResolutionType.KeepLocal ||
                _resolution == ResolutionType.AcceptServer ||
                (_resolution == ResolutionType.CherryPick && _isMergedItemSet);
        }
    }
}
