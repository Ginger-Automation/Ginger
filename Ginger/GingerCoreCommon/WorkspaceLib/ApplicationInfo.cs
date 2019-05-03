using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace amdocs.ginger.GingerCoreNET
{
    public class ApplicationInfo
    {
        public string AppName;
        public string AppFullProductName;
        private string mAppVersion = String.Empty;
        System.Diagnostics.FileVersionInfo fileVersionInfo;

        public ApplicationInfo()
        {
            fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            AppName = fileVersionInfo.FileDescription;//"Ginger"
            AppFullProductName = fileVersionInfo.ProductName;//"Ginger by Amdocs"
        }

        public string AppVersion
            {
                get
                {
                    if (mAppVersion == string.Empty)
                    {
                        if (fileVersionInfo.ProductPrivatePart != 0)//Alpha
                        {
                            mAppVersion = string.Format("{0}.{1}.{2}.{3}", fileVersionInfo.ProductMajorPart, fileVersionInfo.ProductMinorPart, fileVersionInfo.ProductBuildPart, fileVersionInfo.ProductPrivatePart);
                            // mAppVersion += "(Alpha, Build Time: " + App.AppBuildTime.ToString("dd-MMM-yyyy hh:mm tt") + ")";
                            mAppVersion += "(Alpha, Build Time: " + AssemblyCreationDate().ToString("dd-MMM-yyyy hh:mm tt") + ")";

                        }
                        else if (fileVersionInfo.ProductBuildPart != 0)//Beta
                        {
                            mAppVersion = string.Format("{0}.{1}.{2}", fileVersionInfo.ProductMajorPart, fileVersionInfo.ProductMinorPart, fileVersionInfo.ProductBuildPart);
                            // mAppVersion += "(Beta, Build Date: " + App.AppBuildTime.ToString("dd-MMM-yyyy") + ")";
                            mAppVersion += "(Beta, Build Date: " + AssemblyCreationDate().ToString("dd-MMM-yyyy") + ")";

                        }
                        else//Official Release
                        {
                            mAppVersion = string.Format("{0}.{1}", fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart);
                        }
                    }

                    return mAppVersion;
                }
            }


        private bool mAppBuildTimeCalculated = false;
        private DateTime mAppBuildTime;
        public DateTime AppBuildTime
        {
            get
            {
                if (mAppBuildTimeCalculated == false)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
                    mAppBuildTime = fileInfo.LastWriteTime;
                    mAppBuildTimeCalculated = true;
                }

                return mAppBuildTime;
            }
        }


        private static string mAppShortVersion = String.Empty;
        public string AppShortVersion
        {
            get
            {
                if (mAppShortVersion == string.Empty)
                {
                    if (fileVersionInfo.ProductPrivatePart != 0)//Alpha
                    {
                        mAppShortVersion = string.Format("{0}.{1}.{2}.{3}", fileVersionInfo.ProductMajorPart, fileVersionInfo.ProductMinorPart, fileVersionInfo.ProductBuildPart, fileVersionInfo.ProductPrivatePart);
                    }
                    else if (fileVersionInfo.ProductBuildPart != 0)//Beta
                    {
                        mAppShortVersion = string.Format("{0}.{1}.{2}", fileVersionInfo.ProductMajorPart, fileVersionInfo.ProductMinorPart, fileVersionInfo.ProductBuildPart);
                    }
                    else//Official Release
                    {
                        mAppShortVersion = string.Format("{0}.{1}", fileVersionInfo.ProductMajorPart, fileVersionInfo.ProductMinorPart);
                    }
                }

                return mAppShortVersion;
            }
        }

        public string FileName { get; set; }   // !!!!!!!!!!!!!!!!

        DateTime AssemblyCreationDate()
        {
            // !!!!!!!!!!!!!!!!

            // note: doesn't calculate daylight savings - low priority
            // Assembly.GetExecutingAssembly().
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime assemblyCreationDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.MinorRevision * 2);
            return assemblyCreationDate;
        }


    }
}
