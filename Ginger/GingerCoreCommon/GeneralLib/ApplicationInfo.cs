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

using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Amdocs.Ginger.Common.GeneralLib
{
    public class ApplicationInfo
    {
        private static System.Diagnostics.FileVersionInfo mFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                       
        public ApplicationInfo()
        {
        }

        public static string ApplicationName = mFileVersionInfo.ProductName;//"Ginger by Amdocs"

        private static string mApplicationVersion = String.Empty;
        public static string ApplicationVersion
        {
            get
            {
                if (mApplicationVersion == string.Empty)
                {
                    if (mFileVersionInfo.FilePrivatePart != 0)//Alpha
                    {
                        mApplicationVersion = string.Format("{0}.{1}.{2}.{3}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart, mFileVersionInfo.FileBuildPart, mFileVersionInfo.FilePrivatePart);
                    }
                    else if (mFileVersionInfo.FileBuildPart != 0)//Beta
                    {
                        mApplicationVersion = string.Format("{0}.{1}.{2}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart, mFileVersionInfo.FileBuildPart);
                    }
                    else//Official Release
                    {
                        mApplicationVersion = string.Format("{0}.{1}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart);
                    }
                }
                return mApplicationVersion;
            }
        }

        private static string mApplicationMajorVersion = String.Empty;
        public static string ApplicationMajorVersion
        {
            get
            {
                if (mApplicationMajorVersion == string.Empty)
                {
                    mApplicationMajorVersion = string.Format("{0}.{1}.{2}.{3}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart, 0, 0);
                }
                return mApplicationMajorVersion;
            }
        }

        private static string mApplicationVersionWithInfo = String.Empty;
        public static string ApplicationVersionWithInfo
        {
            get
            {
                if (mApplicationVersionWithInfo == string.Empty)
                {
                    if (mFileVersionInfo.FilePrivatePart != 0)//Alpha
                    {
                        mApplicationVersionWithInfo = string.Format("{0}.{1}.{2}.{3}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart, mFileVersionInfo.FileBuildPart, mFileVersionInfo.FilePrivatePart);
                        mApplicationVersionWithInfo += "(Alpha, Build Time: " + ApplicationBuildTime.ToString("dd-MMM-yyyy hh:mm tt") + ")";                       
                    }
                    else if (mFileVersionInfo.FileBuildPart != 0)//Beta
                    {
                        mApplicationVersionWithInfo = string.Format("{0}.{1}.{2}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart, mFileVersionInfo.FileBuildPart);
                        mApplicationVersionWithInfo += "(Beta, Build Date: " + ApplicationBuildTime.ToString("dd-MMM-yyyy") + ")";                       
                    }
                    else//Official Release
                    {
                        mApplicationVersionWithInfo = string.Format("{0}.{1}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart);
                    }
                }

                return mApplicationVersionWithInfo;
            }
        }


        private static bool mAppBuildTimeCalculated = false;
        private static DateTime mApplicationBuildTime;
        public static DateTime ApplicationBuildTime
        {
            get
            {
                if (mAppBuildTimeCalculated == false)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
                    mApplicationBuildTime = fileInfo.LastWriteTime;
                    mAppBuildTimeCalculated = true;
                }

                return mApplicationBuildTime;
            }
        }
        
        public static long ConvertApplicationVersionToLong(string appVersion)
        {
            try
            {
                int iMajor = 0, iMinor = 0, iBuild = 0, iRevision = 0;
                Regex regex = new Regex(@"(\d+)\.(\d+)\.(\d+)\.(\d+)");
                Match match = regex.Match(appVersion);
                if (match.Success)
                {
                    try { iMajor = Int32.Parse(match.Groups[1].Value); }
                    catch (Exception) { }
                    try { iMinor = Int32.Parse(match.Groups[2].Value); }
                    catch (Exception) { }
                    try { iBuild = Int32.Parse(match.Groups[3].Value); }
                    catch (Exception) { }
                    try { iRevision = Int32.Parse(match.Groups[4].Value); }
                    catch (Exception) { }
                }
                else
                {
                    regex = new Regex(@"(\d+)\.(\d+)");
                    match = regex.Match(appVersion);
                    if (match.Success)
                    {
                        try { iMajor = Int32.Parse(match.Groups[1].Value); }
                        catch (Exception) { }
                        try { iMinor = Int32.Parse(match.Groups[2].Value); }
                        catch (Exception) { }
                    }
                    else
                    {
                        return 0;//failed to get the version as long
                    }
                }

                long version = iMajor * 1000000 + iMinor * 10000 + iBuild * 100 + iRevision;
                return version;
            }
            catch (Exception)
            {
                return 0;//failed to get the version as long
            }
        }


        //public string FileName { get; set; }   // !!!!!!!!!!!!!!!!

        //DateTime AssemblyCreationDate()
        //{
        //    // !!!!!!!!!!!!!!!!

        //    // note: doesn't calculate daylight savings - low priority
        //    // Assembly.GetExecutingAssembly().
        //    Version version = Assembly.GetExecutingAssembly().GetName().Version;
        //    DateTime assemblyCreationDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.MinorRevision * 2);
        //    return assemblyCreationDate;
        //}

    }
}
