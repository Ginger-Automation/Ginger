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
    }
}
