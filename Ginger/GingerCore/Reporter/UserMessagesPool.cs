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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Windows;

namespace GingerCore
{
    public enum eUserMsgKeys
    {
        GeneralErrorOccured, MissingImplementation, MissingImplementation2, ApplicationInitError, PageLoadError, UserProfileLoadError, ItemToSaveWasNotSelected, ToSaveChanges, SaveLocalChanges, LooseLocalChanges,
        RegistryValuesCheckFailed, AddRegistryValue, AddRegistryValueSucceed, AddRegistryValueFailed,
        NoItemWasSelected, AskToAddCheckInComment, FailedToGetProjectsListFromSVN, AskToSelectSolution, UpdateToRevision, CommitedToRevision, GitUpdateState, SourceControlConnFaild, SourceControlRemoteCannotBeAccessed, SourceControlUnlockFaild, SourceControlConnSucss, SourceControlLockSucss, SourceControlUnlockSucss, SourceControlConnMissingConnInputs, SourceControlConnMissingLocalFolderInput,
        PleaseStartAgent, AskToSelectValidation, CopiedVariableSuccessfully, AskIfShareVaribalesInRunner, ShareVariableNotSelected,
        EnvironmentItemLoadError, MissingUnixCredential,
        ErrorConnectingToDataBase, ErrorClosingConnectionToDataBase, DbTableError, DbQueryError, DbConnSucceed, DbConnFailed,
        SuccessfullyConnectedToAgent, FailedToConnectAgent, SshCommandError, GoToUrlFailure, HookLinkEventError, AskToStartAgent,
        MissingActionPropertiesEditor, AskToSelectItem, AskToSelectAction, ImportSeleniumScriptError,
        AskToSelectVariable, VariablesAssignError, SetCycleNumError, VariablesParentNotFound, CantStoreToVariable,
        AskToSelectSolutionFolder, SolutionLoadError,
        BeginWithNoSelectSolution,
        AddSupportValidationFailed, UploadSupportRequestSuccess, UploadSupportRequestFailure,
        FunctionReturnedError, ShowInfoMessage,
        LogoutSuccess,
        TestCompleted, CantFindObject, WarnOnDynamicActivities,
        QcConnectSuccess,
        QcConnectFailure,
        QcConnectFailureRestAPI,
        ALMConnectFailureWithCurrSettings,
        ALMOperationFailed,
        QcLoginSuccess,
        ALMLoginFailed,
        ALMConnectFailure,
        QcNeedLogin, ErrorWhileExportingExecDetails, ExportedExecDetailsToALM, ExportItemToALMSucceed, ExportAllItemsToALMSucceed, ExportedExecDetailsToALMIsInProcess, ExportAllItemsToALMFailed, ExportQCNewTestSetSelectDiffFolder,
        TestSetExists, ErrorInTestsetImport,
        TestSetsImportedSuccessfully,
        TestCasesUpdatedSuccessfully,
        TestCasesUploadedSuccessfully,
        PBNotConnected, ASCFNotConnected,
        DeleteRepositoryItemAreYouSure,
        DeleteTreeFolderAreYouSure,
        RenameRepositoryItemAreYouSure,RenameItemError,
        NoPathForCheckIn,
        SaveBusinessFlowChanges,
        BusinessFlowUpdate,        
        UnknownParamInCommandLine,
        SetDriverConfigTypeNotHandled,
        DriverConfigUnknownDriverType,
        SetDriverConfigTypeFieldNotFound,
        ApplicationNotFoundInEnvConfig,
        UnknownConsoleCommand,
        DOSConsolemissingCMDFileName,
        ExecutionReportSent,
        CannontFindBusinessFlow, ResetBusinessFlowRunVariablesFailed,
        AgentNotFound, MissingNewAgentDetails, MissingNewTableDetails, MissingExcelDetails, InvalidExcelDetails, ExportExcelFileFailed, ExportExcelFileDetails, InvalidTableDetails,MissingNewColumn, InvalidColumnName, ChangingAgentDriverAlert, MissingNewDSDetails,DuplicateDSDetails, DeleteDSFileError, InvalidDSPath, GingerKeyNameError, GingerKeyNameDuplicate,        
        ConfirmToAddTreeItem,
        FailedToAddTreeItem,
        SureWantToDeleteAll, NoItemToDelete, SelectItemToDelete, FailedToloadTheGrid,
        ErrorReadingRepositoryItem,
        EnvNotFound, SelectItemToAdd, CannotAddGinger,
        ShortcutCreated,
        SolutionFileNotFound, PlugInFileNotFound, FailedToLoadPlugIn,
        QcConnectError,
        MissingAddSolutionInputs, SolutionAlreadyExist, AddSolutionSucceed, AddSolutionFailed,
        MobileConnectionFailed, MobileRefreshScreenShotFailed, MobileShowElementDetailsFailed, MobileActionWasAdded,
        RefreshTreeGroupFailed, FailedToDeleteRepoItem, RunsetNoGingerPresentForBusinessFlow, ExcelNoWorksheetSelected, ExcelBadWhereClause,
        RecommendNewVersion,
        DragDropOperationFailed,
        ActionIDNotFound, RunsetBuinessFlowWasChanged, RunSetReloadhWarn, RefreshWholeSolution,
        ActivityIDNotFound, ActionsDependenciesLoadFailed, ActivitiesDependenciesLoadFailed,
        SelectValidColumn, SelectValidRow,
        DependenciesMissingActions, DependenciesMissingVariables, DuplicateVariable,
        AskIfToGenerateAutoRunDescription,
        MissingApplicationPlatform,
        NoActivitiesGroupWasSelected, ActivitiesGroupActivitiesNotFound, PartOfActivitiesGroupActsNotFound,
        ItemNameExistsInRepository, ItemExistsInRepository, ItemExternalExistsInRepository, ItemParentExistsInRepository, AskIfWantsToUpdateRepoItemInstances, AskIfWantsToChangeeRepoItem, GetRepositoryItemUsagesFailed, GetModelItemUsagesFailed, UpdateRepositoryItemUsagesSuccess, FailedToAddItemToSharedRepository, OfferToUploadAlsoTheActivityGroupToRepository,
        ConnectionCloseWarning,
        InvalidCharactersWarning,
        InvalidValueExpression, RecoverItemsMissingSelectionToRecover,
        FolderExistsWithName, DownloadedSolutionFromSourceControl, SourceControlFileLockedByAnotherUser, SourceControlItemAlreadyLocked,SoruceControlItemAlreadyUnlocked,
        SourceControlUpdateFailed, SourceControlCommitFailed, SourceControlChkInSucss, SourceControlChkInConflictHandledFailed, SourceControlGetLatestConflictHandledFailed, SourceControlChkInConflictHandled, SourceControlCheckInLockedByAnotherUser, SourceControlCheckInLockedByMe, SourceControlCheckInUnsavedFileChecked, FailedToUnlockFileDuringCheckIn, SourceControlChkInConfirmtion, SourceControlMissingSelectionToCheckIn, SourceControlResolveConflict, SureWantToDoRevert, SureWantToDoCheckIn, SourceControlConflictResolveFailed,
        NoOptionalAgent, MissingActivityAppMapping,
        SettingsChangeRequireRestart, UnsupportedFileFormat, WarnRegradingMissingVariablesUse, NotAllMissingVariablesWereAdded, UpdateApplicationNameChangeInSolution,
        ShareEnvAppWithAllEnvs, ShareEnvAppParamWithAllEnvs, CtrlSsaveEnvApp, CtrlSMissingItemToSave, FailedToSendEmail, FailedToExportBF,
        ReportTemplateNotFound, DriverNotSupportingWindowExplorer, AgentNotRunningAfterWaiting,
        FoundDuplicateAgentsInRunSet, StaticErrorMessage, StaticWarnMessage, StaticInfoMessage, ApplicationAgentNotMapped,
        ActivitiesGroupAlreadyMappedToTC, ExportItemToALMFailed, AskIfToSaveBFAfterExport,
        BusinessFlowAlreadyMappedToTC, AskIfSureWantToClose, WindowClosed, TargetWindowNotSelected,
        ChangingEnvironmentParameterValue,IFSaveChangesOfBF, AskIfToDownloadPossibleValues, AskIfToDownloadPossibleValuesShortProcesss, SelectAndSaveCategoriesValues, WhetherToOpenSolution,
        AutomationTabExecResultsNotExists, FolderNamesAreTooLong, FolderNotExistOrNotAvailible, FolderNameTextBoxIsEmpty, UserHaveNoWritePermission, FolderSizeTooSmall, DefaultTemplateCantBeDeleted, FileNotExist, ExecutionsResultsProdIsNotOn, ExecutionsResultsNotExists, ExecutionsResultsToDelete, AllExecutionsResultsToDelete, FilterNotBeenSet, RetreivingAllElements, ClickElementAgain, CloseFilterPage,
        BusinessFlowNeedTargetApplication,HTMLReportAttachment, ImageSize,
        GherkinAskToSaveFeatureFile, GherkinScenariosGenerated, GherkinNotifyFeatureFileExists, GherkinNotifyFeatureFileSelectedFromTheSolution, GherkinNotifyBFIsNotExistForThisFeatureFile, GherkinFileNotFound, GherkinColumnNotExist, GherkinActivityNotFound, GherkinBusinessFlowNotCreated, GherkinFeatureFileImportedSuccessfully, GherkinFeatureFileImportOnlyFeatureFileAllowedErrorMessage,
        AskIfSureWantToDeLink,AnalyzerFoundIssues,AnalyzerSaveRunSet,
        AskIfSureWantToUndoChange,
        MissingTargetPlatformForConversion, NoConvertibleActionsFound, NoConvertibleActionSelected, SuccessfulConversionDone, NoActivitySelectedForConversion, ActivitiesConversionFailed,
        CurrentActionNotSaved,
        FileExtensionNotSupported, NotifyFileSelectedFromTheSolution, FileImportedSuccessfully, CompilationErrorOccured, CompilationSucceed, Failedtosaveitems, SaveItemParentWarning, SaveAllItemsParentWarning,
        APIParametersListUpdated, APIMappedToActionIsMissing, NoAPIExistToMappedTo, CreateRunset, DeleteRunners, DeleteRunner,DeleteBusinessflow, DeleteBusinessflows, MissingErrorHandler,CantDeleteRunner, AllItemsSaved, APIModelAlreadyContainsReturnValues,
        InitializeBrowser,LoseChangesWarn, AskBeforeDefectProfileDeleting, MissedMandatotryFields, NoDefaultDefectProfileSelected, ALMDefectsWereOpened, AskALMDefectsOpening, WrongValueSelectedFromTheList, WrongNonNumberValueInserted, WrongDateValueInserted, NoDefectProfileCreated, IssuesInSelectedDefectProfile,
        VisualTestingFailedToDeleteOldBaselineImage,ApplitoolsLastExecutionResultsNotExists,ApplitoolsMissingChromeOrFirefoxBrowser, ParameterOptionalValues,
        FindAndRepalceFieldIsEmpty, FindAndReplaceListIsEmpty, FindAndReplaceNoItemsToRepalce, OracleDllIsMissing, ReportsTemplatesSaveWarn,
        POMWizardFailedToLearnElement, POMWizardReLearnWillDeleteAllElements, POMDriverIsBusy, FindAndReplaceViewRunSetNotSupported,
        POMSearchByGUIDFailed, POMElementSearchByGUIDFailed, NoRelevantAgentInRunningStatus, SolutionSaveWarning,
        InvalidIndexValue, FileOperationError, FolderOperationError, ObjectUnavailable, PatternNotHandled, LostConnection, AskToSelectBusinessflow,
        ScriptPaused, MissingFileLocation, ElementNotFound, TextNotFound, ProvideSearchString, NoTextOccurrence, JSExecutionFailed, FailedToInitiate, FailedToCreateRequestResponse, ActionNotImplemented, ValueIssue, MissingTargetApplication,
        ThreadError, ParsingError, SpecifyUniqueValue, ParameterAlreadyExists, DeleteNodesFromRequest, ParameterMerge, ParameterEdit, ParameterUpdate, ParameterDelete, SaveAll, SaveSelected, CopiedErrorInfo, RepositoryNameCantEmpty, 
        ExcelProcessingError, EnterValidBusinessflow, DeleteItem, RefreshFolder, RefreshFailed, ReplaceAll, ItemSelection, DifferentItemType, CopyCutOperation, ObjectLoad, POMAgentIsNotRunning, POMNotOnThePageWarn,
    }

