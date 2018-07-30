using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.Functionalities
{
    public class ItemToSearchIn
    {
        public RepositoryItemBase OriginItemObject;
        public RepositoryItemBase Item;
        public RepositoryItemBase ParentItemToSave;
        public string ItemParent;
        public string FoundField;


        public ItemToSearchIn(RepositoryItemBase originItemObject, RepositoryItemBase item, RepositoryItemBase parentItemToSave, string itemParent, string foundField)
        {
            OriginItemObject = originItemObject;
            Item = item;
            ParentItemToSave = parentItemToSave;
            ItemParent = itemParent;
            FoundField = foundField;
        }

    }
}
