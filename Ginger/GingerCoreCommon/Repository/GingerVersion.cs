#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.Common.Repository
{

    
    public class GingerVersion
    {

        private static string mGingerVersion;
        private static long GingerVersionAsLong = 0;
        public static long GetCurrentVersionAsLong()
        {
            if (GingerVersionAsLong == 0)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                GingerVersionAsLong = fvi.FileMajorPart * 1000000 + fvi.FileMinorPart * 10000 + fvi.FileBuildPart * 100 + fvi.FilePrivatePart;
                // fvi.IsDebug use in help about!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }
            return GingerVersionAsLong;
        }

        public static string GetCurrentVersion()
        {
            if (string.IsNullOrEmpty(mGingerVersion))
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                mGingerVersion = fvi.FileVersion;
            }
            return mGingerVersion;
        }

       
    }
}
