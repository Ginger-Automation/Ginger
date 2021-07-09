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

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for GingerSelfHealingConfiguration.xaml
    /// </summary>
    public partial class GingerSelfHealingConfiguration : Page
    {
        GenericWindow genWin = null;
        public GingerSelfHealingConfiguration()
        {
            InitializeComponent();
        }
        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, Ginger.eWindowShowStyle.Dialog, this.Title, this);
        }
    }
}
