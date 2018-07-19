using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib
{
    public class TemplateFile : INotifyPropertyChanged
    {

        //private eStatus mStatus;
        //public eStatus Status { get { return mStatus; } set { mStatus = value; OnPropertyChanged(nameof(Status)); } }

        private string mFilePath;
        public string FilePath { get { return mFilePath; } set { mFilePath = value; OnPropertyChanged(nameof(FilePath)); } }


        private string mMatchingResponseFilePath;
        public string MatchingResponseFilePath { get { return mMatchingResponseFilePath; } set { mMatchingResponseFilePath = value; OnPropertyChanged(nameof(MatchingResponseFilePath)); } }

        public event PropertyChangedEventHandler PropertyChanged;

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
