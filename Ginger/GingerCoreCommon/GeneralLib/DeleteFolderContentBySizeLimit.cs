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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.GeneralLib
{
    public class DeleteFolderContentBySizeLimit
    {
        public DeleteFolderContentBySizeLimit(string folderName, long maxFolderSize, long offsetSize = 10)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                long currentFolderSizeMB = 0;
                DirectoryInfo di = new DirectoryInfo(folderName);
                currentFolderSizeMB = (long)((di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length) / 1024f) / 1024f);
                IEnumerable<string> folders = di.EnumerateDirectories().OrderBy(d => d.CreationTime).Select(a => a.Name).ToList();
                if ((currentFolderSizeMB + offsetSize) > maxFolderSize)
                {
                    long MemorytoDelete = (currentFolderSizeMB + offsetSize) - maxFolderSize;
                    long DeletedMemory = 0;
                    foreach (string File in folders)
                    {
                        string MainFolderFullPath = Path.Combine(folderName, File);
                        long length = (Directory.GetFiles(MainFolderFullPath, "*", SearchOption.AllDirectories).Sum(a => (new FileInfo(a).Length))) / (1024 * 1024);

                        try
                        {
                            if (Directory.Exists(MainFolderFullPath))
                            {
                                Directory.Delete(MainFolderFullPath, true);
                                DeletedMemory = DeletedMemory + length;
                            }
                        }
                        catch (Exception e)
                        {
                            if (e.ToString().Contains("The directory is not empty"))
                            {

                            }
                        }
                        if (DeletedMemory >= MemorytoDelete)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    return;
                }
            });
            t.Wait();
        }

        
    }
}
