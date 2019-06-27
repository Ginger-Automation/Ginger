using Amdocs.Ginger.Common.Enums;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.UserControlsLib.UCListView
{
    public class ListItemOperation
    {
        public string Group = null;
        public eImageType GroupImageType; 

        public string Header;
        public eImageType ImageType;
        public SolidColorBrush ImageForeground;
        public double ImageSize = 14;
        public object ImageBindingObject;
        public string ImageBindingFieldName;
        public IValueConverter ImageBindingConverter;

        public string ToolTip;

        public RoutedEventHandler OperationHandler;


    }
}
