#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ginger
{
    public class Totals
    {
        public static class Fields
        {
            public static string TotalActivity = "TotalActivity";
            public static string TotalAction = "TotalAction";
        }

        public string TotalActivity { get; set; }
        string mTotalAction = string.Empty;       
        public string TotalAction
        {
            get
            {               
              return mTotalAction;
            }
            set
            {
                mTotalAction = value;
                OnPropertyChanged(Fields.TotalAction);
            }
        }

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

    public class ViewModel
    {        
        private readonly ObservableCollection<StatItems> _seriesdata = new ObservableCollection<StatItems>();
        public ObservableCollection<StatItems> SeriesData
        {
            get
            {
                return _seriesdata;
            }
        }

        public ViewModel(List<StatItems> st)
        {
            _seriesdata = new ObservableCollection<StatItems>(st);
        }
    }

    public class StatItems : INotifyPropertyChanged
    {
        private string _description = string.Empty;
        private int _count = 0;
        private string _formattedvalue = string.Empty;

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                NotifyPropertyChanged("Description");
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                NotifyPropertyChanged("Count");
            }
        }

        public string FormattedValue
        {
            get
            {
                return _formattedvalue;
            }
            set
            {
                _formattedvalue = value;
                NotifyPropertyChanged("FormattedValue");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
