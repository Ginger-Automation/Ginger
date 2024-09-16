using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using OpenQA.Selenium;

namespace Ginger.Environments.GingerAnalyticsEnvWizardLib
{
    class AddGingerAnalyticsEnvWizard : WizardBase
    {
        public RepositoryFolder<ProjEnvironment> EnvsFolder;
        public ProjEnvironment NewEnvironment = new ProjEnvironment();
        public ObservableList<EnvApplication> apps = new ObservableList<EnvApplication>();
        public ObservableList<ProjEnvironment> ImportedEnvs = new ObservableList<ProjEnvironment>();

        public override string Title { get { return "Import Ginger Analytics Environment Wizard"; } }

        public AddGingerAnalyticsEnvWizard(RepositoryFolder<ProjEnvironment> EnvsFolder)
        {
            this.EnvsFolder = EnvsFolder;

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Ginger Analytics Introduction", Page: new GingerAnalyticsIntroPage());

            AddPage(Name: "Environment to Import", Title: "Environment to Import", SubTitle: "Environment to Import", Page: new AddGingerAnalyticsEnvPage());

            AddPage(Name: "Ginger Analytics Environments", Title: "Imported Environments", SubTitle: "Imported Environment Details", Page: new GingerAnalyticsApplicationPage());
            DisableNavigationList();

        }


        public override void Finish()
        {

            //ApplicationPlatform selectedApp = new();
            ////add selected apps
            //foreach (var envapps in ImportedEnvs)
            //{

            //    foreach (EnvApplication app in envapps.Applications)
            //    {
            //        if (app.Active && !WorkSpace.Instance.Solution.ApplicationPlatforms.Contains(new ApplicationPlatform { Platform = app.Platform, AppName = app.Name}))
            //        {
            //            //NewEnvironment.Applications.Add(app);
            //            // for adding the apps in target application
            //            ApplicationPlatform selectedApp = new();
            //            selectedApp.AppName = app.Name;
            //            selectedApp.Platform = app.Platform;
            //            WorkSpace.Instance.Solution.ApplicationPlatforms.Add(selectedApp);
            //        }   
            //    }
            //}

            foreach (ProjEnvironment item in ImportedEnvs)
            {
                //Save the Environment 
                EnvsFolder.AddRepositoryItem(item);
            }
            

        }
    }
}
