using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            EnumValueDescriptionAttribute attribute = (EnumValueDescriptionAttribute)field.GetCustomAttribute(typeof(EnumValueDescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.ValueDescription;
        }
    }
}
