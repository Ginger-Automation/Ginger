#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{

    /// <summary>
    /// We have an interface for node comm monitor so we can have nice UI fwhen running from WPF but will be able also to create Monitor dumper for linux to help in debug
    /// </summary>
    public interface IGingerNodeMonitor
    {
        void ShowMonitor(GingerNodeProxy gingerNodeProxy);
        void Add(GingerSocketLog gingerSocketLog);
        void CloseMonitor();
    }
}
