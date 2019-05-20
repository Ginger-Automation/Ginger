using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    public class ExecutionLoggerHelper
    {
        public ExecutionLoggerHelper()
        {
                
        }
        public void CleanDirectory(string folderName, bool isCleanFile = true)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderName);
            if (isCleanFile)
                foreach (System.IO.FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            foreach (System.IO.DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
        public void CreateTempDirectory()
        {
            try
            {
                if (!Directory.Exists(WorkSpace.Instance.ReportsInfo.EmailReportTempFolder))
                {
                    System.IO.Directory.CreateDirectory(WorkSpace.Instance.ReportsInfo.EmailReportTempFolder);
                }
                else
                {
                    CleanDirectory(WorkSpace.Instance.ReportsInfo.EmailReportTempFolder);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while creating temporary folder", ex);
            }

        }
        public string GetLoggerDirectory(string logsFolder)
        {
            logsFolder = logsFolder.Replace(@"~", WorkSpace.Instance.Solution.Folder);
            try
            {
                if (CheckOrCreateDirectory(logsFolder))
                {
                    return logsFolder;
                }
                else
                {
                    //If the path configured by user in the logger is not accessible, we set the logger path to default path
                    logsFolder = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"ExecutionResults\");
                    System.IO.Directory.CreateDirectory(logsFolder);

                    WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.ExecutionLoggerConfigurationExecResultsFolder = @"~\ExecutionResults\";
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }

            return logsFolder;
        }
        public Boolean CheckOrCreateDirectory(string directoryPath)
        {
            try
            {
                if (System.IO.Directory.Exists(directoryPath))
                {
                    return true;
                }
                else
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public string folderNameNormalazing(string folderName)
        {
            foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(invalidChar.ToString(), "");
            }
            folderName = folderName.Replace(@".", "");
            folderName = folderName.TrimEnd().TrimEnd('-').TrimEnd();
            if (folderName.Length > 30)
            {
                folderName = folderName.Substring(0, 30);
            }
            folderName = folderName.TrimEnd().TrimEnd('-').TrimEnd();
            return folderName;
        }
    }
}
