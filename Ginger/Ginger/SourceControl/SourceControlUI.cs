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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Core;
using Amdocs.Ginger.Repository;
using Ginger.ConflictResolve;
using GingerCore;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace Ginger.SourceControl
{
    public class SourceControlUI
    {
        public static bool TestConnection(SourceControlBase SourceControl, bool ignoreSuccessMessage)
        {
            string error = string.Empty;
            bool res = false;

            res = SourceControl.TestConnection(ref error);
            if (res)
            {
                if (!ignoreSuccessMessage)
                    Reporter.ToUser(eUserMsgKey.SourceControlConnSucss);
                return true;
            }
            else
            {
                if (error.Contains("remote has never connected"))
                    Reporter.ToUser(eUserMsgKey.SourceControlRemoteCannotBeAccessed, error);
                else
                    Reporter.ToUser(eUserMsgKey.SourceControlConnFaild, error);
                return false;
            }
        }
        public static bool GetLatest(string path, SourceControlBase SourceControl)
        {
            string error = string.Empty;
            List<string> conflictsPaths = new List<string>();

            if (!SourceControl.GetLatest(path, ref error, ref conflictsPaths))
            {
                if (conflictsPaths.Count != 0)
                {
                    ResolveConflictWindow conflictWindow = new ResolveConflictWindow(conflictsPaths);
                    if (WorkSpace.Instance.RunningInExecutionMode == true)
                    {
                        conflictsPaths.ForEach(path => SourceControlIntegration.ResolveConflicts(SourceControl, path, eResolveConflictsSide.Server));
                    }
                    else
                    {
                        conflictWindow.ShowAsWindow();
                    }
                    if (!conflictWindow.IsResolved)
                    {
                        if (!string.IsNullOrEmpty(error))
                        {
                            Reporter.ToUser(eUserMsgKey.SourceControlUpdateFailed, error);
                            return false;
                        }
                        else
                        {
                            Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Unable to resolve conflict");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static bool Revert(string path, SourceControlBase SourceControl)
        {
            string error = string.Empty;
            return SourceControl.Revert(path, ref error);
        }


        internal static BitmapImage GetItemSourceControlImage(string FileName, ref SourceControlFileInfo.eRepositoryItemStatus ItemSourceControlStatus)
        {

            if (WorkSpace.Instance.Solution.SourceControl == null || FileName == null)
            {
                return null;
            }

            SourceControlFileInfo.eRepositoryItemStatus RIS = SourceControlIntegration.GetFileStatus(WorkSpace.Instance.Solution.SourceControl, FileName, WorkSpace.Instance.Solution.ShowIndicationkForLockedItems);
            ItemSourceControlStatus = RIS;
            BitmapImage img = null;
            switch (RIS)
            {
                case SourceControlFileInfo.eRepositoryItemStatus.New:
                    img = new BitmapImage(new Uri(@"/Images/" + "@SourceControlItemAdded_10x10.png", UriKind.RelativeOrAbsolute));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Modified:
                    img = new BitmapImage(new Uri(@"/Images/" + "@SourceControlItemChange_10x10.png", UriKind.RelativeOrAbsolute));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Deleted:
                    img = new BitmapImage(new Uri(@"/Images/" + "@SourceControlItemDeleted_10x10.png", UriKind.RelativeOrAbsolute));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Equel:
                    img = new BitmapImage(new Uri(@"/Images/" + "@SourceControlItemUnchanged_10x10.png", UriKind.RelativeOrAbsolute));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.LockedByAnotherUser:
                    img = new BitmapImage(new Uri(@"/Images/" + "@Lock_Red_10x10.png", UriKind.RelativeOrAbsolute));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.LockedByMe:
                    img = new BitmapImage(new Uri(@"/Images/" + "@Lock_Yellow_10x10.png", UriKind.RelativeOrAbsolute));
                    break;
            }
            return img;
        }
    }
}
