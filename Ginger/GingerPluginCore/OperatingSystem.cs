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

using System.Runtime.InteropServices;

namespace GingerPluginCore
{
    //TDOO: Dup with GingerUtils remove and use GingerUtils but verify it is included in the nuget created 
    public static class OperatingSystem
    {

        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static string GetCurrentOS()
        {
            return
            (IsWindows() ? "Windows" : null) ??
            (IsMacOS() ? "Mac" : null) ??
            (IsLinux() ? "Linux" : null);
        }
    }

}
