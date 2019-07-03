using System.Collections.Generic;

namespace Ginger.UserControlsLib.UCListView
{
    public interface IListViewHelper
    {        
        void SetItem(object item);

        string GetItemNameField();

        string GetItemDescriptionField();

        string GetItemNameExtentionField();

        string GetItemTagsField();

        string GetItemIconField();

        string GetItemIconTooltipField();

        string GetItemExecutionStatusField();

        string GetItemActiveField();

        List<ListItemOperation> GetListOperations();

        List<ListItemOperation> GetListExtraOperations();

        List<ListItemGroupOperation> GetItemGroupOperationsList();

        ListItemUniqueIdentifier GetItemUniqueIdentifier(object item);

        List<ListItemNotification> GetItemNotificationsList(object item);

        List<ListItemOperation> GetItemOperationsList(object item);

        List<ListItemOperation> GetItemExtraOperationsList(object item);

        List<ListItemOperation> GetItemExecutionOperationsList(object item);        
    }
}
