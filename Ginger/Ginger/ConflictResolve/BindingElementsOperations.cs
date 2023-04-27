using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.ConflictResolve
{
    public static class BindingElementsOperations
    {
        public static Binding GetBindingElement(FrameworkElement control, out Type argsControlType)
        {
            argsControlType = null;
            if (control is TextBox)
            {
                argsControlType = typeof(TextBox);
                return BindingOperations.GetBinding((TextBox)control, TextBox.TextProperty);
            }
            if (control is ComboBox)
            {
                argsControlType = typeof(ComboBox);
                return BindingOperations.GetBinding((ComboBox)control, ComboBox.TextProperty);
            }
            if (control is CheckBox)
            {
                argsControlType = typeof(CheckBox);
                return BindingOperations.GetBinding((CheckBox)control, CheckBox.IsCheckedProperty);
            }
            if(control is ucGrid)
            {

            }
            return null;
        }

        public static void SetStyleToHighlightConflict(this TextBox TextBox)
        {
            TextBox.Style = (Style)Application.Current.Resources.MergedDictionaries[0]["$TextBoxHighlightConflictStyle"];
        }

        public static void SetStyleToHighlightConflict(this ComboBox ComboBox)
        {
            ComboBox.Style = (Style)Application.Current.Resources.MergedDictionaries[0]["$FlatInputComboBoxHighlightConflictStyle"];
        }

        public static void SetStyleToHighlightConflict(this CheckBox CheckBox)
        {
            CheckBox.Style = (Style)Application.Current.Resources.MergedDictionaries[0]["$CheckBoxHighlightConflictStyle"];
        }
    }
}
