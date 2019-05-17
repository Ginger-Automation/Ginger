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
                    if (mFileVersionInfo.ProductPrivatePart != 0)//Alpha
                    {
                        mApplicationVersion = string.Format("{0}.{1}.{2}.{3}", mFileVersionInfo.ProductMajorPart, mFileVersionInfo.ProductMinorPart, mFileVersionInfo.ProductBuildPart, mFileVersionInfo.ProductPrivatePart);
                    }
                    else if (mFileVersionInfo.ProductBuildPart != 0)//Beta
                    {
                        mApplicationVersion = string.Format("{0}.{1}.{2}", mFileVersionInfo.ProductMajorPart, mFileVersionInfo.ProductMinorPart, mFileVersionInfo.ProductBuildPart);
                    }
                    else//Official Release
                    {
                        mApplicationVersion = string.Format("{0}.{1}", mFileVersionInfo.ProductMajorPart, mFileVersionInfo.ProductMinorPart);
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
                    mApplicationMajorVersion = string.Format("{0}.{1}.{2}.{3}", mFileVersionInfo.ProductMajorPart, mFileVersionInfo.ProductMinorPart, 0, 0);
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
                    if (mFileVersionInfo.ProductPrivatePart != 0)//Alpha
                    {
                        mApplicationVersionWithInfo = string.Format("{0}.{1}.{2}.{3}", mFileVersionInfo.ProductMajorPart, mFileVersionInfo.ProductMinorPart, mFileVersionInfo.ProductBuildPart, mFileVersionInfo.ProductPrivatePart);
                        mApplicationVersionWithInfo += "(Alpha, Build Time: " + ApplicationBuildTime.ToString("dd-MMM-yyyy hh:mm tt") + ")";                       
                    }
                    else if (mFileVersionInfo.ProductBuildPart != 0)//Beta
                    {
                        mApplicationVersionWithInfo = string.Format("{0}.{1}.{2}", mFileVersionInfo.ProductMajorPart, mFileVersionInfo.ProductMinorPart, mFileVersionInfo.ProductBuildPart);
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
