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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Ginger.UserControlsLib.UCListView
{
    public class ListItemGroupOperation
    {
        public string Group = null;
        public eImageType GroupImageType;

        public string Header;
        public eImageType ImageType;
        public SolidColorBrush ImageForeground;
        public double ImageSize = 16;

        public string ToolTip;
        public string AutomationID;

        public RoutedEventHandler OperationHandler;

        public List<General.eRIPageViewMode> SupportedViews = new List<General.eRIPageViewMode>();

        public bool Visible = true;
    }
}
