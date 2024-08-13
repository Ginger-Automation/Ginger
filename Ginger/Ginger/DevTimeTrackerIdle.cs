using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;

namespace Ginger
    {
    public class DevTimeTrackerIdle
    {
        private DispatcherTimer idleTimer;
        private DateTime lastActivity;
        private bool isPaused;


        public DevTimeTrackerIdle()
        {
            idleTimer = new DispatcherTimer();
            idleTimer.Interval = TimeSpan.FromMinutes(1); // Check every minute
            idleTimer.Tick += IdleTimer_Tick;
            idleTimer.Start();

            lastActivity = DateTime.Now;
            isPaused = false;
        }

        public void AttachActivityHandlers(UIElement element)
        {
            element.MouseMove += OnActivity;
            element.KeyDown += OnActivity;
            element.MouseDown += OnActivity;
        }


        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now - lastActivity > TimeSpan.FromMinutes(5) && !isPaused)
            {
                PauseDevelopmentTimeTracker();
            }
        }

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
