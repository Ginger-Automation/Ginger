using System;

namespace Amdocs.Ginger.Common.UIElement
{
    public class ProgressNotifier
    {
        public event EventHandler<string> LabelHandler;
        public event EventHandler<(string ProgressLabel,int CompletedSteps, int TotalSteps)> StatusUpdateHandler;

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
}
