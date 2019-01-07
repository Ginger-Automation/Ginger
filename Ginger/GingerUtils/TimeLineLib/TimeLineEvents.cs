using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace GingerUtils.TimeLine
{
    public class TimeLineEvents
    {
        // We track all events to same base using one Stopwatch and taking millis 
        public static readonly Stopwatch Stopwatch = new Stopwatch();

        private List<TimeLineEvent> mEvents = new List<TimeLineEvent>();

        public IEnumerable<TimeLineEvent> Events
        {
            get
            {
                return mEvents;
            }
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
