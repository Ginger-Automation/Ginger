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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace Ginger
{
    /// <summary>
    /// The DevTimeTrackerIdle class tracks user activity within a WPF application and triggers events when the user is idle for a specified duration.
    /// It monitors various input events (mouse movement, keyboard input, mouse click) and raises events to pause and resume
    /// time-based activities when the user becomes idle or active, respectively.
    /// </summary>
    public class DevTimeTrackerIdle
    {
        private DispatcherTimer idleTimer;
        private DateTime lastActivity;
        private bool isPaused;


        /// <summary>
        /// Initializes a new instance of the DevTimeTrackerIdle class.
        /// The DevTimeTrackerIdle starts tracking user activity and will raise PauseTime and ResumeTime events as appropriate.
        /// </summary>

        public DevTimeTrackerIdle()
        {
            idleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1) // Check every minute
            };
            idleTimer.Tick += IdleTimer_Tick;
            idleTimer.Start();

            lastActivity = DateTime.Now;
            isPaused = false;
        }

        /// <summary>
        /// Attaches event handlers to monitor user activity on the specified UIElement.
        /// The IdleMonitor will track mouse movement, keyboard input, mouse clicks.
        /// </summary>
        /// <param name="element">The UIElement to monitor for activity.</param>
        public void AttachActivityHandlers(UIElement element)
        {
            element.MouseMove += OnActivity;
            element.KeyDown += OnActivity;
            element.MouseDown += OnActivity;
        }

        /// <summary>
        /// Detaches the event handlers previously attached to monitor user activity on the specified UIElement.
        /// This method should be called to clean up resources when the monitoring is no longer needed.
        /// </summary>
        /// <param name="element">The UIElement to stop monitoring for activity.</param>
        public void DetachActivityHandlers(UIElement element)
        {
            element.MouseMove -= OnActivity;
            element.KeyDown -= OnActivity;
            element.MouseDown -= OnActivity;
        }

        /// <summary>
        /// Handles the DispatcherTimer tick event to check for user inactivity.
        /// If the user has been idle for longer than the specified duration (5 minutes), the OnPauseTime event is raised.
        /// </summary>
        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now - lastActivity > TimeSpan.FromMinutes(5) && !isPaused)
            {
                PauseDevelopmentTimeTracker();
            }
        }

        /// <summary>
        /// Handles user activity events by resetting the last activity time.
        /// If the user was previously idle and becomes active again, the OnResumeTime event is raised.
        /// </summary>
        private void OnActivity(object sender, EventArgs e)
        {
            if (isPaused)
            {
                ResumeDevelopmentTimeTracker();
            }

            lastActivity = DateTime.Now;

        }

        private List<RepositoryItemBase> _itemsWithPausedDevelopmentTimeTracker = [];

        /// <summary>
        /// Pauses the development time tracker for modified files in the solution.
        /// </summary>
        public void PauseDevelopmentTimeTracker()
        {
            if (WorkSpace.Instance == null ||
                WorkSpace.Instance.SolutionRepository == null ||
                WorkSpace.Instance.SolutionRepository.ModifiedFiles == null)
            {
                return;
            }

            try
            {
                _itemsWithPausedDevelopmentTimeTracker.Clear();
                List<RepositoryItemBase> modifiedFiles = new(WorkSpace.Instance.SolutionRepository.ModifiedFiles);

                foreach (RepositoryItemBase modifiedFile in modifiedFiles)
                {
                    if (modifiedFile is GingerCore.BusinessFlow bf && bf.IsTimerRunning())
                    {
                        bf.StopTimer();
                        _itemsWithPausedDevelopmentTimeTracker.Add(bf);

                        foreach (GingerCore.Activity bfActivity in bf.Activities)
                        {
                            if (bfActivity.IsTimerRunning())
                            {
                                bfActivity.StopTimer();
                                _itemsWithPausedDevelopmentTimeTracker.Add(bfActivity);
                            }
                        }
                    }
                    else if (modifiedFile is GingerCore.Activity activity && activity.IsTimerRunning())
                    {
                        activity.StopTimer();
                        _itemsWithPausedDevelopmentTimeTracker.Add(activity);
                    }
                    else if (modifiedFile is Amdocs.Ginger.Repository.ApplicationPOMModel applicationPOMModel && applicationPOMModel.IsTimerRunning())
                    {
                        applicationPOMModel.StopTimer();
                        _itemsWithPausedDevelopmentTimeTracker.Add(applicationPOMModel);
                    }
                }

                //Setting flag to true
                if (_itemsWithPausedDevelopmentTimeTracker.Count > 0)
                {
                    isPaused = true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "error while pausing development tracker", ex);
            }
        }

        /// <summary>
        /// Resumes the development time tracker for paused items.
        /// </summary>
        public void ResumeDevelopmentTimeTracker()
        {
            try
            {
                List<RepositoryItemBase> items = new(_itemsWithPausedDevelopmentTimeTracker);
                _itemsWithPausedDevelopmentTimeTracker.Clear();

                foreach (RepositoryItemBase item in items)
                {
                    if (item is GingerCore.BusinessFlow bf)
                    {
                        bf.StartTimer();
                    }
                    else if (item is GingerCore.Activity activity)
                    {
                        activity.StartTimer();
                    }
                    else if (item is Amdocs.Ginger.Repository.ApplicationPOMModel applicationPOMModel)
                    {
                        applicationPOMModel.StartTimer();
                    }
                }

                //Setting flag to false
                if (_itemsWithPausedDevelopmentTimeTracker.Count == 0)
                {
                    isPaused = false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "error while resuming development tracker", ex);
            }
        }
    }
}
