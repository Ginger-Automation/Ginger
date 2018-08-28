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
using Amdocs.Ginger;
using Ginger.Run;
using GingerCore;

namespace Ginger.GeneralWindows
{
    /// <summary>
    /// Interaction logic for GlobalRetryMechanism.xaml
    /// </summary>
    public partial class GlobalRetryMechanism : Page
    {
        GenericWindow _pageGenericWin;
        private GingerRunner mGR;
        

        public GlobalRetryMechanism(GingerRunner GR)
        {
            InitializeComponent();
            mGR = GR;
            
            App.ObjFieldBinding(GlobalEnableRetryMechnanismCheckBox, CheckBox.IsCheckedProperty, mGR, Ginger.Run.GingerRunner.Fields.GlobalRetry);
            App.ObjFieldBinding(GlobalRetryIntervalTextBox, TextBox.TextProperty, mGR, Ginger.Run.GingerRunner.Fields.GlobalRetryInterval);
            App.ObjFieldBinding(GlobalRetryMaxRetriesNumberTextBox, TextBox.TextProperty, mGR, Ginger.Run.GingerRunner.Fields.GlobalRetryMaxRetriesNumber);

        }

     
        public void ShowAsWindow()
        {
            UpdateGlobalRetryMechanismTabVisual();
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, eWindowShowStyle.Dialog, this.Title, this, closeEventHandler: CloseWindow);
            
        }

        private void EnableGlobalRetryMechanismCheckBox_CheckedUnChecked(object sender, RoutedEventArgs e)
        {
           UpdateGlobalRetryMechanismTabVisual();
        }

        private void UpdateGlobalRetryMechanismTabVisual()
        {
         
            if (mGR.GlobalRetry)
                GlobalRetryMechnanismConfigsPnl.IsEnabled = true;
            else
                GlobalRetryMechnanismConfigsPnl.IsEnabled = false;

        }
        private void CloseWindow(object sender, EventArgs e)
        {
            
            _pageGenericWin.Close();

        }
    }
}
