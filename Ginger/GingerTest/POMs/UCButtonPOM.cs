#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Amdocs.Ginger.UserControls;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GingerWPFUnitTest.POMs
{
    public class UCButtonPOM : GingerPOMBase
    {
        public ucButton ucButton { get; set; }

        public UCButtonPOM(ucButton b)
        {
            ucButton = b;
        }

        ucButton mButton
        {
            get
            {
                return ucButton; 
            }
        }

        public void Click()
        {            
            Task.Factory.StartNew(() => { 
                Dispatcher.Invoke(() =>
                {
                    //TODO: verify enabled mButton.IsEnabled
                    // mButton.BorderThickness = new Thickness(3);
                    // mButton.BorderBrush =  System.Windows.Media.Brushes.Red;
                    
                    // mButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, mButton));                                        
                    mButton.DoClick();
                });
            });
            
            SleepWithDoEvents(500);            

        }

     
        public bool IsEnabled
        {
            get
            {
                bool b = false;
                Execute(() =>
                {                    
                    b = mButton.IsEnabled;
                });
                return b;
            }
        }

        //TODO: remove the set
        public string Text { get { return mButton.ButtonText;  }  }
    }
}
