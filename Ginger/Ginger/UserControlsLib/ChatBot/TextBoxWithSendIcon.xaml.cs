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
    /// Interaction logic for TextBoxWithSendIcon.xaml
    /// </summary>
    public partial class TextBoxWithSendIcon : UserControl
    {
        public TextBoxWithSendIcon()
        {
            InitializeComponent();
        }

        private void TxtInput_Changed(object sender, TextChangedEventArgs e)
        {
            //if(string.IsNullOrEmpty(TxtInput_Changed))
        }
    }
}
