#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.DriversLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Ginger.GingerCoreNETTestLib
{
    // -------------------------------------------------------
    //
    //   DO NOT CLEAN commented code in this class !!!!!!!!!!!!!!
    //
    // -------------------------------------------------------


    /// <summary>
    /// Interaction logic for GingerCoreNETWindow.xaml
    /// </summary>
    public partial class GingerCoreNETWindow : Window
    {
        class MyAction
        {
            public string Name { get; set; }
            public Action Action { get; set; }
        }

        SolutionRepository mSolutionRepository;
        // GingerGrid mGingerGrid;
        List<MyAction> Actions = [];

        public GingerCoreNETWindow()
        {
            InitializeComponent();

            mSolutionRepository = WorkSpace.Instance.SolutionRepository;
            LogTextBox.Clear();
            ElapsedLabel.Visibility = Visibility.Collapsed;
            MemKBLabel.Visibility = Visibility.Collapsed;

            Actions.Add(new MyAction() { Name = "Report.Error", Action = () => ReporterError() });
            Actions.Add(new MyAction() { Name = "Get all Files", Action = () => GetAllFiles() });
            Actions.Add(new MyAction() { Name = "Get All BFs", Action = () => GetBFs() });
            Actions.Add(new MyAction() { Name = "Get All BFs + keep refs", Action = () => GetBFsKeepRef() });
            Actions.Add(new MyAction() { Name = "Get All BFs and Drill down", Action = () => GetAllBFsandDrilldown() });
            Actions.Add(new MyAction() { Name = "Get All BFs and Save", Action = () => GetAllBFsandSave() });
            Actions.Add(new MyAction() { Name = "GetEnvironments", Action = () => GetEnvironments() });
            Actions.Add(new MyAction() { Name = "Repository Item Base Report", Action = () => RepositoryItemBaseReport() });
            Actions.Add(new MyAction() { Name = "Test crash on non UI thread", Action = () => TestCrash() });
            Actions.Add(new MyAction() { Name = "LongPath Test", Action = () => LongPathTest() });
            ActionsListBox.ItemsSource = Actions;
            MainDataGrid.MouseDoubleClick += MainDataGrid_MouseDoubleClick;
        }

        private void RepositoryItemBaseReport()
        {

            string s = "";
            RemoteObjectProxy<RepositoryItem> dp = new RemoteObjectProxy<RepositoryItem>(); //dummy to load the dll
            //s += getRIBase(typeof(Ginger.App).Assembly);  // Ginger
            //s += getRIBase(typeof(RepositoryItemBase).Assembly);  // GingerCoreCommon
            //s += getRIBase(typeof(RepositoryItem).Assembly);  // GingerCore

            s += getRIBase(typeof(GingerNode).Assembly);  // GingerCoreNet

            System.IO.File.WriteAllText(@"c:\temp\RIBase.txt", s);
        }

        private string getRIBase(Assembly a)
        {
            string s = "";
            var RepositoryItemTypes =
              from type in a.GetTypes()
              where type.IsSubclassOf(typeof(RepositoryItemBase))
              select type;

            foreach (Type t in RepositoryItemTypes)
            {
                string Notes = "";
                if (t.IsSubclassOf(typeof(Act)))
                {
                    Notes = "Act";
                }
                s += t.FullName + "," + t.Assembly.ManifestModule.Name + "," + Notes + Environment.NewLine;
            }
            return s;
        }

        private void ReporterError()
        {
            Reporter.ToLog(eLogLevel.ERROR, "Test Reporter Error!");
        }


        Action<Object> GridDoubleClick = null;

        private void MainDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridDoubleClick != null)
            {
                GridDoubleClick.Invoke(MainDataGrid.CurrentItem);
            }
        }



        private static Random random = new Random((int)DateTime.Now.Ticks);
        private string GetRandomString()
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            int v;
            for (int i = 0; i < 30; i++)
            {

                v = (int)Math.Floor(74 * random.NextDouble() + 48);

                while (!(Enumerable.Range(48, 10).Contains(v) || Enumerable.Range(97, 26).Contains(v) || Enumerable.Range(65, 26).Contains(v)))
                {
                    v = (int)Math.Floor(74 * random.NextDouble() + 48);
                }

                ch = Convert.ToChar(v);

                builder.Append(ch);
            }
            return builder.ToString();
        }



        private void GetAllBFsandSave()
        {
            ObservableList<BusinessFlow> BFs = mSolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            foreach (BusinessFlow BF in BFs)
            {
                ObservableList<Activity> activities = BF.Activities; // force it to parse the activities objects
                int i = activities.Count;
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(BF);
            }
            Log("Done");
        }

        private void GetAllBFsandDrilldown()
        {
            ObservableList<BusinessFlow> BFs = mSolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            foreach (BusinessFlow BF in BFs)
            {
                ObservableList<Activity> activities = BF.Activities;
                Log(BF.Name + ", Activities count= " + activities.Count);
                foreach (Activity a in BF.Activities)
                {
                    int count = a.Acts.Count;
                }
            }
            MainDataGrid.ItemsSource = BFs;
            Log("BFs count=" + BFs.Count);
        }

        private void GetEnvironments()
        {
            ObservableList<ProjEnvironment> envs = mSolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
            MainDataGrid.ItemsSource = envs;
        }

        private void GetBFsKeepRef()
        {
            // demo real usage of cache when items have ref
            ObservableList<BusinessFlow> BFs = mSolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            MainDataGrid.ItemsSource = BFs;
            Log("BFs count=" + BFs.Count);
        }



        private void GetAllFiles()
        {
            IEnumerable<RepositoryFile> files = mSolutionRepository.GetAllSolutionRepositoryFiles().ToList();
            MainDataGrid.ItemsSource = files;
            Log("Files count = " + files.Count());
        }

        private void GetBFs()
        {
            ObservableList<BusinessFlow> BFs = mSolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            MainDataGrid.ItemsSource = BFs;
            Log("BFs count=" + BFs.Count);
        }

        private void Log(string txt, System.Diagnostics.Stopwatch st = null)
        {
            LogTextBox.Dispatcher.Invoke(() =>
                {
                    string s = DateTime.Now.ToString() + " " + txt;
                    if (st != null)
                    {
                        ElapsedLabel.Visibility = Visibility.Visible;
                        ElapsedLabel.Content = st.ElapsedMilliseconds;
                    }
                    s += Environment.NewLine;

                    LogTextBox.AppendText(s);

                    // refresh every 100 ms - speed - not for every log write
                    LogTextBox.ScrollToEnd();
                    GingerCore.General.DoEvents();
                }
            );
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        void Clear()
        {
            MainDataGrid.ItemsSource = null;
            LogTextBox.Text = "";
            ElapsedLabel.Visibility = Visibility.Collapsed;
            MemKBLabel.Visibility = Visibility.Collapsed;
        }

        private void ActionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ExecuteSelectedAction();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSelectedAction();
        }

        private void ExecuteSelectedAction()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                GridDoubleClick = null;
                Clear();
                GingerCore.General.DoEvents();

                MyAction a = (MyAction)ActionsListBox.SelectedItem;
                long StartBytes = System.GC.GetTotalMemory(true);
                Log("Running: " + a.Name);

                System.Diagnostics.Stopwatch st = System.Diagnostics.Stopwatch.StartNew();
                a.Action.Invoke();
                st.Stop();

                Log("Done: Elapsed=" + st.Elapsed);

                long StopBytes = System.GC.GetTotalMemory(true);
                long MemUsedBytes = StopBytes - StartBytes;

                double memkb = (double)(MemUsedBytes / (double)1000);
                MemKBLabel.Content = memkb.ToString("N2");
                ElapsedLabel.Content = st.ElapsedMilliseconds;
                TotalmemeoryLabel.Content = (StopBytes / 1000000).ToString("N0");

                ElapsedLabel.Visibility = Visibility.Visible;
                MemKBLabel.Visibility = Visibility.Visible;

                Mouse.OverrideCursor = null;
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                throw ex;
            }
        }
        private void TestCrash()
        {
            ThreadStart newThreadStart = new ThreadStart(newThread_Execute);
            Thread newThread = new Thread(newThreadStart);
            newThread.Start();
        }
        void newThread_Execute()
        {
            throw new Exception("Thread crash");
        }
        private void LongPathTest()
        {
            io.testPath();
        }


        bool RunCrazy = false;
        private void CrazyRandomRun_Click(object sender, RoutedEventArgs e)
        {
            RunCrazy = true;
            //TODO: add stop button on screen
            while (RunCrazy)
            {
                Random rnd = new Random();
                int i = rnd.Next(0, Actions.Count);
                ActionsListBox.SelectedItem = Actions[i];
                GingerCore.General.DoEvents();
                ExecuteSelectedAction();

                int sleep = rnd.Next(0, 10);
                Thread.Sleep(sleep * 1000);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RunCrazy = false;
        }
    }
}