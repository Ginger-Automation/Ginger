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
using System.Text;

namespace GingerCoreNET.Drivers.CommunicationProtocol
{
    public class GingerSocketLog
    {
        public DateTime TimeStamp {get; set;}
        public string LogType {get; set;}
        public string Name { get; set; }
        public string Info {get; set;}
        public int Len { get; set; }
        public long Elapsed { get; set; }
        public NewPayLoad PayLoad { get; set; }

        public GingerSocketLog()
        {
            TimeStamp = DateTime.Now;
        }

        internal void SetPayLoad(NewPayLoad pl)
        {            
            Name = pl.Name;
            Info = pl.BufferInfo;
            Len = pl.PackageLen();
            PayLoad = pl;
        }

        public string ascii { get {
                byte[] bytes = PayLoad.GetPackage();
                string asciiString = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                return asciiString;
            }
        }
    }
}
