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

using Amdocs.Ginger.Common.Enums;
using GingerCore;
using System.ComponentModel;
using System.Drawing;

namespace Amdocs.Ginger.CoreNET
{
    /// <summary>
    /// This enum is used to hold the status for conversion
    /// </summary>
    public enum eConversionStatus
    {
        NA,
        Pending,
        Running,
        Stopped,
        Finish,
        Failed
    }

    /// <summary>
    /// This enum is used to hold the save status for the converted businessflows
    /// </summary>
    public enum eConversionSaveStatus
    {
        NA,
        Saved,
        Saving,
        Pending,
        Failed
    }

    /// <summary>
    /// This class is used to hold the wrapper for businessflow conversion status
    /// </summary>
    public class BusinessFlowToConvert : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        BusinessFlow mBusinessFlow;
        public BusinessFlow BusinessFlow
        {
            get
            {
                return mBusinessFlow;
            }
            set
            {
                if (mBusinessFlow != value)
                {
                    mBusinessFlow = value;
                    RelativeFilePath = value.ContainingFolder.Replace("~\\\\BusinessFlows", "");
                    BusinessFlowName = string.Format("{0}\\{1}", RelativeFilePath, value.Name);
                    Description = value.Description;
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
                    SetSaveStatusIcon();
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

        eImageType mSaveStatusIcon = eImageType.Pending;
        public eImageType SaveStatusIcon
        {
            get
            {
                return mSaveStatusIcon;
            }
            set
            {
                if (mSaveStatusIcon != value)
                {
                    mSaveStatusIcon = value;
                    OnPropertyChanged(nameof(SaveStatusIcon));
                }
            }
        }

        int mConvertedActionsCount = 0;
        public int ConvertedActionsCount
        {
            get
            {
                return mConvertedActionsCount;
            }
            set
            {
                mConvertedActionsCount = value;
                OnPropertyChanged(nameof(ConvertedActionsCount));
            }
        }

        int mTotalConvertibleActionsCount = 0;
        public int TotalProcessingActionsCount
        {
            get
            {
                return mTotalConvertibleActionsCount;
            }
            set
            {
                mTotalConvertibleActionsCount = value;
                OnPropertyChanged(nameof(TotalProcessingActionsCount));
            }
        }

        /// <summary>
        /// This method is used to set the status icon
        /// </summary>
        private void SetStatusIcon()
        {
            switch (mConversionStatus)
            {
                case eConversionStatus.NA:
                    StatusIcon = eImageType.Skipped;
                    break;
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
                case eConversionStatus.Failed:
                    StatusIcon = eImageType.Failed;
                    break;
            }
        }

        /// <summary>
        /// This method is used to set the save status icon
        /// </summary>
        private void SetSaveStatusIcon()
        {
            switch (mSaveStatus)
            {
                case eConversionSaveStatus.NA:
                    SaveStatusIcon = eImageType.Skipped;
                    break;
                case eConversionSaveStatus.Pending:
                    SaveStatusIcon = eImageType.Pending;
                    break;
                case eConversionSaveStatus.Saved:
                    SaveStatusIcon = eImageType.Passed;
                    break;
                case eConversionSaveStatus.Saving:
                    SaveStatusIcon = eImageType.Running;
                    break;
                case eConversionSaveStatus.Failed:
                    SaveStatusIcon = eImageType.Failed;
                    break;
            }
        }

        bool mIsSelected = false;
        public bool IsSelected
        {
            get
            {
                return mIsSelected;
            }
            set
            {
                if (mIsSelected != value)
                {
                    mIsSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public string RelativeFilePath { get; set; }

        public string BusinessFlowName { get; set; }

        public string Description { get; set; }

        public int ConvertableActionsCount { get; set; }

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
