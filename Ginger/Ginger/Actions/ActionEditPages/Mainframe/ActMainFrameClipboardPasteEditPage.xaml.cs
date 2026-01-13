using Amdocs.Ginger.CoreNET.ActionsLib.MainFrame;
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

namespace Ginger.Actions.Mainframe
{
    /// <summary>
    /// Interaction logic for ActMainFrameClipboardPasteEditPage.xaml
    /// </summary>
    public partial class ActMainFrameClipboardPasteEditPage : Page
    {
        private ActMainframeClipboardPaste mAct;
        public ActMainFrameClipboardPasteEditPage(ActMainframeClipboardPaste act)
        {
            InitializeComponent();
            mAct = act;
            
        }

        private void txtValueToPaste_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Save the value back to the action whenever the user types
            if (mAct != null)
            {
                mAct.Value = txtValueToPaste.Text;
            }
        }
    }
}
