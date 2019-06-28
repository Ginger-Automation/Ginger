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
        public string Platform { get; }

        public string ActionHandler { get; }

        public string ActionType { get; }

        public Dictionary<string, object> InputParams;
        
        public PlatformAction(string platform, string actionHandler, string action)
        {
            Platform = platform;
            ActionHandler = actionHandler;
            ActionType = action;
            InputParams = new Dictionary<string, object>();
        }


    }
}
