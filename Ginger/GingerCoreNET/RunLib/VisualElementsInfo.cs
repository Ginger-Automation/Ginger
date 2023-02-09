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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GingerCore.Actions.VisualTesting
{
    // We save the info as Json so can be used as baseline to load
    [JsonObject(MemberSerialization.OptIn)]
    public class VisualElementsInfo
    {
        static JsonSerializer mJsonSerializer = null;

        // No need to serialize the bitmap
        public Bitmap Bitmap { get; set; }

        [JsonProperty]
        public List<VisualElement> Elements = new List<VisualElement>();

        private static void initJSon()
        {
            mJsonSerializer = new JsonSerializer();
            mJsonSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public void Save(string FileName)
        {
            if (mJsonSerializer == null) initJSon();
            
            //TODO: for speed we can do it async on another thread...

            using (StreamWriter SW = new StreamWriter(FileName))
            using (JsonWriter writer = new JsonTextWriter(SW))
            {
                mJsonSerializer.Serialize(writer, this);
            }
        }

        //constructor which load the data from file
        public static VisualElementsInfo Load(string FileName)
        {
            if (mJsonSerializer == null) initJSon();

            using (StreamReader SR = new StreamReader(FileName))
            using (JsonReader reader = new JsonTextReader(SR))
            {
                VisualElementsInfo VEI = (VisualElementsInfo)mJsonSerializer.Deserialize(reader, typeof(VisualElementsInfo));
                return VEI;
            }
        }

        public void Compare(VisualElementsInfo VEI)
        {
            // we try to match elem by tag and text 
            // Each elem found we mark it 
            foreach (VisualElement VE in Elements)
            {
                // Make sure we are in allowed offset
                //TODO: add to config
                int offset = 100;  // up to +/- 100 pixels move is allowed

                VisualElement VE1 = (from x in VEI.Elements where 
                                     x.ElementType == VE.ElementType 
                                     && x.Text == VE.Text 
                                     && (Math.Abs(x.X - VE.X) < offset)
                                     && Math.Abs(x.Y - VE.Y) < offset
                                     select x
                                     ).FirstOrDefault();
                if (VE1 != null)
                {
                    if (!string.IsNullOrEmpty(VE1.Text))
                    {
                        if (VE1.MatchingElement != null) // there is already a match
                        {
                            //TODO: handle match more than once
                        }
                        else
                        {
                            // We found matching element create the connection at both sides
                            VE.MatchingElement = VE1;
                            VE1.MatchingElement = VE;
                        }
                    }
                }
            }
            //TODO: use it when we want to create the piece of the emep in seperate bitmap - for comapre
        }
    }
}
