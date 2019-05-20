using GingerCoreNET.Drivers.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.ActionsLib
{
    public interface IPlatformActionHandler
    {

        NewPayLoad HandleRunAction(IPlatformService service,NewPayLoad ActionPayload);
    }
}
