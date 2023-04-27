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

namespace Ginger.ConflictResolve
{
    /// <summary>
    /// Interaction logic for CompareAndMergeConflictedItem.xaml
    /// </summary>
    public partial class CompareAndMergeConflictedItem : Page
    {
        GenericWindow genWin = null;
        List<string> ListComparisonObject = new List<string>();
        FrameworkElement LocalVersion, ServerVersion;
        //public ScrollPosition ScrollPosition { get; set; }
        public CompareAndMergeConflictedItem(FrameworkElement argsLocalVersion, FrameworkElement argsServerVersion, List<string> argsObjectComparison)
        {
            InitializeComponent();
            //ScrollPosition = new ScrollPosition();
            LocalVersion = argsLocalVersion;
            ServerVersion = argsServerVersion;
            xLocalVersion.Content = LocalVersion;
            xServerVersion.Content = ServerVersion;
            ListComparisonObject = argsObjectComparison;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, "Compare and Merge Conflicted Items", this, null, true, "Close", CloseWindow);

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //RenderConflictChanges();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender == LocalScroll)
            {
                ServerScroll.ScrollToVerticalOffset(e.VerticalOffset);
                ServerScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
            else
            {
                LocalScroll.ScrollToVerticalOffset(e.VerticalOffset);
                LocalScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            genWin.Close();
        }
    }
}
