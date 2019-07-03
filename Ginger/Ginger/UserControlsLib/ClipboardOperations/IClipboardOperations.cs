using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;

namespace Ginger.UserControlsLib
{
    public delegate void PasteItemEventHandler(PasteItemEventArgs EventArgs);

    public interface IClipboardOperations
    {
        ObservableList<RepositoryItemBase> GetSelectedItems();

        IObservableList GetItemsSourceList();

        void SetSelectedIndex(int index);

        event PasteItemEventHandler PasteItemEvent;

        void OnPasteItemEvent(PasteItemEventArgs.ePasteType pasteType, RepositoryItemBase item);

    }
}
