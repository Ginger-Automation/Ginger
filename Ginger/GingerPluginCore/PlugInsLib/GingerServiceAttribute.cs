using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public class GingerServiceAttribute : Attribute
    {
        string mID;        
        string mDescription;

        /// <summary>
        /// Define the class as Ginger service
        /// </summary>
        /// <param name="Id">Service Id is kept in the XML and should never change</param>
        /// <param name="Description">Description is displayed to the user when he select Service</param>
        public GingerServiceAttribute(string Id, string Description)
        {
            mID = Id;
            mDescription = Description;
        }

        public string Id { get { return mID; } set { mID = value; } }
        public string Description { get { return mDescription; } set { mDescription = value; } }

        public override string ToString()
        {
            return "GingerService";
        }
    }
}
