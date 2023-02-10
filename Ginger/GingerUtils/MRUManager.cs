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

using System.Collections.Generic;
using System.IO;

namespace Amdocs.Ginger.Utils
{
    // Most Recently Used Manager Class
    public class MRUManager
    {
        public string MRUFileName {get; set;}
        public int MaxEntries { get; set; }

        // TODO: config for user to decide list long
        public void Init(string FileName, int MaxEntries = 10)
        {
            this.MaxEntries = MaxEntries;
            this.MRUFileName = FileName;
        }

        public string[] GetList()
        { 
            string[] list;
            if (File.Exists(MRUFileName))
            {
                list = File.ReadAllLines(MRUFileName);
            }
            else
            {
                 list = new string[0];
            }
            return list;                
        }

        public void AddItem(string item)
        {
            string[] list = GetList();

            // using List is easier
            List<string> alist = new List<string>(list);

            // If exist remove
            while (true)
            {
                int i = alist.FindIndex(x => x == item);
                if (i >= 0)
                    alist.RemoveAt(i);
                else
                    break;
            }
            
            // insert it in first place
            alist.Insert(0, item);

            if (alist.Count > MaxEntries)
            {
                alist.RemoveAt(alist.Count - 1);
            }
            try
            {
                File.WriteAllLines(MRUFileName, alist);
            }
            catch { }
        }
    }
}
