using Amdocs.Ginger.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.UserControlsLib.UCListView
{
    public class ListItemNotification
    {
        public eImageType ImageType;
        public SolidColorBrush ImageForeground;
        public double ImageSize = 16;

        public string ToolTip;

        public object BindingObject;
        public string BindingFieldName;
        public IValueConverter BindingConverter;
    }
}
