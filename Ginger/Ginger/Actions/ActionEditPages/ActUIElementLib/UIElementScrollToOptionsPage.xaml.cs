using GingerCore.Actions.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using static GingerCoreNET.GeneralLib.General;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for UIElementScrollToOptionsPage.xaml
    /// </summary>
    public partial class UIElementScrollToElementOptionsPage : Page
    {
        public UIElementScrollToElementOptionsPage(ActUIElement action)
        {
            InitializeComponent();

            List<eScrollAlignment> verticalAlignments = [
                eScrollAlignment.Start,
                eScrollAlignment.Center,
                eScrollAlignment.End,
                eScrollAlignment.Nearest,
            ];

            verticalScrollAlignmentComboBox.Init(action.GetOrCreateInputParam(ActUIElement.Fields.VerticalScrollAlignment, nameof(eScrollAlignment.Start)), verticalAlignments, false);
        }
    }
}
