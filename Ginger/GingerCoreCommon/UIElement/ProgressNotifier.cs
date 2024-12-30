using System;

namespace Amdocs.Ginger.Common.UIElement
{
    public class ProgressNotifier
    {
        public event EventHandler<string> ProgressText;
        public event EventHandler<(int CompletedSteps, int TotalSteps)> ProgressUpdated;

        /// <summary>
        /// Notifies subscribers with a detailed progress message.
        /// </summary>
        /// <param name="message">The progress message to notify.</param>
        public void NotifyProgressDetailText(string message)
        {
            ProgressText?.Invoke(this, message);
        }

        /// <summary>
        /// Notifies subscribers with the updated progress steps.
        /// </summary>
        /// <param name="completedSteps">The number of completed steps.</param>
        /// <param name="totalSteps">The total number of steps.</param>
        public void NotifyProgressUpdated(int completedSteps, int totalSteps)
        {
            ProgressUpdated?.Invoke(this, (completedSteps, totalSteps));
        }
    }
}
