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

using Amdocs.Ginger.Common.UIElement;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.WindowExplorer.Appium
{
    public class AppiumElementInfoConverter
    {
        internal static ITreeViewItem GetTreeViewItemFor(ElementInfo EI)
        {
            // TODO verify if pl.Name = ElementInfo

            AppiumElementInfo AEI = (AppiumElementInfo)EI;

            switch (AEI.ElementType)
            {
                case "android.widget.Button":
                    return new AppiumAndroidWidgetButtonTreeItem() { AppiumElementInfo = AEI };
                case "android.widget.CheckBox":
                    return new AppiumAndroidWidgetCheckBoxTreeItem() { AppiumElementInfo = AEI };
                case "android.widget.TextView":
                    return new AppiumAndroidWidgetTextViewTreeItem() { AppiumElementInfo = AEI };
                case "android.widget.ImageView":
                    return new AppiumAndroidWidgetImageTreeItem() { AppiumElementInfo = AEI };
                case "android.widget.EditText":
                    return new AppiumAndroidWidgetEditTextTreeItem() { AppiumElementInfo = AEI };

                default:
                    // return simple basic Appium TVI
                    return new AppiumElementTreeItemBase() { AppiumElementInfo = AEI };
            }
        }
    }
}
