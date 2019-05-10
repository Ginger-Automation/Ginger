using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class FolderTypeAttribute : Attribute, IActionParamProperty
    {
        public string PropertyName => "FolderType";

        public Environment.SpecialFolder FolderType { get; set; }

        public FolderTypeAttribute(Environment.SpecialFolder folderType)
        {
            FolderType = folderType;
        }

        public FolderTypeAttribute()
        {
        }
    }
}