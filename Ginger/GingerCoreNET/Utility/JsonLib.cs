using Ginger.Reports;
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

        public static ActivityGroupReport LoadObjFromJSonString(string str, Type type, JsonSerializer serializer = null)
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
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            using (StreamReader SR = new StreamReader(stream))
            using (JsonReader reader = new JsonTextReader(SR))
            {
                return mJsonSerializer.Deserialize(reader, t);
            }
        }
    }
}
