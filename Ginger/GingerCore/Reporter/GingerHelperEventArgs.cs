#region License
/*
Copyright © 2014-2019 European Support Limited

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

//#region License
///*
//Copyright © 2014-2019 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Common;
//using GingerCoreNET.ReporterLib;
//using System;
//using System.Windows;

//namespace GingerCore
//{
//   public class GingerHelperEventArgs : EventArgs
//    {
//        public enum eGingerHelperEventActions
//        {
//            Show,
//            Close
//        }
//        private  GingerHelperMsg mHelperMsg;
//        public GingerHelperMsg HelperMsg
//        {
//            get
//            {
//                return mHelperMsg;
//            }
//        }

//        private eGingerHelperEventActions mGingerHelperEventActions;
//        public eGingerHelperEventActions GingerHelperEventActions
//        {
//            get
//            {
//                return mGingerHelperEventActions;
//            }
//        }

//        private RoutedEventHandler mbtnHandler;
//        public RoutedEventHandler ButtonHandler
//        {
//            get
//            {
//                return mbtnHandler;
//            }
//        }

//        private eStatusMessageType mMessageType;
//        public eStatusMessageType MessageType
//        {
//            get
//            {
//                return mMessageType;
//            }
//        }

//        public GingerHelperEventArgs(eGingerHelperEventActions EventAction, eStatusMessageType messageType, RoutedEventHandler btnHandler = null, GingerHelperMsg helperMsg=null)
//        {
//            mHelperMsg = helperMsg;
//            mGingerHelperEventActions = EventAction;
//            mbtnHandler = btnHandler;
//            mMessageType = messageType;
//        }
//    }
//}
