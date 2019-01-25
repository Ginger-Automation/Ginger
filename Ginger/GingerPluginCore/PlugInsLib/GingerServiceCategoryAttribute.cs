using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
   
    public class GingerServiceCategoryAttribute : Attribute
    {
        string mCategory;        
      
        public GingerServiceCategoryAttribute(string category)
        {
            mCategory = category;            
        }

        public string Category { get { return mCategory; } set { mCategory = value; } }

        public override string ToString()
        {
            return "GingerServiceCategory";
        }
    }

}
