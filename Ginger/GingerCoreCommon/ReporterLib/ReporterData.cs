using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Amdocs.Ginger.Common
{
    public class ReporterData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        int mErrorCounter = 0;
        public int ErrorCounter
        {
            get
            {
                return mErrorCounter;
            }
            internal set
            {
                if (mErrorCounter != value)
                {
                    mErrorCounter = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorCounter)));
                    }
                }                
            }
        }

        public void ResetErrorCounter()
        {
            mErrorCounter = 0;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorCounter)));
        }
    }
}
