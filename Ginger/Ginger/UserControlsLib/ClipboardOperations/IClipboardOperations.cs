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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;

namespace Ginger.UserControlsLib
{
    public delegate void PasteItemEventHandler(PasteItemEventArgs EventArgs);

    public interface IClipboardOperations
    {
        ObservableList<RepositoryItemBase> GetSelectedItems();

        IObservableList GetSourceItemsAsIList();

        ObservableList<RepositoryItemBase> GetSourceItemsAsList();

        void SetSelectedIndex(int index);

        event PasteItemEventHandler PasteItemEvent;

        void OnPasteItemEvent(PasteItemEventArgs.ePasteType pasteType, RepositoryItemBase item);

    }
}
