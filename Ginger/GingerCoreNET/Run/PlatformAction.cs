using Amdocs.Ginger.CoreNET.Run;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCore.Platforms
{

    /// <summary>
    ///  Use this struct to pack platform action before sending to Ginger node
    //  Fields must match exectly PlatformActionData
    /// </summary>
    public struct PlatformAction
    {        


        public string ActionType { get; }

        public Dictionary<string, object> InputParams;
        
        public PlatformAction(IActPluginExecution Action)
        {            

            ActionType = Action.GetName();
            InputParams = new Dictionary<string, object>();
        }
        public PlatformAction(IActPluginExecution Action, Dictionary<string, object> InputParameters)
        {

            ActionType = Action.GetName();
            InputParams = InputParameters;
        }

    }
}
