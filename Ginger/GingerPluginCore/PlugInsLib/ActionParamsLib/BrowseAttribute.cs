using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class BrowseAttribute : Attribute, IActionParamProperty
    {
        public string PropertyName => "Browse";

        public bool IsNeeded { get; set; }

        public BrowseAttribute(bool isNeeded)
        {
            IsNeeded = isNeeded;
        }

        public BrowseAttribute()
        {
        }
    }
}
