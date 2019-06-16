using Amdocs.Ginger.Common.Enums;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.UserControlsLib.UCListView
{
    public class ListItemOperation
    {
        public string Header;
        public eImageType ImageType;
        public SolidColorBrush ImageForeground;
        public double ImageSize = 16;
        public object ImageBindingObject;
        public string ImageBindingFieldName;
        public IValueConverter ImageBindingConverter;

        public string ToolTip;

        public RoutedEventHandler OperationHandler;


    }
}
