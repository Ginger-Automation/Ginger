#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Ginger.ScottLogic.PieChart;
using GingerCoreNET.GeneralLib;
using System;
using System.Windows;
using System.Windows.Media;
namespace Ginger.UserControlsLib.PieChart
{
    /// <summary>
    /// Selects a color based on Status
    /// </summary>
    public class StatusColorSelector : DependencyObject, IColorSelector
    {
        /// <summary>
        /// An array of brushes.
        /// </summary>
        public Brush[] Brushes
        {
            get { return (Brush[])GetValue(BrushesProperty); }
            set { SetValue(BrushesProperty, value); }
        }

        public static readonly DependencyProperty BrushesProperty =
                       DependencyProperty.Register("BrushesProperty", typeof(Brush[]), typeof(StatusColorSelector), new UIPropertyMetadata(null));

        ResourceDictionary resourceDictionary { get; set; }
        /// <summary>
        ///Return Brush color based on StatItem Description 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Brush SelectBrush(object item, int index)
        {
            StatItem st = (StatItem)item;
            resourceDictionary = [];
            resourceDictionary.Source = new Uri("pack://application:,,,/Ginger;component/Dictionaries/Skins/GingerDefaultSkinDictionary.xaml");
            //TODO: find better pallets colors
            return st.Description switch
            {
                "Automated" or "Executed" or "Passed" => (Brush)resourceDictionary["$PassedStatusColor"],
                "Failed" or "Not Automated" or "Not Executed" or "Not Passed" => (Brush)resourceDictionary["$FailedStatusColor"],
                "Blocked" => (Brush)resourceDictionary["$BlockedStatusColor"],
                "Pending" => (Brush)resourceDictionary["$PendingStatusColor"],
                "Started" or "Running" or "Wait" or "Canceling" => (Brush)resourceDictionary["$RunningStatusColor"],
                "Stopped" => (Brush)resourceDictionary["$StoppedStatusColor"],
                "NA" or "FailIgnored" or "Skipped" => (Brush)resourceDictionary["$SkippedStatusColor"],
                //TODO: add all cover all status, or go to Act, Activity, BF and get color from status
                _ => (Brush)resourceDictionary["$SkippedStatusColor"],
            };
        }
    }
}
