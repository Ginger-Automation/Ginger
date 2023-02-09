#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
            object obj = null;
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
                obj= mJsonSerializer.Deserialize(reader, t);
            }

            return obj;
        }

        public static object LoadObjFromJSonString(string str, Type type, JsonSerializer serializer = null)
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
                return mJsonSerializer.Deserialize(reader, type);
            }
        }
    }
}
