using System;
using System.Collections.Generic;
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

namespace Amdocs.Ginger.UserControls
{
    /// <summary>
    /// Interaction logic for ucExplorerWindowSelection.xaml
    /// </summary>
    public partial class UCExplorerWindowSelection : UserControl
    {
        public UCExplorerWindowSelection()
        {
            InitializeComponent();
        }

        private void POMsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void WindowsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
