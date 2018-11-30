using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

 namespace Amdocs.Ginger.CoreNET.Utility
{
  public  class JsonLib
    {

        public static object LoadObjFromJSonFile(string FileName, Type t,JsonSerializer serializer=null )
        {
            JsonSerializer mJsonSerializer;
            if (serializer == null)
            {
                mJsonSerializer = new JsonSerializer();
                mJsonSerializer.NullValueHandling = NullValueHandling.Ignore;
            }
            else
            {
                mJsonSerializer = serializer;
            }
            using (StreamReader SR = new StreamReader(FileName))
            using (JsonReader reader = new JsonTextReader(SR))
            {
                return mJsonSerializer.Deserialize(reader, t);
            }
        }
    }
}
