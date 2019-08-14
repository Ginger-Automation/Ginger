using Amdocs.Ginger.Common.Enums;
using GingerCore;
using System.ComponentModel;
using System.Drawing;

namespace Amdocs.Ginger.CoreNET
{
    /// <summary>
    /// This class is used to hold the wrapper for businessflow conversion status
    /// </summary>
    public class BusinessFlowConversionStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        BusinessFlow mBusinessFlow;
        public BusinessFlow BusinessFlow
        {
            get {
                return mBusinessFlow;
            }
            set {
                if (mBusinessFlow != value)
                {
                    mBusinessFlow = value;
                    RelativeFilePath = value.ContainingFolder;
                    BusinessFlowName = value.Name;
                    IsSelected = value.Selected;
                    OnPropertyChanged(nameof(BusinessFlow));
                }
            }
        }

        eConversionStatus mConversionStatus = eConversionStatus.Pending;
        public eConversionStatus ConversionStatus
        {
            get
            {
                return mConversionStatus;
            }
            set
            {
                if (mConversionStatus != value)
                {
                    mConversionStatus = value;
                    SetStatusIcon();
                    OnPropertyChanged(nameof(ConversionStatus));
                }
            }
        }

        eConversionSaveStatus mSaveStatus = eConversionSaveStatus.Pending; 
        public eConversionSaveStatus SaveStatus
        {
            get
            {
                return mSaveStatus;
            }
            set
            {
                if (mSaveStatus != value)
                {
                    mSaveStatus = value;
                    OnPropertyChanged(nameof(SaveStatus));
                }
            }
        }

        eImageType mStatusIcon = eImageType.Pending;
        public eImageType StatusIcon
        {
            get
            {
                return mStatusIcon;
            }
            set
            {
                if (mStatusIcon != value)
                {
                    mStatusIcon = value;
                    OnPropertyChanged(nameof(StatusIcon));
                }
            }
        }

        /// <summary>
        /// This method is used to set the status icon
        /// </summary>
        private void SetStatusIcon()
        {
            switch (mConversionStatus)
            {
                case eConversionStatus.Finish:
                    StatusIcon = eImageType.Passed;
                    break;
                case eConversionStatus.Stopped:
                    StatusIcon = eImageType.Stopped;
                    break;
                case eConversionStatus.Pending:
                    StatusIcon = eImageType.Pending;
                    break;
                case eConversionStatus.Running:
                    StatusIcon = eImageType.Running;
                    break;
            }
        }

        bool mIsSelected = false;
        public bool IsSelected
        {
            get {
                return mIsSelected;
            }
            set {
                if(mIsSelected != value)
                {
                    mIsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public string RelativeFilePath { get; set; }

        public string BusinessFlowName { get; set; }

        public int ActivitiesCount { get; set; }

        public int ConvertedActivitiesCount { get; set; }

        public int CurrentActivityIndex { get; set; }

        public int CurrentActivityActionsCount { get; set; }

        public int CurrentActivityConvertedActionsCount { get; set; }

        public int CurrentActivityCurrentActionIndex { get; set; }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
