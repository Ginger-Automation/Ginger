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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Ginger.GherkinLib
{    
    public class StepStyleSelector : StyleSelector
    {
        Style[] mStyles = null;

        void CreateStyles()
        {
            //TODO: add more colors
            mStyles = new Style[8];
            string[] variableColumnBackgroundColor = new string[] { "#FFFFCC", "#E5FFCC", "#CCE5FF", "#CCCCFF", "#FFCCFF", "#FFCC99", "#CCFFFF", "#FF99CC" };
            for (int colorIndx = 0; colorIndx < 8; colorIndx++)
            {
                Style rowStyle = new System.Windows.Style(typeof(DataGridRow));
                SolidColorBrush backgroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString(variableColumnBackgroundColor[colorIndx % 8]);
                rowStyle.Setters.Add(new Setter(DataGridColumnHeader.BackgroundProperty, backgroundColor));
                mStyles[colorIndx] = rowStyle;
            }                          
        }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (mStyles == null)
            {                
                CreateStyles();
            }
            if (item is GherkinStep)
            {
                GherkinStep step = (GherkinStep)item;
                return mStyles[step.ColorIndex];                
            }
            return null;
        }
    }
}
