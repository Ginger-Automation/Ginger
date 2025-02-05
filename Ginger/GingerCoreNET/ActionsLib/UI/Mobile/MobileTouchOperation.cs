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
