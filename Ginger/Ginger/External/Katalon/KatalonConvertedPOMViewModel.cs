using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Ginger.External.Katalon
{
    public sealed class KatalonConvertedPOMViewModel : INotifyPropertyChanged
    {
        private bool _active;
        private string _name;
        private string _url;
        private string _targetApplication;
        private bool _showTargetApplicationErrorHighlight;

        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                PropertyChanged?.Invoke(sender: this, new(nameof(Active)));
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value ?? string.Empty;
                PropertyChanged?.Invoke(sender: this, new(nameof(Name)));
            }
        }

        public string URL
        {
            get => _url;
            set
            {
                _url = value ?? string.Empty;
                PropertyChanged?.Invoke(sender: this, new(nameof(URL)));
            }
        }

        public string TargetApplication
        {
            get => _targetApplication;
            set
            {
                _targetApplication = value ?? string.Empty;
                if (IsTargetApplicationValid())
                {
                    ShowTargetApplicationErrorHighlight = false;
                }
                PropertyChanged?.Invoke(sender: this, new(nameof(TargetApplication)));
            }
        }

        public IEnumerable<string> TargetApplicationOptions { get; }

        public bool ShowTargetApplicationErrorHighlight
        {
            get => _showTargetApplicationErrorHighlight;
            set
            {
                _showTargetApplicationErrorHighlight = value;
                PropertyChanged?.Invoke(sender: this, new(nameof(ShowTargetApplicationErrorHighlight)));
            }
        }

        public ApplicationPOMModel POM { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public KatalonConvertedPOMViewModel(ApplicationPOMModel pom, ePlatformType platform)
        {
            ArgumentNullException.ThrowIfNull(pom);

            POM = pom;
            _active = true;
            _name = POM.Name;
            _url = string.Empty;
            _targetApplication = string.Empty;

            List<string> solutionTargetApps = WorkSpace
                .Instance
                .Solution
                .GetSolutionTargetApplications()
                .Where(ta => GetApplicationPlatform(ta.Name) == platform)
                .Select(ta => ta.Name)
                .ToList();

            TargetApplicationOptions =
                new string[] { string.Empty }
                .Concat(solutionTargetApps)
                .ToArray();
        }

        private ePlatformType GetApplicationPlatform(string application)
        {
            ArgumentException.ThrowIfNullOrEmpty(application);

            if (WorkSpace.Instance?.Solution == null)
            {
                throw new InvalidOperationException("Workspace or Solution is not initialized");
            }

            return WorkSpace.Instance.Solution.GetApplicationPlatformForTargetApp(application);
        }

        public bool IsValid()
        {
            if (!IsTargetApplicationValid())
            {
                return false;
            }
            return true;
        }

        public void ShowAllErrorHighlights()
        {
            if (!IsTargetApplicationValid())
            {
                ShowTargetApplicationErrorHighlight = true;
            }
        }

        public void ClearAllErrorHighlights()
        {
            ShowTargetApplicationErrorHighlight = false;
        }

        public bool IsTargetApplicationValid()
        {
            return !string.IsNullOrWhiteSpace(TargetApplication);
        }

        public void CommitChanges()
        {
            if (!string.IsNullOrWhiteSpace(URL))
            {
                POM.PageURL = URL.Trim();
            }

            ApplicationPlatform? appPlatform = WorkSpace.Instance
                .Solution
                .ApplicationPlatforms
                .FirstOrDefault(ap => string.Equals(ap.AppName, TargetApplication));
            if (appPlatform != null)
            {
                POM.TargetApplicationKey = appPlatform.Key;
            }
        }
    }
}
