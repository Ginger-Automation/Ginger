using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IActivityIdentifiers
    {
        eActivityAutomationStatus? ActivityAutomationStatus { get; set; }
        string ActivityName { get; set; }
        Guid ActivityGuid { get; set; }
        string ActivityExternalID { get; set; }
        IActivity IdentifiedActivity { get; set; }
    }
}
