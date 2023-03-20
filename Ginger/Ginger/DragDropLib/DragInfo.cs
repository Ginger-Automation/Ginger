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

using Amdocs.Ginger.Common;
using System;

namespace GingerWPF.DragDropLib
{
    public class DragInfo
    {
        // Text to show when item(s) is dragged
        public string Header { get; set; }

        // The data being passed like Activity, Action etc.. one row or more from grid or tree 
        public object Data { get; set; }

        // The drag source like UCGrid, UCTree etc...
        public System.Windows.DependencyObject DragSource { get; set; }

        // When drag enter we update the drag target like UCGrid, UCTree etc...
        public System.Windows.DependencyObject DragTarget { get; set; }

        // When Mouse left button clicked keep the original source
        public object OriginalSource { get; set; }

        public eDragIcon DragIcon { get; set; }

        public enum eDragIcon
        {
            DoNotDrop,
            Add,
            Move,
            Unknown,
            MultiAdd
        }

        public bool DataIsAssignableToType(Type T, bool isObservableList = false)
        {
            if (isObservableList && (Data as ObservableList<Amdocs.Ginger.Repository.RepositoryItemBase>) != null)
            {
                ObservableList<Amdocs.Ginger.Repository.RepositoryItemBase> obList = (Data as ObservableList<Amdocs.Ginger.Repository.RepositoryItemBase>);
                object listData = obList[0];
                return T.IsAssignableFrom(listData.GetType());
            }
            else
            {
                return T.IsAssignableFrom(Data.GetType());
            }
        }
    }
}
