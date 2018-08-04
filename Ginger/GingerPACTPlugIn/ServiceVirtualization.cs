#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Newtonsoft.Json;
using PactNet;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace GingerPACTPlugIn
{
    public class ServiceVirtualization
    {
        public IPactBuilder PactBuilder { get; private set; }
        public IMockProviderService MockProviderService { get; set; }

        public int MockServerPort { get; set;}  
        public string MockProviderServiceBaseUri { get { return String.Format("http://localhost:{0}", MockServerPort); } }

        private static int mStartingPort = 3333;

        public ServiceVirtualization(int Port = 0, string PathToSaveJsonFile = "",string ServiceConsumer = null, string HasPactWith = null)
        {
            if (Port == 0)
                Port = FindFreePort();
            MockServerPort = Port;
            // Init
            String timeStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string targetPath = userFolder + "\\Temp\\GingerTemp";

            if (string.IsNullOrEmpty(PathToSaveJsonFile))
                PathToSaveJsonFile = targetPath;
            else
                PathToSaveJsonFile = PathToSaveJsonFile + @"\PactToJson" + timeStamp;
            
            PactBuilder = new PactBuilder(new PactConfig { PactDir = PathToSaveJsonFile , LogDir = PathToSaveJsonFile + @"\logs" }); 

            if (string.IsNullOrEmpty(ServiceConsumer))
                ServiceConsumer = "Consumer";
            if (string.IsNullOrEmpty(HasPactWith))
                HasPactWith = "Something API";

            PactBuilder
                .ServiceConsumer(ServiceConsumer)
                .HasPactWith(HasPactWith);  
            
            JsonSerializerSettings js = new JsonSerializerSettings();
            CustomJsonConverter cjc = new CustomJsonConverter();
            js.Converters.Add(cjc);
            
            //TODO: Try to read the json and inject to server before it's start

            MockProviderService = PactBuilder.MockService(MockServerPort, js); //You can also change the default Json serialization settings using this overload
        }

        private int FindFreePort()
        {
            Boolean foundFreePort = false;
            int portStartValue = mStartingPort;
            List<int> reservedPorts = new List<int>();

            while (!foundFreePort)
            {

                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
                Boolean portTaken = false;
                foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                {
                    if (tcpi.LocalEndPoint.Port == portStartValue)
                    {
                        portTaken = true;
                        break;
                    }
                }
                if (portTaken || reservedPorts.Contains(portStartValue))
                {
                    portStartValue += 2;
                }
                else
                {
                    foundFreePort = true;
                    reservedPorts.Add(portStartValue);
                }
            }
            mStartingPort = portStartValue + 5;
            return portStartValue;
        }

        public void ClearInteractions()
        {
            MockProviderService.ClearInteractions();
        }

        public int LoadInteractions(string filename)
        {            
            //TODO: find the correct class which has also "provider": and "consumer": - YW- meanwhile it's working for loading
            ProviderServicePactFile mm = (ProviderServicePactFile)JSonHelper.LoadObjFromJSonFile(filename, typeof(ProviderServicePactFile));
            foreach (ProviderServiceInteraction PSI in mm.Interactions)
            {
                AddInteraction(PSI);
            }
            return mm.Interactions.Count();
        }

        public void AddInteraction(ProviderServiceInteraction PSI)
        {
            MockProviderService
                .Given(PSI.ProviderState)
                .UponReceiving(PSI.Description)
                .With(new ProviderServiceRequest
                {
                    Method = PSI.Request.Method,
                    Path = PSI.Request.Path,
                    Headers = PSI.Request.Headers,
                    Body = PSI.Request.Body
                })
                .WillRespondWith(new ProviderServiceResponse
                {
                    Status = PSI.Response.Status,
                    Headers = PSI.Response.Headers,
                    Body = PSI.Response.Body
                });
        }

        public void SaveInteractions()
        {
            PactBuilder.Build();
        }
    }
}