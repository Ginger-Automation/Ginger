using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class BrowseTypeAttribute : Attribute, IActionParamProperty
    {
        public string PropertyName => "BrowseType";

        public enum eBrowseType
        {
            File,
            Folder
        }

        public eBrowseType BrowseType { get; set; }


        public BrowseTypeAttribute(eBrowseType browseType)
        {
            BrowseType = browseType;
        }

        public BrowseTypeAttribute()
        {
        }
    }
}