using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
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

namespace Ginger.ApplicationModelsLib.APIModels
{
    public partial class APIModelBodyNodeSyncPage : Page
    {
        GenericWindow _pageGenericWin = null;
        public APIModelBodyNodeSyncPage(ApplicationAPIModel applicationAPIModel, List<AppModelParameter> paramsToDelete)
        {
            InitializeComponent();


            //Analyze JSON or XML body
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            //Button addActionBtn = new Button();
            //addActionBtn.Content = "Add Action";
            //addActionBtn.Click += new RoutedEventHandler(AddActionButton_Click);
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { });
        }
    }
}
