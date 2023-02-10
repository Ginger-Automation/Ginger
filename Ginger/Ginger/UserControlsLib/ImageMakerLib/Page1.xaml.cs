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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Core;
using Amdocs.Ginger.UserControls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.UserControlsLib.ImageMakerLib
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        List<ImageMakerControl> mImageMakerControls = new List<ImageMakerControl>();

        public Page1()
        {
            InitializeComponent();            
            ShowIcons(1,20);
        }

        public void StopSpinners()
        {
            foreach(ImageMakerControl c in mImageMakerControls)
            {
                c.StopImageSpin();
            }
        }

        public void ShowIcons(int from , int count)
        {
            // Arrange            
            Grid grid = IconsGrid;
            if (grid.ColumnDefinitions.Count == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80, GridUnitType.Star) });
                }

                for (int i = 0; i < 5; i++)
                {
                    grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Star) });
                }
            }
            else
            {
                grid.Children.Clear();
            }

            var icons = Enum.GetValues(typeof(eImageType));
            int row = 0;
            int col = 0;
            int counter = 0;
            foreach (eImageType icon in icons)
            {
                counter++;
                if (counter < from) continue;
                if (counter >= from + count) return;

                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Vertical;

                ImageMakerControl IMK = new ImageMakerControl();
                IMK.ImageType = icon;
                IMK.Width = 32;
                IMK.Height = 32;
                IMK.FontSize = 32;                
                sp.Children.Add(IMK);
                mImageMakerControls.Add(IMK);
                Label l = new Label();
                l.Content = icon.ToString();
                sp.Children.Add(l);


                grid.Children.Add(sp);
                Grid.SetRow(sp, row);
                Grid.SetColumn(sp, col);
                col++;
                if (col == grid.ColumnDefinitions.Count)
                {
                    col = 0;
                    row++;
                }
            }
        }
    }
}