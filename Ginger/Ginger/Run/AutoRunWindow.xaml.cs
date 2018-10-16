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

using GingerCore;
using System;
using System.Windows;
using System.Linq;
using System.Reflection;
using GingerCore.Environments;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for AutoRunWindow.xaml
    /// </summary>
    public partial class AutoRunWindow : Window
    {
        public AutoRunWindow()
        {
            InitializeComponent();

            this.Show();

            NewRunSetPage runSetPage = new NewRunSetPage(App.RunsetExecutor.RunSetConfig);
            this.Content = runSetPage;
        }

        private bool LoadExecutionConfigurations(string runSetName, string environmentName)
        {
            ObservableList<RunSetConfig> runSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
            RunSetConfig runSetConfig = runSets.Where(x=>x.Name.ToLower().Trim() == runSetName.ToLower().Trim()).FirstOrDefault();
            if (runSetConfig == null)            
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, string.Format("The configured {0} with the name '{1}' was not found in the Solution", GingerDicser.GetTermResValue(eTermResKey.RunSet), runSetName));
                return false;
            }

            ProjEnvironment env = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name.ToLower().Trim() == environmentName.ToLower().Trim()).FirstOrDefault();
            if (env == null)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, string.Format("The configured Environment with the name '{0}' was not found in the Solution", environmentName));
                return false;
            }
            
            try
            {
                App.RunsetExecutor.RunSetConfig = runSetConfig;

                //TODO: create intizilize run inside the RunSet executer
                App.RunsetExecutor.RunsetExecutionEnvironment = env;
                App.RunsetExecutor.SetRunnersEnv(env, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>());                
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, string.Format("Failed to setup the RunsetExecutor for the {0} '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), runSetName));
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                return false;
            }            

            return true;
        }

        protected override void OnClosed(EventArgs e)
        {
            App.RunsetExecutor.StopRun();

            base.OnClosed(e);            
            App.MainWindow.Close();
            Environment.Exit(0);
        }
    }
}
