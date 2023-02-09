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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Ginger.Actions.UserControls;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for FailedActionsScreenshotsPage.xaml
    /// </summary>
    public partial class FailedActionsScreenshotsPage : Page
    {
        List<string> mFailedActionsScreenshots = new List<string>();
        
        public List<string> FailedActionsScreenshots
        {
            get { return mFailedActionsScreenshots; }
        }

        GenericWindow genWin = null;
        
        public FailedActionsScreenshotsPage(List<string> failedActionsScreenshots)
        {
            InitializeComponent();
            mFailedActionsScreenshots = failedActionsScreenshots;

            // create grid row cols based on screen shots count, can be 1x1, 2x2, 3x3 etc.. 
            int rowcount = 1;
            int colsPerRow = 1;
            while (rowcount * colsPerRow < mFailedActionsScreenshots.Count)
            {
                if (rowcount < colsPerRow)
                    rowcount++;    // enable 1 row 2 columns, 2x3, 3x4 etc.. - avoid showing empty row
                else
                    colsPerRow++;
                // we can limit cols if we want for example max 3 per row, and then the grid will have vertical scroll bar
            }

            for (int rows = 0; rows < rowcount; rows++)
            {
                RowDefinition rf = new RowDefinition() { Height = new GridLength(50, GridUnitType.Star) };
                ScreenShotsGrid.RowDefinitions.Add(rf);
            }

            for (int cols = 0; cols < colsPerRow; cols++)
            {
                ColumnDefinition cf = new ColumnDefinition() { Width = new GridLength(50, GridUnitType.Star) };
                ScreenShotsGrid.ColumnDefinitions.Add(cf);
            }

            // loop through the screen shot and create new frame per each to show and place in the grid

            int r = 0;
            int c = 0;

            for (int i = 0; i < mFailedActionsScreenshots.Count; i++)
            {
                //TODO: clean me when Screenshots changed to class instead of list of strings
                // just in case we don't have name, TOOD: fix all places where we add screen shots to include name
                string Name = "";
                Name = "Screenshot " + (i+1).ToString();

                ScreenShotViewPage p = new ScreenShotViewPage(Name, mFailedActionsScreenshots[i]);
                Frame f = new Frame();
                Grid.SetRow(f, r);
                Grid.SetColumn(f, c);
                f.HorizontalAlignment = HorizontalAlignment.Center;
                f.VerticalAlignment = VerticalAlignment.Center;
                f.Content = p;
                ScreenShotsGrid.Children.Add(f);

                c++;
                if (c == colsPerRow)
                {
                    c = 0;
                    r++;
                }
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, null);
        }
    }
}
