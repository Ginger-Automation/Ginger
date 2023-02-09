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

using Amdocs.Ginger.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Amdocs.Ginger.Common.GeneralLib
{
    // put here functions to read files, folders etc...
    public class io
    {
        public static void testPath()
        {
            // string path = @"\\?\c:\temp\";
            string path = @"c:\temp\";
            int i = 1;
            while (path.Length < 300)
            {
                path = Path.Combine(path, "LongSubFolderName_" + i);
                // Directory.CreateDirectory(path);
                Directory.CreateDirectory(PathHelper.GetLongPath(path));
                i++;

            }
        }
    }
}
