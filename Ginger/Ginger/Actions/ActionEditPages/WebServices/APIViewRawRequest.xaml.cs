using Amdocs.Ginger.Common;
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

namespace Ginger.Actions.ActionEditPages.WebServices
{
    /// <summary>
    /// Interaction logic for APIViewRawRequest.xaml
    /// </summary>
    public partial class APIViewRawRequest : Page
    {
        private string mRawRequestContent;
        GenericWindow mGenericWindow = null;

        public APIViewRawRequest(string rawRequestContent)
        {
            InitializeComponent();

            mRawRequestContent = rawRequestContent;
            xViewRawRequestTxtBlock.Text = mRawRequestContent;
            this.Title = "View Raw API Request";

        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {

            GenericWindow.LoadGenericWindow(ref mGenericWindow, null, windowStyle, this.Title, this, null, true, "Cancel");
        }
    }
}
