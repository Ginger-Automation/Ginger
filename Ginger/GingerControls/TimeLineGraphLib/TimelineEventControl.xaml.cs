using GingerUtils.TimeLine;
using System.Windows;
using System.Windows.Controls;

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
