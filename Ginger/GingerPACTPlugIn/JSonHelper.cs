#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Newtonsoft.Json;
using System;
using System.IO;

namespace GingerPACTPlugIn
{
    public class JSonHelper
    {
        static JsonSerializer mJsonSerializer = new JsonSerializer();
        private static void SaveObjToJSonFile(object obj, string FileName)
        {
            //TODO: for speed we can do it async on another thread...

            using (StreamWriter SW = new StreamWriter(FileName))
            using (JsonWriter writer = new JsonTextWriter(SW))
            {
                mJsonSerializer.Serialize(writer, obj);
            }
        }

        public static object LoadObjFromJSonFile(string FileName, Type t)
        {
            using (StreamReader SR = new StreamReader(FileName))
            using (JsonReader reader = new JsonTextReader(SR))
            {
                return mJsonSerializer.Deserialize(reader, t);
            }
        }
    }
}
