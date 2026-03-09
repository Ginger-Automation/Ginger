#region License
/*
Copyright © 2014-2026 European Support Limited

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
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.WindowExplorer.Appium
{
    public class AppiumElementInfoConverter
    {
        internal static ITreeViewItem GetTreeViewItemFor(ElementInfo EI)
        {
            // TODO verify if pl.Name = ElementInfo

            return EI.ElementType switch
            {
                "android.widget.Button" => new AppiumAndroidWidgetButtonTreeItem() { ElementInfo = EI },
                "android.widget.CheckBox" => new AppiumAndroidWidgetCheckBoxTreeItem() { ElementInfo = EI },
                "android.widget.TextView" => new AppiumAndroidWidgetTextViewTreeItem() { ElementInfo = EI },
                "android.widget.ImageView" => new AppiumAndroidWidgetImageTreeItem() { ElementInfo = EI },
                "android.widget.EditText" => new AppiumAndroidWidgetEditTextTreeItem() { ElementInfo = EI },
                _ => new AppiumElementTreeItemBase() { ElementInfo = EI },// return simple basic Appium TVI
            };
        }
    }
}
