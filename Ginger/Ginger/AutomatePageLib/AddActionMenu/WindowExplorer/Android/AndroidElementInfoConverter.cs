#region License
/*
Copyright © 2014-2024 European Support Limited

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
using GingerCore.Drivers.AndroidADB;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.WindowExplorer.Android
{
    public class AndroidElementInfoConverter
    {
        internal static ITreeViewItem GetTreeViewItemFor(ElementInfo EI)
        {
            // TODO verify if pl.Name = ElementInfo

            AndroidElementInfo AEI = (AndroidElementInfo)EI;

            return AEI.ElementType switch
            {
                "android.widget.Button" => new AndroidWidgetButtonTreeItem() { AndroidElementInfo = AEI },
                "android.widget.CheckBox" => new AndroidWidgetCheckBoxTreeItem() { AndroidElementInfo = AEI },
                "android.widget.TextView" => new AndroidWidgetTextViewTreeItem() { AndroidElementInfo = AEI },
                "android.widget.ImageView" => new AndroidWidgetImageTreeItem() { AndroidElementInfo = AEI },
                "android.widget.EditText" => new AndroidWidgetEditTextTreeItem() { AndroidElementInfo = AEI },
                _ => new AndroidElementTreeItemBase() { AndroidElementInfo = AEI },// return simple basic Android TVI
            };
        }
    }
}
