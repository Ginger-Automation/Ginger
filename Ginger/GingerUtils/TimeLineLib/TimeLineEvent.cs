using System;
using System.Collections.Generic;
using System.Windows;

namespace GingerUtils.TimeLine
{
    public class TimeLineEvent 
    {        
        private string mText;

        // keep the data in memory efficient and small use uint,  each unit is millis so max can be 4,294,967,295 = 1,193 hours
        private uint mStart;
        private uint mEnd;

        // Item level in the tree, root items are 0 
        public int Level { get; set; }

        List<TimeLineEvent> mChildren = new List<TimeLineEvent>();


        public TimeLineEvent()
        {
        }

        public TimeLineEvent(string itemType, string text, uint start, uint end)
        {
            mItemType = itemType;
            mText = text;
            mStart = start;
            mEnd = end;
        }

        public static TimeLineEvent StartNew(string itemType , string text)
        {
            TimeLineEvent timeLineEvent = new TimeLineEvent();
            timeLineEvent.mItemType = itemType;
            timeLineEvent.mText = text;
            timeLineEvent.mStart = (uint)(TimeLineEvents.Stopwatch.ElapsedMilliseconds);
            return timeLineEvent;
        }

        public void AddSubEvent(TimeLineEvent timeLineEvent)
        {
            timeLineEvent.Level = this.Level + 1;
            mChildren.Add(timeLineEvent);
        }

        public void Stop()
        {
            mEnd = (uint)(TimeLineEvents.Stopwatch.ElapsedMilliseconds);
        }

        
        public IEnumerable<TimeLineEvent> Children
        {
            get
            {
                return mChildren;
            }
        }


        public string Text { get { return mText; } set { { mText = value; } } }

        string mItemType;
        public string ItemType { get { return mItemType; } set { mItemType = value; } }

        public uint Elapsed
        {
            get
            {
                return mEnd - mStart;
            }            
        }

        //public Thickness TextOffset
        //{
        //    get
        //    {
        //        int offset = Level * 19;   // 19 is node indent - TODO: find it progr                
                
        //        return new Thickness(offset, 0, 0, 0);
        //    }
        //}


        //public Thickness XOffset
        //{
        //    get
        //    {                
        //        int offset = Level * 19;   // 19 is node indent - TODO: find it progr                                
        //        return new Thickness(-offset, 0, 0, 0);
        //    }
        //}

       
        public double Start { get { return mStart; }  set { mStart = (uint)value; } }
        public double End { get { return mEnd; } set { mEnd = (uint)value; } }



        
    }
}
