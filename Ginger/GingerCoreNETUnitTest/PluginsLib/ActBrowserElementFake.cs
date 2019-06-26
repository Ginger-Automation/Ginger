//using Amdocs.Ginger.Common.InterfacesLib;
//using Amdocs.Ginger.CoreNET.Run;
//using GingerCore.Actions;
//using GingerCore.Platforms;
//using GingerCoreNET.Drivers.CommunicationProtocol;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using YamlDotNet.Core.Tokens;

//namespace GingerCoreNETUnitTest.PluginsLib
//{
//    public class ActBrowserElementFake : Act, IActPluginExecution
//    {
//        public override string ActionType => throw new NotImplementedException();

//        public override string ActionDescription => throw new NotImplementedException();

//        public override bool ObjectLocatorConfigsNeeded => throw new NotImplementedException();

//        public override bool ValueConfigsNeeded => throw new NotImplementedException();

//        public override List<ePlatformType> Platforms => throw new NotImplementedException();

//        public override string ActionEditPage => throw new NotImplementedException();

//        public override string ActionUserDescription => throw new NotImplementedException();

//        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
//        {
//            throw new NotImplementedException();
//        }

//        public string BrowserAction { get; set; }
//        public string value { get; set; }

//        public PlatformAction GetAsPlatformAction()
//        {
//            PlatformAction platformAction = new PlatformAction(platform: "Web", actionHandler:"BroweserActions",  action: "browser");

//            return platformAction;
//            //NewPayLoad payload = new NewPayLoad("RunPlatformAction");   
//            //payload.AddValue("IWebPlatform"); // Interface    
//            //payload.AddValue("BrowserActions"); // Field
//            //payload.AddValue(BrowserAction); // Method
//            //payload.AddValue(value);
//            //payload.ClosePackage();
//            //return payload;
//        }

//        public string GetName()
//        {
//            return "BrowserAction";
//        }
//    }
//}
