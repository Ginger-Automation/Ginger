using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.GeneralLib
{
    public static class General
    {
        public static string LocalUserApplicationDataFolderPath
        {
            get
            {
                //TODO: check where it goes - not roaming,.,
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                appDataFolder = Path.Combine(appDataFolder, @"Amdocs\Ginger");

                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                }

                return appDataFolder;
            }
        }

        public static string DefualtUserLocalWorkingFolder
        {
            get
            {
                string workingFolder = Path.Combine(LocalUserApplicationDataFolderPath, @"\WorkingFolder");

                if (!Directory.Exists(workingFolder))
                {
                    Directory.CreateDirectory(workingFolder);
                }

                return workingFolder;
            }
        }

        /// <summary>
        /// Should use the function temporary till solution will be implemented for VE fields search
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool IsFieldToAvoidInVeFieldSearch(string fieldName)
        {
            if (fieldName == "BackupDic" || fieldName == "GetNameForFileName" || fieldName == "FilePath" || fieldName == "FileName" ||
                fieldName == "ObjFileExt" || fieldName == "ItemNameField" || fieldName == "ItemImageType" || fieldName == "ItemName" ||
                fieldName == "RelativeFilePath" ||
                fieldName == "ObjFolderName" || fieldName == "ContainingFolder" || fieldName == "ContainingFolderFullPath" ||
                fieldName == "ActInputValues" || fieldName == "ActReturnValues" || fieldName == "ActFlowControls" ||
                fieldName == "ScreenShots" ||
                fieldName == "ListStringValue" || fieldName == "ListDynamicValue")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
