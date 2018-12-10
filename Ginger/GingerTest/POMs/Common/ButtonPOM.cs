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

using GingerTest.VideosLib;
using GingerWPFUnitTest;
using GingerWPFUnitTest.POMs;
using System.Threading;
using System.Windows.Controls;

namespace GingerTest.POMs.Common
{
    public class ButtonPOM : GingerPOMBase
    {
        Button mButton;
        public ButtonPOM(Button button)
        {
            mButton = button;
        }

        public void Click()
        {
            Execute(() => {

                if (GingerAutomator.Highlight)
                {
                    HighlightAdorner highlightAdorner = new HighlightAdorner(mButton);
                    DoEvents();
                    SleepWithDoEvents(100);
                    Speach.Say("Click Button " + mButton.Content.ToString());
                    Thread.Sleep(500);
                }
                mButton.RaiseEvent(new System.Windows.RoutedEventArgs(Button.ClickEvent));
            });
            
        }
    }
}
