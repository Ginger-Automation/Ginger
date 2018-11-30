using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IRepositoryItem
    {
        string GetNameForFileName();
        string ItemName { get; set; }
 
    }
}
