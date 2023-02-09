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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowPages;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.UserControlsLib
{
    public class ClipboardOperationsHandler
    {
        public static ObservableList<RepositoryItemBase> CopiedorCutItems = new ObservableList<RepositoryItemBase>();
        public static IObservableList CutSourceList = null;

        public static void CopySelectedItems(IClipboardOperations containerControl)
        {
            try
            {
                CutSourceList = null;
                CopiedorCutItems.Clear();
                foreach (RepositoryItemBase item in containerControl.GetSelectedItems())
                {
                    CopiedorCutItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Copy Operation Failed");
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while Copying item", ex);
            }
        }

        public static void SetCopyItems(ObservableList<RepositoryItemBase> items)
        {
            try
            {
                CutSourceList = null;
                CopiedorCutItems.Clear();
                foreach (RepositoryItemBase item in items)
                {
                    CopiedorCutItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Copy Operation Failed");
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while Copying item", ex);
            }
        }

        public static void CutSelectedItems(IClipboardOperations containerControl)
        {
            try
            {
                CutSourceList = containerControl.GetSourceItemsAsIList();
                CopiedorCutItems.Clear();
                foreach (RepositoryItemBase item in containerControl.GetSelectedItems())
                {
                    CopiedorCutItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Cut Operation Failed");
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while Cutting item", ex);
            }
        }

        public static void SetCutItems(IClipboardOperations containerControl, ObservableList<RepositoryItemBase> items)
        {
            try
            {
                CutSourceList = containerControl.GetSourceItemsAsIList();
                CopiedorCutItems.Clear();
                foreach (RepositoryItemBase item in items)
                {
                    CopiedorCutItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Cut Operation Failed");
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while Cutting item", ex);
            }
        }

        /// <summary>
        /// Paste the Copied or Cut Items
        /// </summary>
        /// <param name="containerControl">The UI control which paste is done on (DataGrid/TreeView/ListView)</param>
        /// <param name="propertiesToSet">List of properties PropertyName-Value to set in reflection to they paste item</param>
        public static void PasteItems(IClipboardOperations containerControl,  List<Tuple<string,object>> propertiesToSet = null, int currentIndex = -1, Context context = null)
        {
            bool IsValidActionPlatform = true;

            ((Control)containerControl).Dispatcher.Invoke(() =>
            {
                try
                {
                    if (CopiedorCutItems.Count > 0)
                    {
                        Reporter.ToStatus(eStatusMsgKey.PasteProcess, null, string.Format("Performing paste operation for {0} items...", CopiedorCutItems.Count));

                        if (CutSourceList != null)
                        {
                            //CUT
                            //first remove from cut source
                            foreach (RepositoryItemBase item in CopiedorCutItems)
                            {
                                //clear from source
                                CutSourceList.Remove(item);
                                if (currentIndex > 0)
                                {
                                    currentIndex--;
                                }
                                //set needed properties if any
                                SetProperties(item, propertiesToSet);
                                if (!containerControl.GetSourceItemsAsIList().Contains(item))//Not cut & paste on same grid
                                {
                                    //set unique name
                                    GingerCoreNET.GeneralLib.General.SetUniqueNameToRepoItem(containerControl.GetSourceItemsAsList(), item);
                                }
                                //check action platform before copy
                                if (!ActionsFactory.IsValidActionPlatformForActivity(item, context))
                                {
                                    IsValidActionPlatform = false;
                                    continue;
                                }
                                //paste on target and select                           
                                containerControl.SetSelectedIndex(AddItemAfterCurrent(containerControl, item, currentIndex));
                                //Trigger event for changing sub classes fields
                                containerControl.OnPasteItemEvent(PasteItemEventArgs.ePasteType.PasteCutedItem, item);
                            }

                            //clear so will be past only once
                            CutSourceList = null;
                            CopiedorCutItems.Clear();
                        }
                        else
                        {
                            //COPY
                            //paste on target
                            foreach (RepositoryItemBase item in CopiedorCutItems)
                            {
                                RepositoryItemBase copiedItem = item.CreateCopy();
                                //set unique name
                                GingerCoreNET.GeneralLib.General.SetUniqueNameToRepoItem(containerControl.GetSourceItemsAsList(), copiedItem, "_Copy");
                                //set needed properties if any
                                SetProperties(copiedItem, propertiesToSet);

                                //check action platform before copy
                                if (!ActionsFactory.IsValidActionPlatformForActivity(copiedItem,context))
                                {
                                    IsValidActionPlatform = false;
                                    continue;
                                }
                                //add and select
                                containerControl.SetSelectedIndex(AddItemAfterCurrent(containerControl, copiedItem, currentIndex));
                                //Trigger event for changing sub classes fields
                                containerControl.OnPasteItemEvent(PasteItemEventArgs.ePasteType.PasteCopiedItem, copiedItem);
                            }
                        }
                    }
                    else
                    {
                        Reporter.ToStatus(eStatusMsgKey.PasteProcess, null, "No items found to paste");
                    }
                    if (!IsValidActionPlatform)
                    {
                        Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "Unable to copy actions with different platform.");
                    }
                }
                catch (Exception ex)
                {                   
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation Failed, make sure the copied/cut items type and destination type is matching." + System.Environment.NewLine + System.Environment.NewLine + "Error: " + ex.Message);
                    //CutSourceList = null;
                    //CopiedorCutItems.Clear();                    
                }
                finally
                {
                    Reporter.HideStatusMessage();
                }
            });
        }

        private static void SetProperties(RepositoryItemBase item, List<Tuple<string, object>> propertiesToSet)
        {
            if (propertiesToSet != null)
            {
                try
                {
                    foreach (Tuple<string, object> property in propertiesToSet)
                    {
                        item.GetType().GetProperty(property.Item1).SetValue(item, property.Item2);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to set the property to the item {0} as part of Paste process", item.ItemName), ex);
                }
            }
        }

        private static int AddItemAfterCurrent(IClipboardOperations containerControl, RepositoryItemBase item, int currentIndex = -1)
        {
            int insertIndex = 0;

            //adding the new item after current selected item            
            if (currentIndex == -1 || containerControl.GetSourceItemsAsIList().CurrentItem != null) 
            {
                currentIndex = containerControl.GetSourceItemsAsIList().IndexOf((RepositoryItemBase)containerControl.GetSourceItemsAsIList().CurrentItem);
            }

            if (currentIndex >= 0)
            {
                insertIndex = currentIndex + 1;
            }

            containerControl.GetSourceItemsAsIList().Insert(insertIndex, item);

            return insertIndex;            
        }

    }
}
