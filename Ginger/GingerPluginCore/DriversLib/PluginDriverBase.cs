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

//using Amdocs.Ginger.Plugin;
//using Amdocs.Ginger.Plugin.Core.ActionsLib;
//using Amdocs.Ginger.Plugin.Core.PlugInsLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace  Amdocs.Ginger.Plugin.Core.DriversLib
//{

//    //TODO; remove and use only service - can be sub type driver


//    // Base class for capability of type Actions, for plugin who want to add new actions to Ginger.
//    public abstract class PluginDriverBase 
//    {
//        //TODO: use dictionary
//        public List<ActionHandler> ActionHandlers = null;
//        public PluginDriverBase()
//        {            
//            InitActions();

//            int i = 0;
//            Console.WriteLine("------------------------------------------------");
//            Console.WriteLine("Action Handlers:");
//            foreach (ActionHandler AH in ActionHandlers)
//            {
//                i++;
//                Console.WriteLine(i + ". " + AH.ID);
//            }
//            Console.WriteLine("------------------------------------------------");
//        }
        
//        private void InitActions()
//        {
//            if (ActionHandlers != null) return;
//            ActionHandlers = new List<ActionHandler>();

//            // First we register all actions which have 'GingerAction' attribute
//            Type t = this.GetType();
//            var v = t.GetMethods(); //BindingFlags.Public  BindingFlags.DeclaredOnly);
//            foreach (MethodInfo MI in v)
//            {
//                GingerActionAttribute GAA = (GingerActionAttribute)MI.GetCustomAttribute(typeof(GingerActionAttribute));
//                if (GAA != null)
//                {
//                    ActionHandler AH = new ActionHandler();
//                    AH.ID = GAA.ID;                    
//                    AH.MethodInfo = MI;
//                    ActionHandlers.Add(AH);
//                }                
//            }

//            //TODO:  register all known interfaces using reflection - ID should be the method interface name

//            // temp !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!            
//            if (this is IWebBrowser)
//            {
//                ActionHandler AH = new ActionHandler();
//                AH.ID = "GotoURL";  // temp
//                MethodInfo MI = t.GetMethod(nameof(IWebBrowser.Navigate));
//                AH.MethodInfo = MI;
//                ActionHandlers.Add(AH);
//            }

//            if (this is IUIElementAction)
//            {
//                ActionHandler AH = new ActionHandler();
//                AH.ID = "UIElementAction";  // temp
//                MethodInfo MI = t.GetMethod(nameof(IUIElementAction.UIElementAction));
//                AH.MethodInfo = MI;
//                ActionHandlers.Add(AH);
//            }

//            if (this is ITakeScreenShot)
//            {
//                ActionHandler AH = new ActionHandler();
//                AH.ID = nameof(ITakeScreenShot.TakeScreenShot);
//                MethodInfo MI = t.GetMethod(nameof(ITakeScreenShot.TakeScreenShot));
//                AH.MethodInfo = MI;
//                ActionHandlers.Add(AH);
//            }
//        }

//        public abstract string Name { get;  }

//        /// <summary>
//        ///  Start the Driver
//        /// </summary>
//        public abstract void StartDriver();
//        public abstract void CloseDriver();
        
//        public virtual void BeforeRunAction(GingerAction GA)
//        {
//            // If the driver want to do something before the action run start then override
//        }

//        public virtual void AfterRunAction(GingerAction GA)
//        {
//            // If the driver want to do something after the action run completed then override
//        }

//        public void RunAction(GingerAction gingerAction)
//        {
//            ActionHandler AH = (from x in ActionHandlers where x.ID == gingerAction.ID select x).FirstOrDefault();
//            ActionRunner.RunAction(this, gingerAction, AH);
//        }
//    }
//}
