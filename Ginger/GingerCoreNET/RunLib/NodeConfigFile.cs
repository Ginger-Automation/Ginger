using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    class NodeConfigFile
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
            return line.Substring(line.IndexOf("=") + 1);
        }

        public static string CreateNodeConfigFile(string name)
        {
            string txt = "Name=" + name + Environment.NewLine;
            txt += "GingerGridHost=" + SocketHelper.GetLocalHostIP() + Environment.NewLine;
            txt += "GingerGridPort=" + WorkSpace.Instance.LocalGingerGrid.Port + Environment.NewLine;
            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, txt);
            return fileName;
        }
    }
}
