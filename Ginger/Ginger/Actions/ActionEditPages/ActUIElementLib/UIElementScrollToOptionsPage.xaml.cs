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

using GingerCore.Actions.Common;
using System.Collections.Generic;
using System.Windows.Controls;
using static GingerCoreNET.GeneralLib.General;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for UIElementScrollToOptionsPage.xaml
    /// </summary>
    public partial class UIElementScrollToElementOptionsPage : Page
    {
        public UIElementScrollToElementOptionsPage(ActUIElement action)
        {
            InitializeComponent();

            List<eScrollAlignment> verticalAlignments = [
                eScrollAlignment.Start,
                eScrollAlignment.Center,
                eScrollAlignment.End,
                eScrollAlignment.Nearest,
            ];

            verticalScrollAlignmentComboBox.Init(action.GetOrCreateInputParam(ActUIElement.Fields.VerticalScrollAlignment, nameof(eScrollAlignment.Start)), verticalAlignments, false);
        }
    }
}
