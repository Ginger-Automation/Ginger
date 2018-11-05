using System;
using System.Collections.Generic;
using System.Text;
using GingerCore.Variables;

namespace Amdocs.Ginger.Common
{
    public interface IBusinessFlow
    {
        VariableBase GetHierarchyVariableByName(string varName, bool considerLinkedVar = true);
    }
}
