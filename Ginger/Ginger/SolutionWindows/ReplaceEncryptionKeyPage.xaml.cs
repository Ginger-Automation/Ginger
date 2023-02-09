#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using Ginger.SolutionGeneral;
using Ginger.UserControls;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

            xReplaceRadioButton.Click += radioBtn_Click;
            xForgetRadioButton.Click += radioBtn_Click;

            xSolutionPasswordsParamtersGrid.btnMarkAll.Visibility = Visibility.Collapsed;
            xSolutionPasswordsParamtersGrid.Visibility = Visibility.Collapsed;
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
            xSolutionPasswordsParamtersGrid.SetAllColumnsDefaultView(defView);
            xSolutionPasswordsParamtersGrid.InitViewItems();
            xSolutionPasswordsParamtersGrid.SetTitleLightStyle = true;
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

            uSaveKeyBtn = new Button();
            uSaveKeyBtn.Content = "Save Key";
            uSaveKeyBtn.Click += new RoutedEventHandler(SaveKeyBtn_Click);

            uCloseBtn = new Button();
            uCloseBtn.Content = "Cancel";
            uCloseBtn.Click += new RoutedEventHandler(CloseBtn_Click);
            winButtons.Add(uCloseBtn);
            winButtons.Add(uSaveKeyBtn);
            winButtons.Add(uOkBtn);
            loaderElement = new ImageMakerControl();
            loaderElement.Name = "xProcessingImage";
            loaderElement.Height = 30;
            loaderElement.Width = 30;
            loaderElement.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Processing;
            loaderElement.Visibility = Visibility.Collapsed;

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "Replace/Forget Encryption key", this, winButtons, false, "Cancel", CloseBtn_Click, false, loaderElement);
            return validKeyAdded;
        }

        private void radioBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xForgetRadioButton.IsChecked.Value)
            {
                xDescriptionLabel.Content = "Setting a new key will clear all encrypted values and list of encrypted variables will be shown to set a new values. " +
                    "\nAlso the new encryption key needs to be updated on all integrations like Jenkins, Bamboo, eTDM etc." +
                    "\nEnsure to make a note of new Key.";
                UCEncryptionKeyPrevious.Visibility = Visibility.Collapsed;

                UCEncryptionKey.Visibility = Visibility.Visible;
                //UCEncryptionKey.Validate.Visibility = Visibility.Hidden;
            }
            else if (xReplaceRadioButton.IsChecked.Value)
            {

                xDescriptionLabel.Content = "Replacing a key will re-encrypt all the encrypted values with a new key." +
                    "\nAlso the new key needs to be updated on all integrations like Jenkins, Bamboo, eTDM etc." +
                    "\nEnsure to make a note of new Key.";
                UCEncryptionKeyPrevious.Visibility = Visibility.Visible;
                UCEncryptionKey.Visibility = Visibility.Visible;

                UCEncryptionKeyPrevious.Visibility = Visibility.Visible;
                UCEncryptionKeyPrevious.ValidFlag.Visibility = Visibility.Collapsed;
            }
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

        private void ShowStatusMessage(string message)
        {
            xStatusLabel.Dispatcher.Invoke(() =>
            {
                xStatusLabel.Visibility = Visibility.Visible;
                xStatusLabel.Content = message;
            });
        }
        private void HideStatusMessage()
        {
            xStatusLabel.Dispatcher.Invoke(() =>
            {
                xStatusLabel.Visibility = Visibility.Collapsed;
                xStatusLabel.Content = "";
            });
        }


        private void grdGroups_RowChangedEvent(object sender, EventArgs e)
        {
            EncryptGridValues();
        }

        public void EncryptGridValues()
        {
            foreach (GingerCore.Variables.VariablePasswordString vp in xSolutionPasswordsParamtersGrid.DataSourceList)
            {
                if (!string.IsNullOrEmpty(vp.Password) && !EncryptionHandler.IsStringEncrypted(vp.Password))
                {
                    vp.Password = EncryptionHandler.EncryptwithKey(vp.Password);
                }
            }
        }


        private async void SaveKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xForgetRadioButton.IsChecked.Value && UCEncryptionKey.CheckKeyCombination())
            {
                if (Reporter.ToUser(eUserMsgKey.ForgotKeySaveChanges) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    _solution.EncryptionKey = UCEncryptionKey.EncryptionKeyPasswordBox.Password;
                    _solution.SolutionOperations.SaveEncryptionKey();
                    _solution.SolutionOperations.SaveSolution(false);

                    uOkBtn.Visibility = Visibility.Visible;
                    uSaveKeyBtn.Visibility = Visibility.Collapsed;
                    uCloseBtn.Visibility = Visibility.Collapsed;

                    xForgetRadioButton.IsEnabled = false;
                    xReplaceRadioButton.IsEnabled = false;
                    UCEncryptionKey.IsEnabled = false;
                    validKeyAdded = true;
                    await LoadEncryptedParamtersList();
                }


            }
            else if (xReplaceRadioButton.IsChecked.Value && UCEncryptionKeyPrevious.ValidateKey() && UCEncryptionKey.CheckKeyCombination())
            {
                this.ShowLoader();
                ShowStatusMessage("Updating new encryption key for solution");
                uSaveKeyBtn.Visibility = Visibility.Collapsed;
                uCloseBtn.Visibility = Visibility.Collapsed;
                _solution.EncryptionKey = UCEncryptionKey.EncryptionKeyPasswordBox.Password;
                _solution.SolutionOperations.SaveEncryptionKey();
                _solution.SolutionOperations.SaveSolution(false);
                if (WorkSpace.Instance.SolutionRepository == null)
                {
                    WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
                    WorkSpace.Instance.SolutionRepository.Open(_solution.ContainingFolderFullPath);
                    WorkSpace.Instance.Solution = _solution;
                }
                int varReencryptedCount = await HandlePasswordValuesReEncryption(UCEncryptionKeyPrevious.EncryptionKeyPasswordBox.Password);
                this.HideLoader();
                HideStatusMessage();
                if (varReencryptedCount > 0)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, varReencryptedCount + " Variables Re-encrypted using new Encryption key across Solution.\n Please check in all changes to source control");
                }

                xSolutionPasswordsParamtersGrid.Visibility = Visibility.Collapsed;
                validKeyAdded = true;

                _pageGenericWin.Close();
            }
        }

        private async void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {
                    this.ShowLoader();
                    ShowStatusMessage("Saving the new password values for parameters...");
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
                            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(false);
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
                                    gp.Value = ((ObservableList<GingerCore.Variables.VariablePasswordString>)xSolutionPasswordsParamtersGrid.DataSourceList).Where(f => f.Guid.Equals(gp.Guid)).FirstOrDefault().Password;
                                    res1 = true;
                                }

                                foreach (Database db in ea.Dbs)
                                {
                                    if (!string.IsNullOrEmpty(db.Pass))
                                    {
                                        db.Pass = ((ObservableList<GingerCore.Variables.VariablePasswordString>)xSolutionPasswordsParamtersGrid.DataSourceList).Where(f => f.Guid.Equals(db.Guid)).FirstOrDefault().Password;
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
                                    ((RunSetActionHTMLReportSendEmail)ra).Email.SMTPPass = ((ObservableList<GingerCore.Variables.VariablePasswordString>)xSolutionPasswordsParamtersGrid.DataSourceList)
                                    .Where(f => f.Guid.Equals(((RunSetActionHTMLReportSendEmail)ra).Email.Guid)).FirstOrDefault().Password;
                                    res = true;
                                }
                                else if (ra is RunSetActionSendFreeEmail && ((RunSetActionSendFreeEmail)ra).Email != null
                                && !string.IsNullOrEmpty(((RunSetActionSendFreeEmail)ra).Email.SMTPPass))
                                {
                                    ((RunSetActionSendFreeEmail)ra).Email.SMTPPass = ((ObservableList<GingerCore.Variables.VariablePasswordString>)xSolutionPasswordsParamtersGrid.DataSourceList)
                                    .Where(f => f.Guid.Equals(((RunSetActionSendFreeEmail)ra).Email.Guid)).FirstOrDefault().Password;
                                    res = true;
                                }
                                else if (ra is RunSetActionSendSMS && ((RunSetActionSendSMS)ra).SMSEmail != null
                                    && !string.IsNullOrEmpty(((RunSetActionSendSMS)ra).SMSEmail.SMTPPass))
                                {
                                    ((RunSetActionSendSMS)ra).SMSEmail.SMTPPass = ((ObservableList<GingerCore.Variables.VariablePasswordString>)xSolutionPasswordsParamtersGrid.DataSourceList)
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

                    _solution.SolutionOperations.SaveSolution(false);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error while in Encrypting variables passwords", ex);
                }
                finally
                {
                    this.HideLoader();
                    HideStatusMessage();
                }
            });

            _pageGenericWin.Close();
        }


        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            _pageGenericWin.Close();
        }

        private async Task LoadEncryptedParamtersList()
        {
            ObservableList<GingerCore.Variables.VariablePasswordString> variables = new ObservableList<GingerCore.Variables.VariablePasswordString>();
            await Task.Run(() =>
            {
                this.ShowLoader();
                this.ShowStatusMessage("Searching all password values in the solution...");
                try
                {
                    if (WorkSpace.Instance.SolutionRepository == null)
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
                        // GingerCore.Variables.VariablePasswordString vp;
                        foreach (EnvApplication ea in pe.Applications)
                        {
                            foreach (Database db in ea.Dbs)
                            {
                                if (!string.IsNullOrEmpty(db.Pass))
                                {
                                    variables.Add(CreatePasswordVariable(db.Name, "Database Password", pe.Name + "-->" + ea.Name, db.Guid));
                                }
                            }
                            foreach (GeneralParam gp in ea.GeneralParams.Where(f => f.Encrypt))
                            {
                                variables.Add(CreatePasswordVariable(gp.Name, "Environment Parameter", pe.Name + "-->" + ea.Name, gp.Guid));
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
                        //VariablePasswordString vp;
                        foreach (var ra in rsc.RunSetActions)
                        {
                            try
                            {
                                if (ra is RunSetActionHTMLReportSendEmail)
                                {
                                    if (((RunSetActionHTMLReportSendEmail)ra).Email != null && !string.IsNullOrEmpty(((RunSetActionHTMLReportSendEmail)ra).Email.SMTPPass))
                                    {
                                        variables.Add(CreatePasswordVariable(ra.ItemName, "Run Set Operation", rsc.Name, ((RunSetActionHTMLReportSendEmail)ra).Email.Guid));
                                    }
                                }
                                else if (ra is RunSetActionSendFreeEmail)
                                {
                                    if (((RunSetActionSendFreeEmail)ra).Email != null && !string.IsNullOrEmpty(((RunSetActionSendFreeEmail)ra).Email.SMTPPass))
                                    {
                                        variables.Add(CreatePasswordVariable(ra.ItemName, "Run Set Operation", rsc.Name, ((RunSetActionSendFreeEmail)ra).Email.Guid));
                                    }
                                }
                                else if (ra is RunSetActionSendSMS)
                                {
                                    if (((RunSetActionSendSMS)ra).SMSEmail != null && !string.IsNullOrEmpty(((RunSetActionSendSMS)ra).SMSEmail.SMTPPass))
                                    {
                                        variables.Add(CreatePasswordVariable(ra.ItemName, "Run Set Operation", rsc.Name, ((RunSetActionSendSMS)ra).SMSEmail.Guid));
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
                    ShowStatusMessage("Please set the new values for password parameters");
                }
            });

            if (!variables.Any())
            {
                _pageGenericWin.Close();
            }
            SetGridsView();

            xSolutionPasswordsParamtersGrid.DataSourceList = variables;
            xSolutionPasswordsParamtersGrid.RowChangedEvent += grdGroups_RowChangedEvent;
            xSolutionPasswordsParamtersGrid.Title = "List of Passwords/Encrypted Values to update new value";

            xSolutionPasswordsParamtersGrid.Visibility = Visibility.Visible;
        }

        private VariablePasswordString CreatePasswordVariable(string itemName, string parentType, string parentName, Guid guid)
        {
            VariablePasswordString variablePassword = new VariablePasswordString();
            variablePassword.Name = itemName;
            variablePassword.ParentType = parentType;
            variablePassword.ParentName = parentName;
            variablePassword.Guid = guid;
            variablePassword.Password = "";
            return variablePassword;
        }

        public async Task<int> HandlePasswordValuesReEncryption(string oldKey = null)
        {

            return await Task.Run(async () =>
            {
                int varReencryptedCount = 0;
                varReencryptedCount += await ReEncryptBFAndACtivityVariable(oldKey);
                varReencryptedCount += await ReEncryptGlobalVariables(oldKey);
                varReencryptedCount += await ReEncryptEnvironmentPasswordValues(oldKey);
                varReencryptedCount += await ReEncryptRunsetOperationsPassowrdValues(oldKey);
                varReencryptedCount += await ReEncryptSharedRepositoryPasswordValues(oldKey);
                return varReencryptedCount;
            });
        }

        private async Task<int> ReEncryptBFAndACtivityVariable(string oldKey = null)
        {
            ShowStatusMessage("Re-Encrypting Business flow and activity Password variables Values with new Key...");
            return await Task.Run(() =>
            {
                int varReencryptedCount = 0;
                List<BusinessFlow> Bfs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().ToList();
                // For BF and Activity
                Parallel.ForEach(Bfs, Bf =>
               {
                   try
                   {
                       // Get all variables from BF
                       List<GingerCore.Variables.VariableBase> variables = Bf.GetBFandActivitiesVariabeles(false).Where(f => f is GingerCore.Variables.VariablePasswordString).ToList();
                       variables.ForEach(v =>
                       {
                           try
                           {
                               ((GingerCore.Variables.VariablePasswordString)v).Password =
                               EncryptionHandler.ReEncryptString(((GingerCore.Variables.VariablePasswordString)v).Password, oldKey);

                               varReencryptedCount++;
                           }
                           catch (Exception ex)
                           {
                               Reporter.ToLog(eLogLevel.ERROR, string.Format("ReEncryptVariable- Failed to Re-encrypt password variable of {0} for {1}", Bf.Name, v.Name), ex);
                           }
                       });

                       if (variables.Any())
                       {
                           WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(Bf);
                       }

                   }
                   catch (Exception ex)
                   {
                       Reporter.ToLog(eLogLevel.ERROR, string.Format("ReEncryptVariable- Failed to Re encrypt password variable of {0}.", Bf.Name), ex);
                   }
               });
                return varReencryptedCount;
            });

        }

        private async Task<int> ReEncryptGlobalVariables(string oldKey = null)
        {

            ShowStatusMessage("Re-Encrypting Global Password variables Values with new Key...");
            return await Task.Run(() =>
            {
                // For Global Variables
                bool isSaveRequired = false;
                int varReencryptedCount = 0;
                foreach (GingerCore.Variables.VariableBase v in WorkSpace.Instance.Solution.Variables.Where(f => f is GingerCore.Variables.VariablePasswordString))
                {
                    try
                    {
                        ((GingerCore.Variables.VariablePasswordString)v).Password =
                            EncryptionHandler.ReEncryptString(((GingerCore.Variables.VariablePasswordString)v).Password, oldKey);
                        isSaveRequired = true;

                        varReencryptedCount++;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, string.Format("ReEncryptVariable- Failed to Re-encrypt password Global variable {1}", v.Name), ex);
                    }
                }
                if (isSaveRequired)
                {
                    WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(false);
                }
                return varReencryptedCount;
            });
        }

        private async Task<int> ReEncryptEnvironmentPasswordValues(string oldKey = null)
        {
            ShowStatusMessage("Re-Encrypting Environment parameters and DB Password Values with new Key...");
            return await Task.Run(() =>
            {
                bool isSaveRequired = false;
                int varReencryptedCount = 0;

                //For project environment variable
                List<ProjEnvironment> projEnvironments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().ToList();
                projEnvironments.ForEach(pe =>
                {
                    try
                    {
                        isSaveRequired = false;
                        foreach (EnvApplication ea in pe.Applications)
                        {
                            foreach (GeneralParam gp in ea.GeneralParams.Where(f => f.Encrypt))
                            {
                                gp.Value = EncryptionHandler.ReEncryptString(gp.Value, oldKey);
                                isSaveRequired = true;
                                varReencryptedCount++;
                            }
                            foreach (Database db in ea.Dbs)
                            {
                                if (!string.IsNullOrEmpty(db.Pass))
                                {
                                    //if Pass is stored in the form of variable, encryption not required at this stage
                                    if (!db.Pass.Contains("{Var Name") && !db.Pass.Contains("{EnvParam"))
                                    {
                                        string encryptedPassWord = EncryptionHandler.ReEncryptString(db.Pass, oldKey);
                                        if (string.IsNullOrEmpty(encryptedPassWord))
                                        {
                                            encryptedPassWord = EncryptionHandler.EncryptwithKey(db.Pass);
                                        }
                                        db.Pass = encryptedPassWord;
                                    }
                                    isSaveRequired = true;
                                    varReencryptedCount++;
                                }
                            }
                        }
                        if (isSaveRequired)
                        {
                            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(pe);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "ReEncryptVariable- Failed to Re-encrypt password ProjEnvironment variable for " + pe.Name, ex);
                    }
                });

                return varReencryptedCount;
            });
        }

        private async Task<int> ReEncryptSharedRepositoryPasswordValues(string oldKey = null)
        {
            ShowStatusMessage("Re-Encrypting Shared Repository Password variables Values with new Key...");
            return await Task.Run(() =>
            {
                int varReencryptedCount = 0;
                //For Shared Variables
                List<GingerCore.Variables.VariableBase> sharedRepoVarsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerCore.Variables.VariableBase>().Where(f => f is GingerCore.Variables.VariablePasswordString).ToList();
                foreach (var sharedVar in sharedRepoVarsList)
                {
                    try
                    {
                        ((GingerCore.Variables.VariablePasswordString)sharedVar).Password =
                        EncryptionHandler.ReEncryptString(((GingerCore.Variables.VariablePasswordString)sharedVar).Password, oldKey);

                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(sharedVar);

                        varReencryptedCount++;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, string.Format("ReEncryptVariable- Failed to Re-encrypt shared password variable of {0}.", sharedVar.Name), ex);
                    }
                }

                //For Shared Activites
                List<Activity> sharedActivityList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>().ToList();
                foreach (var sharedAct in sharedActivityList)
                {
                    try
                    {
                        List<GingerCore.Variables.VariableBase> variables = sharedAct.Variables.Where(f => f is GingerCore.Variables.VariablePasswordString).ToList();
                        variables.ForEach(v =>
                        {
                            try
                            {
                                ((GingerCore.Variables.VariablePasswordString)v).Password =
                                EncryptionHandler.ReEncryptString(((GingerCore.Variables.VariablePasswordString)v).Password, oldKey);
                                varReencryptedCount++;
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, string.Format("ReEncryptVariable- Failed to Re-encrypt password variable of shared activity {0} for {1}", sharedAct.ActivityName, v.Name), ex);
                            }
                        });

                        if (variables.Any())
                        {
                            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(sharedAct);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, string.Format("ReEncryptVariable- Failed to update shared activity {0}.", sharedAct.ActivityName), ex);
                    }
                }

                return varReencryptedCount;
            });
        }

        private async Task<int> ReEncryptRunsetOperationsPassowrdValues(string oldKey = null)
        {
            ShowStatusMessage("Re-Encrypting Runset Operations SMTP Password Values with new Key...");
            return await Task.Run(() =>
            {
                bool isSaveRequired = false;
                int varReencryptedCount = 0;
                //Email Passwords
                var runSetConfigs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();

                foreach (var rsc in runSetConfigs)
                {
                    try
                    {
                        isSaveRequired = false;
                        foreach (var ra in rsc.RunSetActions)
                        {
                            if (ra is RunSetActionHTMLReportSendEmail && ((RunSetActionHTMLReportSendEmail)ra).Email != null
                            && !string.IsNullOrEmpty(((RunSetActionHTMLReportSendEmail)ra).Email.SMTPPass))
                            {
                                ((RunSetActionHTMLReportSendEmail)ra).Email.SMTPPass =
                                EncryptionHandler.ReEncryptString(((RunSetActionHTMLReportSendEmail)ra).Email.SMTPPass, oldKey);
                                isSaveRequired = true;
                                varReencryptedCount++;
                            }
                            else if (ra is RunSetActionSendFreeEmail && ((RunSetActionSendFreeEmail)ra).Email != null
                            && !string.IsNullOrEmpty(((RunSetActionSendFreeEmail)ra).Email.SMTPPass))
                            {
                                ((RunSetActionSendFreeEmail)ra).Email.SMTPPass =
                                EncryptionHandler.ReEncryptString(((RunSetActionSendFreeEmail)ra).Email.SMTPPass, oldKey);
                                isSaveRequired = true;
                                varReencryptedCount++;
                            }
                            else if (ra is RunSetActionSendSMS && ((RunSetActionSendSMS)ra).SMSEmail != null
                            && !string.IsNullOrEmpty(((RunSetActionSendSMS)ra).SMSEmail.SMTPPass))
                            {
                                ((RunSetActionSendSMS)ra).SMSEmail.SMTPPass =
                                EncryptionHandler.ReEncryptString(((RunSetActionSendSMS)ra).SMSEmail.SMTPPass, oldKey);
                                isSaveRequired = true;
                                varReencryptedCount++;
                            }
                        }
                        if (isSaveRequired)
                        {
                            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(rsc);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Encrypting Email SMTP password of " + rsc.Name, ex);
                    }
                }
                return varReencryptedCount;
            });

        }
    }
}
