using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.DriversLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Amdocs.Ginger.Plugin.Core
{
    public class GingerNodeStarter : IDisposable
    {

        List<GingerNode> mNodes = new List<GingerNode>();
      
        public void Dispose()
        {
            ClosedAllNodes();
        }

        /// <summary>
        /// Start service on node and connect to local GingerGrid
        /// </summary>
        /// <param name="gingerServiceObject">Service object with [GingerService] annotation</param>
        /// <param name="name">Name which will apear in GingerGrid</param>
        public void StartNode(string name, object gingerServiceObject)
        {
            GingerNode gingerNode = new GingerNode(gingerServiceObject);
            gingerNode.StartGingerNode(name, SocketHelper.GetLocalHostIP(), 15001);
            if (gingerNode.Connected)
            {
                mNodes.Add(gingerNode);
            }
        }

        public void Listen()
        {
            Console.WriteLine(mNodes.Count + " Node(s) Connected succesfully");
            int i = 0;
            foreach(GingerNode gingerNode in mNodes)
            {
                i++;
                Console.WriteLine("Node #" + i + " " + gingerNode.Info);
            }

            Console.WriteLine("Press any key to close");
            Console.ReadKey();
        }

        public void ClosedAllNodes()
        {
            Console.WriteLine("Ginger Node(s) is closing");
            foreach (GingerNode gingerNode in mNodes)
            {
                gingerNode.NotifyNodeClosing();
            }
            Console.WriteLine("Done sending notifications");
        }

        /// <summary>
        /// Start service on node and connect to remote GingerGrid/// 
        /// </summary>
        /// <param name="gingerServiceObject">Service object with [GingerService] annotation</param>
        /// <param name="name">Name which will apear in GingerGrid</param>
        /// <param name="gingerGridIP">GingerGrid Host/IP</param>
        /// <param name="gingerGridport">GingerGrid port</param>

        public void StartNode(string name, object gingerServiceObject, string gingerGridIP, int gingerGridport)
        {
            GingerNode gingerNode = new GingerNode(gingerServiceObject);
            gingerNode.StartGingerNode(name, gingerGridIP, gingerGridport);
            mNodes.Add(gingerNode);
        }

        public void StartNode(string name, Type serviceType)
        {
            object serviceObject = Activator.CreateInstance(serviceType);
            StartNode(name, serviceObject);            
        }

        public void StartNode(string name, Type serviceType, string gingerGridIP, int gingerGridport)
        {
            object serviceObject = Activator.CreateInstance(serviceType);
            StartNode(name, serviceObject, gingerGridIP, gingerGridport);            
        }

        public object CreateServiceObjectByServiceId(string serviceId)
        {
            Console.WriteLine("Creating Service object for Service Id: " + serviceId);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asssembly in assemblies)
            {
                IEnumerable<Type> types = from x in asssembly.GetTypes() where x.GetCustomAttribute(typeof(GingerServiceAttribute)) != null select x;
                foreach (Type type in types)
                {
                    GingerServiceAttribute gingerServiceAttribute = (GingerServiceAttribute)Attribute.GetCustomAttribute(type, typeof(GingerServiceAttribute), false);
                    if (gingerServiceAttribute.Id == serviceId)
                    {
                        Console.WriteLine("Found service class, full name =  " + type.FullName);
                        object obj = Activator.CreateInstance(type);
                        Console.WriteLine("Service class created successfully");
                        return obj;
                    }
                }
            }
            Console.WriteLine("GetServiceType failed for Service Id: " + serviceId);
            throw new Exception("Error: Cannot find class type to create for Service Id: " + serviceId);
        }
  

        public void StartNode(string name, string serviceId)
        {            
            object serviceObject = CreateServiceObjectByServiceId(serviceId); 
            StartNode(name, serviceObject);                                                 
        }

        public void StartNode(string name, string serviceId, string gingerGridIP, int gingerGridport)
        {            
            object serviceObject = CreateServiceObjectByServiceId(serviceId);
            StartNode(name, serviceObject, gingerGridIP, gingerGridport);
        }



        /// <summary>
        /// Config file is text file which contain list of: name, service class type name, IP, port
        /// use // at start of line for comment
        /// use | as delimiter between parameters
        /// Spaces are being trimmed from start and end of parameter, name can contain space
        /// 
        /// Example 1:
        /// // Start two Memo service on local grid
        /// memo1 | MemoPlugin.MemoService
        /// memo2 | MemoPlugin.MemoService
        /// 
        /// Exampe 2:
        /// // Start SeleniumChrome and SeleniumFF connect to remote grid
        /// Chrome 1 | SeleniumPlugin.SeleniumChromeService | 1.2.3.4 | 15001
        /// FF 1     | SeleniumPlugin.SeleniumFFService     | 1.2.3.4 | 15001
        /// </summary>
        /// <param name="fileName">File name containing the configuration to run</param>
        public void StartFromConfigFile(string fileName)
        {
            Console.WriteLine("Processing config file: " + fileName);
            string[] lines = System.IO.File.ReadAllLines(fileName);
            int i = 0;
            foreach (string line in lines)
            {
                i++;
                Console.WriteLine("Processing line #" + i + "> " + line);
                if (line.StartsWith(@"//"))
                {
                    Console.WriteLine("Comment line ignoed");
                    continue;
                }
                
                string[] paramList = line.Split('|');
                if (paramList.Length == 2)
                {
                    string name = paramList[0].Trim();
                    string className = paramList[1].Trim();
                    StartNode(name, className);
                }
                else if (paramList.Length == 4)
                {
                    string name = paramList[0].Trim();
                    string className = paramList[1].Trim();
                    string ip = paramList[2].Trim();
                    int port = int.Parse(paramList[3].Trim());                    
                    StartNode(name, className, ip, port);
                }
                else
                {
                    Console.WriteLine("Params mismatch should be 2 or 4");
                }
            }
            
        }

        
    }
}
