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

using System;

namespace Amdocs.Ginger.Common.UIElement
{
    public class ProgressNotifier
    {
        public event EventHandler<string> LabelHandler;
        public event EventHandler<(string ProgressLabel, int CompletedSteps, int TotalSteps)> StatusUpdateHandler;

        /// <summary>
        /// Notifies subscribers with a detailed progress message.
        /// </summary>
        /// <param name="message">The progress message to notify.</param>
        public void NotifyProgressDetailText(string message)
        {
            LabelHandler?.Invoke(this, message);
        }

        /// <summary>
        /// Notifies subscribers with the updated progress steps.
        /// </summary>
        /// <param name="completedSteps">The number of completed steps.</param>
        /// <param name="totalSteps">The total number of steps.</param>
        public void NotifyProgressUpdated(string progressLabel, int completedSteps, int totalSteps)
        {
            StatusUpdateHandler?.Invoke(this, (progressLabel, completedSteps, totalSteps));
        }
    }
    public class ProgressStatus
    {
        public string ProgressMessage { get; set; }
        public int ProgressStep { get; set; }
        public int TotalSteps { get; set; }

    }
}
