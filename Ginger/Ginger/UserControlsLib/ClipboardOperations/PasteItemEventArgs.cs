using Amdocs.Ginger.Repository;

namespace Ginger.UserControlsLib
{
    public class PasteItemEventArgs
    {
        public enum ePasteType
        {
            PasteCopiedItem,
            PasteCutedItem,
        }

        public ePasteType PasteType;
        public RepositoryItemBase Item;

        public PasteItemEventArgs(ePasteType pasteType, RepositoryItemBase item)
        {
            this.PasteType = pasteType;
            this.Item = item;
        }
    }
}
