using System.Collections.Generic;

namespace Amdocs.Ginger.CoreNET.RunLib
{

    // This is the readonly platform action data executed by the Ginger node
    // This struct must match exectly to PlatformAction - so json deserialize witl match

    public struct NodePlatformAction
    {
        public string Platform;
        public string ActionHandler;
        public string ActionType;
        public Dictionary<string, object> InputParams;
        public string error;
        public NodeActionOutput Output;
        public string exInfo;


        public void addError(string err)
        {
            exInfo += err;
        }
        
    }
}