#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.DataSource;
using Ginger.Environments;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.WizardLib;
using GingerCore;
using GingerCore.DataSource;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace Ginger.SolutionWindows
{
    public class ImportDataSourceFromExcelWizard : WizardBase
    {
        public DataSourceBase DSDetails { get; set; }

        public override string Title { get { return "Create new solution wizard"; } }

        public List<PluginPackage> SelectedPluginPackages = new List<PluginPackage>();

        /// <summary>
        /// This is used to initialise the wizard
        /// </summary>
        public ImportDataSourceFromExcelWizard(DataSourceBase mDSDetails)
        {
            DSDetails = mDSDetails;
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Import DataSource From Excel File", Page: new WizardIntroPage("/SolutionWindows/ImportDataSourceIntro.md"));
            AddPage(Name: "Browse File", Title: "Browse File", SubTitle: "Import DataSource From Excel File", Page: new ImportDataSourceBrowseFile());
            AddPage(Name: "Sheet Selection", Title: "Sheet Selection", SubTitle: "Import DataSource From Excel File", Page: new ImportDataSourceSheetSelection());
            AddPage(Name: "Display Data", Title: "Display Data", SubTitle: "Import DataSource From Excel File", Page: new ImportDataSourceDisplayData(), AlternatePage: new ImportDataSourceDisplayAllData());
            AddPage(Name: "Finish", Title: "Finish", SubTitle: "Import DataSource From Excel File", Page: new ImportDataSourceFinishPage(DSDetails));
        }

        /// <summary>
        /// This method is the final finish method
        /// </summary>
        public override void Finish()
        {
        }
    }
}