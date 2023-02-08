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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.IO;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    public class ExecutionLoggerHelper
    {
        public ExecutionLoggerHelper()
        {
                
        }
        public void CleanDirectory(string folderName, bool isCleanFile = true)
        {
            try
            {
                if (System.IO.Directory.Exists(folderName) && isCleanFile)
                {
                    string[] files = Directory.GetFiles(folderName);
                    string[] dirs = Directory.GetDirectories(folderName);

                    foreach (string file in files)
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }

                    foreach (string dir in dirs)
                    {
                        CleanDirectory(dir, isCleanFile);
                    }

                    Directory.Delete(folderName, false);
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to Clean Execution Logger Folder '{0}', Issue:'{1}'", folderName, ex.Message));
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
                   // CleanDirectory(WorkSpace.Instance.ReportsInfo.EmailReportTempFolder);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while creating temporary folder", ex);
            }

        }
        public string GetLoggerDirectory(string logsFolder)
        {
            if (logsFolder.StartsWith(@"~"))
            {
                logsFolder = SetAbsolutePath(logsFolder);
            }
            try
            {
                if (CheckOrCreateDirectory(logsFolder))
                {
                    return logsFolder;
                }
                else
                {
                    if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null)
                    {
                        //If the path configured by user in the logger is not accessible, we set the logger path to default path
                        //logsFolder = WorkSpace.Instance.TestArtifactsFolder;                        
                        logsFolder = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder);
                        WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionLoggerConfigurationExecResultsFolder = SolutionRepository.cSolutionRootFolderSign + "ExecutionResults";
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }

            return logsFolder;
        }

        private string SetAbsolutePath(string logsFolder)
        {
            char[] delimiterChars = { '\\', '/' };
            logsFolder = logsFolder.TrimStart('~').TrimStart(delimiterChars);
            string[] pathArray = logsFolder.Split(delimiterChars);
            logsFolder = Path.Combine(pathArray);
            logsFolder = Path.Combine(WorkSpace.Instance.Solution.Folder, logsFolder);
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
                Console.WriteLine("CheckOrCreateDirectory - " + ex.Message);
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
