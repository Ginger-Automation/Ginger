using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Plugin.Core.Database;

namespace Amdocs.Ginger.Common.DataBaseLib
{
    public interface IDBProvider
    {
        IDatabase GetDBImpl(string serviceId);
    }
}
