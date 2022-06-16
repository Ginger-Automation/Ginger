using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ginger.Drivers.DriversWindows
{
    public class DeviceInfo : INotifyPropertyChanged
    {
        public DeviceInfo(string name, string value, eCategory category, string extraInfo = null)
        {
            DetailName = name;
            DetailValue = value;
            ExtraInfo = extraInfo;
            Category = category;
        }
        public enum eCategory
        {
            DeviceDetails = 1,
            DeviceMetrics = 2
        }
        public eCategory Category { get; set; }

        public string DetailName { get; set; }
        public string DetailValue { get; set; }
        private string mExtraInfo;
        public string ExtraInfo
        {
            get
            {
                return mExtraInfo;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    mExtraInfo = value;
                    mIsVisible = Visibility.Visible;
                }

            }
        }
        private Visibility mIsVisible = Visibility.Collapsed;
        public Visibility IsVisible { get { return mIsVisible; } }


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
