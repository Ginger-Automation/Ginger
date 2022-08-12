using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.VariablesLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCMultiSelectCombobox.xaml
    /// </summary>
    public partial class UCMultiSelectCombobox : UserControl
    {
     
        public delegate void MultiSelectEventHandler(bool EventArgs);
        private static event MultiSelectEventHandler MultSelectEvent;
        public void OnMultiSelectEvent(bool SelectionChanged)
        {
            MultiSelectEventHandler handler = MultSelectEvent;
            if (handler != null)
            {
                handler(SelectionChanged);
            }
        }

        public static void SetMultiSelectEvent(MultiSelectEventHandler multiSelectEvent)
        {
            if (MultSelectEvent == null)
            {
                MultSelectEvent -= multiSelectEvent;
                MultSelectEvent += multiSelectEvent;
            }
        }

        private object obj;
        private string AttrName;
        private Context mContext;

        public UCMultiSelectCombobox()
        {
            InitializeComponent();           
        }

        public void Init(object obj, string AttrName)
        {
            //// If the VE is on stand alone form:
            this.obj = obj;
            this.AttrName = AttrName;          

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xMultiSelectCombobox, ComboBox.ItemsSourceProperty, obj, AttrName);
        }

        private void SetSelectedString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (SelectableObject<string> cbObject in xMultiSelectCombobox.Items)
            {
                if (cbObject.IsSelected)
                    sb.AppendFormat("{0}, ", cbObject.TextData);
            }
            tbObjects.Text = sb.ToString().Trim().TrimEnd(',');
            OnMultiSelectEvent(true);
        }

        private void OnCbObjectsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            comboBox.SelectedItem = null;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SetSelectedString();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SetSelectedString();
        }
    }
   
}
