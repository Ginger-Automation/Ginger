using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using OpenQA.Selenium;
using System.Linq;

namespace Ginger.Environments.GingerOpsEnvWizardLib
{
    public class AddGingerOpsEnvWizard : WizardBase
    {
        public RepositoryFolder<ProjEnvironment> EnvsFolder;
        public ProjEnvironment NewEnvironment = new ProjEnvironment();
        public ObservableList<EnvApplication> apps = new ObservableList<EnvApplication>();
        public ObservableList<ProjEnvironment> ImportedEnvs = new ObservableList<ProjEnvironment>();
        public ObservableList<ApplicationPlatform> tempAppPlat = new ObservableList<ApplicationPlatform>();
        public override string Title { get { return "Import GingerOps Environment Wizard"; } }

        public AddGingerOpsEnvWizard(RepositoryFolder<ProjEnvironment> EnvsFolder)
        {
            this.EnvsFolder = EnvsFolder;

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "GingerOps Introduction", Page: new GingerOpsIntroPage());

            AddPage(Name: "Environment to Import", Title: "Environment to Import", SubTitle: "Environment to Import", Page: new AddGingerOpsEnvPage());

            AddPage(Name: "Ginger Analytics Environments", Title: "Imported Environments", SubTitle: "Imported Environment Details", Page: new GingerOpsApplicationPage());
            DisableNavigationList();

        }


        public override void Finish()
        {
            
            foreach (var item in tempAppPlat)
            {
                bool isExist = WorkSpace.Instance.Solution.ApplicationPlatforms.Any(app => app.AppName == item.AppName && app.GingerOpsAppId ==item.GingerOpsAppId);
                if (!isExist)
                {
                    WorkSpace.Instance.Solution.ApplicationPlatforms.Add(item);
                    //WorkSpace.Instance.Solution.AllowAutoSave = true;
                }
               
            }

            foreach (ProjEnvironment item in ImportedEnvs)
            {
                if (item.GOpsFlag)
                {
                    //Save the Environment 
                    EnvsFolder.AddRepositoryItem(item);
                }
    
            }
            
        }
    }
}
