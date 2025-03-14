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


namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class NodeConfigFile
    {
        public string Name { get; set; }
        public string GingerGridHost { get; set; }
        public int GingerGridPort { get; set; }

        public NodeConfigFile(string configFileName)
        {
            // config file contains:
            // Name=
            // GingerGridHost=
            // GingerGridPort=
            string[] lines = System.IO.File.ReadAllLines(configFileName);
            Name = GetValue(lines[0]);
            GingerGridHost = GetValue(lines[1]);
            GingerGridPort = int.Parse(GetValue(lines[2]));
        }

        private string GetValue(string line)
        {
            return line[(line.IndexOf("=") + 1)..];
        }

        //public static string CreateNodeConfigFile(string name, string serviceId)
        //{
        //    string txt = name + " | " + serviceId + " | " + SocketHelper.GetLocalHostIP() + " | " + WorkSpace.Instance.LocalGingerGrid.Port + Environment.NewLine;
        //    string fileName = Path.GetTempFileName();
        //    File.WriteAllText(fileName, txt);
        //    return fileName;
        //}
    }
}
