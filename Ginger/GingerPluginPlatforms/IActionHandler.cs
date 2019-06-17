using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin
{
    interface IActionHandler
    {

        string ExecutionInfo { get; set; }
        string Error { get; set; }
     //   string Log { get; set; }
    }
}
