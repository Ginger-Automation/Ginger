using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.WebService
{
    public interface IHTTPClient
    {

        GingerHttpResponseMessage PerformHttpOperation(GingerHttpRequestMessage GingerRequestMessageRequestMessage);

    }
}
