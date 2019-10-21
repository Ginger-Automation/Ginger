using GingerCore.Environments;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.DatabaseLib
{
    public class AddDatabaseWizard : WizardBase
    {
        public override string Title { get { return "Add Database Wizard"; } }

        public EnvApplication EnvApplication { get; set; }

        public AddDatabaseWizard(EnvApplication envApplication)
        {
            EnvApplication = envApplication;
            AddPage(Name: "General Details", Title: "General Details", SubTitle: String.Format("Add Database to App env"), Page: new AddDataBaseWizardPage());

            // AddPage(Name: String.Format("{0} Configurations", GingerDicser.GetTermResValue(eTermResKey.Activity)), Title: String.Format("{0} Configurations", GingerDicser.GetTermResValue(eTermResKey.Activity)), SubTitle: String.Format("Set New {0} Configurations", GingerDicser.GetTermResValue(eTermResKey.Activity)), Page: new AddActivityConfigsPage());
        }
        

        public override void Finish()
        {
            Database db = new Database();
            db.ConnectionString = "???????????";
            EnvApplication.Dbs.Add(db);
        }
    }
}
