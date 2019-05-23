using GingerCoreNET.Drivers.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run
{
   public interface IActPluginExecution
    {

        NewPayLoad GetActionPayload();
    }
}
