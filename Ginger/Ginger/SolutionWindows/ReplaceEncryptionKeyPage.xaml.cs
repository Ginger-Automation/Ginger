using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for ReplaceEncryptionKeyPage.xaml
    /// </summary>
    public partial class ReplaceEncryptionKeyPage : Page
    {
        GenericWindow _pageGenericWin = null;
        Solution _solution = null;        
        Button uSaveKeyBtn, uCloseBtn, uOkBtn;

        bool validKeyAdded = false;
        public ReplaceEncryptionKeyPage()
        {
            InitializeComponent();
            UCEncryptionKeyPrevious.ChangeLabel("Old Encryption Key");
            UCEncryptionKey.ChangeLabel("New Encryption Key");

            ReplaceRadioBtn.Click += radioBtn_Click;
            ForgetRadioBtn.Click += radioBtn_Click;

            variablesGrid.btnMarkAll.Visibility = Visibility.Collapsed;
            variablesGrid.Visibility = Visibility.Collapsed;
            UCEncryptionKeyPrevious.Visibility = Visibility.Collapsed;
            UCEncryptionKey.Visibility = Visibility.Collapsed;

            UCEncryptionKeyPrevious.EncryptionKeyPasswordBox.PasswordChanged += PrevEncryptionKeyBox_Changed;
            UCEncryptionKey.EncryptionKeyPasswordBox.PasswordChanged += EncryptionKeyBox_Changed;
        }

        private void SetGridsView()
        {
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(GingerCore.Variables.VariablePasswordString.Name), WidthWeight = 10, Header = GingerDicser.GetTermResValue(eTermResKey.Variable) + " Name", ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(GingerCore.Variables.VariablePasswordString.ParentType), WidthWeight = 10, Header = GingerDicser.GetTermResValue(eTermResKey.Variable) + "  Type", ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(GingerCore.Variables.VariablePasswordString.Password), WidthWeight = 10, Header = "Value" });
            variablesGrid.SetAllColumnsDefaultView(defView);
            variablesGrid.InitViewItems();
            variablesGrid.SetTitleLightStyle = true;
        }

        public bool ShowAsWindow(Solution solution, eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            UCEncryptionKey.mSolution = solution;
            UCEncryptionKeyPrevious.mSolution = solution;
            _solution = solution;

            ObservableList<Button> winButtons = new ObservableList<Button>();
            uOkBtn = new Button();
            uOkBtn.Content = "Ok";
            uOkBtn.Click += new RoutedEventHandler(OkBtn_Click);
            uOkBtn.Visibility = Visibility.Collapsed;
            winButtons.Add(uOkBtn);
            uSaveKeyBtn = new Button();
            uSaveKeyBtn.Content = "Save Key";
            uSaveKeyBtn.Click += new RoutedEventHandler(SaveKeyBtn_Click);
            winButtons.Add(uSaveKeyBtn);
            uCloseBtn = new Button();
            uCloseBtn.Content = "Cancel";
            uCloseBtn.Click += new RoutedEventHandler(CloseBtn_Click);
            winButtons.Add(uCloseBtn);


            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Replace/Forget Encryption key", this, winButtons, false, "Cancel", CloseBtn_Click);
            return validKeyAdded;
        }

        private async void PrevEncryptionKeyBox_Changed(object sender, RoutedEventArgs e)
        {
            //this inner method checks if user is still typing
            async Task<bool> UserKeepsTyping()
            {
                string txt = UCEncryptionKeyPrevious.EncryptionKeyPasswordBox.Password;
                await Task.Delay(2000);
                return txt != UCEncryptionKeyPrevious.EncryptionKeyPasswordBox.Password;
            }
            if (await UserKeepsTyping()) { return; }

            UCEncryptionKeyPrevious.ValidateKey();
        }

        private async void EncryptionKeyBox_Changed(object sender, RoutedEventArgs e)
        {
            //this inner method checks if user is still typing
            async Task<bool> UserKeepsTyping()
            {
                string txt = UCEncryptionKey.EncryptionKeyPasswordBox.Password;
                await Task.Delay(2000);
                return txt != UCEncryptionKey.EncryptionKeyPasswordBox.Password;
            }
            if (await UserKeepsTyping()) { return; }

            UCEncryptionKey.CheckKeyCombination();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ForgetRadioBtn.IsEnabled)
            {
                _pageGenericWin.Close();
            }
            else {
                Reporter.ToUser(eUserMsgKey.ShowInfoMessage,"Please populate all Values in grid.");
            }
        }

        private void SaveKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ForgetRadioBtn.IsChecked.Value && UCEncryptionKey.CheckKeyCombination())
            {
                _solution.EncryptionKey = UCEncryptionKey.EncryptionKeyPasswordBox.Password;
                _solution.SaveEncryptionKey();
                _solution.SaveSolution(false);

                uOkBtn.Visibility = Visibility.Visible;
                uSaveKeyBtn.IsEnabled = false;
                ForgetRadioBtn.IsEnabled = false;
                ReplaceRadioBtn.IsEnabled = false;
                UCEncryptionKey.IsEnabled = false;
                validKeyAdded = true;
                InitGrid();
            }
            else if (ReplaceRadioBtn.IsChecked.Value && UCEncryptionKeyPrevious.ValidateKey() && UCEncryptionKey.CheckKeyCombination())
            {
                _solution.EncryptionKey = UCEncryptionKey.EncryptionKeyPasswordBox.Password;
                _solution.SaveEncryptionKey();
                _solution.SaveSolution(false);
                if (WorkSpace.Instance.SolutionRepository == null)
                {
                    WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
                    WorkSpace.Instance.SolutionRepository.Open(_solution.ContainingFolderFullPath);
                    WorkSpace.Instance.Solution = _solution;
                }
                WorkSpace.ReEncryptVariable(UCEncryptionKeyPrevious.EncryptionKeyPasswordBox.Password);
                variablesGrid.Visibility = Visibility.Collapsed;
                validKeyAdded = true;
                _pageGenericWin.Close();
            }
        }

        private void InitGrid()
        {
            if (WorkSpace.Instance.SolutionRepository == null)
            {
                WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
                WorkSpace.Instance.SolutionRepository.Open(_solution.ContainingFolderFullPath);
            }
            ObservableList<GingerCore.Variables.VariablePasswordString> variables = new ObservableList<GingerCore.Variables.VariablePasswordString>();
            List<BusinessFlow> Bfs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().ToList();
            // For BF and Activity
            Parallel.ForEach(Bfs, Bf =>
            {
                bool res = false;
                foreach (GingerCore.Variables.VariablePasswordString item in Bf.GetBFandActivitiesVariabeles(true).Where(f => f is GingerCore.Variables.VariablePasswordString))
                {
                    item.Password = "";
                    variables.Add(item);
                }
            });

            foreach (GingerCore.Variables.VariablePasswordString v in _solution.Variables.Where(f => f is GingerCore.Variables.VariablePasswordString))
            {
                v.Password = "";
                v.ParentType = string.IsNullOrEmpty(v.ParentType) ? "Solution Variables" : v.ParentType;
                variables.Add(v);
            }

            List<ProjEnvironment> projEnvironments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().ToList();
            projEnvironments.ForEach(pe =>
            {
                bool res1 = false;
                GingerCore.Variables.VariablePasswordString vp;
                foreach (EnvApplication ea in pe.Applications)
                {
                    foreach (GeneralParam gp in ea.GeneralParams.Where(f => f.Encrypt))
                    {
                        vp = new GingerCore.Variables.VariablePasswordString();
                        vp.Name = gp.Name;
                        vp.Password = "";
                        vp.ParentType = "Environment Variables";
                        vp.Guid = gp.Guid;
                        variables.Add(vp);
                    }
                }
            });

            SetGridsView();

            variablesGrid.DataSourceList = variables;
            variablesGrid.RowChangedEvent += grdGroups_RowChangedEvent;
            variablesGrid.Title = "List of Password " + GingerDicser.GetTermResValue(eTermResKey.Variable);

            variablesGrid.Visibility = Visibility.Visible;
        }

        private void grdGroups_RowChangedEvent(object sender, EventArgs e)
        {
            EncryptGridValues();
        }

        public void EncryptGridValues()
        {
            foreach (GingerCore.Variables.VariablePasswordString vp in variablesGrid.DataSourceList)
            {
                if (!string.IsNullOrEmpty(vp.Password) && !EncryptionHandler.IsStringEncrypted(vp.Password))
                {
                    vp.Password = EncryptionHandler.EncryptwithKey(vp.Password);
                }
            }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            EncryptGridValues();
            List<BusinessFlow> Bfs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().ToList();
            // For BF and Activity
            Parallel.ForEach(Bfs, Bf =>
            {
                if (Bf.GetBFandActivitiesVariabeles(false).Where(f => f is GingerCore.Variables.VariablePasswordString).Any())
                {
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(Bf);
                }
            });


            List<ProjEnvironment> projEnvironments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().ToList();
            projEnvironments.ForEach(pe =>
            {
                bool res1 = false;
                foreach (EnvApplication ea in pe.Applications)
                {
                    foreach (GeneralParam gp in ea.GeneralParams.Where(f => f.Encrypt))
                    {
                        gp.Value = ((ObservableList<GingerCore.Variables.VariablePasswordString>)variablesGrid.DataSourceList).Where(f => f.Guid.Equals(gp.Guid)).FirstOrDefault().Password;
                        res1 = true;
                    }
                }

                if (res1)
                {
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(pe);
                }
            });

            _solution.SaveSolution(false);
            _pageGenericWin.Close();
        }

        private void radioBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ForgetRadioBtn.IsChecked.Value)
            {
                UCEncryptionKeyPrevious.Visibility = Visibility.Collapsed;

                UCEncryptionKey.Visibility = Visibility.Visible;
                UCEncryptionKey.Validate.Visibility = Visibility.Hidden;
            }
            else if (ReplaceRadioBtn.IsChecked.Value)
            {
                UCEncryptionKeyPrevious.Visibility = Visibility.Visible;
                UCEncryptionKey.Visibility = Visibility.Visible;

                UCEncryptionKeyPrevious.Visibility = Visibility.Visible;
                UCEncryptionKeyPrevious.ValidFlag.Visibility = Visibility.Collapsed;
                UCEncryptionKeyPrevious.InvalidFlag.Visibility = Visibility.Visible;
                UCEncryptionKeyPrevious.Validate.Visibility = Visibility.Visible;
            }
        }
    }
}