    public static class UserMessagesPool
    {
        public static void LoadUserMessgaesPool()
        {
            //Initialize the pool
            //Reporter.UserMessagesPool = new Dictionary<string, UserMessage>();
            Reporter.UserMessagesPool = new Dictionary<eUserMsgKeys, UserMessage>();

            //Add user messages to the pool
            #region General Application Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CompilationErrorOccured, new UserMessage(eAppReporterMessageType.ERROR, "Compilation Error Occurred", "Compilation error occurred." + Environment.NewLine + "Error Details: "+ Environment.NewLine +" '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CompilationSucceed, new UserMessage(eAppReporterMessageType.INFO, "Compilation Succeed", "Compilation Passed successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FileExtensionNotSupported, new UserMessage(eAppReporterMessageType.ERROR, "File Extension Not Supported", "The selected file extension is not supported by this editor." + Environment.NewLine + "Supported extensions: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NotifyFileSelectedFromTheSolution, new UserMessage(eAppReporterMessageType.ERROR, "File Already Exists", "File - '{0}'." + Environment.NewLine + "Selected From The Solution folder hence its already exist and cannot be copy to the same place" + Environment.NewLine + "Please select another File to continue.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FileImportedSuccessfully, new UserMessage(eAppReporterMessageType.INFO, "File imported successfully", "The File was imported successfully", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinNotifyFeatureFileExists, new UserMessage(eAppReporterMessageType.ERROR, "Feature File Already Exists", "Feature File with the same name already exist - '{0}'." + Environment.NewLine + "Please select another Feature File to continue.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinNotifyFeatureFileSelectedFromTheSolution, new UserMessage(eAppReporterMessageType.ERROR, "Feature File Already Exists", "Feature File - '{0}'." + Environment.NewLine + "Selected From The Solution folder hence its already exist and cannot be copy to the same place"+ Environment.NewLine + "Please select another Feature File to continue.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinNotifyBFIsNotExistForThisFeatureFile, new UserMessage(eAppReporterMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)+" Is Not Exists", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)+ " has to been generated for Feature File - '{0}'." + Environment.NewLine  + Environment.NewLine + "Please create " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " from the Editor Page.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinFileNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Gherkin File Not Found", "Gherkin file was not found at the path: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinColumnNotExist, new UserMessage(eAppReporterMessageType.WARN, "Column Not Exist", "Cant find value for: '{0}' Item/s. since column/s not exist in example table", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinActivityNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Activity Not Found", "Activity not found, Name: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinBusinessFlowNotCreated, new UserMessage(eAppReporterMessageType.WARN, "Gherkin "+ GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Creation Failed", "The file did not passed Gherkin compilation hence the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " not created" + Environment.NewLine + "please correct the imported file and create the Business Flow from Gherkin Page", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinFeatureFileImportedSuccessfully, new UserMessage(eAppReporterMessageType.INFO, "Gherkin feature file imported successfully", "Gherkin feature file imported successfully", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinFeatureFileImportOnlyFeatureFileAllowedErrorMessage, new UserMessage(eAppReporterMessageType.ERROR, "Gherkin feature file not valid", "Only Gherkin feature files can be imported", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinAskToSaveFeatureFile, new UserMessage(eAppReporterMessageType.WARN, "Feature File Changes were made", "Do you want to Save the Feature File?" + Environment.NewLine + "WARNING: If you do not manually Save your Feature File, you may loose your work if you close out of Ginger.", MessageBoxButton.YesNoCancel, MessageBoxResult.Yes));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinScenariosGenerated, new UserMessage(eAppReporterMessageType.INFO, "Scenarios generated", "{0} Scenarios generated successfully", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GeneralErrorOccured, new UserMessage(eAppReporterMessageType.ERROR, "Error Occurred", "Application error occurred." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingImplementation, new UserMessage(eAppReporterMessageType.WARN, "Missing Implementation", "The {0} functionality hasn't been implemented yet.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingImplementation2, new UserMessage(eAppReporterMessageType.WARN, "Missing Implementation", "The functionality hasn't been implemented yet.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ApplicationInitError, new UserMessage(eAppReporterMessageType.ERROR, "Application Initialization Error", "Error occurred during application initialization." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.PageLoadError, new UserMessage(eAppReporterMessageType.ERROR, "Page Load Error", "Failed to load the page '{0}'." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UserProfileLoadError, new UserMessage(eAppReporterMessageType.ERROR, "User Profile Load Error", "Error occurred during user profile loading." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemToSaveWasNotSelected, new UserMessage(eAppReporterMessageType.WARN, "Save", "Item to save was not selected.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RecommendNewVersion, new UserMessage(eAppReporterMessageType.WARN, "Upgrade required", "You are not using the latest version of Ginger. Please go to https://github.com/Ginger-Automation/Ginger/releases to get the latest build.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ToSaveChanges, new UserMessage(eAppReporterMessageType.QUESTION, "Save Changes?", "Do you want to save changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UnsupportedFileFormat, new UserMessage(eAppReporterMessageType.ERROR, "Save Changes", "File format not supported.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.CtrlSsaveEnvApp, new UserMessage(eAppReporterMessageType.WARN, "Save Environment", "Please select the environment to save.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CtrlSMissingItemToSave, new UserMessage(eAppReporterMessageType.WARN, "Missing Item to Save", "Please select individual item to save or use menu toolbar to save all the items.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.StaticErrorMessage, new UserMessage(eAppReporterMessageType.ERROR, "Error Message", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidIndexValue, new UserMessage(eAppReporterMessageType.ERROR, "Invalid index value", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FileOperationError, new UserMessage(eAppReporterMessageType.ERROR, "Error occured during file operation", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderOperationError, new UserMessage(eAppReporterMessageType.ERROR, "Error occured during folder operation", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ObjectUnavailable, new UserMessage(eAppReporterMessageType.ERROR, "Object Unavailable", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.PatternNotHandled, new UserMessage(eAppReporterMessageType.ERROR, "Pattern not handled", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.LostConnection, new UserMessage(eAppReporterMessageType.ERROR, "Lost Connection", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectBusinessflow, new UserMessage(eAppReporterMessageType.INFO, "Select Businessflow", "Please select " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingFileLocation, new UserMessage(eAppReporterMessageType.INFO, "Missing file location", "Please select a location for the file.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ScriptPaused, new UserMessage(eAppReporterMessageType.INFO, "Script Paused", "Script is paused!", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ElementNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Element Not Found", "Element not found", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TextNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Text Not Found", "Text not found", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ProvideSearchString, new UserMessage(eAppReporterMessageType.ERROR, "Provide Search String", "Please provide search string", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoTextOccurrence, new UserMessage(eAppReporterMessageType.ERROR, "No Text Occurrence", "No more text occurrence", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToInitiate, new UserMessage(eAppReporterMessageType.ERROR, "Failed to initiate", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToCreateRequestResponse, new UserMessage(eAppReporterMessageType.ERROR, "Failed to create Request / Response", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActionNotImplemented, new UserMessage(eAppReporterMessageType.ERROR, "Action is not implemented yet for control type ", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ValueIssue, new UserMessage(eAppReporterMessageType.ERROR, "Value Issue", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.JSExecutionFailed, new UserMessage(eAppReporterMessageType.ERROR, "Error Executing JS", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingTargetApplication, new UserMessage(eAppReporterMessageType.ERROR, "Missing target ppplication", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ThreadError, new UserMessage(eAppReporterMessageType.ERROR, "Thread Exception", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParsingError, new UserMessage(eAppReporterMessageType.ERROR, "Error while parsing", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SpecifyUniqueValue, new UserMessage(eAppReporterMessageType.WARN, "Please specify unique value", "Optional value already exists", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterAlreadyExists, new UserMessage(eAppReporterMessageType.WARN, "Cannot upload the selected parameter", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteNodesFromRequest, new UserMessage(eAppReporterMessageType.WARN, "Delete nodes from request body?", "Do you want to delete also nodes from request body that contain those parameters?", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterMerge, new UserMessage(eAppReporterMessageType.WARN, "Models Parameters Merge", "Do you want to update the merged instances on all model configurations?", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterEdit, new UserMessage(eAppReporterMessageType.WARN, "Global Parameter Edit", "Global Parameter Place Holder cannot be edit.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterUpdate, new UserMessage(eAppReporterMessageType.WARN, "Update Global Parameter Value Expression Instances", "{0}", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterDelete, new UserMessage(eAppReporterMessageType.WARN, "Delete Parameter", "{0}", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveAll, new UserMessage(eAppReporterMessageType.WARN, "Save All", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveSelected, new UserMessage(eAppReporterMessageType.WARN, "Save Selected", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CopiedErrorInfo, new UserMessage(eAppReporterMessageType.INFO, "Copied Error information", "Error Information copied to Clipboard", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RepositoryNameCantEmpty, new UserMessage(eAppReporterMessageType.WARN, "QTP to Ginger Converter", "Object Repository name cannot be empty", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExcelProcessingError, new UserMessage(eAppReporterMessageType.ERROR, "Excel processing error", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.EnterValidBusinessflow, new UserMessage(eAppReporterMessageType.WARN, "Enter valid businessflow", "Please enter a Valid " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Name", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteItem, new UserMessage(eAppReporterMessageType.WARN, "Delete Item", "Are you sure you want to delete '{0}' item ?", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RefreshFolder, new UserMessage(eAppReporterMessageType.WARN, "Refresh Folder", "Un saved items changes under the refreshed folder will be lost, to continue with refresh?", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RefreshFailed, new UserMessage(eAppReporterMessageType.ERROR, "Refresh Failed", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ReplaceAll, new UserMessage(eAppReporterMessageType.QUESTION, "Replace All", "{0}", MessageBoxButton.OKCancel, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemSelection, new UserMessage(eAppReporterMessageType.WARN, "Item Selection", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DifferentItemType, new UserMessage(eAppReporterMessageType.WARN, "Not Same Item Type", "The source item type do not match to the destination folder type", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CopyCutOperation, new UserMessage(eAppReporterMessageType.INFO, "Copy/Cut Operation", "Please select Copy/Cut operation first.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ObjectLoad, new UserMessage(eAppReporterMessageType.ERROR, "Object Load", "Not able to load the object details", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.StaticWarnMessage, new UserMessage(eAppReporterMessageType.WARN, "Warning Message", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.StaticInfoMessage, new UserMessage(eAppReporterMessageType.INFO, "Info Message", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfSureWantToClose, new UserMessage(eAppReporterMessageType.QUESTION, "Close Ginger", "Are you sure you want to close Ginger?" + Environment.NewLine + Environment.NewLine + "Notice: Un-saved changes won't be saved.", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.BusinessFlowNeedTargetApplication, new UserMessage(eAppReporterMessageType.WARN, "Target Application Not Selected", "Target Application Not Selected! Please Select at least one Target Application", MessageBoxButton.OK, MessageBoxResult.None));
         
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfSureWantToUndoChange, new UserMessage(eAppReporterMessageType.WARN, "Undo Changes", "Are you sure you want to undo all changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InitializeBrowser, new UserMessage(eAppReporterMessageType.INFO, "Initialize Browser", "Initialize Browser action automatically added."+Environment.NewLine+"Please continue to spy the required element.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.LoseChangesWarn, new UserMessage(eAppReporterMessageType.WARN, "Save Changes", "The operation may result with lost of un-saved local changes." + Environment.NewLine + "Please make sure all changes were saved before continue." + Environment.NewLine + Environment.NewLine + "To perform the operation?", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion General Application Messages

            #region Settings
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SettingsChangeRequireRestart, new UserMessage(eAppReporterMessageType.INFO, "Settings Change", "For the settings change to take affect you must restart Ginger.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Settings

            #region Repository
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemNameExistsInRepository, new UserMessage(eAppReporterMessageType.WARN, "Add Item to Repository", "Item with the name '{0}' already exist in the repository." + Environment.NewLine + Environment.NewLine + "Please change the item name to be unique and try to upload it again.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemExistsInRepository, new UserMessage(eAppReporterMessageType.WARN, "Add Item to Repository", "The item '{0}' already exist in the repository (item name in repository is '{1}')." + Environment.NewLine + Environment.NewLine + "Do you want to continue and overwrite it?" + Environment.NewLine + Environment.NewLine + "Note: If you select 'Yes', backup of the existing item will be saved into 'PreVersions' folder.", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemExternalExistsInRepository, new UserMessage(eAppReporterMessageType.WARN, "Add Item to Repository", "The item '{0}' is mapped to the same external item like the repository item '{1}'." + Environment.NewLine + Environment.NewLine + "Do you want to continue and overwrite the existing repository item?" + Environment.NewLine + Environment.NewLine + "Note: If you select 'Yes', backup of the existing repository item will be saved into 'PreVersions' folder.", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemParentExistsInRepository, new UserMessage(eAppReporterMessageType.WARN, "Add Item to Repository", "The item '{0}' source is from the repository item '{1}'." + Environment.NewLine + Environment.NewLine + "Do you want to overwrite the source repository item?" + Environment.NewLine + Environment.NewLine + "Note:" + Environment.NewLine + "If you select 'No', the item will be added as a new item to the repository." + Environment.NewLine + "If you select 'Yes', backup of the existing repository item will be saved into 'PreVersions' folder.", MessageBoxButton.YesNoCancel, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToAddItemToSharedRepository, new UserMessage(eAppReporterMessageType.ERROR, "Add Item to Repository", "Failed to add the item '{0}' to shared repository." + Environment.NewLine + Environment.NewLine + "Error Details: {1}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfWantsToUpdateRepoItemInstances, new UserMessage(eAppReporterMessageType.WARN, "Update Repository Item Usages", "The item '{0}' has {1} instances." + Environment.NewLine + Environment.NewLine + "Do you want to review them and select which one to get updated as well?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfWantsToChangeeRepoItem, new UserMessage(eAppReporterMessageType.WARN, "Change Repository Item", "The item '{0}' is been used in {1} places." + Environment.NewLine + Environment.NewLine + "Are you sure you want to {2} it?" + Environment.NewLine + Environment.NewLine + "Note: Anyway the changes won't affect the linked instances of this item", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GetRepositoryItemUsagesFailed, new UserMessage(eAppReporterMessageType.ERROR, "Repository Item Usages", "Failed to get the '{0}' item usages." + Environment.NewLine + Environment.NewLine + "Error Details: {1}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GetModelItemUsagesFailed, new UserMessage(eAppReporterMessageType.ERROR, "Model Item Usages", "Failed to get the '{0}' item usages." + Environment.NewLine + Environment.NewLine + "Error Details: {1}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UpdateRepositoryItemUsagesSuccess, new UserMessage(eAppReporterMessageType.INFO, "Update Repository Item Usages", "Finished to update the repository items usages." + Environment.NewLine + Environment.NewLine + "Note: Updates were not saved yet.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfSureWantToDeLink, new UserMessage(eAppReporterMessageType.WARN, "De-Link to Shared Repository", "Are you sure you want to de-link the item from it Shared Repository source item?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.OfferToUploadAlsoTheActivityGroupToRepository, new UserMessage(eAppReporterMessageType.QUESTION, "Add the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to Repository", "The " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " '{0}' is part of the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " '{1}', do you want to add the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to the shared repository as well?" + System.Environment.NewLine + System.Environment.NewLine + "Note: If you select Yes, only the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " will be added to the repository and not all of it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + ".", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.SolutionSaveWarning, new UserMessage(eAppReporterMessageType.WARN, "Save", "Note: save will include saving also changes which were done to: {0}." + System.Environment.NewLine + System.Environment.NewLine + "To continue with Save operation?", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion Repository

            #region Analyzer
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AnalyzerFoundIssues, new UserMessage(eAppReporterMessageType.WARN, "Issues Detected By Analyzer", "Critical/High Issues were detected, please handle them before execution.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AnalyzerSaveRunSet, new UserMessage(eAppReporterMessageType.WARN, "Issues Detected By Analyzer", "Please save the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " first", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveItemParentWarning, new UserMessage(eAppReporterMessageType.WARN, "Item Parent Save", "Save process will actually save the item {0} parent which called '{1}'." + System.Environment.NewLine + System.Environment.NewLine + "To continue with save?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveAllItemsParentWarning, new UserMessage(eAppReporterMessageType.WARN, "Items Parent Save", "Save process will actually save item\\s parent\\s (" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ")" + System.Environment.NewLine + System.Environment.NewLine + "To continue with save?", MessageBoxButton.YesNo, MessageBoxResult.No));

            #endregion Analyzer

            #region Registry Values Check Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RegistryValuesCheckFailed, new UserMessage(eAppReporterMessageType.ERROR, "Run Registry Values Check", "Failed to check if all needed registry values exist.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddRegistryValue, new UserMessage(eAppReporterMessageType.QUESTION, "Missing Registry Value", "The required registry value for the key: '{0}' is missing or wrong. Do you want to add/fix it?", MessageBoxButton.YesNo, MessageBoxResult.Yes));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddRegistryValueSucceed, new UserMessage(eAppReporterMessageType.INFO, "Add Registry Value", "Registry value added successfully for the key: '{0}'." + Environment.NewLine + "Please restart the application.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddRegistryValueFailed, new UserMessage(eAppReporterMessageType.ERROR, "Add Registry Value", "Registry value add failed for the key: '{0}'." + Environment.NewLine + "Please restart the application as administrator and try again.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Registry Values Check Messages

            #region SourceControl Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoItemWasSelected, new UserMessage(eAppReporterMessageType.WARN, "No item was selected", "Please select an item to proceed.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlConflictResolveFailed, new UserMessage(eAppReporterMessageType.ERROR, "Resolve Conflict Failed", "Ginger failed to resolve the conflicted file" + Environment.NewLine + "File Path: {0}" + Environment.NewLine + Environment.NewLine + "It seems like the SVN conflict content (e.g. '<<<<<<< .mine') has been updated on the remote repository", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToAddCheckInComment, new UserMessage(eAppReporterMessageType.WARN, "Check-In Changes", "Please enter check-in comments.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToGetProjectsListFromSVN, new UserMessage(eAppReporterMessageType.ERROR, "Source Control Error", "Failed to get the solutions list from the source control." + Environment.NewLine + "Error Details: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectSolution, new UserMessage(eAppReporterMessageType.WARN, "Select Solution", "Please select solution.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlFileLockedByAnotherUser, new UserMessage(eAppReporterMessageType.WARN, "Source Control File Locked", "The file '{0}' was locked by: {1} "  + Environment.NewLine + "Locked comment {2}." + Environment.NewLine + Environment.NewLine + " Are you sure you want to unlock the file?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlItemAlreadyLocked, new UserMessage(eAppReporterMessageType.INFO, "Source Control File Locked", "The file is already locked" + Environment.NewLine + "Please do Get info for more details", MessageBoxButton.OK, MessageBoxResult.OK));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.SoruceControlItemAlreadyUnlocked, new UserMessage(eAppReporterMessageType.INFO, "Source Control File not Locked", "The file is not locked" + Environment.NewLine + "Please do Get info for more details", MessageBoxButton.OK, MessageBoxResult.OK));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.DownloadedSolutionFromSourceControl, new UserMessage(eAppReporterMessageType.INFO, "Download Solution", "The solution '{0}' was downloaded successfully." + Environment.NewLine + "Do you want to open the downloaded Solution?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UpdateToRevision, new UserMessage(eAppReporterMessageType.INFO, "Update Solution", "The solution was updated successfully to revision: {0}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CommitedToRevision, new UserMessage(eAppReporterMessageType.INFO, "Commit Solution", "The changes were committed successfully, Revision: {0}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GitUpdateState, new UserMessage(eAppReporterMessageType.INFO, "Update Solution", "The solution was updated successfully, Update status: {0}, Latest Revision :{1}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoPathForCheckIn, new UserMessage(eAppReporterMessageType.ERROR, "No Path for Check-In", "Missing Path", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlConnFaild, new UserMessage(eAppReporterMessageType.ERROR, "Source Control Connection", "Failed to establish connection to the source control." + Environment.NewLine + "Error Details: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlRemoteCannotBeAccessed, new UserMessage(eAppReporterMessageType.ERROR, "Source Control Connection", "Failed to establish connection to the source control." + Environment.NewLine + "The remote repository cannot be accessed, please check your internet connection and your proxy configurations" + Environment.NewLine + Environment.NewLine + "Error details: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlUnlockFaild, new UserMessage(eAppReporterMessageType.ERROR, "Source Control Unlock", "Failed to unlock remote file, File locked by different user." + Environment.NewLine + "Locked by: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlConnSucss, new UserMessage(eAppReporterMessageType.INFO, "Source Control Connection", "Succeed to establish connection to the source control", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlLockSucss, new UserMessage(eAppReporterMessageType.INFO, "Source Control Lock", "Succeed to Lock the file in the source control", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlUnlockSucss, new UserMessage(eAppReporterMessageType.INFO, "Source Control Unlock", "Succeed to unlock the file in the source control", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlConnMissingConnInputs, new UserMessage(eAppReporterMessageType.WARN, "Source Control Connection", "Missing connection inputs, please set the source control URL, user name and password.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlConnMissingLocalFolderInput, new UserMessage(eAppReporterMessageType.WARN, "Download Solution", "Missing local folder input, please select local folder to download the solution into.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlUpdateFailed, new UserMessage(eAppReporterMessageType.ERROR, "Update Solution", "Failed to update the solution from source control." + Environment.NewLine + "Error Details: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlCommitFailed, new UserMessage(eAppReporterMessageType.ERROR, "Commit Solution", "Failed to commit the solution from source control." + Environment.NewLine + "Error Details: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlChkInSucss, new UserMessage(eAppReporterMessageType.QUESTION, "Check-In Changes", "Check-in process ended successfully." + Environment.NewLine + Environment.NewLine + "Do you want to do another check-in?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlChkInConflictHandled, new UserMessage(eAppReporterMessageType.QUESTION, "Check-In Results", "The check in process ended, please notice that conflict was indentefied during the process and may additional check in will be needed for pushing the conflict items." + Environment.NewLine + Environment.NewLine + "Do you want to do another check-in?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlChkInConflictHandledFailed, new UserMessage(eAppReporterMessageType.WARN, "Check-In Results", "Check in process ended with unhandled conflict please notice that you must handled them prior to the next check in of those items", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlGetLatestConflictHandledFailed, new UserMessage(eAppReporterMessageType.WARN, "Get Latest Results", "Get latest process encountered conflicts which must be resolved for successfull Solution loading", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlCheckInLockedByAnotherUser, new UserMessage(eAppReporterMessageType.WARN, "Locked Item Check-In", "The item: '{0}'" + Environment.NewLine + "is locked by the user: '{1}'" + Environment.NewLine + "The locking comment is: '{2}' ." + Environment.NewLine + Environment.NewLine + "Do you want to unlock the item and proceed with check in process?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlCheckInLockedByMe, new UserMessage(eAppReporterMessageType.WARN, "Locked Item Check-In", "The item: '{0}'" + Environment.NewLine + "is locked by you please noticed that during check in the lock will need to be removed please confirm to continue", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlCheckInUnsavedFileChecked, new UserMessage(eAppReporterMessageType.QUESTION, "Unsaved Item Check-in", "The item: '{0}' contains unsaved changes which must be saved prior to the check in process." + Environment.NewLine + Environment.NewLine + "Do you want to save the item and proceed with check in process?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToUnlockFileDuringCheckIn, new UserMessage(eAppReporterMessageType.QUESTION, "Locked Item Check-In Failure", "The item: '{0}' unlock operation failed on the URL: '{1}'." + Environment.NewLine + Environment.NewLine + "Do you want to proceed with the check in  process for rest of the items?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlChkInConfirmtion, new UserMessage(eAppReporterMessageType.QUESTION, "Check-In Changes", "Checking in changes will effect all project users, are you sure you want to continue and check in those {0} changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlMissingSelectionToCheckIn, new UserMessage(eAppReporterMessageType.WARN, "Check-In Changes", "Please select items to check-in.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlResolveConflict, new UserMessage(eAppReporterMessageType.QUESTION, "Source Control Conflicts", "Source control conflicts has been identified for the path: '{0}'." + Environment.NewLine + "You probably won't be able to use the item which in the path till conflicts will be resolved." + Environment.NewLine + Environment.NewLine + "Do you want to automatically resolve the conflicts and keep your local changes for all conflicts?" + Environment.NewLine + Environment.NewLine + "Select 'No' for accepting server updates for all conflicts or select 'Cancel' for canceling the conflicts handling.", MessageBoxButton.YesNoCancel, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SureWantToDoRevert, new UserMessage(eAppReporterMessageType.QUESTION, "Undo Changes", "Are you sure you want to revert changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SureWantToDoCheckIn, new UserMessage(eAppReporterMessageType.QUESTION, "Check-In Changes", "Are you sure you want to check-in changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RefreshWholeSolution, new UserMessage(eAppReporterMessageType.QUESTION, "Refresh Solution", "Do you want to Refresh the whole Solution to get the Latest changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            
            #endregion SourceControl Messages

            #region Validation Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.PleaseStartAgent, new UserMessage(eAppReporterMessageType.WARN, "Start Agent", "Please start agent in order to run validation.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectValidation, new UserMessage(eAppReporterMessageType.WARN, "Select Validation", "Please select validation to edit.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Validation Messages

            #region DataBase Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ErrorConnectingToDataBase, new UserMessage(eAppReporterMessageType.ERROR, "Cannot connect to database", "DB Connection error." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ErrorClosingConnectionToDataBase, new UserMessage(eAppReporterMessageType.ERROR, "Error in closing connection to database", "DB Close Connection error" + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DbTableError, new UserMessage(eAppReporterMessageType.ERROR, "DB Table Error", "Error occurred while trying to get the {0}." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DbQueryError, new UserMessage(eAppReporterMessageType.ERROR, "DB Query Error", "The DB Query returned error, please double check the table name and field name." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DbConnFailed, new UserMessage(eAppReporterMessageType.ERROR, "DB Connection Status", "Connect to the DB failed.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DbConnSucceed, new UserMessage(eAppReporterMessageType.INFO, "DB Connection Status", "Connect to the DB succeeded.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.OracleDllIsMissing, new UserMessage(eAppReporterMessageType.ERROR, "DB Connection Status", "Connect to the DB failed." + Environment.NewLine + "The file Oracle.ManagedDataAccess.dll is missing," + Environment.NewLine + "Please download the file, place it under the below folder, restart Ginger and retry:" + Environment.NewLine + "{0}" + Environment.NewLine + "Do you want to download the file now?", MessageBoxButton.YesNo, MessageBoxResult.None));
            #endregion DataBase Messages

            #region Environment Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingUnixCredential, new UserMessage(eAppReporterMessageType.ERROR, "Unix credential is missing or invalid", "Unix credential is missing or invalid in Environment settings, please open Environment tab to check.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.EnvironmentItemLoadError, new UserMessage(eAppReporterMessageType.ERROR, "Environment Item Load Error", "Failed to load the {0}." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ShareEnvAppWithAllEnvs, new UserMessage(eAppReporterMessageType.INFO, "Share Environment Applications", "The selected application/s were added to the other solution environments." + Environment.NewLine + Environment.NewLine + "Note: The changes were not saved, please perform save for all changed environments.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ShareEnvAppParamWithAllEnvs, new UserMessage(eAppReporterMessageType.INFO, "Share Environment Application Parameter", "The selected parameter/s were added to the other solution environments matching applications." + Environment.NewLine + Environment.NewLine + "Note: The changes were not saved, please perform save for all changed environments.", MessageBoxButton.OK, MessageBoxResult.None));            
            
            #endregion Environment Messages

            #region Agents/Drivers Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SuccessfullyConnectedToAgent, new UserMessage(eAppReporterMessageType.INFO, "Connect Agent Success", "Agent connection successful!", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToConnectAgent, new UserMessage(eAppReporterMessageType.ERROR, "Connect to Agent", "Failed to connect to the {0} agent." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SshCommandError, new UserMessage(eAppReporterMessageType.ERROR, "Ssh Command Error", "The Ssh Run Command returns error, please double check the connection info and credentials." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GoToUrlFailure, new UserMessage(eAppReporterMessageType.ERROR, "Go To URL Error", "Failed to go to the URL: '{0}'." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.HookLinkEventError, new UserMessage(eAppReporterMessageType.ERROR, "Hook Link Event Error", "The link type is unknown.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToStartAgent, new UserMessage(eAppReporterMessageType.WARN, "Missing Agent", "Please start/select agent.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ASCFNotConnected, new UserMessage(eAppReporterMessageType.ERROR, "Not Connected to ASCF", "Please Connect first.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SetDriverConfigTypeNotHandled, new UserMessage(eAppReporterMessageType.ERROR, "Set Driver configuration", "Unknown Type {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DriverConfigUnknownDriverType, new UserMessage(eAppReporterMessageType.ERROR, "Driver Configuration", "Unknown Driver Type {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SetDriverConfigTypeFieldNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Driver Configuration", "Unknown Driver Parameter {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UnknownConsoleCommand, new UserMessage(eAppReporterMessageType.ERROR, "Unknown Console Command", "Command {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DOSConsolemissingCMDFileName, new UserMessage(eAppReporterMessageType.ERROR, "DOS Console Driver Error", "DOSConsolemissingCMDFileName", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CannontFindBusinessFlow, new UserMessage(eAppReporterMessageType.ERROR, "Cannot Find " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), "Missing flow in solution {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AgentNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Cannot Find Agent", "Missing Agent from Run Config- {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingNewAgentDetails, new UserMessage(eAppReporterMessageType.WARN, "Missing Agent Details", "The new Agent {0} is missing.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingNewTableDetails, new UserMessage(eAppReporterMessageType.ERROR, "Missing Table Details", "The new Table {0} is missing.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingExcelDetails, new UserMessage(eAppReporterMessageType.ERROR, "Missing Export Path Details", "The Export Excel File Path is missing", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidExcelDetails, new UserMessage(eAppReporterMessageType.ERROR, "InValid Export Sheet Details", "The Export Excel can be *.xlsx only.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportExcelFileFailed, new UserMessage(eAppReporterMessageType.ERROR, "Export Excel File Failed", "Error Occurred while exporting the Excel File: {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportExcelFileDetails, new UserMessage(eAppReporterMessageType.INFO, "Export Excel File Details", "Export execution ended successfully", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidTableDetails, new UserMessage(eAppReporterMessageType.ERROR, "InValid Table Details", "The Table Name provided is Invalid. It cannot contain spaces", MessageBoxButton.OK, MessageBoxResult.None));            
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingNewDSDetails, new UserMessage(eAppReporterMessageType.WARN, "Missing Data Source Details", "The new Data Source {0} is missing.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DuplicateDSDetails, new UserMessage(eAppReporterMessageType.ERROR, "Duplicate DataSource Details", "The Data Source with the File Path {0} already Exist. Please use another File Path", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteDSFileError, new UserMessage(eAppReporterMessageType.WARN, "Delete DataSource File", "The Data Source File with the File Path '{0}' Could not be deleted. Please Delete file Manually", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidDSPath, new UserMessage(eAppReporterMessageType.ERROR, "Invalid DataSource Path", "The Data Source with the File Path {0} is not valid. Please use correct File Path", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GingerKeyNameError, new UserMessage(eAppReporterMessageType.ERROR, "InValid Ginger Key Name", "The Ginger Key Name cannot be Duplicate or NULL. Please fix Table '{0}' before continuing.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GingerKeyNameDuplicate, new UserMessage(eAppReporterMessageType.ERROR, "InValid Ginger Key Name", "The Ginger Key Name cannot be Duplicated. Please change the Key Name : {0} in Table : {1}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingNewColumn, new UserMessage(eAppReporterMessageType.ERROR, "Missing Column Details", "The new Column {0} is missing.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidColumnName, new UserMessage(eAppReporterMessageType.ERROR, "Invalid Column Details", "The Column Name is invalid.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingApplicationPlatform, new UserMessage(eAppReporterMessageType.WARN, "Missing Application Platform Info", "The default Application Platform Info is missing, please go to Solution level to add at least one Target Application.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ConnectionCloseWarning, new UserMessage(eAppReporterMessageType.WARN, "Connection Close", "Closing this window will cause the connection to {0} to be closed, to continue and close?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ChangingAgentDriverAlert, new UserMessage(eAppReporterMessageType.WARN, "Changing Agent Driver", "Changing the Agent driver type will cause all driver configurations to be reset, to continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidCharactersWarning, new UserMessage(eAppReporterMessageType.WARN, "Invalid Details", "Name can't contain any of the following characters: " + Environment.NewLine + " /, \\, *, :, ?, \", <, >, |", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidValueExpression, new UserMessage(eAppReporterMessageType.WARN, "Invalid Value Expression", "{0} - Value Expression has some Error. Do you want to continue?", MessageBoxButton.YesNo, MessageBoxResult.None)); 
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoOptionalAgent, new UserMessage(eAppReporterMessageType.WARN, "No Optional Agent", "No optional Agent was found." + Environment.NewLine + "Please configure new Agent under the Solution to be used for this application.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DriverNotSupportingWindowExplorer, new UserMessage(eAppReporterMessageType.WARN, "Open Window Explorer", "The driver '{0}' is not supporting the Window Explorer yet.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AgentNotRunningAfterWaiting, new UserMessage(eAppReporterMessageType.WARN, "Running Agent", "The Agent '{0}' failed to start running after {1} seconds.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ApplicationAgentNotMapped, new UserMessage(eAppReporterMessageType.WARN, "Agent Not Mapped to Target Application", "The Agent Mapping for Target Application  '{0}' , Please map Agent.", MessageBoxButton.OK, MessageBoxResult.None));      
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WindowClosed, new UserMessage(eAppReporterMessageType.WARN, "Invalid target window", "Target window is either closed or no longer available. \n\n Please Add switch Window action by selecting correct target window on Window Explorer", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TargetWindowNotSelected, new UserMessage(eAppReporterMessageType.WARN, "Target Window not Selected", "Please choose the target window from available list", MessageBoxButton.OK, MessageBoxResult.None));
            
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ChangingEnvironmentParameterValue, new UserMessage(eAppReporterMessageType.WARN, "Changing Environment Variable Name", "Changing the Environment variable name will cause rename this environment variable name in every " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)+ ", do you want to continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveLocalChanges, new UserMessage(eAppReporterMessageType.QUESTION, "Save Local Changes?", "Your Local Changes will be saved. Do you want to Continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.LooseLocalChanges, new UserMessage(eAppReporterMessageType.WARN, "Loose Local Changes?", "Your Local Changes will be Lost. Do you want to Continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.IFSaveChangesOfBF, new UserMessage(eAppReporterMessageType.WARN, "Save Current" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Before Change?", "Do you want to save the changes made in the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) +"?", MessageBoxButton.YesNo, MessageBoxResult.No));
            
            #endregion Agents/Drivers Messages

            #region Actions Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingActionPropertiesEditor, new UserMessage(eAppReporterMessageType.ERROR, "Action Properties Editor", "No Action properties Editor yet for '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectItem, new UserMessage(eAppReporterMessageType.WARN, "Select Item", "Please select item.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectAction, new UserMessage(eAppReporterMessageType.WARN, "Select Action", "Please select action.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ImportSeleniumScriptError, new UserMessage(eAppReporterMessageType.ERROR, "Import Selenium Script Error", "Error occurred while trying to import Selenium Script, please make sure the html file is a Selenium script generated by Selenium IDE.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Actions Messages

            #region Runset Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RunsetNoGingerPresentForBusinessFlow, new UserMessage(eAppReporterMessageType.WARN, "No Ginger in " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Please add at least one Ginger to your " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " before choosing a " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ".", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ResetBusinessFlowRunVariablesFailed, new UserMessage(eAppReporterMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Variables) + " Reset Failed", "Failed to reset the " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " to original configurations." + System.Environment.NewLine + "Error Details: {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfToGenerateAutoRunDescription, new UserMessage(eAppReporterMessageType.QUESTION, "Automatic Description Creation", "Do you want to automatically populate the Run Description?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CreateRunset, new UserMessage(eAppReporterMessageType.WARN, "Create Runset", "No runset found, Do you want to create new runset", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteRunners, new UserMessage(eAppReporterMessageType.WARN, "Delete Runners", "Are you sure you want to delete all runners", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteRunner, new UserMessage(eAppReporterMessageType.WARN, "Delete Runner", "Are you sure you want to delete selected runner", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteBusinessflows, new UserMessage(eAppReporterMessageType.WARN, "Delete " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), "Are you sure you want to delete all" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteBusinessflow, new UserMessage(eAppReporterMessageType.WARN, "Delete " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), "Are you sure you want to delete selected " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RunsetBuinessFlowWasChanged, new UserMessage(eAppReporterMessageType.WARN, GingerDicser.GetTermResValue(eTermResKey.RunSet) + " " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Changed", "One or more of the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " were changed/deleted." + Environment.NewLine + "You must reload the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " for changes to be viewed/executed." + Environment.NewLine + Environment.NewLine + "Do you want to reload the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) +"?" + Environment.NewLine + Environment.NewLine + "Note: Reload will cause un-saved changes to be lost.", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RunSetReloadhWarn, new UserMessage(eAppReporterMessageType.WARN, "Reload " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Reload process will cause all un-saved changes to be lost." + Environment.NewLine + Environment.NewLine+ " To continue wite reload?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CantDeleteRunner, new UserMessage(eAppReporterMessageType.WARN, "Delete Runner", "You can't delete last Runner, you must have at least one.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Runset Messages

            #region Excel Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExcelNoWorksheetSelected, new UserMessage(eAppReporterMessageType.WARN, "Missing worksheet", "Please select a worksheet", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExcelBadWhereClause, new UserMessage(eAppReporterMessageType.WARN, "Problem with WHERE clause", "Please check your WHERE clause. Do all column names exist and are they spelled correctly?", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Excel Messages

            #region Variables Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectVariable, new UserMessage(eAppReporterMessageType.WARN, "Select " + GingerDicser.GetTermResValue(eTermResKey.Variable), "Please select " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ".", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.VariablesAssignError, new UserMessage(eAppReporterMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Variables) + " Assign Error", "Failed to assign " + GingerDicser.GetTermResValue(eTermResKey.Variables) + "." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SetCycleNumError, new UserMessage(eAppReporterMessageType.ERROR, "Set Cycle Number Error", "Failed to set the cycle number." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.VariablesParentNotFound, new UserMessage(eAppReporterMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Variables) + " Parent Error", "Failed to find the " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " parent.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CantStoreToVariable, new UserMessage(eAppReporterMessageType.ERROR, "Store to " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Failed", "Cannot Store to " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " '{0}'- " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " not found", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WarnRegradingMissingVariablesUse, new UserMessage(eAppReporterMessageType.WARN, "Unassociated " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " been Used in " + GingerDicser.GetTermResValue(eTermResKey.Activity), GingerDicser.GetTermResValue(eTermResKey.Variables) + " which are not part of the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " list are been used in it." + Environment.NewLine + "For the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " to work as a standalone you will need to add those " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: ".") + Environment.NewLine + Environment.NewLine + "Missing " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: ":") + Environment.NewLine + "{1}" + Environment.NewLine + Environment.NewLine + "Do you want to automatically add the missing " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: "?") + Environment.NewLine + "Note: Clicking 'No' will cancel the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Upload.", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NotAllMissingVariablesWereAdded, new UserMessage(eAppReporterMessageType.WARN, "Unassociated " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " been Used in " + GingerDicser.GetTermResValue(eTermResKey.Activity), "Failed to find and add the following missing " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: ":") + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Please add those " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " manually for allowing the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Upload.", MessageBoxButton.OK, MessageBoxResult.No));
            #endregion Variables Messages

            #region Solution Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.BeginWithNoSelectSolution, new UserMessage(eAppReporterMessageType.INFO, "No solution is selected", "You have not selected any existing solution, please open an existing one or create a new solution by pressing the New button.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectSolutionFolder, new UserMessage(eAppReporterMessageType.WARN, "Missing Folder Selection", "Please select the folder you want to perform the action on.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteRepositoryItemAreYouSure, new UserMessage(eAppReporterMessageType.WARN, "Delete", "Are you sure you want to delete '{0}' item?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteTreeFolderAreYouSure, new UserMessage(eAppReporterMessageType.WARN, "Delete Foler", "Are you sure you want to delete the '{0}' folder and all of it content?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RenameRepositoryItemAreYouSure, new UserMessage(eAppReporterMessageType.WARN, "Rename", "Are you sure you want to rename '{0}'?", MessageBoxButton.YesNoCancel, MessageBoxResult.Yes));            
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveBusinessFlowChanges, new UserMessage(eAppReporterMessageType.QUESTION, "Save Changes", "Save Changes to - {0}", MessageBoxButton.YesNoCancel, MessageBoxResult.Yes));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SolutionLoadError, new UserMessage(eAppReporterMessageType.ERROR, "Solution Load Error", "Failed to load the solution." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingAddSolutionInputs, new UserMessage(eAppReporterMessageType.WARN, "Add Solution", "Missing solution inputs, please set the solution name, folder and main application details.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SolutionAlreadyExist, new UserMessage(eAppReporterMessageType.WARN, "Add Solution", "The solution already exist, please select different name/folder.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddSolutionSucceed, new UserMessage(eAppReporterMessageType.INFO, "Add Solution", "The solution was created and loaded successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddSolutionFailed, new UserMessage(eAppReporterMessageType.ERROR, "Add Solution", "Failed to create the new solution. " + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RefreshTreeGroupFailed, new UserMessage(eAppReporterMessageType.ERROR, "Refresh", "Failed to perform the refresh operation." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToDeleteRepoItem, new UserMessage(eAppReporterMessageType.ERROR, "Delete", "Failed to perform the delete operation." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderExistsWithName, new UserMessage(eAppReporterMessageType.WARN, "Folder Creation Failed", "Folder with same name already exists. Please choose a different name for the folder.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UpdateApplicationNameChangeInSolution, new UserMessage(eAppReporterMessageType.WARN, "Target Application Name Change", "Do you want to automatically update the Target Application name in all Solution items?" + Environment.NewLine + Environment.NewLine + "Note: If you choose 'Yes', changes won't be saved, for saving them please click 'SaveAll'", MessageBoxButton.YesNo, MessageBoxResult.Yes));
            
            #endregion Solution Messages

            #region Activities
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActionsDependenciesLoadFailed, new UserMessage(eAppReporterMessageType.ERROR, "Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies Load Failed", "Failed to load the Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies data." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActivitiesDependenciesLoadFailed, new UserMessage(eAppReporterMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies Load Failed", "Failed to load the " + GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies data." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DependenciesMissingActions, new UserMessage(eAppReporterMessageType.INFO, "Missing Actions", "Actions not found." + System.Environment.NewLine + "Please add Actions to the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + ".", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DependenciesMissingVariables, new UserMessage(eAppReporterMessageType.INFO, "Missing " + GingerDicser.GetTermResValue(eTermResKey.Variables), GingerDicser.GetTermResValue(eTermResKey.Variables) + " not found." + System.Environment.NewLine + "Please add " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " from type 'Selection List' to the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + ".", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DuplicateVariable, new UserMessage(eAppReporterMessageType.WARN, "Duplicated " + GingerDicser.GetTermResValue(eTermResKey.Variable), "The " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " name '{0}' and value '{1}' exist more than once." + System.Environment.NewLine + "Please make sure only one instance exist in order to set the " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingActivityAppMapping, new UserMessage(eAppReporterMessageType.WARN, "Missing " + GingerDicser.GetTermResValue(eTermResKey.Activity) + "-Application Mapping", "Target Application was not mapped the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " so the required Actions platform is unknown." + System.Environment.NewLine + System.Environment.NewLine + "Map Target Application to the Activity by double clicking the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " record and select the application you want to test using it.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WarnOnDynamicActivities, new UserMessage(eAppReporterMessageType.QUESTION, "Dynamic " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " Warning", "The dynamically added Shared Repository " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " will not be saved (but they will continue to appear on the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString:".)")+  System.Environment.NewLine + System.Environment.NewLine + "To continue with Save?", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion Activities

            #region Support Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddSupportValidationFailed, new UserMessage(eAppReporterMessageType.WARN, "Add Support Request Validation Error", "Add support request validation failed." + Environment.NewLine + "Failure Reason:" + Environment.NewLine + "'{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UploadSupportRequestSuccess, new UserMessage(eAppReporterMessageType.INFO, "Upload Support Request", "Upload was completed successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UploadSupportRequestFailure, new UserMessage(eAppReporterMessageType.ERROR, "Upload Support Request", "Failed to complete the upload." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Support Messages

            #region SharedFunctions Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FunctionReturnedError, new UserMessage(eAppReporterMessageType.ERROR, "Error Occurred", "The {0} returns error. please check the provided details." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ShowInfoMessage, new UserMessage(eAppReporterMessageType.INFO, "Message", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion SharedFunctions Messages

            #region CustomeFunctions Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.LogoutSuccess, new UserMessage(eAppReporterMessageType.INFO, "Logout", "Logout completed successfully- {0}.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion CustomeFunctions Messages

            #region ALM
            Reporter.UserMessagesPool.Add(eUserMsgKeys.QcConnectSuccess, new UserMessage(eAppReporterMessageType.INFO, "QC/ALM Connection", "QC/ALM connection successful!", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.QcConnectFailure, new UserMessage(eAppReporterMessageType.WARN, "QC/ALM Connection Failed", "QC/ALM connection failed." + System.Environment.NewLine + "Please make sure that the credentials you use are correct and that QC/ALM Client is registered on your machine." + System.Environment.NewLine + System.Environment.NewLine + "For registering QC/ALM Client- please follow below steps:" + System.Environment.NewLine + "1. Launch Internet Explorer as Administrator" + System.Environment.NewLine + "2. Go to http://<QCURL>/qcbin" + System.Environment.NewLine + "3. Click on 'Add-Ins Page' link" + System.Environment.NewLine + "4. In next page, click on 'HP ALM Client Registration'" + System.Environment.NewLine + "5. In next page click on 'Register HP ALM Client'" + System.Environment.NewLine + "6. Restart Ginger and try to reconnect", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.QcConnectFailureRestAPI, new UserMessage(eAppReporterMessageType.WARN, "QC/ALM Connection Failed", "QC/ALM connection failed." + System.Environment.NewLine + "Please make sure that the server url and the credentials you use are correct." , MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ALMConnectFailureWithCurrSettings, new UserMessage(eAppReporterMessageType.WARN, "ALM Connection Failed", "ALM Connection Failed, Please make sure credentials are correct.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ALMConnectFailure, new UserMessage(eAppReporterMessageType.WARN, "ALM Connection Failed", "ALM connection failed." + System.Environment.NewLine + "Please make sure that the credentials you use are correct and that ALM Client is registered on your machine.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.QcLoginSuccess, new UserMessage(eAppReporterMessageType.INFO, "Login Success", "QC/ALM Login successful!", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ALMLoginFailed, new UserMessage(eAppReporterMessageType.WARN, "Login Failed", "ALM Login Failed - {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.QcNeedLogin, new UserMessage(eAppReporterMessageType.WARN, "Not Connected to QC/ALM", "You Must Log Into QC/ALM First.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TestCasesUpdatedSuccessfully, new UserMessage(eAppReporterMessageType.INFO, "TestCase Update", "TestCases Updated Successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TestCasesUploadedSuccessfully, new UserMessage(eAppReporterMessageType.INFO, "TestCases Uploaded", "TestCases Uploaded Successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TestSetsImportedSuccessfully, new UserMessage(eAppReporterMessageType.INFO, "Import ALM Test Set", "ALM Test Set/s import process ended successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TestSetExists, new UserMessage(eAppReporterMessageType.WARN, "Import Exiting Test Set", "The Test Set '{0}' was imported before and already exists in current Solution." + System.Environment.NewLine + "Do you want to delete the existing mapped " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " and import the Test Set again?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ErrorInTestsetImport, new UserMessage(eAppReporterMessageType.ERROR, "Import Test Set Error", "Error Occurred while exporting the Test Set '{0}'." + System.Environment.NewLine + "Error Details:{1}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ErrorWhileExportingExecDetails, new UserMessage(eAppReporterMessageType.ERROR, "Export Execution Details Error", "Error occurred while exporting the execution details to QC/ALM." + System.Environment.NewLine + "Error Details:{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportedExecDetailsToALM, new UserMessage(eAppReporterMessageType.INFO, "Export Execution Details", "Export execution details result: {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportAllItemsToALMSucceed, new UserMessage(eAppReporterMessageType.INFO, "Export All Items to ALM", "All items has been successfully exported to ALM.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportAllItemsToALMFailed, new UserMessage(eAppReporterMessageType.INFO, "Export All Items to ALM", "While exporting to ALM One or more items failed to export.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportItemToALMSucceed, new UserMessage(eAppReporterMessageType.INFO, "Export ALM Item", "Exporting item to ALM process ended successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportedExecDetailsToALMIsInProcess, new UserMessage(eAppReporterMessageType.INFO, "Export Execution Details", "Please Wait, Exporting Execution Details is inprocess.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ALMOperationFailed, new UserMessage(eAppReporterMessageType.ERROR, "ALM Operation Failed", "Failed to perform the {0} operation." + Environment.NewLine + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActivitiesGroupAlreadyMappedToTC, new UserMessage(eAppReporterMessageType.WARN, "Export " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to QC/ALM", "The " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " '{0}' is already mapped to the QC/ALM '{1}' Test Case, do you want to update it?" + Environment.NewLine + Environment.NewLine + "Select 'Yes' to update or 'No' to create new Test Case.", MessageBoxButton.YesNoCancel, MessageBoxResult.Cancel));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.BusinessFlowAlreadyMappedToTC, new UserMessage(eAppReporterMessageType.WARN, "Export " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to QC/ALM", "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " '{0}' is already mapped to the QC/ALM '{1}' Test Set, do you want to update it?" + Environment.NewLine + Environment.NewLine + "Select 'Yes' to update or 'No' to create new Test Set.", MessageBoxButton.YesNoCancel, MessageBoxResult.Cancel));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportQCNewTestSetSelectDiffFolder, new UserMessage(eAppReporterMessageType.INFO, "Export QC Item - Creating new Test Set", "Please select QC folder to export to that the Test Set does not exist there.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportItemToALMFailed, new UserMessage(eAppReporterMessageType.ERROR, "Export to ALM Failed", "The {0} '{1}' failed to be exported to ALM." + Environment.NewLine + Environment.NewLine + "Error Details: {2}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfToSaveBFAfterExport, new UserMessage(eAppReporterMessageType.QUESTION, "Save Links to QC/ALM Items", "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " '{0}' must be saved for keeping the links to QC/ALM items." + Environment.NewLine + "To perform the save now?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskBeforeDefectProfileDeleting, new UserMessage(eAppReporterMessageType.QUESTION, "Profiles Deleting", "After deletion there will be no way to restore deleted profiles.\nAre you sure that you want to delete the selected profiles?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissedMandatotryFields, new UserMessage(eAppReporterMessageType.INFO, "Profiles Saving", "Please, populate value for mandatotry field '{0}' of '{1}' Defect Profile", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoDefaultDefectProfileSelected, new UserMessage(eAppReporterMessageType.INFO, "Profiles Saving", "Please, select one of the Defect Profiles to be a 'Default'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.IssuesInSelectedDefectProfile, new UserMessage(eAppReporterMessageType.INFO, "ALM Defects Opening", "Please, revise the selected Defect Profile, current fields/values are not corresponded with ALM", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoDefectProfileCreated, new UserMessage(eAppReporterMessageType.INFO, "Defect Profiles", "Please, create at least one Defect Profile", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WrongValueSelectedFromTheList, new UserMessage(eAppReporterMessageType.INFO, "Profiles Saving", "Please, select one of the existed values from the list\n(Field '{0}', Defect Profile '{1}')", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WrongNonNumberValueInserted, new UserMessage(eAppReporterMessageType.INFO, "Profiles Saving", "Please, insert numeric value\n(Field '{0}', Defect Profile '{1}')", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WrongDateValueInserted, new UserMessage(eAppReporterMessageType.INFO, "Profiles Saving", "Please, insert Date in format 'yyyy-mm-dd'\n(Field '{0}', Defect Profile '{1}')", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ALMDefectsWereOpened, new UserMessage(eAppReporterMessageType.INFO, "ALM Defects Opening", "{0} ALM Defects were opened", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskALMDefectsOpening, new UserMessage(eAppReporterMessageType.QUESTION, "ALM Defects Opening", "Are you sure that you want to open {0} ALM Defects?", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion ALM

            #region ALM External Items Fields Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfToDownloadPossibleValuesShortProcesss, new UserMessage(eAppReporterMessageType.QUESTION, "ALM External Items Fields", "Would you like to download and save possible values for Categories Items? ", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfToDownloadPossibleValues, new UserMessage(eAppReporterMessageType.QUESTION, "ALM External Items Fields", "Would you like to download and save possible values for Categories Items? " + Environment.NewLine + "This process could take up to several hours.", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SelectAndSaveCategoriesValues, new UserMessage(eAppReporterMessageType.QUESTION, "ALM External Items Fields", "Please select values for each Category Item and click on Save", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion ALM External Items Fields Messages

            #region POM
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMSearchByGUIDFailed, new UserMessage(eAppReporterMessageType.WARN, "POM not found", "Previously saved POM not found. Please choose another one.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMElementSearchByGUIDFailed, new UserMessage(eAppReporterMessageType.WARN, "POM Element not found", "Previously saved POM Element not found. Please choose another one.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoRelevantAgentInRunningStatus, new UserMessage(eAppReporterMessageType.WARN, "No Relevant Agent In Running Status", "Relevant Agent In should be up and running in order to see the highlighted element.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMWizardFailedToLearnElement, new UserMessage(eAppReporterMessageType.WARN, "Learn Elements Failed", "Error occured while learning the elements." + Environment.NewLine + "Error Details:" + Environment.NewLine + "'{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMWizardReLearnWillDeleteAllElements, new UserMessage(eAppReporterMessageType.WARN, "Re-Learn Elements", "Re-Learn Elements will delete all existing elements" + Environment.NewLine + "Do you want to continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMDriverIsBusy, new UserMessage(eAppReporterMessageType.WARN, "Driver Is Busy", "Operation cannot be complete because the Driver is busy with learning operation" + Environment.NewLine + "Do you want to continue?", MessageBoxButton.OK, MessageBoxResult.OK));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMAgentIsNotRunning, new UserMessage(eAppReporterMessageType.WARN, "Agent is Down", "In order to perform this operation the Agent needs to be up and running." + Environment.NewLine + "Please start the agent and re-try", MessageBoxButton.OK, MessageBoxResult.OK));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMNotOnThePageWarn, new UserMessage(eAppReporterMessageType.WARN, "Not On the Same Page", "'{0}' Elements out of '{1}' Elements failed to be found on the page" + Environment.NewLine + "Looks like you are not on the right page" + Environment.NewLine + "Do you want to continue?", MessageBoxButton.YesNo, MessageBoxResult.Yes));

            #endregion POM


            #region UnitTester Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TestCompleted, new UserMessage(eAppReporterMessageType.INFO, "Test Completed", "Test completed successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CantFindObject, new UserMessage(eAppReporterMessageType.INFO, "Missing Object", "Cannot find the {0}, the ID is: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion UnitTester Messages

            #region CommandLineParams
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UnknownParamInCommandLine, new UserMessage(eAppReporterMessageType.ERROR, "Unknown Parameter", "Parameter not recognized {0}", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion CommandLineParams Messages

            #region Tree / Grid
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ConfirmToAddTreeItem, new UserMessage(eAppReporterMessageType.QUESTION, "Add New Item to Tree", "Are you Sure you want to add new item to tree?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToAddTreeItem, new UserMessage(eAppReporterMessageType.ERROR, "Add Tree Item", "Failed to add the tree item '{0}'." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SureWantToDeleteAll, new UserMessage(eAppReporterMessageType.QUESTION, "Delete All", "Are you sure you want to delete all?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoItemToDelete, new UserMessage(eAppReporterMessageType.WARN, "Delete All", "Didn't found item to delete", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SelectItemToDelete, new UserMessage(eAppReporterMessageType.WARN, "Delete", "Please select items to delete", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SelectItemToAdd, new UserMessage(eAppReporterMessageType.WARN, "Add New Item", "Please select an Activity on which you want to add a new Action", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToloadTheGrid, new UserMessage(eAppReporterMessageType.ERROR, "Load Grid", "Failed to load the grid." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DragDropOperationFailed, new UserMessage(eAppReporterMessageType.ERROR, "Drag and Drop", "Drag & Drop operation failed." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SelectValidColumn, new UserMessage(eAppReporterMessageType.WARN, "Column Not Found", "Please select a valid column", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SelectValidRow, new UserMessage(eAppReporterMessageType.WARN, "Row Not Found", "Please select a valid row", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Tree / Grid

            #region ActivitiesGroup
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoActivitiesGroupWasSelected, new UserMessage(eAppReporterMessageType.WARN, "Missing " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " Selection", "No " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " was selected.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActivitiesGroupActivitiesNotFound, new UserMessage(eAppReporterMessageType.WARN, "Import " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " " + GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Activities) + " to import were not found in the repository.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.PartOfActivitiesGroupActsNotFound, new UserMessage(eAppReporterMessageType.WARN, "Import " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " " + GingerDicser.GetTermResValue(eTermResKey.Activities), "The following " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to import were not found in the repository:" + System.Environment.NewLine + "{0}", MessageBoxButton.OK, MessageBoxResult.None));           
            #endregion ActivitiesGroup

            #region Mobile
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MobileConnectionFailed, new UserMessage(eAppReporterMessageType.ERROR, "Mobile Connection", "Failed to connect to the mobile device." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MobileRefreshScreenShotFailed, new UserMessage(eAppReporterMessageType.ERROR, "Mobile Screen Image", "Failed to refresh the mobile device screen image." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MobileShowElementDetailsFailed, new UserMessage(eAppReporterMessageType.ERROR, "Mobile Elements Inspector", "Failed to locate the details of the selected element." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MobileActionWasAdded, new UserMessage(eAppReporterMessageType.INFO, "Add Action", "Action was added.", MessageBoxButton.OK, MessageBoxResult.None));
            //Reporter.UserMessagesPool.Add(eUserMsgKeys.MobileActionWasAdded, new UserMessage(eMessageType.INFO, "Add Action", "Action was added." + System.Environment.NewLine + System.Environment.NewLine + "Do you want to run the Action?", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion Mobile

            #region Reports
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ReportTemplateNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Report Template", "Report Template '{0}' not found", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.AutomationTabExecResultsNotExists, new UserMessage(eAppReporterMessageType.WARN, "Execution Results are not existing", "Results from last execution are not existing (yet). Nothing to report, please wait for execution finish and click on report creation.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderNamesAreTooLong, new UserMessage(eAppReporterMessageType.WARN, "Folders Names Are Too Long", "Provided folders names are too long. Please change them to be less than 100 characters", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderNameTextBoxIsEmpty, new UserMessage(eAppReporterMessageType.WARN, "Folders Names is Empty", "Please provide a proper folder name.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.UserHaveNoWritePermission, new UserMessage(eAppReporterMessageType.WARN, "User Have No Write Permission On Folder", "User that currently in use have no write permission on selected folder. Pay attention that attachment at shared folder may be not created.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderSizeTooSmall, new UserMessage(eAppReporterMessageType.WARN, "Folders Size is Too Small", "Provided folder size is Too Small. Please change it to be bigger than 50 Mb", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderNotExistOrNotAvailible, new UserMessage(eAppReporterMessageType.WARN, "Folder Not Exist Or Not Available", "Folder Not Exist Or Not Available. Please select another one.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.DefaultTemplateCantBeDeleted, new UserMessage(eAppReporterMessageType.WARN, "Default Template Can't Be Deleted", "Default Template Can't Be Deleted. Please change it to be a non-default", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExecutionsResultsProdIsNotOn, new UserMessage(eAppReporterMessageType.WARN, "Executions Results Producing Should Be Switched On", "In order to perform this action, executions results producing should be switched On", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExecutionsResultsNotExists, new UserMessage(eAppReporterMessageType.WARN, "Executions Results Are Not Existing Yet", "In order to get HTML report, please, perform executions before", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExecutionsResultsToDelete, new UserMessage(eAppReporterMessageType.QUESTION, "Delete Executions Results", "Are you sure you want to delete selected Executions Results?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.AllExecutionsResultsToDelete, new UserMessage(eAppReporterMessageType.QUESTION, "Delete All Executions Results", "Are you sure you want to delete all Executions Results?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.HTMLReportAttachment, new UserMessage(eAppReporterMessageType.WARN, "HTML Report Attachment", "HTML Report Attachment already exists, please delete existing one.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ImageSize, new UserMessage(eAppReporterMessageType.WARN, "Image Size", "Image Size should be less than {0} Kb", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ReportsTemplatesSaveWarn, new UserMessage(eAppReporterMessageType.WARN, "Default Template Report Change", "Default change will cause all templates to be updated and saved, to continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            

            #endregion Reports

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ApplicationNotFoundInEnvConfig, new UserMessage(eAppReporterMessageType.ERROR, "Application Not Found In EnvConfig", "Application = {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExecutionReportSent, new UserMessage(eAppReporterMessageType.INFO, "Publish", "Execution report sent by email", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ErrorReadingRepositoryItem, new UserMessage(eAppReporterMessageType.ERROR, "Error Reading Repository Item", "Repository Item {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.EnvNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Env not found", "Env not found {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.CannotAddGinger, new UserMessage(eAppReporterMessageType.ERROR, "Cannot Add Ginger", "Number of Gingers is limited to 12 Gingers.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ShortcutCreated, new UserMessage(eAppReporterMessageType.INFO, "New Shortcut created", "Shortcut created on desktop - {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.SolutionFileNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Solution File Not Found", "Cannot find Solution File at - {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.PlugInFileNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Plugin Configuration File Not Found", "Cannot find Plugin Configuration File at - {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToLoadPlugIn, new UserMessage(eAppReporterMessageType.ERROR, "Failed to Load Plug In", "Ginger could not load the plug in '{0}'" + Environment.NewLine + "Error Details: {1}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActionIDNotFound, new UserMessage(eAppReporterMessageType.ERROR, "Action ID Not Found", "Cannot find action with ID - {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActivityIDNotFound, new UserMessage(eAppReporterMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Activity) + " ID Not Found", "Cannot find " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " with ID - {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToSendEmail, new UserMessage(eAppReporterMessageType.ERROR, "Failed to Send E-mail", "Failed to send the {0} over e-mail using {1}." + Environment.NewLine + Environment.NewLine + "Error Details: {2}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToExportBF, new UserMessage(eAppReporterMessageType.ERROR, "Failed to export BusinessFlow to CSV", "Failed to export BusinessFlow to CSV file. Got error {0}" + Environment.NewLine, MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FoundDuplicateAgentsInRunSet, new UserMessage(eAppReporterMessageType.ERROR, "Found Duplicate Agents in " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Agent name '{0}' is defined more than once in the Run Set", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FileNotExist, new UserMessage(eAppReporterMessageType.WARN, "XML File Not Exist", "XML File has not been found on the specified path, either deleted or been re-named on the define XML path", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FilterNotBeenSet, new UserMessage(eAppReporterMessageType.QUESTION, "Filter Not Been Set", "No filtering criteria has been set, Retrieving all elements from page can take long time to complete, Do you want to continue?", MessageBoxButton.OKCancel, MessageBoxResult.Cancel));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.CloseFilterPage, new UserMessage(eAppReporterMessageType.QUESTION, "Confirmation", "'{0}' Elements Found, Close Filtering Page?", MessageBoxButton.YesNo, MessageBoxResult.Yes));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.RetreivingAllElements, new UserMessage(eAppReporterMessageType.QUESTION, "Retrieving all elements", "Retrieving all elements from page can take long time to complete, Do you want to continue?", MessageBoxButton.OKCancel, MessageBoxResult.Cancel));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.WhetherToOpenSolution, new UserMessage(eAppReporterMessageType.QUESTION, "Open Downloaded Solution?" ,"Do you want to open the downloaded Solution?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ClickElementAgain, new UserMessage(eAppReporterMessageType.INFO, "Please Click" ,"Please click on the desired element again", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.CurrentActionNotSaved, new UserMessage(eAppReporterMessageType.INFO, "Current Action Not Saved", "Before using the 'next/previous action' button, Please save the current action", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.Failedtosaveitems, new UserMessage(eAppReporterMessageType.ERROR, "Failed to Save", "Failed to do Save All", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.AllItemsSaved, new UserMessage(eAppReporterMessageType.INFO, "All Changes Saved", "All Changes Saved", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfShareVaribalesInRunner, new UserMessage(eAppReporterMessageType.QUESTION, "Share Variables", "Are you sure you want to share selected Variable Values to all the similar Business Flows and Activities accross all Runners?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.RenameItemError, new UserMessage(eAppReporterMessageType.ERROR, "Rename", "Failed to rename the Item. Error: '{0}'?", MessageBoxButton.OK, MessageBoxResult.OK));
            #region ActionConversion
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingTargetPlatformForConversion, new UserMessage(eAppReporterMessageType.WARN,
               "Missing Target Platform for Conversion", "For {0}, you need to add a Target Application of type {1}. Please add it to your Business Flow. " + Environment.NewLine +
               "Do you want to continue with the conversion?",
               MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoConvertibleActionsFound, new UserMessage(eAppReporterMessageType.INFO, "No Convertible Actions Found", "The selected "+ GingerDicser.GetTermResValue(eTermResKey.Activity) + " doesn't contain any convertible actions.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoConvertibleActionSelected, new UserMessage(eAppReporterMessageType.WARN, "No Convertible Action Selected", "Please select the actions that you want to convert.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SuccessfulConversionDone, new UserMessage(eAppReporterMessageType.INFO, "Obsolete actions converted successfully", "The obsolete actions have been converted successfully!"
               + Environment.NewLine + "Do you want to convert more actions?" , MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoActivitySelectedForConversion, new UserMessage(eAppReporterMessageType.WARN, "No Activity Selected", "Please select an " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " that you want to convert.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActivitiesConversionFailed, new UserMessage(eAppReporterMessageType.WARN, "Activities Conversion Failed", "Activities Conversion Failed.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion ActionConversion

            Reporter.UserMessagesPool.Add(eUserMsgKeys.CopiedVariableSuccessfully, new UserMessage(eAppReporterMessageType.INFO, "Info Message", "'{0}' Business Flows Affected." + Environment.NewLine + Environment.NewLine + "Notice: Un-saved changes won't be saved.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ShareVariableNotSelected, new UserMessage(eAppReporterMessageType.INFO, "Info Message", "Please select the variables to share.", MessageBoxButton.OK, MessageBoxResult.None));

            //Reporter.UserMessagesPool.Add(eUserMsgKeys.APIParametersListUpdated, new UserMessage(eMessageType.WARN, "API Model Parameters Difference", "Difference was identified between the list of parameters which configured on the API Model and the parameters exists on the Action.\n\nDo you want to update the Action parameters?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.APIParametersListUpdated, new UserMessage(eAppReporterMessageType.INFO, "API Model Parameters Difference", "Difference was identified between the list of parameters which configured on the API Model and the parameters exists on the Action.\n\nAll Action parameters has been updated.", MessageBoxButton.OK, MessageBoxResult.OK));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.APIMappedToActionIsMissing, new UserMessage(eAppReporterMessageType.WARN, "Missing Mapped API Model", "The API Model which mapped to this action is missing, please remap API Model to the action.", MessageBoxButton.OK, MessageBoxResult.OK));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoAPIExistToMappedTo, new UserMessage(eAppReporterMessageType.WARN, "No API Model Found", "API Models repository is empty, please add new API Models into it and map it to the action", MessageBoxButton.OK, MessageBoxResult.OK));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.APIModelAlreadyContainsReturnValues, new UserMessage(eAppReporterMessageType.WARN, "Return Values Already Exsist Warning", "{0} Return Values already exsist for this API Model" + Environment.NewLine + "Do you want to overwride them by importing form new response template file?" + Environment.NewLine + Environment.NewLine + "Please Note! - by clicking yes all {0} return values will be deleted with no option to restore.", MessageBoxButton.YesNo, MessageBoxResult.No));


            Reporter.UserMessagesPool.Add(eUserMsgKeys.VisualTestingFailedToDeleteOldBaselineImage, new UserMessage(eAppReporterMessageType.WARN, "Creating New Baseline Image", "Error while tyring to create and save new Baseline Image.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ApplitoolsLastExecutionResultsNotExists, new UserMessage(eAppReporterMessageType.INFO, "Show Last Execution Results", "No last execution results exists, Please run first the action, Close Applitools Eyes and then view the results.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ApplitoolsMissingChromeOrFirefoxBrowser, new UserMessage(eAppReporterMessageType.INFO, "View Last Execution Results", "Applitools support only Chrome or Firefox Browsers, Please install at least one of them in order to browse Applitools URL results.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterOptionalValues, new UserMessage(eAppReporterMessageType.INFO, "Parameters Optional Values", "{0} parameters were updated.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.RecoverItemsMissingSelectionToRecover, new UserMessage(eAppReporterMessageType.WARN, "Recover Changes", "Please select valid Recover items to {0}", MessageBoxButton.OK, MessageBoxResult.None));
            #region ErrorHandlingMapping
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingErrorHandler, new UserMessage(eAppReporterMessageType.WARN, "Mismatch in Mapped Error Handler Found", "A mismatch has been detected in error handlers mapped with your activity." + Environment.NewLine + "Please check if any mapped error handler has been deleted or marked as inactive. ", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.BusinessFlowUpdate, new UserMessage(eAppReporterMessageType.INFO, "Business Flow", "BusinessFlow '{0}' {1} Sucessfully", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion ErrorHandlingMapping

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FindAndRepalceFieldIsEmpty, new UserMessage(eAppReporterMessageType.WARN, "Field is Empty", "Field '{0}' cannot be empty", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FindAndReplaceListIsEmpty, new UserMessage(eAppReporterMessageType.WARN, "List is Empty", "No items were found hence nothing can be replaced", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FindAndReplaceNoItemsToRepalce, new UserMessage(eAppReporterMessageType.WARN, "No Suitable Items", "No suitable items selected to replace.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FindAndReplaceViewRunSetNotSupported, new UserMessage(eAppReporterMessageType.INFO, "View Run Set", "View RunSet is not supported.", MessageBoxButton.OK, MessageBoxResult.None));

           
            
            
        }
    }
}
