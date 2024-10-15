using GingerCore.Actions.Common;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for UIElementScrollToOptionsPage.xaml
    /// </summary>
    public partial class UIElementScrollToOptionsPage : Page
    {
        private readonly ActUIElement _action;

        private enum eScrollAlignment
        {
            Start,
            Center,
            End,
            Nearest
        }

        public UIElementScrollToOptionsPage(ActUIElement action)
        {
            InitializeComponent();

            _action = action;

            // Generate list of options from Enum
            List<ComboItem> _scrollPositions = Enum.GetValues(typeof(eScrollAlignment))
                .Cast<eScrollAlignment>()
                .Select(pos => new ComboItem()
                {
                    text = pos.ToString(),
                    Value = (int)pos
                })
                .Cast<ComboItem>()
                .ToList();

            // Bind alignment options with combo box
            scrollAlignmentComboBox.Init(_action.GetOrCreateInputParam(ActUIElement.Fields.ScrollAlignment, eScrollAlignment.Start.ToString()), _scrollPositions, false);
        }
    }
}
