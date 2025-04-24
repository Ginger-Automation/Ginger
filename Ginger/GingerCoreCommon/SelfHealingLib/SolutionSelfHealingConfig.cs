#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Repository;
using System;

namespace Amdocs.Ginger.Common.SelfHealingLib
{
    public sealed class SolutionSelfHealingConfig : RepositoryItemBase
    {
        private bool _usePropertyMatcher = true;
        private bool _useImageMatcher = true;
        private int _propertyMatcherAcceptableScore = 60;
        private int _imageMatcherAcceptableScore = 60;

        public override string ItemName { get => string.Empty; set { } }

        [IsSerializedForLocalRepository(DefaultValue: true)]
        public bool UsePropertyMatcher
        {
            get => _usePropertyMatcher;
            set
            {
                _usePropertyMatcher = value;
                OnPropertyChanged(nameof(UsePropertyMatcher));
            }
        }

        [IsSerializedForLocalRepository]
        public int PropertyMatcherAcceptableScore
        {
            get => _propertyMatcherAcceptableScore;
            set
            {
                if (value < 1 || value > 100)
                {
                    throw new ArgumentException("value must be between 1-100");
                }
                _propertyMatcherAcceptableScore = value;
                OnPropertyChanged(nameof(PropertyMatcherAcceptableScore));
            }
        }

        [IsSerializedForLocalRepository]
        public bool UseImageMatcher
        {
            get => _useImageMatcher;
            set
            {
                _useImageMatcher = value;
                OnPropertyChanged(nameof(UseImageMatcher));
            }
        }

        [IsSerializedForLocalRepository]
        public int ImageMatcherAcceptableScore
        {
            get => _imageMatcherAcceptableScore;
            set
            {
                if (value < 1 || value > 100)
                {
                    throw new ArgumentException("value must be between 1-100");
                }
                _imageMatcherAcceptableScore = value;
                OnPropertyChanged(nameof(ImageMatcherAcceptableScore));
            }
        }
    }
}
