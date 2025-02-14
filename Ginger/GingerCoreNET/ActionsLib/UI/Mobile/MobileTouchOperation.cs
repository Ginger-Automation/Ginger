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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.CoreNET.ActionsLib.UI.Mobile
{

    public class MobileTouchOperation : RepositoryItemBase
    {

        public enum eFingerOperationType
        {
            [EnumValueDescription("Finger Move")]
            FingerMove,
            [EnumValueDescription("Finger Down")]
            FingerDown,
            [EnumValueDescription("Finger Up")]
            FingerUp,            
            Pause, 
            Cancel
        }

        private eFingerOperationType mOperationType = eFingerOperationType.FingerMove;
        [IsSerializedForLocalRepository]
        public eFingerOperationType OperationType
        {
            get
            {
                return mOperationType;
            }
            set
            {
                if (value != mOperationType)
                {
                    mOperationType = value;
                    if (mOperationType != eFingerOperationType.FingerMove)
                    {
                        MoveXcoordinate = null;
                        OnPropertyChanged(nameof(MoveXcoordinate));
                        MoveYcoordinate = null;
                        OnPropertyChanged(nameof(MoveYcoordinate));
                        if (mOperationType != eFingerOperationType.Pause)
                        {
                            OperationDuration = null;
                            OnPropertyChanged(nameof(OperationDuration));
                        }
                    }
                    OnPropertyChanged(nameof(OperationType));
                }
            }
        }

        private int? mMoveXcoordinatee;
        [IsSerializedForLocalRepository]
        public int? MoveXcoordinate
        {
            get
            {
                return mMoveXcoordinatee;
            }
            set
            {
                if (value != mMoveXcoordinatee)
                {
                    mMoveXcoordinatee = value;
                    if (mOperationType != eFingerOperationType.FingerMove)
                    {
                        mMoveXcoordinatee = null;
                    }
                    OnPropertyChanged(nameof(MoveXcoordinate));
                }
            }
        }

        private int? mMoveYcoordinate;
        [IsSerializedForLocalRepository]
        public int? MoveYcoordinate
        {
            get
            {
                return mMoveYcoordinate;
            }
            set
            {
                if (value != mMoveYcoordinate)
                {
                    mMoveYcoordinate = value;
                    if (mOperationType != eFingerOperationType.FingerMove)
                    {
                        mMoveYcoordinate = null;
                    }
                    OnPropertyChanged(nameof(MoveYcoordinate));
                }
            }
        }

        private int? mOperationDuration;
        [IsSerializedForLocalRepository]
        public int? OperationDuration
        {
            get
            {
                return mOperationDuration;
            }
            set
            {
                if (value != mOperationDuration)
                {
                    mOperationDuration = value;
                    if (mOperationType != eFingerOperationType.FingerMove && mOperationType != eFingerOperationType.Pause)
                    {
                        mOperationDuration = null;
                    }
                    OnPropertyChanged(nameof(OperationDuration));
                }
            }
        }

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }
    }
}
