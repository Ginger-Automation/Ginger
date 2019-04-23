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

        string GetItemExecutionStatusField();

        string GetItemActiveField();

        List<ListItemNotification> GetNotificationsList(object item);

        List<ListItemOperation> GetOperationsList(object item);
    }
}
