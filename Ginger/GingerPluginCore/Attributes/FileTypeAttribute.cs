using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class FileTypeAttribute : Attribute, IParamProperty
    {
        public string PropertyName => "FileType";

        public string FileType { get; set; }        

        public FileTypeAttribute(string fileType)
        {
            FileType = fileType;
        }

        public FileTypeAttribute()
        {
        }
    }
}

