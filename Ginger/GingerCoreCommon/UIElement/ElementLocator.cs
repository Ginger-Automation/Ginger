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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using System;


namespace Amdocs.Ginger.Common.UIElement
{
    public class ElementLocator : RepositoryItemBase
    {
        private bool mActive { get; set; }

        [IsSerializedForLocalRepository]
        public bool Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }

        private ePosition mPosition { get; set; }
        /// <summary>
        /// It is used of Friendly Locator Position e.g. Left, Right, Above etc.
        /// </summary>
        [IsSerializedForLocalRepository]
        public ePosition Position { get { return mPosition; } set { if (mPosition != value) { mPosition = value; OnPropertyChanged(nameof(Position)); } } }

        private eLocateBy mLocateBy;
        [IsSerializedForLocalRepository]
        public eLocateBy LocateBy
        {
            get { return mLocateBy; }
            set { if (mLocateBy != value) { mLocateBy = value; OnPropertyChanged(nameof(LocateBy)); } }
        }

        bool mIsAutoLearned;
        [IsSerializedForLocalRepository]
        public bool IsAutoLearned
        {
            get { return mIsAutoLearned; }
            set { if (mIsAutoLearned != value) { mIsAutoLearned = value; OnPropertyChanged(nameof(IsAutoLearned)); } }
        }

        private string mLocateValue { get; set; }
        [IsSerializedForLocalRepository]
        public string LocateValue
        {
            get { return mLocateValue; }
            set { if (mLocateValue != value) { mLocateValue = value; OnPropertyChanged(nameof(LocateValue)); } }
        }

        private string mReferanceElement { get; set; }
        public string ReferanceElement
        {
            get { return mReferanceElement; }
            set { if (mReferanceElement != value) { mReferanceElement = value; OnPropertyChanged(nameof(ReferanceElement)); } }
        }

        private string mHelp { get; set; }

        public string Help { get { return mHelp; } set { if (mHelp != value) { mHelp = value; OnPropertyChanged(nameof(Help)); } } }

        private int? mCount { get; set; }
        public int? Count { get { return mCount; } set { if (mCount != value) { mCount = value; OnPropertyChanged(nameof(Count)); } } }

        private string mItemName;

        public override string ItemName
        {
            get
            {
                string currentExpectedName = LocateBy.ToString() + "-" + LocateValue;
                if (mItemName == null)
                {
                    mItemName = currentExpectedName;

                }
                return mItemName;
            }
            set
            {
                mItemName = value;
            }

        }

        public enum eLocateStatus
        {
            Unknown,
            Pending,
            Passed,
            Failed
        }

        eLocateStatus mLocateStatus;
        public eLocateStatus LocateStatus
        {
            get
            {
                return mLocateStatus;
            }
            set
            {
                mLocateStatus = value;
                OnPropertyChanged(nameof(StatusError));
                OnPropertyChanged(nameof(StatusIcon));

            }
        }

        public eImageType StatusIcon
        {
            get
            {
                switch (LocateStatus)
                {
                    case eLocateStatus.Passed:
                        return eImageType.Passed;
                    case eLocateStatus.Failed:
                        return eImageType.Failed;
                    case eLocateStatus.Pending:
                        return eImageType.Pending;
                    default:
                        return eImageType.Unknown;
                }
            }
        }

        private string mLocateStatusError;
        public string StatusError
        {
            get
            {
                return mLocateStatusError;
            }
            set
            {
                mLocateStatusError = value;
            }
        }

        private bool mEnableFriendlyLocator { get; set; }

        [IsSerializedForLocalRepository]
        public bool EnableFriendlyLocator { get { return mEnableFriendlyLocator; } set { if (mEnableFriendlyLocator != value) { mEnableFriendlyLocator = value; OnPropertyChanged(nameof(EnableFriendlyLocator)); } } }
    }
}
