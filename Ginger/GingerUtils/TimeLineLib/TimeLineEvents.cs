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
using System.Collections.Generic;
using System.Linq;

namespace GingerUtils.TimeLine
{
    public class TimeLineEvents
    {
        private List<TimeLineEvent> mEvents = new List<TimeLineEvent>();

        public IEnumerable<TimeLineEvent> Events
        {
            get
            {
                return mEvents;
            }
        }

        public List<TimeLineEvent> EventList
        {
            get
            {
                return mEvents.Cast<TimeLineEvent>().ToList();
            }
        }

        public void Clear()
        {
            mEvents.Clear();
        }

        public void AddEvent(TimeLineEvent timeLineEvent)
        {
            // we use lock to make this method thread safe
            lock (mEvents)
            {
                mEvents.Add(timeLineEvent);
            }
        }

        public void SaveTofile(string fileName)
        {
            string txt = JsonConvert.SerializeObject(mEvents, Formatting.Indented);
            System.IO.File.WriteAllText(fileName, txt);
        }

        public void LoadFromFile(string fileName)
        {
            string txt = System.IO.File.ReadAllText(fileName);
            mEvents = JsonConvert.DeserializeObject<List<TimeLineEvent>>(txt);
            
        }
    }
}
