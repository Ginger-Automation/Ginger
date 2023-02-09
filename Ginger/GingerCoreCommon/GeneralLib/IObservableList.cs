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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using Amdocs.Ginger.Common.Repository;

namespace Amdocs.Ginger.Common
{
    public interface IObservableList : ICollection, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        void Move(int oldIndex, int newIndex);
       
        object CurrentItem { get; set; }

        void Undo();

        void ClearAll();

        void SaveUndoData();

        bool LazyLoad { get;}

        bool AvoidLazyLoad { get; set; }

        LazyLoadListDetails LazyLoadDetails { get; set; }        

        List<object> ListItems { get; }

        //void DoLazyLoadItem(string s);

        bool SyncCurrentItemWithViewSelectedItem { get; set; } 
        bool SyncViewSelectedItemWithCurrentItem { get; set; } 

        string FilterStringData { get; set; }
    }

    public enum eDirection
    {
        Ascending,
        Descending
    }
}
