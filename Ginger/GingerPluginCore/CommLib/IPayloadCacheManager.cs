using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.CommLib
{
    public interface IPayloadCacheManager
    {
        string  GetFilePath(byte[] key);
    }
}
