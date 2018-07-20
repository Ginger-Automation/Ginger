//#region License
///*
//Copyright © 2014-2018 European Support Limited

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

//using amdocs.ginger.GingerCoreNET;
//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using GingerPlugInsNET.ActionsLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using static GingerCoreNET.PlugInsLib.PluginsManager;

//namespace Amdocs.Ginger.CoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common
//{
//    public class ActionFactory
//    {
//        public static ObservableList<DriverAction> GetAllGenericActions()   // interfaces which subclass DriverInterfaceBase like IUIElementAction
//        {
//            ObservableList<DriverAction> list = new ObservableList<DriverAction>();

//            Assembly a = Assembly.GetAssembly(typeof(DriverInterfaceBase));   // we get the assembly of GingerPlugInsNET where all interface are defined
//            IEnumerable<Type> types = from x in a.GetTypes() where typeof(DriverInterfaceBase).IsAssignableFrom(x) select x;


//            foreach (Type t in types)
//            {
//                if (t == typeof(DriverInterfaceBase)) continue;

//                // expecting to get IUIElementAction, ITakeScreenShot...
//                MethodInfo[] methods = t.GetMethods();
//                foreach (MethodInfo MI in methods)
//                {
//                    DriverAction DA = new DriverAction();
//                    DA.ID = MI.Name;
//                    DA.Description = MI.Name;  // TODO: add attr on each action to provide description
//                    foreach (ParameterInfo PI in MI.GetParameters())
//                    {
//                        if (PI.ParameterType.Name != nameof(GingerAction))
//                        {
//                            DA.InputValues.Add(new ActInputValue() { Param = PI.Name, ParamType = PI.ParameterType });
//                        }
//                    }
//                    list.Add(DA);
//                }
//            }
//            return list;
//        }

//        static ObservableList<StandAloneAction> list = null;   // TODO: clean if solution changed
//        public static ObservableList<StandAloneAction> GetAllStandAloneActions()
//        {
//            return WorkSpace.Instance.PlugInsManager.GetStandAloneActions();
//        }

//        public static ObservableList<DriverAction> GetAllPluginActions()
//        {
//            ObservableList<DriverAction> list = new ObservableList<DriverAction>();
//            List<DriverInfo> drivers = WorkSpace.Instance.PlugInsManager.GetAllDrivers();
//            foreach (DriverInfo DI in drivers)
//            {
//                List<GingerAction> actions = WorkSpace.Instance.PlugInsManager.GetDriverActions(DI);
//                foreach (GingerAction GA in actions)
//                {
//                    DriverAction DA = new DriverAction();
//                    DA.ID = GA.ID;
//                    DA.Description = GA.ID;
//                    foreach(ActionParam AP in GA.InputParams.Values)
//                    {
//                        ActInputValue AIV = DA.GetOrCreateInputParam(AP.Name);
//                        AIV.ParamType = AP.ParamType;
//                    }
//                    list.Add(DA);
//                }
//            }
//            return list;
//        }

//        //TODO: add also PluginID
//        internal static Act GetActionByID(string iD)
//        {
//            //cache
//            ObservableList<DriverAction> list = GetAllGenericActions();
//            DriverAction DA = (from x in list where x.ID == iD select x).FirstOrDefault();
//            if (DA != null) return DA;

//            ObservableList<DriverAction> list2 = GetAllPluginActions();
//            DriverAction DA2 = (from x in list2 where x.ID == iD select x).FirstOrDefault();
//            if (DA2 != null) return DA2;

//            ObservableList<StandAloneAction> list3 = GetAllStandAloneActions();
//            StandAloneAction DA3 = (from x in list3 where x.ID == iD select x).FirstOrDefault();
//            if (DA3 != null) return DA3;

//            return null;
//        }
//    }
//}
