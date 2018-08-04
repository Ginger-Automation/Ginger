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

using GingerPlugIns;
using GingerPlugIns.ActionsLib;
using System.Windows;
using System.Windows.Controls;

namespace GingerPACTPlugIn.ActionEditPages
{
    /// <summary>
    /// Interaction logic for LoadInteractionFile.xaml
    /// </summary>
    public partial class LoadInteractionsFile : Page
    {
        GingerAction mAct;

        public LoadInteractionsFile(GingerAction act)
        {
            InitializeComponent();

            mAct = act;

            ActionParam AP1 = mAct.GetOrCreateParam("FileName");
            FileName.BindControl(AP1);
        }

        private void BrowseButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "JSON" + " Files (*.json)|*.json|All Files (*.*)|*.*";
            dlg.FilterIndex = 1;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                
                if (dlg.FileName.ToUpper().StartsWith(mAct.SolutionFolder.ToUpper()))
                {
                    FileName.Text = dlg.FileName.ToUpper().Replace(mAct.SolutionFolder.ToUpper(), @"~\").ToLower();
                }
                else
                    FileName.Text = dlg.FileName;
            }
        }
    }
}
