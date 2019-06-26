using System.Collections.Generic;

namespace Amdocs.Ginger.CoreNET.RunLib
{

    // This is the readonly platform action data executed by the Ginger node
    // This struct must match exectly to PlatformAction

    public struct PlatformActionData
    {
        public string Platform;

        public string ActionType;

        public Dictionary<string, object> InputParams;


        public PlatformActionData(string platform, string action)
        {
            Platform = platform;
            ActionType = action;
            InputParams = new Dictionary<string, object>();
        }

        
    }
}