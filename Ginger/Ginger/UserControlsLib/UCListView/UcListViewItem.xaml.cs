using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.UCListView
{
    /// <summary>
    /// Interaction logic for ListViewItem.xaml
    /// </summary>
    public partial class UcListViewItem : UserControl
    {
        object mItem;
        public object Item { get { return mItem; } }

        public string ItemNameField { get; set; }

        public UcListViewItem()
        {
            InitializeComponent();
        }

        public void SetItem(object item)
        {
            mItem = item;
            SetItemBindings();
        }

        private void SetItemBindings()
        {
            BindingHandler.ObjFieldBinding(xItemNameTxtBlock, TextBlock.TextProperty, mItem, ItemNameField);
        }

        

        //public static DependencyProperty ItemNameFieldProperty =
        //   DependencyProperty.Register(nameof(ItemNameField), typeof(string), typeof(UcListViewItem), new PropertyMetadata(OnItemNameFieldPropertyChanged));
        //private static void OnItemNameFieldPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        //{
        //    var control = sender as UcListViewItem;
        //    if (control != null)
        //        control.OnActParentBusinessFlowChanged((string)args.NewValue);
        //}
        //private void OnActParentBusinessFlowChanged(string itemNameField)
        //{
        //    ItemNameField = itemNameField;
        //}
        

        private void xDetailView_Click(object sender, RoutedEventArgs e)
        {

        }

        private void xRunnerItemContinue_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
