#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal interface IBrowserWindow
    {
        public delegate Task OnWindowClose(IBrowserWindow closedWindow);

        public IEnumerable<IBrowserTab> Tabs { get; }

        public IBrowserTab CurrentTab { get; }

        public bool IsClosed { get; }

        public Task<IBrowserTab> NewTabAsync(bool setAsCurrent = true);

        public Task SetTabAsync(IBrowserTab tab);

        public Task DeleteCookiesAsync();

        public Task CloseAsync();
    }
}
