using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.UserControlsLib.UCListView
{
    public interface IListViewItemInfo 
    {
        void SetItem(object item);

        string GetItemNameField();

        string GetItemDescriptionField();

        string GetItemGroupField();

        string GetItemTagsField();

        string GetItemIconField();

        string GetItemIconTooltipField();

        string GetItemExecutionStatusField();

        string GetItemActiveField();

        ListItemUniqueIdentifier GetItemUniqueIdentifier(object item);

        List<ListItemNotification> GetNotificationsList(object item);

        List<ListItemOperation> GetOperationsList(object item);

        List<ListItemGroupOperation> GetGroupOperationsList();
    }
}
