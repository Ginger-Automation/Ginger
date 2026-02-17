#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Common;
using GingerUtils.TimeLine;
using System.Windows.Controls;

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

            xTimeLineTreeGraph.DataContext = timeLineEvents.Events;
        }

        public void ShowAsWindow()
        {
            ObservableList<Button> winButtons = [];

            //    Button okBtn = new Button();
            //okBtn.Content = "Ok";
            //okBtn.Click += new RoutedEventHandler(okBtn_Click);
            //Button undoBtn = new Button();
            //undoBtn.Content = "Undo & Close";
            //undoBtn.Click += new RoutedEventHandler(undoAndCloseBtn_Click);
            //winButtons.Add(undoBtn);
            //winButtons.Add(okBtn);           

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, eWindowShowStyle.Free, "Execution Timeline view", this);

        }

    }
}
