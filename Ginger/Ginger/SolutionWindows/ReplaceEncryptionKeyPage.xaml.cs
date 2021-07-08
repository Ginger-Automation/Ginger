using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Functionalities;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Environments;
using GingerCore.GeneralLib;
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

        ImageMakerControl loaderElement;

        bool validKeyAdded = false;
        public ReplaceEncryptionKeyPage()
        {
            InitializeComponent();
            UCEncryptionKeyPrevious.ChangeLabel("Solution Passwords Old Encryption Key");
            UCEncryptionKey.ChangeLabel("Solution Password New Encryption Key");

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

            defView.GridColsView.Add(new GridColView() { Field = nameof(GingerCore.Variables.VariablePasswordString.ParentType), WidthWeight = 5, Header = "Item Type", ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(GingerCore.Variables.VariablePasswordString.Name), WidthWeight = 8, Header = "Item Name", ReadOnly = true });
            defView.GridColsView.Add(new GridColView() { Field = nameof(GingerCore.Variables.VariablePasswordString.ParentName), WidthWeight = 7, Header = "Parent Name", ReadOnly = true });
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

            if (!string.IsNullOrEmpty(solution.EncryptionKey))
            {
                UCEncryptionKeyPrevious.EncryptionKeyPasswordBox.Password = solution.EncryptionKey;
                if (!UCEncryptionKeyPrevious.ValidateKey())
                {
                    UCEncryptionKeyPrevious.EncryptionKeyPasswordBox.Password = "";
                }
            }

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

            loaderElement = new ImageMakerControl();
            loaderElement.Name = "xProcessingImage";
            loaderElement.Height = 30;
            loaderElement.Width = 30;
            loaderElement.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing;
            loaderElement.Visibility = Visibility.Collapsed;

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Replace/Forget Encryption key", this, winButtons, false, "Cancel", CloseBtn_Click, false, loaderElement);
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
            else
            {
                Reporter.ToUser(eUserMsgKey.ShowInfoMessage, "Please populate all Values in grid. Or Click Ok.");
            }
        }

        private async void SaveKeyBtn_Click(object sender, RoutedEventArgs e)
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
                await InitGrid();
            }
            else if (ReplaceRadioBtn.IsChecked.Value && UCEncryptionKeyPrevious.ValidateKey() && UCEncryptionKey.CheckKeyCombination())
            {
                this.ShowLoader();
                _solution.EncryptionKey = UCEncryptionKey.EncryptionKeyPasswordBox.Password;
                _solution.SaveEncryptionKey();
                _solution.SaveSolution(false);
                if (WorkSpace.Instance.SolutionRepository == null)
                {
                    WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
                    WorkSpace.Instance.SolutionRepository.Open(_solution.ContainingFolderFullPath);
                    WorkSpace.Instance.Solution = _solution;
                }
                int varReencryptedCount = await WorkSpace.Instance.ReEncryptVariable(UCEncryptionKeyPrevious.EncryptionKeyPasswordBox.Password);
                if (varReencryptedCount > 0)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, varReencryptedCount + " Variables Re-encrypted using new Encryption key across Solution.\n Please check in all changes to source control");
                }

                variablesGrid.Visibility = Visibility.Collapsed;
                validKeyAdded = true;
                this.HideLoader();
                _pageGenericWin.Close();
            }
        }

        private void ShowLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                loaderElement.Visibility = Visibility.Visible;
                uOkBtn.IsEnabled = false;
            });
        }

        private void HideLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                loaderElement.Visibility = Visibility.Collapsed;
                uOkBtn.IsEnabled = true;
            });
        }

        private async Task InitGrid()
        {
            ObservableList<GingerCore.Variables.VariablePasswordString> variables = new ObservableList<GingerCore.Variables.VariablePasswordString>();
            await Task.Run(() =>
            {
                this.ShowLoader();
                try
                {
                    if (WorkSpace.Instance.SolutionRepository == null)//?why this will be null ?????
                    {
                        WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
                        WorkSpace.Instance.SolutionRepository.Open(_solution.ContainingFolderFullPath);
                    }

                    // For BF and Activity
                    List<BusinessFlow> Bfs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>()?.ToList();
                    Parallel.ForEach(Bfs, Bf =>
                    {
                        foreach (GingerCore.Variables.VariablePasswordString item in Bf.GetBFandActivitiesVariabeles(true).Where(f => f is GingerCore.Variables.VariablePasswordString))
                        {
                            item.Password = "";
                            variables.Add(item);
                        }
                    });

                    //for Golbal Variables 
                    foreach (GingerCore.Variables.VariablePasswordString v in _solution.Variables.Where(f => f is GingerCore.Variables.VariablePasswordString))
                    {
                        v.Password = "";
                        v.ParentType = string.IsNullOrEmpty(v.ParentType) ? "Global Variable" : v.ParentType;
                        variables.Add(v);
                    }

                    //For Project environments
                    List<ProjEnvironment> projEnvironments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().ToList();
                    projEnvironments.ForEach(pe =>
                    {
                        GingerCore.Variables.VariablePasswordString vp;
                        foreach (EnvApplication ea in pe.Applications)
                        {
                            foreach (Database db in ea.Dbs)
                            {
                                if (!string.IsNullOrEmpty(db.Pass))
                                {
                                    vp = new GingerCore.Variables.VariablePasswordString();
                                    vp.Name = db.Name;
                                    vp.Password = "";
                                    vp.ParentType = "Database Password";
                                    vp.ParentName = ea.Name;
                                    vp.Guid = db.Guid;
                                    variables.Add(vp);
                                }
                            }
                            foreach (GeneralParam gp in ea.GeneralParams.Where(f => f.Encrypt))
                            {
                                vp = new GingerCore.Variables.VariablePasswordString();
                                vp.Name = gp.Name;
                                vp.Password = "";
                                vp.ParentType = "Environment Param";
                                vp.ParentName = ea.Name;
                                vp.Guid = gp.Guid;
                                variables.Add(vp);
                            }
                        }
                    });

                    //For Shared Variales
                    List<GingerCore.Variables.VariableBase> sharedRepoVarsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerCore.Variables.VariableBase>().Where(f => f is GingerCore.Variables.VariablePasswordString).ToList();
                    Parallel.ForEach(sharedRepoVarsList, sharedVar =>
                    {
                        ((GingerCore.Variables.VariablePasswordString)sharedVar).Password = "";
                        sharedVar.ParentType = "Shared Variable";
                        variables.Add((GingerCore.Variables.VariablePasswordString)sharedVar);
                    });

                    //For Shared Activites
                    List<Activity> sharedActivityList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>().ToList();
                    Parallel.ForEach(sharedActivityList, sharedAct =>
                    {
                        List<GingerCore.Variables.VariableBase> sharedActivityVariables = sharedAct.Variables?.Where(f => f is GingerCore.Variables.VariablePasswordString).ToList();
                        foreach (GingerCore.Variables.VariablePasswordString v in sharedActivityVariables)
                        {
                            v.Password = "";
                            v.ParentType = "Shared Activity";
                            variables.Add(v);
                        }
                    });

                    //For Email passwords
                    var runSetConfigs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                    foreach (var rsc in runSetConfigs)
                    {
                        GingerCore.Variables.VariablePasswordString vp;
                        foreach (var ra in rsc.RunSetActions)
                        {
                            try
                            {
                                if (ra is RunSetActionHTMLReportSendEmail)
                                {
                                    if (((RunSetActionHTMLReportSendEmail)ra).Email != null && !string.IsNullOrEmpty(((RunSetActionHTMLReportSendEmail)ra).Email.SMTPPass))
                                    {
                                        vp = new GingerCore.Variables.VariablePasswordString();
                                        vp.Name = ra.ItemName;
                                        vp.Password = "";
                                        vp.ParentType = "Run Set Actions";
                                        vp.ParentName = rsc.Name;
                                        vp.Guid = ((RunSetActionHTMLReportSendEmail)ra).Email.Guid;
                                        variables.Add(vp);
                                    }
                                }
                                else if (ra is RunSetActionSendFreeEmail)
                                {
                                    if (((RunSetActionSendFreeEmail)ra).Email != null && !string.IsNullOrEmpty(((RunSetActionSendFreeEmail)ra).Email.SMTPPass))
                                    {
                                        vp = new GingerCore.Variables.VariablePasswordString();
                                        vp.Name = ra.ItemName;
                                        vp.Password = "";
                                        vp.ParentType = "Run Set Actions";
                                        vp.ParentName = rsc.Name;
                                        vp.Guid = ((RunSetActionSendFreeEmail)ra).Email.Guid;
                                        variables.Add(vp);
                                    }
                                }
                                else if (ra is RunSetActionSendSMS)
                                {
                                    if (((RunSetActionSendSMS)ra).SMSEmail != null && !string.IsNullOrEmpty(((RunSetActionSendSMS)ra).SMSEmail.SMTPPass))
                                    {
                                        vp = new GingerCore.Variables.VariablePasswordString();
                                        vp.Name = ra.ItemName;
                                        vp.Password = "";
                                        vp.ParentType = "Run Set Actions";
                                        vp.ParentName = rsc.Name;
                                        vp.Guid = ((RunSetActionSendSMS)ra).SMSEmail.Guid;
                                        variables.Add(vp);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.WARN, "Error while Retrieving encrypted SMTP password for " + rsc.Name, ex); throw;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Retrieving encrypted variables for setting new value", ex);
                }
                finally
                {
                    this.HideLoader();
                }
            });

            if (!variables.Any())
            {
                _pageGenericWin.Close();
            }
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

        private async void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    this.ShowLoader();
                    EncryptGridValues();

                    // For BF and Activity
                    List<BusinessFlow> Bfs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().ToList();
                    Parallel.ForEach(Bfs, Bf =>
                    {
                        try
                        {
                            if (Bf.GetBFandActivitiesVariabeles(false).Where(f => f is GingerCore.Variables.VariablePasswordString).Any())
                            {
                                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(Bf);
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Replace Key Page: Error while encrypting variable password of " + Bf.Name, ex);
                        }
                    });

                    // For Global Variables
                    foreach (GingerCore.Variables.VariableBase v in WorkSpace.Instance.Solution.Variables.Where(f => f is GingerCore.Variables.VariablePasswordString))
                    {
                        try
                        {
                            WorkSpace.Instance.Solution.SaveSolution(false);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, string.Format("ReEncryptVariable- Failed to Reencrypt password Global variable {1}", v.Name), ex);
                        }
                    }

                    //For Environment parameters
                    List<ProjEnvironment> projEnvironments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().ToList();
                    projEnvironments.ForEach(pe =>
                    {
                        try
                        {
                            bool res1 = false;
                            foreach (EnvApplication ea in pe.Applications)
                            {
                                foreach (GeneralParam gp in ea.GeneralParams.Where(f => f.Encrypt))
                                {
                                    gp.Value = ((ObservableList<GingerCore.Variables.VariablePasswordString>)variablesGrid.DataSourceList).Where(f => f.Guid.Equals(gp.Guid)).FirstOrDefault().Password;
                                    res1 = true;
                                }

                                foreach (Database db in ea.Dbs)
                                {
                                    if (!string.IsNullOrEmpty(db.Pass))
                                    {
                                        db.Pass = ((ObservableList<GingerCore.Variables.VariablePasswordString>)variablesGrid.DataSourceList).Where(f => f.Guid.Equals(db.Guid)).FirstOrDefault().Password;
                                        res1 = true;
                                    }
                                }
                            }

                            if (res1)
                            {
                                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(pe);
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Replace Key Page: Error while encrypting Environment parameter password of " + pe.Name, ex);
                        }
                    });

                    //For shared variables
                    List<GingerCore.Variables.VariableBase> sharedRepoVarsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerCore.Variables.VariableBase>().Where(f => f is GingerCore.Variables.VariablePasswordString).ToList();
                    foreach (GingerCore.Variables.VariableBase sharedVar in sharedRepoVarsList)
                    {
                        try
                        {
                            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(sharedVar);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Replace Key Page: Error while encrypting Shared variable password  " + sharedVar.Name, ex);
                        }
                    }

                    //For Shared Activites
                    List<Activity> sharedActivityList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>().ToList();
                    foreach (Activity sharedAct in sharedActivityList)
                    {
                        try
                        {
                            if (sharedAct.Variables.Where(f => f is GingerCore.Variables.VariablePasswordString).Any())
                            {
                                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(sharedAct);
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Replace Key Page: Error while encrypting Shared variable password  " + sharedAct.ActivityName, ex);
                        }
                    }

                    //Email Passwords
                    var runSetConfigs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                    bool res = false;
                    foreach (var rsc in runSetConfigs)
                    {
                        try
                        {
                            res = false;
                            foreach (var ra in rsc.RunSetActions)
                            {
                                if (ra is RunSetActionHTMLReportSendEmail && ((RunSetActionHTMLReportSendEmail)ra).Email != null
                                && !string.IsNullOrEmpty(((RunSetActionHTMLReportSendEmail)ra).Email.SMTPPass))
                                {
                                    ((RunSetActionHTMLReportSendEmail)ra).Email.SMTPPass = ((ObservableList<GingerCore.Variables.VariablePasswordString>)variablesGrid.DataSourceList)
                                    .Where(f => f.Guid.Equals(((RunSetActionHTMLReportSendEmail)ra).Email.Guid)).FirstOrDefault().Password;
                                    res = true;
                                }
                                else if (ra is RunSetActionSendFreeEmail && ((RunSetActionSendFreeEmail)ra).Email != null
                                && !string.IsNullOrEmpty(((RunSetActionSendFreeEmail)ra).Email.SMTPPass))
                                {
                                    ((RunSetActionSendFreeEmail)ra).Email.SMTPPass = ((ObservableList<GingerCore.Variables.VariablePasswordString>)variablesGrid.DataSourceList)
                                    .Where(f => f.Guid.Equals(((RunSetActionSendFreeEmail)ra).Email.Guid)).FirstOrDefault().Password;
                                    res = true;
                                }
                                else if (ra is RunSetActionSendSMS && ((RunSetActionSendSMS)ra).SMSEmail != null
                                    && !string.IsNullOrEmpty(((RunSetActionSendSMS)ra).SMSEmail.SMTPPass))
                                {
                                    ((RunSetActionSendSMS)ra).SMSEmail.SMTPPass = ((ObservableList<GingerCore.Variables.VariablePasswordString>)variablesGrid.DataSourceList)
                                    .Where(f => f.Guid.Equals(((RunSetActionSendSMS)ra).SMSEmail.Guid)).FirstOrDefault().Password;
                                    res = true;
                                }
                            }
                            if (res)
                            {
                                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(rsc);
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Replace Key Page: Error while encrypting Email SMTP password of " + rsc.Name, ex);
                        }
                    }

                    _solution.SaveSolution(false);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error while in Encrypting variables passwords", ex);
                }
                finally
                {
                    this.HideLoader();
                }
            });

            _pageGenericWin.Close();
        }

        private void radioBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ForgetRadioBtn.IsChecked.Value)
            {
                xDescriptionLabel.Content = "Setting a new key will clear all encrypted values and list of encrypted variables will be shown to set a new values. " +
                    "\nAlso the new encryption key needs to be updated on all integrations like Jenkins, Bamboo, eTDM etc." +
                    "\nEnsure to make a note of new Key.";
                UCEncryptionKeyPrevious.Visibility = Visibility.Collapsed;

                UCEncryptionKey.Visibility = Visibility.Visible;
                //UCEncryptionKey.Validate.Visibility = Visibility.Hidden;
            }
            else if (ReplaceRadioBtn.IsChecked.Value)
            {

                xDescriptionLabel.Content = "Replacing a key will reencrypt all the encrypted values with a new key." +
                    "\nAlso the new key needs to be updated on all integrations like Jenkins, Bamboo, eTDM etc." +
                    "\nEnsure to make a note of new Key.";
                UCEncryptionKeyPrevious.Visibility = Visibility.Visible;
                UCEncryptionKey.Visibility = Visibility.Visible;

                UCEncryptionKeyPrevious.Visibility = Visibility.Visible;
                UCEncryptionKeyPrevious.ValidFlag.Visibility = Visibility.Collapsed;
                //UCEncryptionKeyPrevious.InvalidFlag.Visibility = Visibility.Collapsed;
                //UCEncryptionKeyPrevious.Validate.Visibility = Visibility.Visible;
            }
        }
    }
}
