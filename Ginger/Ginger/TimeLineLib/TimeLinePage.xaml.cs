using Amdocs.Ginger.Common;
using GingerUtils.TimeLine;
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

namespace Ginger.TimeLineLib
{
    /// <summary>
    /// Interaction logic for TimeLinePage.xaml
    /// </summary>
    public partial class TimeLinePage : Page
    {
        GenericWindow _pageGenericWin = null;
        public TimeLinePage(TimeLineEvents timeLineEvents)
        {
            InitializeComponent();

            xTimeLineTreeGraph.DataContext= timeLineEvents.Events;
        }

        public void ShowAsWindow()
        {            
            ObservableList<Button> winButtons = new ObservableList<Button>();
            
            //    Button okBtn = new Button();
            //okBtn.Content = "Ok";
            //okBtn.Click += new RoutedEventHandler(okBtn_Click);
            //Button undoBtn = new Button();
            //undoBtn.Content = "Undo & Close";
            //undoBtn.Click += new RoutedEventHandler(undoAndCloseBtn_Click);
            //winButtons.Add(undoBtn);
            //winButtons.Add(okBtn);           

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, eWindowShowStyle.FreeMaximized, "Execution Timeline view", this);
            
        }

    }
}
