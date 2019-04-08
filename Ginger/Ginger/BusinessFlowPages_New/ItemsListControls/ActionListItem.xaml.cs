using GingerCore.Actions;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowPages_New.ItemsListControls
{
    /// <summary>
    /// Interaction logic for ucActionListItem.xaml
    /// </summary>
    public partial class ActionListItem : UserControl
    {

        public static readonly DependencyProperty ActionProperty = DependencyProperty.Register(nameof(ActionProperty), typeof(Act), typeof(ActionListItem), new PropertyMetadata(null, new PropertyChangedCallback(OnActionPropertyChanged)));
        public Act Action
        {
            get
            {
                return (Act)GetValue(ActionProperty);
            }
            set
            {
                SetValue(ActionProperty, value);
                xActionListItem.SetItem(value);
            }
        }
        private static void OnActionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ActionListItem;
            if (control != null && e.NewValue != null)
            {
                control.Action = ((Act)e.NewValue);
            }          
        }

        public ActionListItem()
        {
            InitializeComponent();


            xActionListItem.ConfigItem(itemNameField: nameof(Act.Description), itemDescriptionField: nameof(Act.ActionType), itemIconField: null, itemExecutionStatusField: nameof(Act.Status));
        }

    }
}
