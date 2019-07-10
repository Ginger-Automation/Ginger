using Amdocs.Ginger.Common.Enums;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Ginger.UserControlsLib.UCListView
{
    public class ListItemGroupOperation
    {
        public string Group = null;
        public eImageType GroupImageType;

        public string Header;
        public eImageType ImageType;
        public SolidColorBrush ImageForeground;
        public double ImageSize = 16;

        public string ToolTip;
        public string AutomationID;

        public RoutedEventHandler OperationHandler;

        public List<General.eRIPageViewMode> SupportedViews = new List<General.eRIPageViewMode>();
    }
}
