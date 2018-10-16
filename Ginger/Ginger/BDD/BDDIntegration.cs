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

using amdocs.ginger.GingerCoreNET;
using Ginger.GherkinLib;
using Ginger.UserControlsLib.TextEditor.Gherkin;
using GingerCore;
using GingerWPF.WizardLib;
using System.IO;
using Ginger.SolutionWindows.TreeViewItems;

namespace Ginger.BDD
{
    public class BDDIntegration
    {
        public void CreateFeatureFile()
        {
            string FileName = string.Empty;
            if (GingerCore.General.GetInputWithValidation("New Feature File", "File Name:", ref FileName, System.IO.Path.GetInvalidFileNameChars()))
            {
                string FullDirectoryPath = System.IO.Path.Combine(App.UserProfile.Solution.Folder, "Documents", "Features");
                if (!System.IO.Directory.Exists(FullDirectoryPath))
                {
                    System.IO.Directory.CreateDirectory(FullDirectoryPath);

                }
                string FullFilePath = FullDirectoryPath + @"\" + FileName + ".feature";
                if (!System.IO.File.Exists(FullFilePath))
                {
                    string FileContent = "Feature: Description\r\n\r\n@Tag1 @Tag2\r\n\r\nScenario: Scenario1 Description\r\n       Given \r\n       And \r\n       And \r\n       When \r\n       Then \r\n\r\n\r\n@Tag1 @Tag2\r\n\r\nScenario: Scenario2 Description\r\n       Given \r\n       And \r\n       And \r\n       When \r\n       Then \r\n\r\n@Tag1 @Tag2\r\n\r\n\r\nScenario Outline: eating\r\n  Given there are <start> cucumbers\r\n  When I eat <eat> cucumbers\r\n  Then I should have <left> cucumbers\r\n\r\n  Examples:\r\n    | start | eat | left |\r\n    |  12   |  5  |  7   |\r\n    |  20   |  5  |  15  |";
                    using (Stream fileStream = System.IO.File.Create(FullFilePath))
                    {
                        fileStream.Close();
                    }
                    System.IO.File.WriteAllText(FullFilePath, FileContent);
                }
                else
                    Reporter.ToUser(eUserMsgKeys.GherkinNotifyFeatureFileExists, FullFilePath);
            }
        }

        public bool ImportFeatureFile()
        {            
            BusinessFlowsFolderTreeItem bfsFolder = new BusinessFlowsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>(), eBusinessFlowsTreeViewMode.ReadOnly);
            if (WorkSpace.Instance.BetaFeatures.ImportGherkinFeatureWizrd)
            {
                WizardWindow.ShowWizard(new ImportGherkinFeatureWizard(bfsFolder, ImportGherkinFeatureFilePage.eImportGherkinFileContext.BusinessFlowFolder));                
            }
            return true;
        }
    }
}
