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

using Amdocs.Ginger.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    /// <summary>
    /// Runs an ordered chain of automation techniques against a desktop control and
    /// stops at the first one that works, recording every attempt so a failed action
    /// reports what was tried instead of a single generic message.
    /// </summary>
    public sealed class DesktopAutomationEngine
    {
        private readonly List<IDesktopAutomationLayer> mLayers;

        public DesktopAutomationEngine(IEnumerable<IDesktopAutomationLayer> layers)
        {
            mLayers = layers?.ToList() ?? [];
        }

        public IReadOnlyList<IDesktopAutomationLayer> Layers => mLayers;

        public static DesktopAutomationEngine Default { get; } = CreateDefault();

        public static DesktopAutomationEngine CreateDefault()
        {
            return new DesktopAutomationEngine(
            [
                new UiaPatternLayer(),
                new MsaaLayer(),
                new Win32Layer(),
                new PhysicalInputLayer()
            ]);
        }

        public DesktopEngineResult Execute(DesktopActionContext context)
        {
            if (context == null)
            {
                return new DesktopEngineResult { ErrorMessage = "Desktop action context is null" };
            }

            StringBuilder attempts = new StringBuilder();
            DesktopEngineResult emptyRead = null;

            foreach (IDesktopAutomationLayer layer in mLayers)
            {
                if (!layer.CanHandle(context))
                {
                    continue;
                }

                LayerResult layerResult = layer.TryExecute(context);
                if (attempts.Length > 0)
                {
                    attempts.Append(" | ");
                }
                attempts.Append(layer.Name).Append(" [").Append(layerResult.Status).Append("]: ").Append(layerResult.Message);

                if (layerResult.Status != LayerExecutionStatus.Succeeded)
                {
                    continue;
                }

                DesktopEngineResult success = new DesktopEngineResult
                {
                    Success = true,
                    UsedLayer = layerResult.LayerName ?? layer.Name,
                    OutputValue = layerResult.OutputValue,
                    ExecutionInfo = context.Operation + " via " + (layerResult.LayerName ?? layer.Name) + ". " + layerResult.Message
                };

                // A read that comes back blank is kept only as a last resort, so a later
                // layer still gets the chance to return the real text. This mirrors the
                // pattern-then-legacy order the drivers used before the engine existed.
                if (context.Operation == DesktopOperation.GetValue && string.IsNullOrEmpty(success.OutputValue))
                {
                    emptyRead ??= success;
                    continue;
                }

                Reporter.ToLog(eLogLevel.DEBUG, success.ExecutionInfo);
                return success;
            }

            if (emptyRead != null)
            {
                Reporter.ToLog(eLogLevel.DEBUG, emptyRead.ExecutionInfo);
                return emptyRead;
            }

            return new DesktopEngineResult
            {
                Success = false,
                ErrorMessage = "All desktop automation layers failed. " + attempts,
                ExecutionInfo = attempts.ToString()
            };
        }
    }
}
