#region License
/*
Copyright © 2014-2025 European Support Limited

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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class MockTelemetryCollector<TRecord> : ITelemetryCollector<TRecord>
    {
        private readonly Func<IEnumerable<TRecord>, Task<ITelemetryCollector<TRecord>.AddResult>> _recordsHandler;

        internal MockTelemetryCollector(Func<IEnumerable<TRecord>, Task<ITelemetryCollector<TRecord>.AddResult>> recordsHandler)
        {
            _recordsHandler = recordsHandler;
        }

        public Task<ITelemetryCollector<TRecord>.AddResult> AddAsync(IEnumerable<TRecord> records)
        {
            return _recordsHandler(records);
        }
    }
}
