#region License
/*
Copyright © 2014-2019 European Support Limited

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

using GingerWPF.WizardLib;
using System.Windows.Controls;

namespace GingerWPF.PluginsLib.AddPluginWizardLib
{
    /// <summary>
    /// Interaction logic for SelectPluginTypePage.xaml
    /// </summary>
    public partial class SelectPluginPackageTypePage : Page, IWizardPage
    {
        public SelectPluginPackageTypePage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {

        }

        // TODO:
        // if (IsEmbeddedPlugin)
        //            {
        //                GingerCore.General.DirectoryCopy(PlugInSourcePath, solutionPlugInsPath, true);
        //                PlugInRootPath = solutionPlugInsPath;
        //                PlugInType = ePluginType.Embedded;
        //            }
        //            else
        //            {
        //                Directory.CreateDirectory(solutionPlugInsPath);
        //                PlugInRootPath = PlugInSourcePath;
        //                PlugInType = ePluginType.System;
        //            }
               
    }
}
