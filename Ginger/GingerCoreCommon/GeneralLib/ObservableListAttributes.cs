using System;

namespace Amdocs.Ginger.Common.GeneralLib
{
    class ObservableListAttributes
    {
    }

    public class IsLazyLoadAttribute : Attribute
    {
        public IsLazyLoadAttribute()
        {

        }        

        public override string ToString()
        {
            return "Is Lazy Load List";
        }
    }
}
