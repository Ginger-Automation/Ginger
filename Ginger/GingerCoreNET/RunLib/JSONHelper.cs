using Newtonsoft.Json;
using System;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    class JSONHelper
    {
        internal static object DeserializeObject(string txt, Type listType)
        {
            return JsonConvert.DeserializeObject(txt, listType);
        }
    }
}
