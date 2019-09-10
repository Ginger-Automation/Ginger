using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.Plugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSAccessDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "MSAccess Database plugin";
            Console.WriteLine("Starting MSAccess Database Plugin");

            using (GingerNodeStarter gingerNodeStarter = new GingerNodeStarter())
            {
                if (args.Length > 0)
                {
                    gingerNodeStarter.StartFromConfigFile(args[0]);  // file name 
                }
                else
                {                    
                    gingerNodeStarter.StartNode("MSAccess Service 1", new MSAccessDBCon(), SocketHelper.GetLocalHostIP(), 15001);                    
                }
                gingerNodeStarter.Listen();
            }

        }
    }
}
