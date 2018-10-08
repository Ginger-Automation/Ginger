using Amdocs.Ginger.Common.Enums;
using System;
using System.ComponentModel;

namespace Ginger.TwoLevelMenuLib
{
    public class MenuItemBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public string Name { get; set; }

        public bool Active { get; set; }

        public string ToolTip { get; set; }

        public ConsoleKey Key { get; set; }

        public string AutomationID { get; set; }

        eImageType mIconType;
        public eImageType IconType
        {
            get
            {
                return mIconType;
            }
            set
            {
                mIconType = value;
                OnPropertyChanged(nameof(IconType));
            }
        }

    }
}
