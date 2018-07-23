using GingerWPF.WizardLib;
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

namespace Ginger.TestLib.WizardLib
{
    /// <summary>
    /// Interaction logic for MyWizardPropertiesPage.xaml
    /// </summary>
    public partial class MyWizardPropertiesPage : Page, IWizardPage
    {
        public MyWizardPropertiesPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            // throw new NotImplementedException();
        }
    }
}
