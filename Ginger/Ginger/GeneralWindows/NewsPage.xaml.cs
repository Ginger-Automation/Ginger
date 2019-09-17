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

using System;
using System.Windows.Controls;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for NewsPage.xaml
    /// </summary>
    public partial class NewsPage : Page
    {
        GenericWindow _pageGenericWin = null;

        public NewsPage()
        {
            InitializeComponent();
            try
            {
                ViewWebBrowser.Navigate("http://ginger/News");
            }
            catch (Exception)
            {
                //TODO: display message saying support's unavailable & disable buttons
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {          
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, null, windowStyle, "Ginger News", this, null, true);
        }
    }
}
