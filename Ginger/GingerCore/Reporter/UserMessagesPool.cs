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
        FolderExistsWithName, DownloadedSolutionFromSourceControl, SourceControlFileLockedByAnotherUser,
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
        POMSearchByGUIDFailed, POMElementSearchByGUIDFailed, NoRelevantAgentInRunningStatus,InvalidIndexValue, FileOperationError, FolderOperationError, ObjectUnavailable, PatternNotHandled, LostConnection, AskToSelectBusinessflow,
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
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CompilationErrorOccured, new UserMessage(eMessageType.ERROR, "Compilation Error Occurred", "Compilation error occurred." + Environment.NewLine + "Error Details: "+ Environment.NewLine +" '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CompilationSucceed, new UserMessage(eMessageType.INFO, "Compilation Succeed", "Compilation Passed successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FileExtensionNotSupported, new UserMessage(eMessageType.ERROR, "File Extension Not Supported", "The selected file extension is not supported by this editor." + Environment.NewLine + "Supported extensions: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NotifyFileSelectedFromTheSolution, new UserMessage(eMessageType.ERROR, "File Already Exists", "File - '{0}'." + Environment.NewLine + "Selected From The Solution folder hence its already exist and cannot be copy to the same place" + Environment.NewLine + "Please select another File to continue.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FileImportedSuccessfully, new UserMessage(eMessageType.INFO, "File imported successfully", "The File was imported successfully", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinNotifyFeatureFileExists, new UserMessage(eMessageType.ERROR, "Feature File Already Exists", "Feature File with the same name already exist - '{0}'." + Environment.NewLine + "Please select another Feature File to continue.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinNotifyFeatureFileSelectedFromTheSolution, new UserMessage(eMessageType.ERROR, "Feature File Already Exists", "Feature File - '{0}'." + Environment.NewLine + "Selected From The Solution folder hence its already exist and cannot be copy to the same place"+ Environment.NewLine + "Please select another Feature File to continue.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinNotifyBFIsNotExistForThisFeatureFile, new UserMessage(eMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)+" Is Not Exists", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)+ " has to been generated for Feature File - '{0}'." + Environment.NewLine  + Environment.NewLine + "Please create " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " from the Editor Page.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinFileNotFound, new UserMessage(eMessageType.ERROR, "Gherkin File Not Found", "Gherkin file was not found at the path: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinColumnNotExist, new UserMessage(eMessageType.WARN, "Column Not Exist", "Cant find value for: '{0}' Item/s. since column/s not exist in example table", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinActivityNotFound, new UserMessage(eMessageType.ERROR, "Activity Not Found", "Activity not found, Name: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinBusinessFlowNotCreated, new UserMessage(eMessageType.WARN, "Gherkin "+ GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Creation Failed", "The file did not passed Gherkin compilation hence the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " not created" + Environment.NewLine + "please correct the imported file and create the Business Flow from Gherkin Page", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinFeatureFileImportedSuccessfully, new UserMessage(eMessageType.INFO, "Gherkin feature file imported successfully", "Gherkin feature file imported successfully", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinFeatureFileImportOnlyFeatureFileAllowedErrorMessage, new UserMessage(eMessageType.ERROR, "Gherkin feature file not valid", "Only Gherkin feature files can be imported", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinAskToSaveFeatureFile, new UserMessage(eMessageType.WARN, "Feature File Changes were made", "Do you want to Save the Feature File?" + Environment.NewLine + "WARNING: If you do not manually Save your Feature File, you may loose your work if you close out of Ginger.", MessageBoxButton.YesNoCancel, MessageBoxResult.Yes));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GherkinScenariosGenerated, new UserMessage(eMessageType.INFO, "Scenarios generated", "{0} Scenarios generated successfully", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GeneralErrorOccured, new UserMessage(eMessageType.ERROR, "Error Occurred", "Application error occurred." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingImplementation, new UserMessage(eMessageType.WARN, "Missing Implementation", "The {0} functionality hasn't been implemented yet.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingImplementation2, new UserMessage(eMessageType.WARN, "Missing Implementation", "The functionality hasn't been implemented yet.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ApplicationInitError, new UserMessage(eMessageType.ERROR, "Application Initialization Error", "Error occurred during application initialization." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.PageLoadError, new UserMessage(eMessageType.ERROR, "Page Load Error", "Failed to load the page '{0}'." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UserProfileLoadError, new UserMessage(eMessageType.ERROR, "User Profile Load Error", "Error occurred during user profile loading." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemToSaveWasNotSelected, new UserMessage(eMessageType.WARN, "Save", "Item to save was not selected.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RecommendNewVersion, new UserMessage(eMessageType.WARN, "Upgrade required", "You are not using the latest version of Ginger. Please go to https://github.com/Ginger-Automation/Ginger/releases to get the latest build.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ToSaveChanges, new UserMessage(eMessageType.QUESTION, "Save Changes?", "Do you want to save changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UnsupportedFileFormat, new UserMessage(eMessageType.ERROR, "Save Changes", "File format not supported.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.CtrlSsaveEnvApp, new UserMessage(eMessageType.WARN, "Save Environment", "Please select the environment to save.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CtrlSMissingItemToSave, new UserMessage(eMessageType.WARN, "Missing Item to Save", "Please select individual item to save or use menu toolbar to save all the items.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.StaticErrorMessage, new UserMessage(eMessageType.ERROR, "Error Message", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidIndexValue, new UserMessage(eMessageType.ERROR, "Invalid index value", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FileOperationError, new UserMessage(eMessageType.ERROR, "Error occured during file operation", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderOperationError, new UserMessage(eMessageType.ERROR, "Error occured during folder operation", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ObjectUnavailable, new UserMessage(eMessageType.ERROR, "Object Unavailable", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.PatternNotHandled, new UserMessage(eMessageType.ERROR, "Pattern not handled", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.LostConnection, new UserMessage(eMessageType.ERROR, "Lost Connection", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectBusinessflow, new UserMessage(eMessageType.INFO, "Select Businessflow", "Please select " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingFileLocation, new UserMessage(eMessageType.INFO, "Missing file location", "Please select a location for the file.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ScriptPaused, new UserMessage(eMessageType.INFO, "Script Paused", "Script is paused!", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ElementNotFound, new UserMessage(eMessageType.ERROR, "Element Not Found", "Element not found", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TextNotFound, new UserMessage(eMessageType.ERROR, "Text Not Found", "Text not found", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ProvideSearchString, new UserMessage(eMessageType.ERROR, "Provide Search String", "Please provide search string", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoTextOccurrence, new UserMessage(eMessageType.ERROR, "No Text Occurrence", "No more text occurrence", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToInitiate, new UserMessage(eMessageType.ERROR, "Failed to initiate", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToCreateRequestResponse, new UserMessage(eMessageType.ERROR, "Failed to create Request / Response", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActionNotImplemented, new UserMessage(eMessageType.ERROR, "Action is not implemented yet for control type ", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ValueIssue, new UserMessage(eMessageType.ERROR, "Value Issue", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.JSExecutionFailed, new UserMessage(eMessageType.ERROR, "Error Executing JS", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingTargetApplication, new UserMessage(eMessageType.ERROR, "Missing target ppplication", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ThreadError, new UserMessage(eMessageType.ERROR, "Thread Exception", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParsingError, new UserMessage(eMessageType.ERROR, "Error while parsing", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SpecifyUniqueValue, new UserMessage(eMessageType.WARN, "Please specify unique value", "Optional value already exists", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterAlreadyExists, new UserMessage(eMessageType.WARN, "Cannot upload the selected parameter", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteNodesFromRequest, new UserMessage(eMessageType.WARN, "Delete nodes from request body?", "Do you want to delete also nodes from request body that contain those parameters?", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterMerge, new UserMessage(eMessageType.WARN, "Models Parameters Merge", "Do you want to update the merged instances on all model configurations?", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterEdit, new UserMessage(eMessageType.WARN, "Global Parameter Edit", "Global Parameter Place Holder cannot be edit.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterUpdate, new UserMessage(eMessageType.WARN, "Update Global Parameter Value Expression Instances", "{0}", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterDelete, new UserMessage(eMessageType.WARN, "Delete Parameter", "{0}", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveAll, new UserMessage(eMessageType.WARN, "Save All", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveSelected, new UserMessage(eMessageType.WARN, "Save Selected", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CopiedErrorInfo, new UserMessage(eMessageType.INFO, "Copied Error information", "Error Information copied to Clipboard", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RepositoryNameCantEmpty, new UserMessage(eMessageType.WARN, "QTP to Ginger Converter", "Object Repository name cannot be empty", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExcelProcessingError, new UserMessage(eMessageType.ERROR, "Excel processing error", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.EnterValidBusinessflow, new UserMessage(eMessageType.WARN, "Enter valid businessflow", "Please enter a Valid " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Name", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteItem, new UserMessage(eMessageType.WARN, "Delete Item", "Are you sure you want to delete '{0}' item ?", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RefreshFolder, new UserMessage(eMessageType.WARN, "Refresh Folder", "Un saved items changes under the refreshed folder will be lost, to continue with refresh?", MessageBoxButton.YesNo, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RefreshFailed, new UserMessage(eMessageType.ERROR, "Refresh Failed", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ReplaceAll, new UserMessage(eMessageType.QUESTION, "Replace All", "{0}", MessageBoxButton.OKCancel, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemSelection, new UserMessage(eMessageType.WARN, "Item Selection", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DifferentItemType, new UserMessage(eMessageType.WARN, "Not Same Item Type", "The source item type do not match to the destination folder type", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CopyCutOperation, new UserMessage(eMessageType.INFO, "Copy/Cut Operation", "Please select Copy/Cut operation first.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ObjectLoad, new UserMessage(eMessageType.ERROR, "Object Load", "Not able to load the object details", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.StaticWarnMessage, new UserMessage(eMessageType.WARN, "Warning Message", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.StaticInfoMessage, new UserMessage(eMessageType.INFO, "Info Message", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfSureWantToClose, new UserMessage(eMessageType.QUESTION, "Close Ginger", "Are you sure you want to close Ginger?" + Environment.NewLine + Environment.NewLine + "Notice: Un-saved changes won't be saved.", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.BusinessFlowNeedTargetApplication, new UserMessage(eMessageType.WARN, "Target Application Not Selected", "Target Application Not Selected! Please Select at least one Target Application", MessageBoxButton.OK, MessageBoxResult.None));
         
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfSureWantToUndoChange, new UserMessage(eMessageType.WARN, "Undo Changes", "Are you sure you want to undo all changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InitializeBrowser, new UserMessage(eMessageType.INFO, "Initialize Browser", "Initialize Browser action automatically added."+Environment.NewLine+"Please continue to spy the required element.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.LoseChangesWarn, new UserMessage(eMessageType.WARN, "Save Changes", "The operation may result with lost of un-saved local changes." + Environment.NewLine + "Please make sure all changes were saved before continue." + Environment.NewLine + Environment.NewLine + "To perform the operation?", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion General Application Messages

            #region Settings
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SettingsChangeRequireRestart, new UserMessage(eMessageType.INFO, "Settings Change", "For the settings change to take affect you must restart Ginger.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Settings

            #region Repository
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemNameExistsInRepository, new UserMessage(eMessageType.WARN, "Add Item to Repository", "Item with the name '{0}' already exist in the repository." + Environment.NewLine + Environment.NewLine + "Please change the item name to be unique and try to upload it again.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemExistsInRepository, new UserMessage(eMessageType.WARN, "Add Item to Repository", "The item '{0}' already exist in the repository (item name in repository is '{1}')." + Environment.NewLine + Environment.NewLine + "Do you want to continue and overwrite it?" + Environment.NewLine + Environment.NewLine + "Note: If you select 'Yes', backup of the existing item will be saved into 'PreVersions' folder.", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemExternalExistsInRepository, new UserMessage(eMessageType.WARN, "Add Item to Repository", "The item '{0}' is mapped to the same external item like the repository item '{1}'." + Environment.NewLine + Environment.NewLine + "Do you want to continue and overwrite the existing repository item?" + Environment.NewLine + Environment.NewLine + "Note: If you select 'Yes', backup of the existing repository item will be saved into 'PreVersions' folder.", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ItemParentExistsInRepository, new UserMessage(eMessageType.WARN, "Add Item to Repository", "The item '{0}' source is from the repository item '{1}'." + Environment.NewLine + Environment.NewLine + "Do you want to overwrite the source repository item?" + Environment.NewLine + Environment.NewLine + "Note:" + Environment.NewLine + "If you select 'No', the item will be added as a new item to the repository." + Environment.NewLine + "If you select 'Yes', backup of the existing repository item will be saved into 'PreVersions' folder.", MessageBoxButton.YesNoCancel, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToAddItemToSharedRepository, new UserMessage(eMessageType.ERROR, "Add Item to Repository", "Failed to add the item '{0}' to shared repository." + Environment.NewLine + Environment.NewLine + "Error Details: {1}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfWantsToUpdateRepoItemInstances, new UserMessage(eMessageType.WARN, "Update Repository Item Usages", "The item '{0}' has {1} instances." + Environment.NewLine + Environment.NewLine + "Do you want to review them and select which one to get updated as well?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfWantsToChangeeRepoItem, new UserMessage(eMessageType.WARN, "Change Repository Item", "The item '{0}' is been used in {1} places." + Environment.NewLine + Environment.NewLine + "Are you sure you want to {2} it?" + Environment.NewLine + Environment.NewLine + "Note: Anyway the changes won't affect the linked instances of this item", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GetRepositoryItemUsagesFailed, new UserMessage(eMessageType.ERROR, "Repository Item Usages", "Failed to get the '{0}' item usages." + Environment.NewLine + Environment.NewLine + "Error Details: {1}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GetModelItemUsagesFailed, new UserMessage(eMessageType.ERROR, "Model Item Usages", "Failed to get the '{0}' item usages." + Environment.NewLine + Environment.NewLine + "Error Details: {1}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UpdateRepositoryItemUsagesSuccess, new UserMessage(eMessageType.INFO, "Update Repository Item Usages", "Finished to update the repository items usages." + Environment.NewLine + Environment.NewLine + "Note: Updates were not saved yet.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfSureWantToDeLink, new UserMessage(eMessageType.WARN, "De-Link to Shared Repository", "Are you sure you want to de-link the item from it Shared Repository source item?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.OfferToUploadAlsoTheActivityGroupToRepository, new UserMessage(eMessageType.QUESTION, "Add the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to Repository", "The " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " '{0}' is part of the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " '{1}', do you want to add the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to the shared repository as well?" + System.Environment.NewLine + System.Environment.NewLine + "Note: If you select Yes, only the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " will be added to the repository and not all of it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + ".", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion Repository

            #region Analyzer
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AnalyzerFoundIssues, new UserMessage(eMessageType.WARN, "Issues Detected By Analyzer", "Critical/High Issues were detected, please handle them before execution.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AnalyzerSaveRunSet, new UserMessage(eMessageType.WARN, "Issues Detected By Analyzer", "Please save the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " first", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveItemParentWarning, new UserMessage(eMessageType.WARN, "Item Parent Save", "Save process will actually save the item {0} parent which called '{1}'." + System.Environment.NewLine + System.Environment.NewLine + "To continue with save?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveAllItemsParentWarning, new UserMessage(eMessageType.WARN, "Items Parent Save", "Save process will actually save item\\s parent\\s (" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ")" + System.Environment.NewLine + System.Environment.NewLine + "To continue with save?", MessageBoxButton.YesNo, MessageBoxResult.No));

            #endregion Analyzer

            #region Registry Values Check Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RegistryValuesCheckFailed, new UserMessage(eMessageType.ERROR, "Run Registry Values Check", "Failed to check if all needed registry values exist.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddRegistryValue, new UserMessage(eMessageType.QUESTION, "Missing Registry Value", "The required registry value for the key: '{0}' is missing or wrong. Do you want to add/fix it?", MessageBoxButton.YesNo, MessageBoxResult.Yes));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddRegistryValueSucceed, new UserMessage(eMessageType.INFO, "Add Registry Value", "Registry value added successfully for the key: '{0}'." + Environment.NewLine + "Please restart the application.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddRegistryValueFailed, new UserMessage(eMessageType.ERROR, "Add Registry Value", "Registry value add failed for the key: '{0}'." + Environment.NewLine + "Please restart the application as administrator and try again.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Registry Values Check Messages

            #region SourceControl Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoItemWasSelected, new UserMessage(eMessageType.WARN, "No item was selected", "Please select an item to proceed.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlConflictResolveFailed, new UserMessage(eMessageType.ERROR, "Resolve Conflict Failed", "Ginger failed to resolve the conflicted file" + Environment.NewLine + "File Path: {0}" + Environment.NewLine + Environment.NewLine + "It seems like the SVN conflict content (e.g. '<<<<<<< .mine') has been updated on the remote repository", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToAddCheckInComment, new UserMessage(eMessageType.WARN, "Check-In Changes", "Please enter check-in comments.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToGetProjectsListFromSVN, new UserMessage(eMessageType.ERROR, "Source Control Error", "Failed to get the solutions list from the source control." + Environment.NewLine + "Error Details: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectSolution, new UserMessage(eMessageType.WARN, "Select Solution", "Please select solution.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlFileLockedByAnotherUser, new UserMessage(eMessageType.WARN, "Source Control File Locked", "The file '{0}' was locked by: {1} "  + Environment.NewLine + "Locked comment {2}." + Environment.NewLine + Environment.NewLine + " Are you sure you want to unlock the file?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DownloadedSolutionFromSourceControl, new UserMessage(eMessageType.INFO, "Download Solution", "The solution '{0}' was downloaded successfully." + Environment.NewLine + "Do you want to open the downloaded Solution?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UpdateToRevision, new UserMessage(eMessageType.INFO, "Update Solution", "The solution was updated successfully to revision: {0}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CommitedToRevision, new UserMessage(eMessageType.INFO, "Commit Solution", "The changes were committed successfully, Revision: {0}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GitUpdateState, new UserMessage(eMessageType.INFO, "Update Solution", "The solution was updated successfully, Update status: {0}, Latest Revision :{1}.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoPathForCheckIn, new UserMessage(eMessageType.ERROR, "No Path for Check-In", "Missing Path", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlConnFaild, new UserMessage(eMessageType.ERROR, "Source Control Connection", "Failed to establish connection to the source control." + Environment.NewLine + "Error Details: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlRemoteCannotBeAccessed, new UserMessage(eMessageType.ERROR, "Source Control Connection", "Failed to establish connection to the source control." + Environment.NewLine + "The remote repository cannot be accessed, please check your internet connection and your proxy configurations" + Environment.NewLine + Environment.NewLine + "Error details: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlUnlockFaild, new UserMessage(eMessageType.ERROR, "Source Control Unlock", "Failed to unlock remote file, File locked by different user." + Environment.NewLine + "Locked by: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlConnSucss, new UserMessage(eMessageType.INFO, "Source Control Connection", "Succeed to establish connection to the source control", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlLockSucss, new UserMessage(eMessageType.INFO, "Source Control Lock", "Succeed to Lock the file in the source control", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlUnlockSucss, new UserMessage(eMessageType.INFO, "Source Control Unlock", "Succeed to unlock the file in the source control", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlConnMissingConnInputs, new UserMessage(eMessageType.WARN, "Source Control Connection", "Missing connection inputs, please set the source control URL, user name and password.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlConnMissingLocalFolderInput, new UserMessage(eMessageType.WARN, "Download Solution", "Missing local folder input, please select local folder to download the solution into.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlUpdateFailed, new UserMessage(eMessageType.ERROR, "Update Solution", "Failed to update the solution from source control." + Environment.NewLine + "Error Details: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlCommitFailed, new UserMessage(eMessageType.ERROR, "Commit Solution", "Failed to commit the solution from source control." + Environment.NewLine + "Error Details: '{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlChkInSucss, new UserMessage(eMessageType.QUESTION, "Check-In Changes", "Check-in process ended successfully." + Environment.NewLine + Environment.NewLine + "Do you want to do another check-in?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlChkInConflictHandled, new UserMessage(eMessageType.QUESTION, "Check-In Results", "The check in process ended, please notice that conflict was indentefied during the process and may additional check in will be needed for pushing the conflict items." + Environment.NewLine + Environment.NewLine + "Do you want to do another check-in?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlChkInConflictHandledFailed, new UserMessage(eMessageType.WARN, "Check-In Results", "Check in process ended with unhandled conflict please notice that you must handled them prior to the next check in of those items", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlGetLatestConflictHandledFailed, new UserMessage(eMessageType.WARN, "Get Latest Results", "Get latest process encountered conflicts which must be resolved for successfull Solution loading", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlCheckInLockedByAnotherUser, new UserMessage(eMessageType.WARN, "Locked Item Check-In", "The item: '{0}'" + Environment.NewLine + "is locked by the user: '{1}'" + Environment.NewLine + "The locking comment is: '{2}' ." + Environment.NewLine + Environment.NewLine + "Do you want to unlock the item and proceed with check in process?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlCheckInLockedByMe, new UserMessage(eMessageType.WARN, "Locked Item Check-In", "The item: '{0}'" + Environment.NewLine + "is locked by you please noticed that during check in the lock will need to be removed please confirm to continue", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlCheckInUnsavedFileChecked, new UserMessage(eMessageType.QUESTION, "Unsaved Item Check-in", "The item: '{0}' contains unsaved changes which must be saved prior to the check in process." + Environment.NewLine + Environment.NewLine + "Do you want to save the item and proceed with check in process?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToUnlockFileDuringCheckIn, new UserMessage(eMessageType.QUESTION, "Locked Item Check-In Failure", "The item: '{0}' unlock operation failed on the URL: '{1}'." + Environment.NewLine + Environment.NewLine + "Do you want to proceed with the check in  process for rest of the items?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlChkInConfirmtion, new UserMessage(eMessageType.QUESTION, "Check-In Changes", "Checking in changes will effect all project users, are you sure you want to continue and check in those {0} changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlMissingSelectionToCheckIn, new UserMessage(eMessageType.WARN, "Check-In Changes", "Please select items to check-in.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SourceControlResolveConflict, new UserMessage(eMessageType.QUESTION, "Source Control Conflicts", "Source control conflicts has been identified for the path: '{0}'." + Environment.NewLine + "You probably won't be able to use the item which in the path till conflicts will be resolved." + Environment.NewLine + Environment.NewLine + "Do you want to automatically resolve the conflicts and keep your local changes for all conflicts?" + Environment.NewLine + Environment.NewLine + "Select 'No' for accepting server updates for all conflicts or select 'Cancel' for canceling the conflicts handling.", MessageBoxButton.YesNoCancel, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SureWantToDoRevert, new UserMessage(eMessageType.QUESTION, "Undo Changes", "Are you sure you want to revert changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SureWantToDoCheckIn, new UserMessage(eMessageType.QUESTION, "Check-In Changes", "Are you sure you want to check-in changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RefreshWholeSolution, new UserMessage(eMessageType.QUESTION, "Refresh Solution", "Do you want to Refresh the whole Solution to get the Latest changes?", MessageBoxButton.YesNo, MessageBoxResult.No));
            
            #endregion SourceControl Messages

            #region Validation Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.PleaseStartAgent, new UserMessage(eMessageType.WARN, "Start Agent", "Please start agent in order to run validation.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectValidation, new UserMessage(eMessageType.WARN, "Select Validation", "Please select validation to edit.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Validation Messages

            #region DataBase Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ErrorConnectingToDataBase, new UserMessage(eMessageType.ERROR, "Cannot connect to database", "DB Connection error." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ErrorClosingConnectionToDataBase, new UserMessage(eMessageType.ERROR, "Error in closing connection to database", "DB Close Connection error" + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DbTableError, new UserMessage(eMessageType.ERROR, "DB Table Error", "Error occurred while trying to get the {0}." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DbQueryError, new UserMessage(eMessageType.ERROR, "DB Query Error", "The DB Query returned error, please double check the table name and field name." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DbConnFailed, new UserMessage(eMessageType.ERROR, "DB Connection Status", "Connect to the DB failed.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DbConnSucceed, new UserMessage(eMessageType.INFO, "DB Connection Status", "Connect to the DB succeeded.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.OracleDllIsMissing, new UserMessage(eMessageType.ERROR, "DB Connection Status", "Connect to the DB failed." + Environment.NewLine + "The file Oracle.ManagedDataAccess.dll is missing," + Environment.NewLine + "Please download the file, place it under the below folder, restart Ginger and retry:" + Environment.NewLine + "{0}" + Environment.NewLine + "Do you want to download the file now?", MessageBoxButton.YesNo, MessageBoxResult.None));
            #endregion DataBase Messages

            #region Environment Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingUnixCredential, new UserMessage(eMessageType.ERROR, "Unix credential is missing or invalid", "Unix credential is missing or invalid in Environment settings, please open Environment tab to check.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.EnvironmentItemLoadError, new UserMessage(eMessageType.ERROR, "Environment Item Load Error", "Failed to load the {0}." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ShareEnvAppWithAllEnvs, new UserMessage(eMessageType.INFO, "Share Environment Applications", "The selected application/s were added to the other solution environments." + Environment.NewLine + Environment.NewLine + "Note: The changes were not saved, please perform save for all changed environments.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ShareEnvAppParamWithAllEnvs, new UserMessage(eMessageType.INFO, "Share Environment Application Parameter", "The selected parameter/s were added to the other solution environments matching applications." + Environment.NewLine + Environment.NewLine + "Note: The changes were not saved, please perform save for all changed environments.", MessageBoxButton.OK, MessageBoxResult.None));            
            
            #endregion Environment Messages

            #region Agents/Drivers Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SuccessfullyConnectedToAgent, new UserMessage(eMessageType.INFO, "Connect Agent Success", "Agent connection successful!", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToConnectAgent, new UserMessage(eMessageType.ERROR, "Connect to Agent", "Failed to connect to the {0} agent." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SshCommandError, new UserMessage(eMessageType.ERROR, "Ssh Command Error", "The Ssh Run Command returns error, please double check the connection info and credentials." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GoToUrlFailure, new UserMessage(eMessageType.ERROR, "Go To URL Error", "Failed to go to the URL: '{0}'." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.HookLinkEventError, new UserMessage(eMessageType.ERROR, "Hook Link Event Error", "The link type is unknown.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToStartAgent, new UserMessage(eMessageType.WARN, "Missing Agent", "Please start/select agent.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ASCFNotConnected, new UserMessage(eMessageType.ERROR, "Not Connected to ASCF", "Please Connect first.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SetDriverConfigTypeNotHandled, new UserMessage(eMessageType.ERROR, "Set Driver configuration", "Unknown Type {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DriverConfigUnknownDriverType, new UserMessage(eMessageType.ERROR, "Driver Configuration", "Unknown Driver Type {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SetDriverConfigTypeFieldNotFound, new UserMessage(eMessageType.ERROR, "Driver Configuration", "Unknown Driver Parameter {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UnknownConsoleCommand, new UserMessage(eMessageType.ERROR, "Unknown Console Command", "Command {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DOSConsolemissingCMDFileName, new UserMessage(eMessageType.ERROR, "DOS Console Driver Error", "DOSConsolemissingCMDFileName", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CannontFindBusinessFlow, new UserMessage(eMessageType.ERROR, "Cannot Find " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), "Missing flow in solution {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AgentNotFound, new UserMessage(eMessageType.ERROR, "Cannot Find Agent", "Missing Agent from Run Config- {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingNewAgentDetails, new UserMessage(eMessageType.WARN, "Missing Agent Details", "The new Agent {0} is missing.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingNewTableDetails, new UserMessage(eMessageType.ERROR, "Missing Table Details", "The new Table {0} is missing.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingExcelDetails, new UserMessage(eMessageType.ERROR, "Missing Export Path Details", "The Export Excel File Path is missing", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidExcelDetails, new UserMessage(eMessageType.ERROR, "InValid Export Sheet Details", "The Export Excel can be *.xlsx only.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportExcelFileFailed, new UserMessage(eMessageType.ERROR, "Export Excel File Failed", "Error Occurred while exporting the Excel File: {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportExcelFileDetails, new UserMessage(eMessageType.INFO, "Export Excel File Details", "Export execution ended successfully", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidTableDetails, new UserMessage(eMessageType.ERROR, "InValid Table Details", "The Table Name provided is Invalid. It cannot contain spaces", MessageBoxButton.OK, MessageBoxResult.None));            
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingNewDSDetails, new UserMessage(eMessageType.WARN, "Missing Data Source Details", "The new Data Source {0} is missing.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DuplicateDSDetails, new UserMessage(eMessageType.ERROR, "Duplicate DataSource Details", "The Data Source with the File Path {0} already Exist. Please use another File Path", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteDSFileError, new UserMessage(eMessageType.WARN, "Delete DataSource File", "The Data Source File with the File Path '{0}' Could not be deleted. Please Delete file Manually", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidDSPath, new UserMessage(eMessageType.ERROR, "Invalid DataSource Path", "The Data Source with the File Path {0} is not valid. Please use correct File Path", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GingerKeyNameError, new UserMessage(eMessageType.ERROR, "InValid Ginger Key Name", "The Ginger Key Name cannot be Duplicate or NULL. Please fix Table '{0}' before continuing.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.GingerKeyNameDuplicate, new UserMessage(eMessageType.ERROR, "InValid Ginger Key Name", "The Ginger Key Name cannot be Duplicated. Please change the Key Name : {0} in Table : {1}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingNewColumn, new UserMessage(eMessageType.ERROR, "Missing Column Details", "The new Column {0} is missing.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidColumnName, new UserMessage(eMessageType.ERROR, "Invalid Column Details", "The Column Name is invalid.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingApplicationPlatform, new UserMessage(eMessageType.WARN, "Missing Application Platform Info", "The default Application Platform Info is missing, please go to Solution level to add at least one Target Application.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ConnectionCloseWarning, new UserMessage(eMessageType.WARN, "Connection Close", "Closing this window will cause the connection to {0} to be closed, to continue and close?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ChangingAgentDriverAlert, new UserMessage(eMessageType.WARN, "Changing Agent Driver", "Changing the Agent driver type will cause all driver configurations to be reset, to continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidCharactersWarning, new UserMessage(eMessageType.WARN, "Invalid Details", "Name can't contain any of the following characters: " + Environment.NewLine + " /, \\, *, :, ?, \", <, >, |", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.InvalidValueExpression, new UserMessage(eMessageType.WARN, "Invalid Value Expression", "{0} - Value Expression has some Error. Do you want to continue?", MessageBoxButton.YesNo, MessageBoxResult.None)); 
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoOptionalAgent, new UserMessage(eMessageType.WARN, "No Optional Agent", "No optional Agent was found." + Environment.NewLine + "Please configure new Agent under the Solution to be used for this application.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DriverNotSupportingWindowExplorer, new UserMessage(eMessageType.WARN, "Open Window Explorer", "The driver '{0}' is not supporting the Window Explorer yet.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AgentNotRunningAfterWaiting, new UserMessage(eMessageType.WARN, "Running Agent", "The Agent '{0}' failed to start running after {1} seconds.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ApplicationAgentNotMapped, new UserMessage(eMessageType.WARN, "Agent Not Mapped to Target Application", "The Agent Mapping for Target Application  '{0}' , Please map Agent.", MessageBoxButton.OK, MessageBoxResult.None));      
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WindowClosed, new UserMessage(eMessageType.WARN, "Invalid target window", "Target window is either closed or no longer available. \n\n Please Add switch Window action by selecting correct target window on Window Explorer", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TargetWindowNotSelected, new UserMessage(eMessageType.WARN, "Target Window not Selected", "Please choose the target window from available list", MessageBoxButton.OK, MessageBoxResult.None));
            
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ChangingEnvironmentParameterValue, new UserMessage(eMessageType.WARN, "Changing Environment Variable Name", "Changing the Environment variable name will cause rename this environment variable name in every " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)+ ", do you want to continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveLocalChanges, new UserMessage(eMessageType.QUESTION, "Save Local Changes?", "Your Local Changes will be saved. Do you want to Continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.LooseLocalChanges, new UserMessage(eMessageType.WARN, "Loose Local Changes?", "Your Local Changes will be Lost. Do you want to Continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.IFSaveChangesOfBF, new UserMessage(eMessageType.WARN, "Save Current" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Before Change?", "Do you want to save the changes made in the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) +"?", MessageBoxButton.YesNo, MessageBoxResult.No));
            
            #endregion Agents/Drivers Messages

            #region Actions Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingActionPropertiesEditor, new UserMessage(eMessageType.ERROR, "Action Properties Editor", "No Action properties Editor yet for '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectItem, new UserMessage(eMessageType.WARN, "Select Item", "Please select item.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectAction, new UserMessage(eMessageType.WARN, "Select Action", "Please select action.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ImportSeleniumScriptError, new UserMessage(eMessageType.ERROR, "Import Selenium Script Error", "Error occurred while trying to import Selenium Script, please make sure the html file is a Selenium script generated by Selenium IDE.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Actions Messages

            #region Runset Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RunsetNoGingerPresentForBusinessFlow, new UserMessage(eMessageType.WARN, "No Ginger in " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Please add at least one Ginger to your " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " before choosing a " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ".", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ResetBusinessFlowRunVariablesFailed, new UserMessage(eMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Variables) + " Reset Failed", "Failed to reset the " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " to original configurations." + System.Environment.NewLine + "Error Details: {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfToGenerateAutoRunDescription, new UserMessage(eMessageType.QUESTION, "Automatic Description Creation", "Do you want to automatically populate the Run Description?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CreateRunset, new UserMessage(eMessageType.WARN, "Create Runset", "No runset found, Do you want to create new runset", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteRunners, new UserMessage(eMessageType.WARN, "Delete Runners", "Are you sure you want to delete all runners", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteRunner, new UserMessage(eMessageType.WARN, "Delete Runner", "Are you sure you want to delete selected runner", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteBusinessflows, new UserMessage(eMessageType.WARN, "Delete " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), "Are you sure you want to delete all" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteBusinessflow, new UserMessage(eMessageType.WARN, "Delete " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), "Are you sure you want to delete selected " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RunsetBuinessFlowWasChanged, new UserMessage(eMessageType.WARN, GingerDicser.GetTermResValue(eTermResKey.RunSet) + " " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Changed", "One or more of the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " were changed/deleted." + Environment.NewLine + "You must reload the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " for changes to be viewed/executed." + Environment.NewLine + Environment.NewLine + "Do you want to reload the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) +"?" + Environment.NewLine + Environment.NewLine + "Note: Reload will cause un-saved changes to be lost.", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RunSetReloadhWarn, new UserMessage(eMessageType.WARN, "Reload " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Reload process will cause all un-saved changes to be lost." + Environment.NewLine + Environment.NewLine+ " To continue wite reload?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CantDeleteRunner, new UserMessage(eMessageType.WARN, "Delete Runner", "You can't delete last Runner, you must have at least one.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Runset Messages

            #region Excel Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExcelNoWorksheetSelected, new UserMessage(eMessageType.WARN, "Missing worksheet", "Please select a worksheet", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExcelBadWhereClause, new UserMessage(eMessageType.WARN, "Problem with WHERE clause", "Please check your WHERE clause. Do all column names exist and are they spelled correctly?", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Excel Messages

            #region Variables Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectVariable, new UserMessage(eMessageType.WARN, "Select " + GingerDicser.GetTermResValue(eTermResKey.Variable), "Please select " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ".", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.VariablesAssignError, new UserMessage(eMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Variables) + " Assign Error", "Failed to assign " + GingerDicser.GetTermResValue(eTermResKey.Variables) + "." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SetCycleNumError, new UserMessage(eMessageType.ERROR, "Set Cycle Number Error", "Failed to set the cycle number." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.VariablesParentNotFound, new UserMessage(eMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Variables) + " Parent Error", "Failed to find the " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " parent.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CantStoreToVariable, new UserMessage(eMessageType.ERROR, "Store to " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Failed", "Cannot Store to " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " '{0}'- " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " not found", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WarnRegradingMissingVariablesUse, new UserMessage(eMessageType.WARN, "Unassociated " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " been Used in " + GingerDicser.GetTermResValue(eTermResKey.Activity), GingerDicser.GetTermResValue(eTermResKey.Variables) + " which are not part of the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " list are been used in it." + Environment.NewLine + "For the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " to work as a standalone you will need to add those " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: ".") + Environment.NewLine + Environment.NewLine + "Missing " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: ":") + Environment.NewLine + "{1}" + Environment.NewLine + Environment.NewLine + "Do you want to automatically add the missing " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: "?") + Environment.NewLine + "Note: Clicking 'No' will cancel the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Upload.", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NotAllMissingVariablesWereAdded, new UserMessage(eMessageType.WARN, "Unassociated " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " been Used in " + GingerDicser.GetTermResValue(eTermResKey.Activity), "Failed to find and add the following missing " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: ":") + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Please add those " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " manually for allowing the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Upload.", MessageBoxButton.OK, MessageBoxResult.No));
            #endregion Variables Messages

            #region Solution Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.BeginWithNoSelectSolution, new UserMessage(eMessageType.INFO, "No solution is selected", "You have not selected any existing solution, please open an existing one or create a new solution by pressing the New button.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskToSelectSolutionFolder, new UserMessage(eMessageType.WARN, "Missing Folder Selection", "Please select the folder you want to perform the action on.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteRepositoryItemAreYouSure, new UserMessage(eMessageType.WARN, "Delete", "Are you sure you want to delete '{0}' item?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DeleteTreeFolderAreYouSure, new UserMessage(eMessageType.WARN, "Delete Foler", "Are you sure you want to delete the '{0}' folder and all of it content?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RenameRepositoryItemAreYouSure, new UserMessage(eMessageType.WARN, "Rename", "Are you sure you want to rename '{0}'?", MessageBoxButton.YesNoCancel, MessageBoxResult.Yes));            
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SaveBusinessFlowChanges, new UserMessage(eMessageType.QUESTION, "Save Changes", "Save Changes to - {0}", MessageBoxButton.YesNoCancel, MessageBoxResult.Yes));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SolutionLoadError, new UserMessage(eMessageType.ERROR, "Solution Load Error", "Failed to load the solution." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingAddSolutionInputs, new UserMessage(eMessageType.WARN, "Add Solution", "Missing solution inputs, please set the solution name, folder and main application details.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SolutionAlreadyExist, new UserMessage(eMessageType.WARN, "Add Solution", "The solution already exist, please select different name/folder.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddSolutionSucceed, new UserMessage(eMessageType.INFO, "Add Solution", "The solution was created and loaded successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddSolutionFailed, new UserMessage(eMessageType.ERROR, "Add Solution", "Failed to create the new solution. " + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.RefreshTreeGroupFailed, new UserMessage(eMessageType.ERROR, "Refresh", "Failed to perform the refresh operation." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToDeleteRepoItem, new UserMessage(eMessageType.ERROR, "Delete", "Failed to perform the delete operation." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderExistsWithName, new UserMessage(eMessageType.WARN, "Folder Creation Failed", "Folder with same name already exists. Please choose a different name for the folder.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UpdateApplicationNameChangeInSolution, new UserMessage(eMessageType.WARN, "Target Application Name Change", "Do you want to automatically update the Target Application name in all Solution items?" + Environment.NewLine + Environment.NewLine + "Note: If you choose 'Yes', changes won't be saved, for saving them please click 'SaveAll'", MessageBoxButton.YesNo, MessageBoxResult.Yes));
            
            #endregion Solution Messages

            #region Activities
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActionsDependenciesLoadFailed, new UserMessage(eMessageType.ERROR, "Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies Load Failed", "Failed to load the Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies data." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActivitiesDependenciesLoadFailed, new UserMessage(eMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies Load Failed", "Failed to load the " + GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies data." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DependenciesMissingActions, new UserMessage(eMessageType.INFO, "Missing Actions", "Actions not found." + System.Environment.NewLine + "Please add Actions to the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + ".", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DependenciesMissingVariables, new UserMessage(eMessageType.INFO, "Missing " + GingerDicser.GetTermResValue(eTermResKey.Variables), GingerDicser.GetTermResValue(eTermResKey.Variables) + " not found." + System.Environment.NewLine + "Please add " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " from type 'Selection List' to the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + ".", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DuplicateVariable, new UserMessage(eMessageType.WARN, "Duplicated " + GingerDicser.GetTermResValue(eTermResKey.Variable), "The " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " name '{0}' and value '{1}' exist more than once." + System.Environment.NewLine + "Please make sure only one instance exist in order to set the " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingActivityAppMapping, new UserMessage(eMessageType.WARN, "Missing " + GingerDicser.GetTermResValue(eTermResKey.Activity) + "-Application Mapping", "Target Application was not mapped the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " so the required Actions platform is unknown." + System.Environment.NewLine + System.Environment.NewLine + "Map Target Application to the Activity by double clicking the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " record and select the application you want to test using it.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WarnOnDynamicActivities, new UserMessage(eMessageType.QUESTION, "Dynamic " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " Warning", "The dynamically added Shared Repository " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " will not be saved (but they will continue to appear on the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString:".)")+  System.Environment.NewLine + System.Environment.NewLine + "To continue with Save?", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion Activities

            #region Support Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AddSupportValidationFailed, new UserMessage(eMessageType.WARN, "Add Support Request Validation Error", "Add support request validation failed." + Environment.NewLine + "Failure Reason:" + Environment.NewLine + "'{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UploadSupportRequestSuccess, new UserMessage(eMessageType.INFO, "Upload Support Request", "Upload was completed successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UploadSupportRequestFailure, new UserMessage(eMessageType.ERROR, "Upload Support Request", "Failed to complete the upload." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Support Messages

            #region SharedFunctions Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FunctionReturnedError, new UserMessage(eMessageType.ERROR, "Error Occurred", "The {0} returns error. please check the provided details." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ShowInfoMessage, new UserMessage(eMessageType.INFO, "Message", "{0}", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion SharedFunctions Messages

            #region CustomeFunctions Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.LogoutSuccess, new UserMessage(eMessageType.INFO, "Logout", "Logout completed successfully- {0}.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion CustomeFunctions Messages

            #region ALM
            Reporter.UserMessagesPool.Add(eUserMsgKeys.QcConnectSuccess, new UserMessage(eMessageType.INFO, "QC/ALM Connection", "QC/ALM connection successful!", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.QcConnectFailure, new UserMessage(eMessageType.WARN, "QC/ALM Connection Failed", "QC/ALM connection failed." + System.Environment.NewLine + "Please make sure that the credentials you use are correct and that QC/ALM Client is registered on your machine." + System.Environment.NewLine + System.Environment.NewLine + "For registering QC/ALM Client- please follow below steps:" + System.Environment.NewLine + "1. Launch Internet Explorer as Administrator" + System.Environment.NewLine + "2. Go to http://<QCURL>/qcbin" + System.Environment.NewLine + "3. Click on 'Add-Ins Page' link" + System.Environment.NewLine + "4. In next page, click on 'HP ALM Client Registration'" + System.Environment.NewLine + "5. In next page click on 'Register HP ALM Client'" + System.Environment.NewLine + "6. Restart Ginger and try to reconnect", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.QcConnectFailureRestAPI, new UserMessage(eMessageType.WARN, "QC/ALM Connection Failed", "QC/ALM connection failed." + System.Environment.NewLine + "Please make sure that the server url and the credentials you use are correct." , MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ALMConnectFailureWithCurrSettings, new UserMessage(eMessageType.WARN, "ALM Connection Failed", "ALM Connection Failed, Please make sure credentials are correct.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ALMConnectFailure, new UserMessage(eMessageType.WARN, "ALM Connection Failed", "ALM connection failed." + System.Environment.NewLine + "Please make sure that the credentials you use are correct and that ALM Client is registered on your machine.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.QcLoginSuccess, new UserMessage(eMessageType.INFO, "Login Success", "QC/ALM Login successful!", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ALMLoginFailed, new UserMessage(eMessageType.WARN, "Login Failed", "ALM Login Failed - {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.QcNeedLogin, new UserMessage(eMessageType.WARN, "Not Connected to QC/ALM", "You Must Log Into QC/ALM First.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TestCasesUpdatedSuccessfully, new UserMessage(eMessageType.INFO, "TestCase Update", "TestCases Updated Successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TestCasesUploadedSuccessfully, new UserMessage(eMessageType.INFO, "TestCases Uploaded", "TestCases Uploaded Successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TestSetsImportedSuccessfully, new UserMessage(eMessageType.INFO, "Import ALM Test Set", "ALM Test Set/s import process ended successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TestSetExists, new UserMessage(eMessageType.WARN, "Import Exiting Test Set", "The Test Set '{0}' was imported before and already exists in current Solution." + System.Environment.NewLine + "Do you want to delete the existing mapped " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " and import the Test Set again?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ErrorInTestsetImport, new UserMessage(eMessageType.ERROR, "Import Test Set Error", "Error Occurred while exporting the Test Set '{0}'." + System.Environment.NewLine + "Error Details:{1}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ErrorWhileExportingExecDetails, new UserMessage(eMessageType.ERROR, "Export Execution Details Error", "Error occurred while exporting the execution details to QC/ALM." + System.Environment.NewLine + "Error Details:{0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportedExecDetailsToALM, new UserMessage(eMessageType.INFO, "Export Execution Details", "Export execution details result: {0}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportAllItemsToALMSucceed, new UserMessage(eMessageType.INFO, "Export All Items to ALM", "All items has been successfully exported to ALM.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportAllItemsToALMFailed, new UserMessage(eMessageType.INFO, "Export All Items to ALM", "While exporting to ALM One or more items failed to export.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportItemToALMSucceed, new UserMessage(eMessageType.INFO, "Export ALM Item", "Exporting item to ALM process ended successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportedExecDetailsToALMIsInProcess, new UserMessage(eMessageType.INFO, "Export Execution Details", "Please Wait, Exporting Execution Details is inprocess.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ALMOperationFailed, new UserMessage(eMessageType.ERROR, "ALM Operation Failed", "Failed to perform the {0} operation." + Environment.NewLine + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActivitiesGroupAlreadyMappedToTC, new UserMessage(eMessageType.WARN, "Export " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to QC/ALM", "The " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " '{0}' is already mapped to the QC/ALM '{1}' Test Case, do you want to update it?" + Environment.NewLine + Environment.NewLine + "Select 'Yes' to update or 'No' to create new Test Case.", MessageBoxButton.YesNoCancel, MessageBoxResult.Cancel));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.BusinessFlowAlreadyMappedToTC, new UserMessage(eMessageType.WARN, "Export " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to QC/ALM", "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " '{0}' is already mapped to the QC/ALM '{1}' Test Set, do you want to update it?" + Environment.NewLine + Environment.NewLine + "Select 'Yes' to update or 'No' to create new Test Set.", MessageBoxButton.YesNoCancel, MessageBoxResult.Cancel));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportQCNewTestSetSelectDiffFolder, new UserMessage(eMessageType.INFO, "Export QC Item - Creating new Test Set", "Please select QC folder to export to that the Test Set does not exist there.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExportItemToALMFailed, new UserMessage(eMessageType.ERROR, "Export to ALM Failed", "The {0} '{1}' failed to be exported to ALM." + Environment.NewLine + Environment.NewLine + "Error Details: {2}", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfToSaveBFAfterExport, new UserMessage(eMessageType.QUESTION, "Save Links to QC/ALM Items", "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " '{0}' must be saved for keeping the links to QC/ALM items." + Environment.NewLine + "To perform the save now?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskBeforeDefectProfileDeleting, new UserMessage(eMessageType.QUESTION, "Profiles Deleting", "After deletion there will be no way to restore deleted profiles.\nAre you sure that you want to delete the selected profiles?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissedMandatotryFields, new UserMessage(eMessageType.INFO, "Profiles Saving", "Please, populate value for mandatotry field '{0}' of '{1}' Defect Profile", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoDefaultDefectProfileSelected, new UserMessage(eMessageType.INFO, "Profiles Saving", "Please, select one of the Defect Profiles to be a 'Default'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.IssuesInSelectedDefectProfile, new UserMessage(eMessageType.INFO, "ALM Defects Opening", "Please, revise the selected Defect Profile, current fields/values are not corresponded with ALM", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoDefectProfileCreated, new UserMessage(eMessageType.INFO, "Defect Profiles", "Please, create at least one Defect Profile", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WrongValueSelectedFromTheList, new UserMessage(eMessageType.INFO, "Profiles Saving", "Please, select one of the existed values from the list\n(Field '{0}', Defect Profile '{1}')", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WrongNonNumberValueInserted, new UserMessage(eMessageType.INFO, "Profiles Saving", "Please, insert numeric value\n(Field '{0}', Defect Profile '{1}')", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.WrongDateValueInserted, new UserMessage(eMessageType.INFO, "Profiles Saving", "Please, insert Date in format 'yyyy-mm-dd'\n(Field '{0}', Defect Profile '{1}')", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ALMDefectsWereOpened, new UserMessage(eMessageType.INFO, "ALM Defects Opening", "{0} ALM Defects were opened", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskALMDefectsOpening, new UserMessage(eMessageType.QUESTION, "ALM Defects Opening", "Are you sure that you want to open {0} ALM Defects?", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion ALM

            #region ALM External Items Fields Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfToDownloadPossibleValuesShortProcesss, new UserMessage(eMessageType.QUESTION, "ALM External Items Fields", "Would you like to download and save possible values for Categories Items? ", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfToDownloadPossibleValues, new UserMessage(eMessageType.QUESTION, "ALM External Items Fields", "Would you like to download and save possible values for Categories Items? " + Environment.NewLine + "This process could take up to several hours.", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SelectAndSaveCategoriesValues, new UserMessage(eMessageType.QUESTION, "ALM External Items Fields", "Please select values for each Category Item and click on Save", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion ALM External Items Fields Messages

            #region POM
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMSearchByGUIDFailed, new UserMessage(eMessageType.WARN, "POM not found", "Previously saved POM not found. Please choose another one.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMElementSearchByGUIDFailed, new UserMessage(eMessageType.WARN, "POM Element not found", "Previously saved POM Element not found. Please choose another one.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoRelevantAgentInRunningStatus, new UserMessage(eMessageType.WARN, "No Relevant Agent In Running Status", "Relevant Agent In should be up and running in order to see the highlighted element.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMWizardFailedToLearnElement, new UserMessage(eMessageType.WARN, "Learn Elements Failed", "Error occured while learning the elements." + Environment.NewLine + "Error Details:" + Environment.NewLine + "'{0}'", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMWizardReLearnWillDeleteAllElements, new UserMessage(eMessageType.WARN, "Re-Learn Elements", "Re-Learn Elements will delete all existing elements" + Environment.NewLine + "Do you want to continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMDriverIsBusy, new UserMessage(eMessageType.WARN, "Driver Is Busy", "Operation cannot be complete because the Driver is busy with learning operation" + Environment.NewLine + "Do you want to continue?", MessageBoxButton.OK, MessageBoxResult.OK));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMAgentIsNotRunning, new UserMessage(eMessageType.WARN, "Agent is Down", "In order to perform this operation the Agent needs to be up and running." + Environment.NewLine + "Please start the agent and re-try", MessageBoxButton.OK, MessageBoxResult.OK));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.POMNotOnThePageWarn, new UserMessage(eMessageType.WARN, "Not On the Same Page", "'{0}' Elements out of '{1}' Elements failed to be found on the page" + Environment.NewLine + "Looks like you are not on the right page" + Environment.NewLine + "Do you want to continue?", MessageBoxButton.YesNo, MessageBoxResult.Yes));

            #endregion POM


            #region UnitTester Messages
            Reporter.UserMessagesPool.Add(eUserMsgKeys.TestCompleted, new UserMessage(eMessageType.INFO, "Test Completed", "Test completed successfully.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.CantFindObject, new UserMessage(eMessageType.INFO, "Missing Object", "Cannot find the {0}, the ID is: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion UnitTester Messages

            #region CommandLineParams
            Reporter.UserMessagesPool.Add(eUserMsgKeys.UnknownParamInCommandLine, new UserMessage(eMessageType.ERROR, "Unknown Parameter", "Parameter not recognized {0}", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion CommandLineParams Messages

            #region Tree / Grid
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ConfirmToAddTreeItem, new UserMessage(eMessageType.QUESTION, "Add New Item to Tree", "Are you Sure you want to add new item to tree?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToAddTreeItem, new UserMessage(eMessageType.ERROR, "Add Tree Item", "Failed to add the tree item '{0}'." + Environment.NewLine + "Error Details: '{1}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SureWantToDeleteAll, new UserMessage(eMessageType.QUESTION, "Delete All", "Are you sure you want to delete all?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoItemToDelete, new UserMessage(eMessageType.WARN, "Delete All", "Didn't found item to delete", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SelectItemToDelete, new UserMessage(eMessageType.WARN, "Delete", "Please select items to delete", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SelectItemToAdd, new UserMessage(eMessageType.WARN, "Add New Item", "Please select an Activity on which you want to add a new Action", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToloadTheGrid, new UserMessage(eMessageType.ERROR, "Load Grid", "Failed to load the grid." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.DragDropOperationFailed, new UserMessage(eMessageType.ERROR, "Drag and Drop", "Drag & Drop operation failed." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SelectValidColumn, new UserMessage(eMessageType.WARN, "Column Not Found", "Please select a valid column", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SelectValidRow, new UserMessage(eMessageType.WARN, "Row Not Found", "Please select a valid row", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion Tree / Grid

            #region ActivitiesGroup
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoActivitiesGroupWasSelected, new UserMessage(eMessageType.WARN, "Missing " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " Selection", "No " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " was selected.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActivitiesGroupActivitiesNotFound, new UserMessage(eMessageType.WARN, "Import " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " " + GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Activities) + " to import were not found in the repository.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.PartOfActivitiesGroupActsNotFound, new UserMessage(eMessageType.WARN, "Import " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " " + GingerDicser.GetTermResValue(eTermResKey.Activities), "The following " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to import were not found in the repository:" + System.Environment.NewLine + "{0}", MessageBoxButton.OK, MessageBoxResult.None));           
            #endregion ActivitiesGroup

            #region Mobile
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MobileConnectionFailed, new UserMessage(eMessageType.ERROR, "Mobile Connection", "Failed to connect to the mobile device." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MobileRefreshScreenShotFailed, new UserMessage(eMessageType.ERROR, "Mobile Screen Image", "Failed to refresh the mobile device screen image." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MobileShowElementDetailsFailed, new UserMessage(eMessageType.ERROR, "Mobile Elements Inspector", "Failed to locate the details of the selected element." + Environment.NewLine + "Error Details: '{0}'.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MobileActionWasAdded, new UserMessage(eMessageType.INFO, "Add Action", "Action was added.", MessageBoxButton.OK, MessageBoxResult.None));
            //Reporter.UserMessagesPool.Add(eUserMsgKeys.MobileActionWasAdded, new UserMessage(eMessageType.INFO, "Add Action", "Action was added." + System.Environment.NewLine + System.Environment.NewLine + "Do you want to run the Action?", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion Mobile

            #region Reports
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ReportTemplateNotFound, new UserMessage(eMessageType.ERROR, "Report Template", "Report Template '{0}' not found", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.AutomationTabExecResultsNotExists, new UserMessage(eMessageType.WARN, "Execution Results are not existing", "Results from last execution are not existing (yet). Nothing to report, please wait for execution finish and click on report creation.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderNamesAreTooLong, new UserMessage(eMessageType.WARN, "Folders Names Are Too Long", "Provided folders names are too long. Please change them to be less than 100 characters", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderNameTextBoxIsEmpty, new UserMessage(eMessageType.WARN, "Folders Names is Empty", "Please provide a proper folder name.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.UserHaveNoWritePermission, new UserMessage(eMessageType.WARN, "User Have No Write Permission On Folder", "User that currently in use have no write permission on selected folder. Pay attention that attachment at shared folder may be not created.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderSizeTooSmall, new UserMessage(eMessageType.WARN, "Folders Size is Too Small", "Provided folder size is Too Small. Please change it to be bigger than 50 Mb", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FolderNotExistOrNotAvailible, new UserMessage(eMessageType.WARN, "Folder Not Exist Or Not Available", "Folder Not Exist Or Not Available. Please select another one.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.DefaultTemplateCantBeDeleted, new UserMessage(eMessageType.WARN, "Default Template Can't Be Deleted", "Default Template Can't Be Deleted. Please change it to be a non-default", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExecutionsResultsProdIsNotOn, new UserMessage(eMessageType.WARN, "Executions Results Producing Should Be Switched On", "In order to perform this action, executions results producing should be switched On", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExecutionsResultsNotExists, new UserMessage(eMessageType.WARN, "Executions Results Are Not Existing Yet", "In order to get HTML report, please, perform executions before", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExecutionsResultsToDelete, new UserMessage(eMessageType.QUESTION, "Delete Executions Results", "Are you sure you want to delete selected Executions Results?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.AllExecutionsResultsToDelete, new UserMessage(eMessageType.QUESTION, "Delete All Executions Results", "Are you sure you want to delete all Executions Results?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.HTMLReportAttachment, new UserMessage(eMessageType.WARN, "HTML Report Attachment", "HTML Report Attachment already exists, please delete existing one.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ImageSize, new UserMessage(eMessageType.WARN, "Image Size", "Image Size should be less than 30 Kb", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ReportsTemplatesSaveWarn, new UserMessage(eMessageType.WARN, "Default Template Report Change", "Default change will cause all templates to be updated and saved, to continue?", MessageBoxButton.YesNo, MessageBoxResult.No));
            

            #endregion Reports

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ApplicationNotFoundInEnvConfig, new UserMessage(eMessageType.ERROR, "Application Not Found In EnvConfig", "Application = {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ExecutionReportSent, new UserMessage(eMessageType.INFO, "Publish", "Execution report sent by email", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ErrorReadingRepositoryItem, new UserMessage(eMessageType.ERROR, "Error Reading Repository Item", "Repository Item {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.EnvNotFound, new UserMessage(eMessageType.ERROR, "Env not found", "Env not found {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.CannotAddGinger, new UserMessage(eMessageType.ERROR, "Cannot Add Ginger", "Number of Gingers is limited to 12 Gingers.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ShortcutCreated, new UserMessage(eMessageType.INFO, "New Shortcut created", "Shortcut created on desktop - {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.SolutionFileNotFound, new UserMessage(eMessageType.ERROR, "Solution File Not Found", "Cannot find Solution File at - {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.PlugInFileNotFound, new UserMessage(eMessageType.ERROR, "Plugin Configuration File Not Found", "Cannot find Plugin Configuration File at - {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToLoadPlugIn, new UserMessage(eMessageType.ERROR, "Failed to Load Plug In", "Ginger could not load the plug in '{0}'" + Environment.NewLine + "Error Details: {1}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActionIDNotFound, new UserMessage(eMessageType.ERROR, "Action ID Not Found", "Cannot find action with ID - {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActivityIDNotFound, new UserMessage(eMessageType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Activity) + " ID Not Found", "Cannot find " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " with ID - {0}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToSendEmail, new UserMessage(eMessageType.ERROR, "Failed to Send E-mail", "Failed to send the {0} over e-mail using {1}." + Environment.NewLine + Environment.NewLine + "Error Details: {2}", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FailedToExportBF, new UserMessage(eMessageType.ERROR, "Failed to export BusinessFlow to CSV", "Failed to export BusinessFlow to CSV file. Got error {0}" + Environment.NewLine, MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FoundDuplicateAgentsInRunSet, new UserMessage(eMessageType.ERROR, "Found Duplicate Agents in " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Agent name '{0}' is defined more than once in the Run Set", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FileNotExist, new UserMessage(eMessageType.WARN, "XML File Not Exist", "XML File has not been found on the specified path, either deleted or been re-named on the define XML path", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FilterNotBeenSet, new UserMessage(eMessageType.QUESTION, "Filter Not Been Set", "No filtering criteria has been set, Retrieving all elements from page can take long time to complete, Do you want to continue?", MessageBoxButton.OKCancel, MessageBoxResult.Cancel));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.CloseFilterPage, new UserMessage(eMessageType.QUESTION, "Confirmation", "'{0}' Elements Found, Close Filtering Page?", MessageBoxButton.YesNo, MessageBoxResult.Yes));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.RetreivingAllElements, new UserMessage(eMessageType.QUESTION, "Retrieving all elements", "Retrieving all elements from page can take long time to complete, Do you want to continue?", MessageBoxButton.OKCancel, MessageBoxResult.Cancel));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.WhetherToOpenSolution, new UserMessage(eMessageType.QUESTION, "Open Downloaded Solution?" ,"Do you want to open the downloaded Solution?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ClickElementAgain, new UserMessage(eMessageType.INFO, "Please Click" ,"Please click on the desired element again", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.CurrentActionNotSaved, new UserMessage(eMessageType.INFO, "Current Action Not Saved", "Before using the 'next/previous action' button, Please save the current action", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.Failedtosaveitems, new UserMessage(eMessageType.ERROR, "Failed to Save", "Failed to do Save All", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.AllItemsSaved, new UserMessage(eMessageType.INFO, "All Changes Saved", "All Changes Saved", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.AskIfShareVaribalesInRunner, new UserMessage(eMessageType.QUESTION, "Share Variables", "Are you sure you want to share selected Variable Values to all the similar Business Flows and Activities accross all Runners?", MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.RenameItemError, new UserMessage(eMessageType.ERROR, "Rename", "Failed to rename the Item. Error: '{0}'?", MessageBoxButton.OK, MessageBoxResult.OK));
            #region ActionConversion
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingTargetPlatformForConversion, new UserMessage(eMessageType.WARN,
               "Missing Target Platform for Conversion", "For {0}, you need to add a Target Application of type {1}. Please add it to your Business Flow. " + Environment.NewLine +
               "Do you want to continue with the conversion?",
               MessageBoxButton.YesNo, MessageBoxResult.No));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoConvertibleActionsFound, new UserMessage(eMessageType.INFO, "No Convertible Actions Found", "The selected "+ GingerDicser.GetTermResValue(eTermResKey.Activity) + " doesn't contain any convertible actions.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoConvertibleActionSelected, new UserMessage(eMessageType.WARN, "No Convertible Action Selected", "Please select the actions that you want to convert.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.SuccessfulConversionDone, new UserMessage(eMessageType.INFO, "Obsolete actions converted successfully", "The obsolete actions have been converted successfully!"
               + Environment.NewLine + "Do you want to convert more actions?" , MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoActivitySelectedForConversion, new UserMessage(eMessageType.WARN, "No Activity Selected", "Please select an " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " that you want to convert.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ActivitiesConversionFailed, new UserMessage(eMessageType.WARN, "Activities Conversion Failed", "Activities Conversion Failed.", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion ActionConversion

            Reporter.UserMessagesPool.Add(eUserMsgKeys.CopiedVariableSuccessfully, new UserMessage(eMessageType.INFO, "Info Message", "'{0}' Business Flows Affected." + Environment.NewLine + Environment.NewLine + "Notice: Un-saved changes won't be saved.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.ShareVariableNotSelected, new UserMessage(eMessageType.INFO, "Info Message", "Please select the variables to share.", MessageBoxButton.OK, MessageBoxResult.None));

            //Reporter.UserMessagesPool.Add(eUserMsgKeys.APIParametersListUpdated, new UserMessage(eMessageType.WARN, "API Model Parameters Difference", "Difference was identified between the list of parameters which configured on the API Model and the parameters exists on the Action.\n\nDo you want to update the Action parameters?", MessageBoxButton.YesNo, MessageBoxResult.No));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.APIParametersListUpdated, new UserMessage(eMessageType.INFO, "API Model Parameters Difference", "Difference was identified between the list of parameters which configured on the API Model and the parameters exists on the Action.\n\nAll Action parameters has been updated.", MessageBoxButton.OK, MessageBoxResult.OK));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.APIMappedToActionIsMissing, new UserMessage(eMessageType.WARN, "Missing Mapped API Model", "The API Model which mapped to this action is missing, please remap API Model to the action.", MessageBoxButton.OK, MessageBoxResult.OK));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.NoAPIExistToMappedTo, new UserMessage(eMessageType.WARN, "No API Model Found", "API Models repository is empty, please add new API Models into it and map it to the action", MessageBoxButton.OK, MessageBoxResult.OK));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.APIModelAlreadyContainsReturnValues, new UserMessage(eMessageType.WARN, "Return Values Already Exsist Warning", "{0} Return Values already exsist for this API Model" + Environment.NewLine + "Do you want to overwride them by importing form new response template file?" + Environment.NewLine + Environment.NewLine + "Please Note! - by clicking yes all {0} return values will be deleted with no option to restore.", MessageBoxButton.YesNo, MessageBoxResult.No));


            Reporter.UserMessagesPool.Add(eUserMsgKeys.VisualTestingFailedToDeleteOldBaselineImage, new UserMessage(eMessageType.WARN, "Creating New Baseline Image", "Error while tyring to create and save new Baseline Image.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ApplitoolsLastExecutionResultsNotExists, new UserMessage(eMessageType.INFO, "Show Last Execution Results", "No last execution results exists, Please run first the action, Close Applitools Eyes and then view the results.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ApplitoolsMissingChromeOrFirefoxBrowser, new UserMessage(eMessageType.INFO, "View Last Execution Results", "Applitools support only Chrome or Firefox Browsers, Please install at least one of them in order to browse Applitools URL results.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.ParameterOptionalValues, new UserMessage(eMessageType.INFO, "Parameters Optional Values", "{0} parameters were updated.", MessageBoxButton.OK, MessageBoxResult.None));

            Reporter.UserMessagesPool.Add(eUserMsgKeys.RecoverItemsMissingSelectionToRecover, new UserMessage(eMessageType.WARN, "Recover Changes", "Please select valid Recover items to {0}", MessageBoxButton.OK, MessageBoxResult.None));
            #region ErrorHandlingMapping
            Reporter.UserMessagesPool.Add(eUserMsgKeys.MissingErrorHandler, new UserMessage(eMessageType.WARN, "Mismatch in Mapped Error Handler Found", "A mismatch has been detected in error handlers mapped with your activity." + Environment.NewLine + "Please check if any mapped error handler has been deleted or marked as inactive. ", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.BusinessFlowUpdate, new UserMessage(eMessageType.INFO, "Business Flow", "BusinessFlow '{0}' {1} Sucessfully", MessageBoxButton.OK, MessageBoxResult.None));
            #endregion ErrorHandlingMapping

            Reporter.UserMessagesPool.Add(eUserMsgKeys.FindAndRepalceFieldIsEmpty, new UserMessage(eMessageType.WARN, "Field is Empty", "Field '{0}' cannot be empty", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FindAndReplaceListIsEmpty, new UserMessage(eMessageType.WARN, "List is Empty", "No items were found hence nothing can be replaced", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FindAndReplaceNoItemsToRepalce, new UserMessage(eMessageType.WARN, "No Suitable Items", "No suitable items selected to replace.", MessageBoxButton.OK, MessageBoxResult.None));
            Reporter.UserMessagesPool.Add(eUserMsgKeys.FindAndReplaceViewRunSetNotSupported, new UserMessage(eMessageType.INFO, "View Run Set", "View RunSet is not supported.", MessageBoxButton.OK, MessageBoxResult.None));

           
            
            
        }
    }
}
