#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
