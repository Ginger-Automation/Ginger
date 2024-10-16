using GingerCore.Actions.Common;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for UIElementScrollToOptionsPage.xaml
    /// </summary>
    public partial class UIElementScrollToOptionsPage : Page
    {
        public enum eScrollAlignment
        {
            Start,
            Center,
            End,
            Nearest
        }

        public UIElementScrollToOptionsPage(ActUIElement action)
        {
            InitializeComponent();

            List<eScrollAlignment> scrollPositions = [
                eScrollAlignment.Start,
                eScrollAlignment.Center,
                eScrollAlignment.End,
                eScrollAlignment.Nearest,
            ];

            scrollAlignmentComboBox.Init(action.GetOrCreateInputParam(ActUIElement.Fields.ScrollAlignment), scrollPositions, false);
        }
    }
}
