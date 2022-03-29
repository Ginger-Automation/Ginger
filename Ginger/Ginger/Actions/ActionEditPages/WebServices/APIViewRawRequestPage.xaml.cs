using Amdocs.Ginger.Common;
using GingerCore.Actions.WebAPI;
using GingerCore.Actions.WebServices;
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
    public partial class APIViewRawRequestPage : Page
    {
        private string mRawRequestContent;
        GenericWindow mGenericWindow = null;
        ActWebAPIBase mAct;

        public APIViewRawRequestPage(ActWebAPIBase action)
        {
            InitializeComponent();

            mAct = action;
            

        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            this.Height = 800;
            this.Width = 800;
            this.Title = "Raw API Request";
            this.xViewRawRequestTxtBlock.Text = mRawRequestContent;

            Button CopyToClipboradBtn = new Button();
            CopyToClipboradBtn.Content = "Copy to Clipboard";
            CopyToClipboradBtn.Click += new RoutedEventHandler(CopyToClipboradBtn_Click);

            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(CopyToClipboradBtn);
            //add "copy to clipboard" button
            //change 'Cancel' to "Close"
            GenericWindow.LoadGenericWindow(ref mGenericWindow, null, windowStyle, this.Title, this, winButtons, true, "Close");
        }

        public void PrepareActionValues()
        {
            Context context = Context.GetAsContext(mAct.Context);
            if (context != null && context.Runner != null)
            {
                context.Runner.PrepActionValueExpression(mAct, context.BusinessFlow);
            }
        }

        public void CreateRawRequestContent()
        {
            HttpWebClientUtils webAPI = new HttpWebClientUtils();
            webAPI.RequestContstructor(mAct, null, false);
            webAPI.CreateRawRequestContent();

            mRawRequestContent = webAPI.RequestFileContent;
        }

        private void CopyToClipboradBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(mRawRequestContent);
        }
    }


}
