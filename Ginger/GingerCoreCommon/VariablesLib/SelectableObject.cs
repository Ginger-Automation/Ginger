using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.VariablesLib
{
    public class SelectableObject<T>
    {
        public bool IsSelected { get; set; }
        public T TextData { get; set; }

        public SelectableObject(T objectData)
        {
            TextData = objectData;
        }

        public SelectableObject(T objectData, bool isSelected)
        {
            IsSelected = isSelected;
            TextData = objectData;
        }
    }
}
