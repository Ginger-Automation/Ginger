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

using GingerCore.GeneralLib;
using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GingerTest.POMs.Common
{
    public class InputBoxWindowPOM : GingerPOMBase
    {        
        public void SetText(string txt)
        {
            Execute(() => {                
                TextBox textBox = (TextBox)InputBoxWindow.CurrentInputBoxWindow.FindName("ValueTextBox");
                textBox.Text = txt;
            });
        }

        public void ClickOK()
        {
            Execute(() => {
                Button OKbutton = (Button)InputBoxWindow.CurrentInputBoxWindow.FindName("OKButton");
                OKbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                // wait for window to close
                while (InputBoxWindow.CurrentInputBoxWindow != null && InputBoxWindow.CurrentInputBoxWindow.IsVisible)
                {
                    SleepWithDoEvents(100);
                }
            });
        }
    }
}
