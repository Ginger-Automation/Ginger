using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Platforms;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreNETUnitTest.PluginsLib
{
    public class ActUIElementFake : Act, IActPluginExecution
    {
        public override string ActionType => throw new NotImplementedException();

        public override string ActionDescription => throw new NotImplementedException();

        public override bool ObjectLocatorConfigsNeeded => throw new NotImplementedException();

        public override bool ValueConfigsNeeded => throw new NotImplementedException();

        public override List<ePlatformType> Platforms => throw new NotImplementedException();

        public override string ActionEditPage => throw new NotImplementedException();

        public override string ActionUserDescription => throw new NotImplementedException();

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            throw new NotImplementedException();
        }

        public string LocateBy { get; set; }
        public string LocateValue { get; set; }
        public string ElementType { get; set; }
        public string ElementAction { get; set; }

        

        public NewPayLoad GetActionPayload()
        {            
            NewPayLoad PL = new NewPayLoad("RunPlatformAction");

            // Design best way to send an action with generic packing if possible

            PL.AddValue("UIElementAction");
            PL.AddValue(LocateBy);
            PL.AddValue(LocateValue); 
            PL.AddValue(ElementType);
            PL.AddValue(ElementAction);

            // TODO: think of smart way to pack the extra param or send all AIVs...
            if (ElementAction == "SetText")
            {
                PL.AddValue(Value);  // temp for set text box need to be per action   // We double pass Value also in Input values !!!!!!!!!!!!!!!!!!!! FixME
            }

            //List<NewPayLoad> PLParams = new List<NewPayLoad>();
            //foreach (ActInputValue AIV in this.InputValues)
            //{
            //    if (!string.IsNullOrEmpty(AIV.Value))
            //    {
            //        NewPayLoad AIVPL = new NewPayLoad("AIV", AIV.Param, AIV.Value);  // AIV.ValueForDriver
            //        PLParams.Add(AIVPL);
            //    }
            //}
            //PL.AddListPayLoad(PLParams);
            PL.ClosePackage();

            return PL;
        }

        public string GetName()
        {
            return "UIElementAction";
        }


        public struct SimpleAction
        {
            public List<string> Locators;
            public string action;
        }

        public NewPayLoad GetActionPayload2()
        {
            //Create data struct
            SimpleAction simpleAction = new SimpleAction();
            simpleAction.action = ElementAction.ToString();
            // buttonAction.Locators = ...

            //Create action Payload
            NewPayLoad PL = new NewPayLoad("RunPlatformAction");
            PL.AddValue("UIElementActionFake");
            PL.AddValue("SimpleAction");
            PL.AddJSONValue<SimpleAction>(simpleAction);
            PL.ClosePackage();
            return PL;
        }

        PlatformAction IActPluginExecution.GetAsPlatformAction()
        {
            throw new NotImplementedException();
        }
    }
}
