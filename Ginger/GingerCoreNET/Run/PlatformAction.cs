using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCore.Platforms
{

    /// <summary>
    ///  Use this struct to pack platform action before sendign to Ginger node
    //  Fields must match exectly PlatformActionData
    /// </summary>
    public struct PlatformAction
    {
        public string Platform;

        public string ActionType;

        public Dictionary<string, object> InputParams;


        public PlatformAction(string platform, string action)
        {
            Platform = platform;
            ActionType = action;
            InputParams = new Dictionary<string, object>();
        }


    }
}
