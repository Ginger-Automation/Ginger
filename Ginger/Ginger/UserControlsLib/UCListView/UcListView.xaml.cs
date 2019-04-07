using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.UCListView
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UcListView : UserControl
    {
        public ListView List
        {
            get
            {
                return xListView;
            }
            set
            {
                xListView = value;
            }
        }
        public UcListView()
        {
            InitializeComponent();
        }

        private void XDeleteAllBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void XMoveUpBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void XMoveDownBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void XExpandCollapseBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void XListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
