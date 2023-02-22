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
using System.Reflection;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.Drivers.CommunicationProtocol;

namespace Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol
{
    public class RemoteObjectsClient
    {
        GingerSocketClient2 mGingerSocketClient2;
        public void Connect(string host, int port)
        {
            mGingerSocketClient2 = new GingerSocketClient2();
            mGingerSocketClient2.Connect(host, port);
            mGingerSocketClient2.MessageHandler += MessageHandler;
        }

        private void MessageHandler(GingerSocketInfo obj)
        {
            //TODO: handle request from server
           throw new NotImplementedException();
        }

        public T GetObject<T>(string id)
        {
            NewPayLoad PL = new NewPayLoad("GetObject", id);
            NewPayLoad PLRC = mGingerSocketClient2.SendRequestPayLoad(PL);
            Guid guid = Guid.Parse(PLRC.GetValueString());   //TODO: add Guid in Payload
            T obj  = RemoteObjectProxy<T>.Create(this, guid);
            return obj;
        }

        internal object Invoke<T>(RemoteObjectProxy<T> remoteObjectProxy, MethodInfo targetMethod, object[] args)
        {
            NewPayLoad pl = new NewPayLoad("Invoke");
            pl.AddValue(remoteObjectProxy.RemoteObjectGuid.ToString());
            pl.AddValue(targetMethod.Name);
            pl.AddValue(args.Length); // count of params
            foreach (object o in args)
            {
                pl.AddValueByObjectType(o);                
            }
            pl.ClosePackage();
            NewPayLoad PLRC =  mGingerSocketClient2.SendRequestPayLoad(pl);

            object rc = PLRC.GetValueByObjectType();
            return rc;
        }
    }
}