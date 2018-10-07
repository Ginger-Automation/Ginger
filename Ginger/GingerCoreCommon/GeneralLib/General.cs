using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Amdocs.Ginger.Common.GeneralLib
{
    public class General
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
    }
}
