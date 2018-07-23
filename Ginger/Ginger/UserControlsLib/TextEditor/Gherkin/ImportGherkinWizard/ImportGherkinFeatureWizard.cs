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

using Ginger.GherkinLib;
using GingerCore;
using GingerWPF.WizardLib;
using System;
using System.IO;
using static Ginger.GherkinLib.ImportGherkinFeatureFilePage;

namespace Ginger.UserControlsLib.TextEditor.Gherkin
{
    public class ImportGherkinFeatureWizard : WizardBase 
    {
        // shared data across pages goes here        

        public string Folder { get; internal set; }
        public string mFolder;
        public bool Imported;
        public string mFeatureFile;
        public BusinessFlow BizFlow;
        public GenericWindow genWin;
        public eImportGherkinFileContext mContext;
        public string FetaureFileName;

        public ImportGherkinFeatureWizard(string folder)
        {
            Folder = folder;

            AddPage(Name: "Intro", Title: "Import Gherkin Intro", SubTitle: "Importing BDD Gherkin File...", Page: new ImportGherkinIntroPage());
                        
            AddPage(Name: "SelectFile", Title: "Select Feature File", SubTitle: "Choose ...", Page: new ImportGherkinFeatureFilePage(folder, ImportGherkinFeatureFilePage.eImportGherkinFileContext.BusinessFlowFolder));

            AddPage(Name: "Summary", Title: "Summary", SubTitle: "here is what will happen when you click finish", Page: new ImportGherkinFeatureSummaryPage());
        }

        public override string Title { get { return "Import Gherkin Feature Wizard"; } }

        public override void Finish()
        {
            Import();
        }

        private string Import()
        {
            // Copy the feature file to local docs
            string FeatureFolder = App.UserProfile.Solution.ContainingFolderFullPath + @"\Documents\Features\";

            if (mFolder != "Business Flows" && mContext == eImportGherkinFileContext.BusinessFlowFolder)
                FeatureFolder = FeatureFolder + mFolder + "\\";
            else if (mFolder != "Documents" && mFolder != "Features" && mContext == eImportGherkinFileContext.DocumentsFolder)
                FeatureFolder = FeatureFolder + mFolder + "\\";

            if (!Directory.Exists(FeatureFolder))
            {
                Directory.CreateDirectory(FeatureFolder);
            }

            string FileName = System.IO.Path.GetFileName(FetaureFileName);
            string targetFile = Path.Combine(FeatureFolder, FileName);


            if (targetFile == FetaureFileName)
            {                
                Reporter.ToUser(eUserMsgKeys.GherkinNotifyFeatureFileSelectedFromTheSolution, targetFile);
                return String.Empty;
            }

            // TODO: make the check earlier in wizard !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Remove
            if (File.Exists(targetFile))
            {                
                Reporter.ToUser(eUserMsgKeys.GherkinNotifyFeatureFileExists, targetFile);
                return String.Empty;
            }
            File.Copy(FetaureFileName, targetFile);
            Reporter.ToUser(eUserMsgKeys.GherkinFeatureFileImportedSuccessfully, targetFile);
            if (genWin != null)
            {
                genWin.Close();
            }

            return targetFile;
        }
    }
}
