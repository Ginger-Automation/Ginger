#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System.Windows.Controls;

namespace Ginger
{
    public class AppProgressBar
    {
        public ProgressBar ProgressBarControl { get; set; }
        public TextBlock ProgressBarTextControl { get; set; }
        public string mActionTitle { get; set; }
        public int mTotalItems { get; set; }

        public void Init(string ActionTitle, int totalItems)
        {
            mActionTitle = ActionTitle;
            mTotalItems = totalItems;

            ProgressBarControl.Dispatcher.Invoke(() =>
            {
                ProgressBarControl.Maximum = totalItems;
                ProgressBarControl.Minimum = 0;
                ProgressBarControl.Value = 0;
            });
        }

        public void NextItem(string Title)
        {
            ProgressBarControl.Dispatcher.Invoke(() =>
           {
               ProgressBarControl.Value++;
               string perc = (int)(ProgressBarControl.Value / mTotalItems * 100) + "%";
               ProgressBarTextControl.Text = mActionTitle + " - " + Title + " " + perc;
           });

            ProgressBarControl.Refresh();
            ProgressBarTextControl.Refresh();
        }

        public void Completed()
        {
            ProgressBarControl.Dispatcher.Invoke(() =>
            {
                ProgressBarControl.Value = 100;
                ProgressBarTextControl.Text = "Completed";
            });
        }
    }
}
