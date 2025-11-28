#region License
/*
Copyright © 2014-2025 European Support Limited

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

        private static string mApplicationUIversion = String.Empty;
        public static string ApplicationUIversion
        {
            get
            {
                if (mApplicationUIversion == string.Empty)
                {
                    if (mFileVersionInfo.FilePrivatePart != 0)//Alpha
                    {
                        mApplicationUIversion = string.Format("20{0}.{1}-Alpha {2}.{3}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart, mFileVersionInfo.FileBuildPart, mFileVersionInfo.FilePrivatePart);
                    }
                    else if (mFileVersionInfo.FileBuildPart != 0)//Beta
                    {
                        mApplicationUIversion = string.Format("20{0}.{1}-Beta {2}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart, mFileVersionInfo.FileBuildPart);
                    }
                    else//Official Release
                    {
                        mApplicationUIversion = string.Format("20{0}.{1}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart);
                    }
                }

                return mApplicationUIversion;
            }
        }

        private static string mApplicationBackendVersion = String.Empty;
        public static string ApplicationBackendVersion
        {
            get
            {
                if (mApplicationBackendVersion == string.Empty)
                {
                    mApplicationBackendVersion = string.Format("{0}.{1}.{2}.{3}", mFileVersionInfo.FileMajorPart, mFileVersionInfo.FileMinorPart, mFileVersionInfo.FileBuildPart, mFileVersionInfo.FilePrivatePart);
                }
                return mApplicationBackendVersion;
            }
        }

        public static bool IsOfficialRelease
        {
            get
            {
                if (mFileVersionInfo.FilePrivatePart == 0 && mFileVersionInfo.FileBuildPart == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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

        public static string ConvertBackendApplicationVersionToUIVersion(string appVersion)
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
                        Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to get the '{0}' backend version as UI version", appVersion));
                        return null;//failed to get the version as long
                    }
                }

                string uiVersion = string.Empty;
                string yearPrefix = string.Empty;
                if (iMajor > 20)
                {
                    yearPrefix = "20";
                }
                if (iRevision != 0)//Alpha
                {
                    uiVersion = string.Format("{0}{1}.{2}-Alpha {3}.{4}", yearPrefix, iMajor, iMinor, iBuild, iRevision);
                }
                else if (iBuild != 0)//Beta
                {
                    uiVersion = string.Format("{0}{1}.{2}-Beta {3}", yearPrefix, iMajor, iMinor, iBuild);
                }
                else//Official Release
                {
                    uiVersion = string.Format("{0}{1}.{2}", yearPrefix, iMajor, iMinor);
                }
                return uiVersion;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to get the '{0}' backend version as UI version", appVersion), ex);
                return null;//failed to get the version as long
            }
        }
        public record VersionParts(int Major, int Minor, int Build, int Revision)
        {
            public override string ToString()
            {    
                return $"{Major}.{Minor}.{Build}.{Revision}";
            }
        }

        private static readonly Regex s_FourPart = new(@"^(\d+)\.(\d+)\.(\d+)\.(\d+)$", RegexOptions.Compiled);
        private static readonly Regex s_TwoPart = new(@"^(\d+)\.(\d+)$", RegexOptions.Compiled);

        public static long ConvertApplicationVersionToLong(string appVersion)
        {
            try
            {
                int iMajor = 0, iMinor = 0, iBuild = 0, iRevision = 0;
                Match match = s_FourPart.Match(appVersion);
                if (match.Success)
                {
                    try { iMajor = Int32.Parse(match.Groups[1].Value); }
                    catch { }
                    try { iMinor = Int32.Parse(match.Groups[2].Value); }
                    catch { }
                    try { iBuild = Int32.Parse(match.Groups[3].Value); }
                    catch { }
                    try { iRevision = Int32.Parse(match.Groups[4].Value); }
                    catch { }
                }
                else
                {
                    match = s_TwoPart.Match(appVersion);
                    if (match.Success)
                    {
                        try { iMajor = Int32.Parse(match.Groups[1].Value); }
                        catch { }
                        try { iMinor = Int32.Parse(match.Groups[2].Value); }
                        catch { }
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to get the '{0}' version as Long", appVersion));
                        return 0;//failed to get the version as long
                    }
                }

                if (iBuild == 0 && iRevision == 0)//to make sure official release will be count as higher version
                {
                    iBuild = 99;
                    iRevision = 99;
                }

                long version = iMajor * 1000000 + iMinor * 10000 + iBuild * 100 + iRevision;
                return version;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to get the '{0}' version as Long", appVersion), ex);
                return 0;//failed to get the version as long
            }
        }

        public static VersionParts ConvertApplicationVersionToRecord(string appVersion)
        {
            try
            {
                var fourPart = s_FourPart.Match(appVersion);
                if (fourPart.Success)
                {
                    if (int.TryParse(fourPart.Groups[1].Value, out int maj) &&
                        int.TryParse(fourPart.Groups[2].Value, out int min) &&
                        int.TryParse(fourPart.Groups[3].Value, out int bld) &&
                        int.TryParse(fourPart.Groups[4].Value, out int rev))
                    {
                        return new VersionParts(maj, min, bld, rev);
                    }
                }

                // 2-part: "X.Y" -> default Build/Revision to 99
                var twoPart = s_TwoPart.Match(appVersion);
                if (twoPart.Success)
                {
                    if (int.TryParse(twoPart.Groups[1].Value, out int maj) &&
                        int.TryParse(twoPart.Groups[2].Value, out int min))
                    {
                        return new VersionParts(maj, min, 99, 99);
                    }
                }

                return new VersionParts(0, 0, 0, 0);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to get the '{0}' version as Version Parts Record", appVersion), ex);

                return new VersionParts(0, 0, 0, 0);
            }
        }
    }
}