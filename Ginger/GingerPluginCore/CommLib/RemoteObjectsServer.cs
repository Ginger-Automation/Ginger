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
using System.Collections.Generic;
using GingerCoreNET.Drivers.CommunicationProtocol;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol
{
    public class RemoteObjectsServer
    {
        GingerSocketServer2 mGingerSocketServer;

        // Object created and referred from remote client
        Dictionary<Guid, RemoteObjectHandle> mObjects = new Dictionary<Guid, RemoteObjectHandle>();

        // one input string which is the object id and return object - callback to the using class - must be set
        public Func<string, object> GetObjectHandler;

        public string Info { get { return mGingerSocketServer.IPInfo; }  }

        public void Start(int port)
        {
            mGingerSocketServer = new GingerSocketServer2();
            mGingerSocketServer.MessageHandler = MessageHandler;
            mGingerSocketServer.StartServer(port); 
        }

        public void ShutDown()
        {
            mGingerSocketServer.Shutdown();
        }

        private void MessageHandler(GingerSocketInfo gingerSocketInfo)
        {
            NewPayLoad PLRC = HandlePayLoad(gingerSocketInfo.DataAsPayload);
            gingerSocketInfo.Response = PLRC;
        }

        // Handle Client request
        private NewPayLoad HandlePayLoad(NewPayLoad PL)
        {
            switch (PL.Name)
            {
                case "GetObject":
                    string id = PL.GetValueString();
                    object  obj = GetObjectHandler(id);
                    RemoteObjectHandle remoteObjectHandle = new RemoteObjectHandle();
                    Guid guid = Guid.NewGuid();
                    remoteObjectHandle.GUID = guid;
                    remoteObjectHandle.Object = obj;
                    
                    //check if the object have Dispatcher - means GUI element so STA thread then run it on the STA, so we keep the Dispatcher for the invoke part later
                    PropertyInfo PI = obj.GetType().GetProperty("Dispatcher");
                    if (PI != null)
                    {
                        object DispObj = obj;
                        dynamic d = DispObj;
                        // It means it is UI control - so invoke on the UI control Dispatcher, this way we avoid code on the page to change the UI on the Dispatcher every time
                        // temp comment for build
                        // !!!!!!!!! d. requires Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo 
                        // remoteObjectHandle.Dispatcher = d.Dispatcher;
                    }

                    mObjects.Add(guid, remoteObjectHandle);
                    NewPayLoad PLEcho = new NewPayLoad("Object", guid.ToString());
                    return PLEcho;
                case "SendObject":
                    string txt2 = "a=1;b=2";
                    NewPayLoad PLEcho2 = new NewPayLoad("Object", txt2);
                    return PLEcho2;
                case "Invoke":
                    Guid objguid = Guid.Parse(PL.GetValueString());
                    string methodName = PL.GetValueString();
                    RemoteObjectHandle ROH;                    
                    // Get the object by guid
                    bool bFound = mObjects.TryGetValue(objguid, out ROH);
                    object obj1 = ROH.Object;
                    //TODO: if not found...
                    MethodInfo mi = obj1.GetType().GetMethod(methodName);

                    int ParamCounter = PL.GetValueInt();

                    object[] param = new object[ParamCounter];
                    for (int i=0;i<ParamCounter;i++)
                    {                        
                        param[i] = PL.GetValueByObjectType();                        
                    }

                    // invoke 
                    object rc = null;

                    // temp comment for build
                    // !!!!!!!!! d. requires Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo 
                    //if (ROH.Dispatcher == null)
                    //{

                    //    // Non UI object safe to call from another thread
                    //    rc = mi.Invoke(obj1, param);                        
                    //}
                    //else
                    //{
                    //    // It means the obj is UI control like driver Page- so invoke on the UI control Dispatcher, 
                    //    // this way we avoid code on the page to change the UI on the Dispatcher every time we do UI changes and avoid getting exception
                    //    ROH.Dispatcher.BeginInvoke(
                    //                    (Action)(() => {
                    //                        rc = mi.Invoke(obj1, param);
                    //                    }
                    //                ));
                    //}

                    // return result 
                    NewPayLoad PLRC = new NewPayLoad("OK");                    
                    if (rc != null)
                    {
                        PLRC.AddValueByObjectType(rc);                        
                    }
                    else
                    {
                        PLRC.AddValue("NULL");
                        PLRC.AddValue("NULL");
                    }
                    PLRC.ClosePackage();                    
                    return PLRC;                
                default:
                    throw new InvalidOperationException("Unknown PayLoad Action - " + PL.Name);
            }
        }

        internal NewPayLoad SendPayLoad(Guid sessionID, NewPayLoad pL)
        {
            return mGingerSocketServer.SendPayLoad(sessionID, pL);
        }
    }
}