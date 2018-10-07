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

using Amdocs.Ginger.CoreNET.Drivers;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using System;
using System.Reflection;

namespace GingerCoreNET.Drivers.CommunicationProtocol
{    
    // a wrapper class for the objects requested by the clients
    public class RemoteObjectProxy<T> : DispatchProxy
    {        
        // Socket client which is used to communcaite with the RemoteObjectsServer
        public RemoteObjectsClient mRemoteObjectsClient;

        public Guid RemoteObjectGuid { get; set; }
        
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {     
                var result = mRemoteObjectsClient.Invoke(this, targetMethod, args);             
                return result;
            }
            catch (Exception ex) when (ex is TargetInvocationException)
            {                
                throw ex; 
            }
        }
        
        internal static T Create(RemoteObjectsClient remoteObjectsClient, Guid guid)
        {
            object proxy = Create<T, RemoteObjectProxy<T>>();
            ((RemoteObjectProxy<T>)proxy).mRemoteObjectsClient = remoteObjectsClient;
            ((RemoteObjectProxy<T>)proxy).RemoteObjectGuid = guid;
            return (T)proxy;
        }
    }
}
