using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
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
                _name = value;
                PropertyChanged?.Invoke(sender: this, new(nameof(Name)));
            }
        }

        public string URL
        {
            get => _url;
            set
            {
                _url = value;
                PropertyChanged?.Invoke(sender: this, new(nameof(URL)));
            }
        }

        public string TargetApplication
        {
            get => _targetApplication;
            set
            {
                _targetApplication = value;
                PropertyChanged?.Invoke(sender: this, new(nameof(TargetApplication)));
            }
        }

        public IEnumerable<string> TargetApplicationOptions { get; }

        public ApplicationPOMModel POM { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public KatalonConvertedPOMViewModel(ApplicationPOMModel pom, ePlatformType platform)
        {
            POM = pom;
            _active = true;
            _name = POM.Name;
            _url = string.Empty;
            _targetApplication = string.Empty;

            TargetApplicationOptions = WorkSpace
                .Instance
                .Solution
                .GetSolutionTargetApplications()
                .Where(ta => GetApplicationPlatform(ta.Name) == platform)
                .Select(ta => ta.Name);
        }

        private ePlatformType GetApplicationPlatform(string application)
        {
            return
                WorkSpace
                .Instance
                .Solution
                .GetApplicationPlatformForTargetApp(application);
        }

        public void CommitChanges()
        {
            if (URL != null && !string.Equals(URL.Trim(), string.Empty))
            {
                POM.PageURL = URL;
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
