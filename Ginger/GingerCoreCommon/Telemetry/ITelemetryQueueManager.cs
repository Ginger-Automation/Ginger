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
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Telemetry
{
    public interface ITelemetryQueueManager : IDisposable
    {
        public sealed class Config : RepositoryItemBase
        {
            [IsSerializedForLocalRepository]
            public string CollectorURL { get; set; } = string.Empty;

            [IsSerializedForLocalRepository(DefaultValue: 10)]
            public int BufferSize { get; set; } = 10;

            [IsSerializedForLocalRepository(DefaultValue: 1800)]
            public int RetryIntervalInSeconds { get; set; } = 1800;

            [IsSerializedForLocalRepository(DefaultValue: 10)]
            public int RetryPollingSize { get; set; } = 10;

            [IsSerializedForLocalRepository(DefaultValue: eLogLevel.ERROR)]
            public eLogLevel MinLogLevel { get; set; } = eLogLevel.ERROR;

            public override string ItemName { get; set; }
        }

        public void AddLog(eLogLevel level, string msg);

        public void AddLog(eLogLevel level, string msg, TelemetryMetadata metadata);

        public void AddFeatureUsage(FeatureId featureId);

        public void AddFeatureUsage(FeatureId featureId, TelemetryMetadata metadata);

        public IFeatureTracker StartFeatureTracking(FeatureId featureId);
    }
}
