using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.WebService.Execution
{
    public class WebServicePlatformActionHandler : IPlatformActionHandler
    {
        enum RequestMethod
        {
            Copy,
            Delete,
            Get,
            head,
            Link,
            Lock,
            Options,
            Patch,
            Post,
            Profin,
            Purge,
            Put,
            Unlink,
            Unlock,
            View


        }

        public void HandleRunAction(IPlatformService service, ref NodePlatformAction platformAction)
        {
            throw new NotImplementedException();
        }
    }
}
