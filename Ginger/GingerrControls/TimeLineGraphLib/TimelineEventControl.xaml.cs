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

namespace TimeLineControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TimelineEventControl : UserControl
    {
        public TimeLineEvent Event
        {
            get { return (TimeLineEvent)this.GetValue(EventProperty); }
            set {                
                this.SetValue(EventProperty, value);
                //this.xElapsed.Text = Event.Elapsed.ToString();
            }
        }
        

        public static readonly DependencyProperty EventProperty = DependencyProperty.Register(
                "Event", typeof(TimeLineEvent), typeof(TimelineEventControl), new PropertyMetadata(new TimeLineEvent(), OnObjectChanged));


        private static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimelineEventControl timelineEventControl = (TimelineEventControl)d;
            TimeLineEvent timeLineEvent = ((TimeLineEvent)e.NewValue);
            timelineEventControl.xElapsed.Text = timeLineEvent.Elapsed.ToString();
            
            timelineEventControl.xLine.X1 = timeLineEvent.Start;
            timelineEventControl.xLine.X2 = timeLineEvent.End;

        }



        public TimelineEventControl()
        {
            InitializeComponent();            
        }
    }
}
