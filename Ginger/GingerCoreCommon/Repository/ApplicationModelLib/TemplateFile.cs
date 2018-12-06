#region License
/*
Copyright © 2014-2018 European Support Limited

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
