#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.CommunicationProtocol;
using System;
using System.Collections.Generic;

namespace Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol
{
    public static class PayloadHelper
    {
        public static PayLoad GetPayLoad(Act action, ElementLocator elementLocator = null)
        {
            if (action is ActUIElement actUIElement)
            {
                string payLoadName = @"UIElementAction";
                if (Convert.ToBoolean(actUIElement.GetInputParamValue(ActUIElement.Fields.IsWidgetsElement)))
                {
                    payLoadName = @"WidgetsUIElementAction";
                }
                PayLoad PL = new PayLoad(payLoadName);
                // Make it generic function in Act.cs to be used by other actions
                List<PayLoad> PLParams = [];
                foreach (ActInputValue AIV in actUIElement.InputValues)
                {
                    if (!string.IsNullOrEmpty(AIV.Value))
                    {
                        PayLoad AIVPL = new("AIV", AIV.Param, AIV.ValueForDriver);
                        PLParams.Add(AIVPL);
                    }
                }
                PL.AddListPayLoad(PLParams);

                //for Java POM Element
                if (elementLocator != null)
                {
                    PL.AddKeyValuePair(elementLocator.LocateBy.ToString(), elementLocator.LocateValue);
                }

                PL.ClosePackage();

                return PL;
            }

            if (action is ActSwitchWindow actSwitchWindow)
            {
                PayLoad PL = new("SwitchWindow");
                if (string.IsNullOrEmpty(actSwitchWindow.LocateValueCalculated) == false)
                {
                    PL.AddValue(actSwitchWindow.LocateValueCalculated);
                }
                else
                {
                    PL.AddValue(actSwitchWindow.ValueForDriver);
                }

                PL.ClosePackage();
                return PL;
            }
            return null;
        }

        // MK: This is the old code moved from ActUIElement, its not getting called from anywhere so commented it
        //      public NewPayLoad GetActionPayload()
        //      {
        //          // Need work to cover all options per platfrom !!!!!!!!!!!!!!!!!!!!
        //          //TODO:     // Make it generic function in Act.cs to be used by other actions

        //          NewPayLoad PL = new NewPayLoad("RunPlatformAction");
        //          PL.AddValue("UIElementAction");
        //          List<NewPayLoad> PLParams = new List<NewPayLoad>();

        //          foreach (FieldInfo FI in typeof(ActUIElement.Fields).GetFields())
        //          {
        //              string Name = FI.Name;
        //              string Value = GetOrCreateInputParam(Name).ValueForDriver;

        //              if (string.IsNullOrEmpty(Value))
        //              {
        //                  object Output = this.GetType().GetProperty(Name) != null ? this.GetType().GetProperty(Name).GetValue(this, null) : string.Empty;

        //                  if (Output != null)
        //                  {
        //                      Value = Output.ToString();
        //                  }
        //              }

        //              if (!string.IsNullOrEmpty(Value))
        //              {
        //                  NewPayLoad FieldPL = new NewPayLoad("Field", Name, Value);
        //                  PLParams.Add(FieldPL);
        //              }
        //          }
        //          /*
        //          PL.AddValue(this.ElementLocateBy.ToString());
        //          PL.AddValue(GetOrCreateInputParam(Fields.ElementLocateValue).ValueForDriver); // Need Value for driver
        //          PL.AddValue(this.ElementType.ToString());
        //          PL.AddValue(this.ElementAction.ToString());
        //*/

        //          foreach (ActInputValue AIV in this.InputValues)
        //          {
        //              if (!string.IsNullOrEmpty(AIV.Value))
        //              {
        //                  NewPayLoad AIVPL = new NewPayLoad("AIV", AIV.Param, AIV.ValueForDriver);
        //                  PLParams.Add(AIVPL);
        //              }
        //          }
        //          PL.AddListPayLoad(PLParams);
        //          PL.ClosePackage();

        //          return PL;
        //      }

        // MK: This is the old code moved from ActBrowserElement, its not getting called from anywhere so commented it
        //public NewPayLoad GetActionPayload()
        //{
        //    NewPayLoad PL = new NewPayLoad("RunPlatformAction");
        //    PL.AddValue("BrowserAction");
        //    List<NewPayLoad> PLParams = new List<NewPayLoad>();

        //    foreach (FieldInfo FI in typeof(ActBrowserElement.Fields).GetFields())
        //    {
        //        string Name = FI.Name;
        //        string Value = GetOrCreateInputParam(Name).ValueForDriver;

        //        if (string.IsNullOrEmpty(Value))
        //        {
        //            object Output = this.GetType().GetProperty(Name) != null ? this.GetType().GetProperty(Name).GetValue(this, null) : string.Empty;

        //            if (Output != null)
        //            {
        //                Value = Output.ToString();
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(Value))
        //        {
        //            NewPayLoad FieldPL = new NewPayLoad("Field", Name, Value);
        //            PLParams.Add(FieldPL);
        //        }
        //    }

        //    foreach (FieldInfo FI in typeof(Act.Fields).GetFields())
        //    {
        //        string Name = FI.Name;
        //        string Value = GetOrCreateInputParam(Name).ValueForDriver;

        //        if (string.IsNullOrEmpty(Value))
        //        {
        //            object Output = this.GetType().GetProperty(Name) != null ? this.GetType().GetProperty(Name).GetValue(this, null) : string.Empty;

        //            if (Output != null)
        //            {
        //                Value = Output.ToString();
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(Value))
        //        {
        //            NewPayLoad FieldPL = new NewPayLoad("Field", Name, Value);
        //            PLParams.Add(FieldPL);
        //        }
        //    }


        //    foreach (ActInputValue AIV in this.InputValues)
        //    {
        //        if (!string.IsNullOrEmpty(AIV.ValueForDriver))
        //        {
        //            NewPayLoad AIVPL = new NewPayLoad("AIV", AIV.Param, AIV.ValueForDriver);
        //            PLParams.Add(AIVPL);
        //        }
        //    }




        //    PL.AddListPayLoad(PLParams);
        //    PL.ClosePackage();

        //    return PL;
        //}
    }

}
