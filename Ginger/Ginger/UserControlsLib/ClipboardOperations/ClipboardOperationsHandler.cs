using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Windows.Controls;

namespace Ginger.UserControlsLib
{
    public class ClipboardOperationsHandler
    {
        public static ObservableList<RepositoryItemBase> mCopiedorCutItems = new ObservableList<RepositoryItemBase>();
        public static IObservableList mCutSourceList = null;

        public static void CopySelectedItems(IClipboardOperations control)
        {
            try
            {
                mCutSourceList = null;
                mCopiedorCutItems.Clear();
                foreach (RepositoryItemBase item in control.GetSelectedItems())
                {
                    mCopiedorCutItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Copy Operation Failed");
                Reporter.ToLog(eLogLevel.ERROR, "Error occured while Copying item", ex);
            }
        }

        public static void SetCopyItems(ObservableList<RepositoryItemBase> items)
        {
            try
            {
                mCutSourceList = null;
                mCopiedorCutItems.Clear();
                foreach (RepositoryItemBase item in items)
                {
                    mCopiedorCutItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Copy Operation Failed");
                Reporter.ToLog(eLogLevel.ERROR, "Error occured while Copying item", ex);
            }
        }

        public static void CutSelectedItems(IClipboardOperations control)
        {
            try
            {
                mCutSourceList = control.GetItemsSourceList();
                mCopiedorCutItems.Clear();
                foreach (RepositoryItemBase item in control.GetSelectedItems())
                {
                    mCopiedorCutItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Cut Operation Failed");
                Reporter.ToLog(eLogLevel.ERROR, "Error occured while Cuting item", ex);
            }
        }

        public static void SetCutItems(IClipboardOperations control, ObservableList<RepositoryItemBase> items)
        {
            try
            {
                mCutSourceList = control.GetItemsSourceList();
                mCopiedorCutItems.Clear();
                foreach (RepositoryItemBase item in items)
                {
                    mCopiedorCutItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Cut Operation Failed");
                Reporter.ToLog(eLogLevel.ERROR, "Error occured while Cuting item", ex);
            }
        }

        public static void PasteItems(IClipboardOperations control)
        {
            ((Control)control).Dispatcher.Invoke(() =>
            {
                try
                {
                    if (mCutSourceList != null)
                    {
                        //CUT
                        //first remove from cut source
                        foreach (RepositoryItemBase item in mCopiedorCutItems)
                        {
                            if (control.GetItemsSourceList().Contains(item))//cut & paste on same grid
                            {
                                //move item
                                control.SetSelectedIndex(MoveItemAfterCurrent(control, item));
                            }
                            else
                            {
                                //set unique name
                                SetItemUniqueName(control, item, string.Empty);
                                //Trigger event for changing sub classes fields
                                control.OnPasteItemEvent(PasteItemEventArgs.ePasteType.PasteCutedItem, item);
                                //paste on target
                                AddItemAfterCurrent(control, item);
                                //clear from source
                                mCutSourceList.Remove(item);
                            }
                        }

                        //clear so will be past only once
                        mCutSourceList = null;
                        mCopiedorCutItems.Clear();
                    }
                    else
                    {
                        //COPY
                        //paste on target
                        foreach (RepositoryItemBase item in mCopiedorCutItems)
                        {
                            RepositoryItemBase copiedItem = item.CreateCopy();
                            //set unique name
                            SetItemUniqueName(control, copiedItem, "_Copy");
                            //Trigger event for changing sub classes fields
                            control.OnPasteItemEvent(PasteItemEventArgs.ePasteType.PasteCopiedItem, copiedItem);
                            //add                        
                            AddItemAfterCurrent(control, copiedItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation Failed, make sure the copied/cut items type and destination type is matching." + System.Environment.NewLine + System.Environment.NewLine + "Error: " + ex.Message);
                    mCutSourceList = null;
                    mCopiedorCutItems.Clear();                    
                }
            });
        }

        private static void SetItemUniqueName(IClipboardOperations control, RepositoryItemBase item, string suffix = "_Copy")
        {
            string originalName = item.ItemName;
            bool nameUnique = false;
            int counter = 0;
            while (nameUnique == false)
            {
                nameUnique = true;
                foreach (RepositoryItemBase t in control.GetItemsSourceList())
                    if (t.ItemName == item.ItemName)
                    {
                        nameUnique = false;
                        break;
                    }
                if (nameUnique)
                    break;
                else
                {
                    if (counter == 0)
                    {
                        item.ItemName = originalName + suffix;
                        counter = 2;
                    }
                    else
                    {
                        item.ItemName = originalName + suffix + counter;
                        counter++;
                    }
                }
            }
        }

        private static void AddItemAfterCurrent(IClipboardOperations control, RepositoryItemBase item)
        {
            //adding the new item after current selected item
            int currentIndex = -1;
            if (control.GetItemsSourceList().CurrentItem != null)
            {
                currentIndex = control.GetItemsSourceList().IndexOf((RepositoryItemBase)control.GetItemsSourceList().CurrentItem);
            }

            control.GetItemsSourceList().Insert(currentIndex + 1, item);

            control.SetSelectedIndex(currentIndex + 1);
        }

        private static int MoveItemAfterCurrent(IClipboardOperations control, RepositoryItemBase item)
        {
            int itemIndex = control.GetItemsSourceList().IndexOf(item);
            int currentIndex = control.GetItemsSourceList().IndexOf((RepositoryItemBase)control.GetItemsSourceList().CurrentItem);

            if (currentIndex >= 0)
            {
                if (itemIndex < currentIndex)
                {
                    control.GetItemsSourceList().Move(itemIndex, currentIndex);
                    return currentIndex;
                }
                else
                {
                    control.GetItemsSourceList().Move(itemIndex, currentIndex + 1);
                    return currentIndex + 1;
                }
            }
            else
            {
                return itemIndex;
            }
        }
    }
}
