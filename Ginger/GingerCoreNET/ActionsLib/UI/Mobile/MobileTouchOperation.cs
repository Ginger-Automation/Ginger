using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.CoreNET.ActionsLib.UI.Mobile
{

    public class MobileTouchOperation : RepositoryItemBase
    {

        public enum eFingerOperationType
        {
            Down, Up, Move, Pause, Cancel
        }

        private eFingerOperationType mOperationType;
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
                    //if (mOperationType != eFingerOperationType.Move)
                    //{
                    //    mMoveXcoordinatee = null;
                    //}
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
                    //if (mOperationType != eFingerOperationType.Move)
                    //{
                    //    mMoveYcoordinate = null;
                    //}
                    OnPropertyChanged(nameof(MoveYcoordinate));
                }
            }
        }

        private int? mMoveDuration;
        [IsSerializedForLocalRepository]
        public int? MoveDuration
        {
            get
            {
                return mMoveDuration;
            }
            set
            {
                if (value != mMoveDuration)
                {
                    mMoveDuration = value;
                    if (mOperationType != eFingerOperationType.Move)
                    {
                        mMoveDuration = null;
                    }
                    OnPropertyChanged(nameof(MoveDuration));
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
