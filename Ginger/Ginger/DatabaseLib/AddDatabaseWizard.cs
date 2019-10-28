using GingerCore.Environments;
using GingerWPF.WizardLib;
using System;

namespace Ginger.DatabaseLib
{
    public class AddDatabaseWizard : WizardBase
    {
        public override string Title { get { return "Add Database Wizard"; } }

        public EnvApplication EnvApplication { get; set; }

        public string ServiceID { get; set; }

        public string ConnectionString { get; set; }

        public AddDatabaseWizard(EnvApplication envApplication)
        {
            EnvApplication = envApplication;
            AddPage(Name: "General Details", Title: "General Details", SubTitle: String.Format("Add Database to App env"), Page: new AddDataBaseWizardPage());

            AddPage(Name: "Connection String", Title: "Connection String", SubTitle: String.Format("Add Database to App env"), Page: new AddDatabaseConnWizardPage());
            
        }
        

        public override void Finish()
        {            
            Database db = new Database();            
            db.ServiceID = ServiceID;
            db.ConnectionString = ConnectionString;
            EnvApplication.Dbs.Add(db);
        }
    }
}
