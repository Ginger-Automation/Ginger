using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Core;
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
        public static bool TestConnection(SourceControlBase SourceControl, SourceControlConnDetailsPage.eSourceControlContext context, bool ignoreSuccessMessage)
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
            bool result = true;
            bool conflictHandled = false;
            if (!SourceControl.GetLatest(path, ref error, ref conflictsPaths))
            {

                foreach (string cPath in conflictsPaths)
                {
                    ResolveConflictPage resConfPage = new ResolveConflictPage(cPath);
                    if (WorkSpace.Instance.RunningInExecutionMode == true)
                        SourceControlIntegration.ResolveConflicts(SourceControl, cPath, eResolveConflictsSide.Server);
                    else
                        resConfPage.ShowAsWindow();
                    result = resConfPage.IsResolved;

                    if (!result)
                    {
                        Reporter.ToUser(eUserMsgKey.SourceControlGetLatestConflictHandledFailed);
                        return false;
                    }
                    conflictHandled = true;
                }
                if (!conflictHandled)
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlUpdateFailed, error);
                    return false;
                }
            }
            return true;
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
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@SourceControlItemAdded_10x10.png"));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Modified:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@SourceControlItemChange_10x10.png"));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Deleted:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@SourceControlItemDeleted_10x10.png"));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Equel:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@SourceControlItemUnchanged_10x10.png"));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.LockedByAnotherUser:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Lock_Red_10x10.png"));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.LockedByMe:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Lock_Yellow_10x10.png"));
                    break;
            }
            return img;
        }
    }
}
