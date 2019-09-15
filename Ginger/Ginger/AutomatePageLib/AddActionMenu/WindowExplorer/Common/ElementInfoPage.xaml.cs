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
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Drivers.Common;

namespace Ginger.Drivers.Common
{
    /// <summary>
    /// Generic class for ElementInfo to show basic values, 
    /// element which have more info can override and create it's own page for view 
    /// </summary>
    public partial class ElementInfoPage : Page
    {        
        ElementInfo mElementInfo;

        public ElementInfoPage(ElementInfo EI)
        {
            InitializeComponent();
            mElementInfo = EI;
            ShowInfo();
        }

        private void ShowInfo()
        {
            TitleTextBox.Text = mElementInfo.ElementTitle;
            TypeTextBox.Text = mElementInfo.ElementType;
            ValueTextBox.Text = mElementInfo.Value;
            PathTextBox.Text = mElementInfo.Path;
            XPathTextBox.Text = mElementInfo.XPath;
        }
    }
}
