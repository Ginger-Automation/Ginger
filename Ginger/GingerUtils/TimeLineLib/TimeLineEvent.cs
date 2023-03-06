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

        public double Start { get { return mStart; } set { mStart = (uint)value; } }
        public double End { get { return mEnd; } set { mEnd = (uint)value; } }



        // Item level in the tree, root items are 0 
        public int Level { get; set; }

        List<TimeLineEvent> mChildren = new List<TimeLineEvent>();

        public List<TimeLineEvent> ChildrenList
        {
            get
            {
                return mChildren;               
            }
        }

        public TimeLineEvent()
        {
        }

        public TimeLineEvent(string itemType, string text, uint start)
        {
            mItemType = itemType;
            mText = text;
            mStart = start;
        }

        public TimeLineEvent(string itemType, string text, uint start, uint end)
        {
            mItemType = itemType;
            mText = text;
            mStart = start;
            mEnd = end;
        }

       
        public void AddSubEvent(TimeLineEvent timeLineEvent)
        {
            timeLineEvent.Level = this.Level + 1;
            mChildren.Add(timeLineEvent);
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

        /// <summary>
        /// Return elapsed in millis 
        /// </summary>
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

       
        


        
    }
}
