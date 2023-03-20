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

namespace Amdocs.Ginger.Common.UIElement
{
    public class AppWindow
    {
        public enum eWindowType
        {
            Windows,
            PowerBuilder,
            ASCFForm,
            SeleniumWebPage,
            InternalBrowserWebPageDocument,
            JFrmae,
            Appium,
            // AndroidDevice,
            Mainframe
        }

        public string Title { get; set; }
        public object RefObject { get; set; }
        public eWindowType? WindowType { get; set; }
        public string Path { get; set; }

        public string WinInfo
        {
            get
            {
                if (WindowType != eWindowType.JFrmae)
                    return Title + " - " + Path;
                else
                    return Title;
            }
        }
    }
}
