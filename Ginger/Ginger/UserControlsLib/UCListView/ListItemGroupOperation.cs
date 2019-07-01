using Amdocs.Ginger.Common.Enums;
using System.Windows;
using System.Windows.Media;

namespace Ginger.UserControlsLib.UCListView
{
    public class ListItemGroupOperation
    {
        public string Header;
        public eImageType ImageType;
        public SolidColorBrush ImageForeground;
        public double ImageSize = 16;

        public string ToolTip;
        public string AutomationID;

        public RoutedEventHandler OperationHandler;
    }
}
