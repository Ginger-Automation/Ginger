using Amdocs.Ginger.Common.Enums;
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
