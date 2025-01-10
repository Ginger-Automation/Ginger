using System;
using Amdocs.Ginger.Repository;

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
