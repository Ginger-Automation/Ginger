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

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal class TelemetryBaseRecord
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();

        public string SolutionId { get; init; }

        public string Account { get; init; }

        public required DateTime CreationTimestamp { get; init; }

        public required DateTime LastUpdateTimestamp { get; set; }

        public required string AppVersion { get; init; }

        public required string UserId { get; init; }

        public int UploadAttempt { get; set; }
    }
}
