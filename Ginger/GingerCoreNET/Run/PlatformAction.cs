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
        
        public PlatformAction(string action)
        {
        
         
            ActionType = action;
            InputParams = new Dictionary<string, object>();
        }


    }
}
