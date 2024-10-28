using GingerCore.Actions.Common;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for UIElementScrollToOptionsPage.xaml
    /// </summary>
    public partial class UIElementScrollToElementOptionsPage : Page
    {
        public enum eScrollAlignment
        {
            Start,
            Center,
            End
        }

        public UIElementScrollToElementOptionsPage(ActUIElement action)
        {
            InitializeComponent();

            List<eScrollAlignment> verticalAlignments = [
                eScrollAlignment.Start,
                eScrollAlignment.Center,
                eScrollAlignment.End,
            ];

            verticalScrollAlignmentComboBox.Init(action.GetOrCreateInputParam(ActUIElement.Fields.VerticalScrollAlignment), verticalAlignments, false);
        }
    }
}
