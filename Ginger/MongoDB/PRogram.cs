using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.Plugin.Core;
using MongoDB;

namespace GingerMongoDB
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Ginger MongoDb Database plugin";
            Console.WriteLine("Starting MongoDb Database Plugin");

            using (GingerNodeStarter gingerNodeStarter = new GingerNodeStarter())
            {
                if (args.Length > 0)
                {
                    gingerNodeStarter.StartFromConfigFile(args[0]);  // file name 
                }
                else
                {
                    gingerNodeStarter.StartNode("MSAccess Service 1", new MongoDbConnection(), SocketHelper.GetLocalHostIP(), 15001);
                }
                gingerNodeStarter.Listen();
            }

        }
    }
}
