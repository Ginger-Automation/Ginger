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


using GingerCore;
using System;
using System.Collections.Generic;

namespace Amdocs.Ginger.Common
{
    public enum eUserMsgKey
    {
        GeneralErrorOccured, MissingImplementation, MissingImplementation2, ApplicationInitError, PageLoadError, UserProfileLoadError, ItemToSaveWasNotSelected, ToSaveChanges, SaveLocalChanges, LooseLocalChanges,
        RegistryValuesCheckFailed, AddRegistryValue, AddRegistryValueSucceed, AddRegistryValueFailed,
        NoItemWasSelected, AskToAddCheckInComment, FailedToGetProjectsListFromSVN, AskToSelectSolution, UpdateToRevision, CommitedToRevision, GitUpdateState, SourceControlConnFaild, SourceControlRemoteCannotBeAccessed, SourceControlUnlockFaild, SourceControlConnSucss, SourceControlLockSucss, SourceControlUnlockSucss, SourceControlConnMissingConnInputs, SourceControlConnMissingLocalFolderInput,
        PleaseStartAgent, AskToSelectValidation,
        EnvironmentItemLoadError, MissingUnixCredential,
        ErrorConnectingToDataBase, ErrorClosingConnectionToDataBase, DbTableError, DbTableNameError, DbQueryError, DbConnSucceed, DbConnFailed,
        TestagentSucceed, MobileDriverNotConnected, FailedToConnectAgent, RecordingStopped, SshCommandError, GoToUrlFailure, HookLinkEventError, AskToStartAgent, RestartAgent,
        MissingActionPropertiesEditor, AskToSelectItem, AskToSelectAction, ImportSeleniumScriptError,
        AskToSelectVariable, VariablesAssignError, SetCycleNumError, VariablesParentNotFound, CantStoreToVariable,
        AskToSelectSolutionFolder, SolutionLoadError,
        BeginWithNoSelectSolution,
        AddSupportValidationFailed, UploadSupportRequestSuccess, UploadSupportRequestFailure,
        FunctionReturnedError, ShowInfoMessage,
        LogoutSuccess,
        TestCompleted, CantFindObject,
        QcConnectSuccess,
        QcConnectFailure,
        ALMConnectFailureWithCurrSettings,
        ALMOperationFailed,
        QcLoginSuccess,
        ALMLoginFailed,
        ALMConnectFailure,
        QcNeedLogin, ErrorWhileExportingExecDetails, ExportedExecDetailsToALM, ExportItemToALMSucceed, ExportAllItemsToALMSucceed, ExportAllItemsToALMFailed, ExportQCNewTestSetSelectDiffFolder,
        TestSetExists, ErrorInTestsetImport,
        TestSetsImportedSuccessfully,
        TestCasesUpdatedSuccessfully,
        TestCasesUploadedSuccessfully,
        ASCFNotConnected,
        DeleteRepositoryItemAreYouSure,
        SolutionEncryptionKeyUpgrade,
        ForgotKeySaveChanges,
        DeleteTreeFolderAreYouSure,
        RenameRepositoryItemAreYouSure,
        NoPathForCheckIn,
        SaveBusinessFlowChanges,
        UnknownParamInCommandLine,
        SetDriverConfigTypeNotHandled,
        DriverConfigUnknownDriverType,
        SetDriverConfigTypeFieldNotFound,
        ApplicationNotFoundInEnvConfig,
        UnknownConsoleCommand,
        DOSConsolemissingCMDFileName,
        ExecutionReportSent,
        CannontFindBusinessFlow, ResetBusinessFlowRunVariablesFailed,
        AgentNotFound, MissingNewAgentDetails, MissingNewTableDetails, InvalidTableDetails, MissingNewColumn, ChangingAgentDriverAlert, MissingNewDSDetails, DuplicateDSDetails, GingerKeyNameError, GingerKeyNameDuplicate,
        ConfirmToAddTreeItem,
        FailedToAddTreeItem,
        SureWantToDeleteAll, SureWantToDeleteSelectedItems, SureWantToDelete, NoItemToDelete, SelectItemToDelete, FailedToloadTheGrid,
        SureWantToContinue, BaseAPIWarning, MultipleMatchingAPI,
        ErrorReadingRepositoryItem,
        EnvNotFound, SelectItemToAdd, CannotAddGinger,
        ShortcutCreated, ShortcutCreationFailed, CannotRunShortcut,
        SolutionFileNotFound, PlugInFileNotFound,
        MissingAddSolutionInputs, SolutionAlreadyExist, AddSolutionSucceed, AddSolutionFailed,
        MobileConnectionFailed, MobileRefreshScreenShotFailed, MobileShowElementDetailsFailed, MobileActionWasAdded,
        RefreshTreeGroupFailed, FailedToDeleteRepoItem, RunsetNoGingerPresentForBusinessFlow, ExcelInvalidFieldData, ExcelNoWorksheetSelected, ExcelBadWhereClause,
        RecommendNewVersion,
        DragDropOperationFailed,
        ActionIDNotFound,
        ActivityIDNotFound, ActionsDependenciesLoadFailed, ActivitiesDependenciesLoadFailed,
        SelectValidColumn, SelectValidRow,
        DependenciesMissingActions, DependenciesMissingVariables, DuplicateVariable,
        AskIfToGenerateAutoRunDescription,
        MissingApplicationPlatform,
        NoActivitiesGroupWasSelected, ActivitiesGroupActivitiesNotFound, PartOfActivitiesGroupActsNotFound, SureWantToDeleteGroup,
        ItemNameExistsInRepository, ItemExistsInRepository, ItemExternalExistsInRepository, ItemParentExistsInRepository, AskIfWantsToUpdateRepoItemInstances, AskIfWantsToChangeeRepoItem, AskIfWantsToUpdateAllLinkedRepoItem, AskIfWantsToChangeLinkedRepoItem, GetRepositoryItemUsagesFailed, UpdateRepositoryItemUsagesSuccess, FailedToAddItemToSharedRepository, OfferToUploadAlsoTheActivityGroupToRepository,
        ConnectionCloseWarning,
        InvalidCharactersWarning,
        InvalidValueExpression,
        FolderExistsWithName, DownloadedSolutionFromSourceControl, SourceControlFileLockedByAnotherUser,
        SourceControlUpdateFailed, SourceControlCommitFailed, SourceControlChkInSucss, SourceControlChkInConflictHandledFailed, SourceControlGetLatestConflictHandledFailed, SourceControlChkInConflictHandled, SourceControlCheckInLockedByAnotherUser, SourceControlCheckInLockedByMe, SourceControlCheckInUnsavedFileChecked, FailedToUnlockFileDuringCheckIn, SourceControlChkInConfirmtion, SourceControlMissingSelectionToCheckIn, SourceControlResolveConflict, SureWantToDoRevert, SureWantToDoCheckIn,
        NoOptionalAgent, MissingActivityAppMapping,
        SettingsChangeRequireRestart, ChangesRequireRestart, UnsupportedFileFormat, WarnRegradingMissingVariablesUse, NotAllMissingVariablesWereAdded, UpdateApplicationNameChangeInSolution,
        ShareEnvAppWithAllEnvs, ShareEnvAppParamWithAllEnvs, CtrlSsaveEnvApp, CtrlSMissingItemToSave, FailedToSendEmail, FailedToExportBF,
        ReportTemplateNotFound, DriverNotSupportingWindowExplorer, AgentNotRunningAfterWaiting,
        FoundDuplicateAgentsInRunSet, StaticErrorMessage, StaticWarnMessage, StaticInfoMessage, StaticQuestionsMessage, ApplicationAgentNotMapped,
        ActivitiesGroupAlreadyMappedToTC, ExportItemToALMFailed, AskIfToSaveBFAfterExport,
        BusinessFlowAlreadyMappedToTC, AskIfSureWantToClose, AskIfSureWantToRestart, WindowClosed, TargetWindowNotSelected,
        ChangingEnvironmentParameterValue, IFSaveChangesOfBF, AskIfToLoadExternalFields, WhetherToOpenSolution,
        AutomationTabExecResultsNotExists, FolderNamesAreTooLong, FolderSizeTooSmall, DefaultTemplateCantBeDeleted, FileNotExist, ExecutionsResultsProdIsNotOn, ExecutionsResultsNotExists, ExecutionsResultsToDelete, AllExecutionsResultsToDelete, FilterNotBeenSet, RetreivingAllElements, ClickElementAgain, CloseFilterPage,
        BusinessFlowNeedTargetApplication, HTMLReportAttachment, ImageSize,
        GherkinAskToSaveFeatureFile, GherkinScenariosGenerated, GherkinNotifyFeatureFileExists, GherkinNotifyFeatureFileSelectedFromTheSolution, GherkinNotifyBFIsNotExistForThisFeatureFile, GherkinFileNotFound, GherkinColumnNotExist, GherkinActivityNotFound, GherkinBusinessFlowNotCreated, GherkinFeatureFileImportedSuccessfully, GherkinFeatureFileImportOnlyFeatureFileAllowedErrorMessage,
        AskIfSureWantToDeLink, AnalyzerFoundIssues, AnalyzerSaveRunSet,
        AskIfSureWantToUndoChange,
        CurrentActionNotSaved,
        LoseChangesWarn,
        BFNotExistInDB,
        RemoteExecutionResultsCannotBeAccessed,
        // Merged from GingerCore        
        CopiedVariableSuccessfully, AskIfShareVaribalesInRunner, ShareVariableNotSelected,
        WarnOnDynamicActivities,
        WarnOnLinkSharedActivities,
        WarnOnEditLinkSharedActivities,
        EditLinkSharedActivities,
        QcConnectFailureRestAPI,
        ExportedExecDetailsToALMIsInProcess,
        RenameItemError,
        BusinessFlowUpdate,
        MissingExcelDetails, InvalidExcelDetails, InvalidDataSourceDetails, ExportFailed, ExportDetails, ParamExportMessage, MappedtoDataSourceError, CreateTableError, InvalidColumnName, DeleteDSFileError, InvalidDSPath,
        FailedToLoadPlugIn,
        RunsetBuinessFlowWasChanged, RunSetReloadhWarn, RefreshWholeSolution,
        GetModelItemUsagesFailed,
        RecoverItemsMissingSelectionToRecover,
        SourceControlItemAlreadyLocked, SoruceControlItemAlreadyUnlocked,
        SourceControlConflictResolveFailed,
        AskIfToCloseAgent,
        AskIfToDownloadPossibleValues, AskIfToDownloadPossibleValuesShortProcesss, SelectAndSaveCategoriesValues,
        FolderNotExistOrNotAvailible, FolderNameTextBoxIsEmpty, UserHaveNoWritePermission,
        MissingTargetPlatformForConversion, NoConvertibleActionsFound, NoConvertibleActionSelected, SuccessfulConversionDone, NoActivitySelectedForConversion, ActivitiesConversionFailed,
        FileExtensionNotSupported, NotifyFileSelectedFromTheSolution, FileImportedSuccessfully, CompilationErrorOccured, CompilationSucceed, Failedtosaveitems, SaveItemParentWarning, SaveAllItemsParentWarning,
        APIParametersListUpdated, APIMappedToActionIsMissing, NoAPIExistToMappedTo, CreateRunset, DeleteRunners, DeleteRunner, DeleteBusinessflow, DeleteBusinessflows, MissingErrorHandler, CantDeleteRunner, AllItemsSaved, APIModelAlreadyContainsReturnValues,
        InitializeBrowser, AskBeforeDefectProfileDeleting, MissedMandatotryFields, NoDefaultDefectProfileSelected, ALMDefectsWereOpened, AskALMDefectsOpening, WrongValueSelectedFromTheList, WrongNonNumberValueInserted, WrongDateValueInserted, NoDefectProfileCreated, IssuesInSelectedDefectProfile,
        VisualTestingFailedToDeleteOldBaselineImage, ApplitoolsLastExecutionResultsNotExists, ApplitoolsMissingChromeOrFirefoxBrowser, ParameterOptionalValues,
        FindAndRepalceFieldIsEmpty, FindAndReplaceListIsEmpty, FindAndReplaceNoItemsToRepalce, OracleDllIsMissing, ReportsTemplatesSaveWarn,
        POMWizardFailedToLearnElement, POMWizardReLearnWillDeleteAllElements, WizardCantMoveWhileInProcess, POMDriverIsBusy, FindAndReplaceViewRunSetNotSupported, WizardSureWantToCancel,
        POMSearchByGUIDFailed, POMElementSearchByGUIDFailed, NoRelevantAgentInRunningStatus, SolutionSaveWarning,
        InvalidIndexValue, FileOperationError, FolderOperationError, ObjectUnavailable, PatternNotHandled, LostConnection, AskToSelectBusinessflow,
        ScriptPaused, MissingFileLocation, ElementNotFound, TextNotFound, ProvideSearchString, NoTextOccurrence, JSExecutionFailed, FailedToInitiate, FailedToCreateRequestResponse, ActionNotImplemented, RunSetNotExecuted, OperationNotSupported, ValueIssue, MissingTargetApplication,
        ThreadError, ParsingError, SpecifyUniqueValue, ParameterAlreadyExists, DeleteNodesFromRequest, ParameterMerge, ParameterEdit, ParameterUpdate, ParameterDelete, SaveAll, SaveSelected, SaveAllModifiedItems, CopiedErrorInfo, RepositoryNameCantEmpty,
        ExcelProcessingError, EnterValidBusinessflow, DeleteItem, RefreshFolder, RefreshFailed, ReplaceAll, ItemSelection, DifferentItemType, CopyCutOperation, ObjectLoad, POMAgentIsNotRunning, POMNotOnThePageWarn, POMCannotDeleteAutoLearnedElement, ALMDefectsUserInOtaAPI, DuplicateRunsetName,
        POMElementNotExist, UpdateExistingPOMElement, POMMoveElementFromUnmappedToMapped, SavePOMChanges,
        AskIfToUndoChanges, AskIfToUndoItemChanges, AskIfToImportFile, FileAlreadyExistWarn,
        POMDeltaWizardReLearnWillEraseModification, WarnAddLegacyAction, WarnAddLegacyActionAndOfferNew,
        PluginDownloadInProgress, SaveRunsetChanges, LegacyActionsCleanup,
        MissingImplementationForPlatform,
        WarnAddSwingOrWidgetElement,
        AskToSelectValidItem,
        InvalidEncryptionKey,
        SaveRunsetChangesWarn,
        PublishRepositoryInfo,
        FailedToPublishRepositoryInfo,
        MissingErrorString,
        RunsetAutoRunResult,
        RunsetAutoConfigBackWarn,
        SolutionOpenedOnNewerVersion,
        UploadSolutionInfo,
        UploadSolutionToSourceControl,
        UploadSolutionFailed,
        SourceControlBranchNameEmpty, DataSourceSheetNameHasSpace, DataSourceColumnHasSpace
    }

    public static class UserMsgsPool
    {
        public static void LoadUserMsgsPool()
        {
            //Initialize the pool
            Reporter.UserMsgsPool = new Dictionary<eUserMsgKey, UserMsg>();

            //Add user messages to the pool
            #region General Application Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinNotifyFeatureFileExists, new UserMsg(eUserMsgType.ERROR, "Feature File Already Exists", "Feature File with the same name already exist - '{0}'." + Environment.NewLine + "Please select another Feature File to continue.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinNotifyFeatureFileSelectedFromTheSolution, new UserMsg(eUserMsgType.ERROR, "Feature File Already Exists", "Feature File - '{0}'." + Environment.NewLine + "Selected From The Solution folder hence its already exist and cannot be copied to the same place" + Environment.NewLine + "Please select another Feature File to continue.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinNotifyBFIsNotExistForThisFeatureFile, new UserMsg(eUserMsgType.ERROR, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Is Not Exists", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " has to been generated for Feature File - '{0}'." + Environment.NewLine + Environment.NewLine + "Please create " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " from the Editor Page.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinFileNotFound, new UserMsg(eUserMsgType.ERROR, "Gherkin File Not Found", "Gherkin file was not found at the path: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinColumnNotExist, new UserMsg(eUserMsgType.WARN, "Column Not Exist", "Cant find value for: '{0}' Item/s. since column/s not exist in example table", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinActivityNotFound, new UserMsg(eUserMsgType.ERROR, "Activity Not Found", "Activity not found, Name: '{0}'", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinBusinessFlowNotCreated, new UserMsg(eUserMsgType.WARN, "Gherkin " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Creation Failed", "The file did not passed Gherkin compilation hence the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " not created" + Environment.NewLine + "please correct the imported file and create the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " from Gherkin Page", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinFeatureFileImportedSuccessfully, new UserMsg(eUserMsgType.INFO, "Gherkin feature file imported successfully", "Gherkin feature file imported successfully", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinFeatureFileImportOnlyFeatureFileAllowedErrorMessage, new UserMsg(eUserMsgType.ERROR, "Gherkin feature file not valid", "Only Gherkin feature files can be imported", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinAskToSaveFeatureFile, new UserMsg(eUserMsgType.WARN, "Feature File Changes were made", "Do you want to Save the Feature File?" + Environment.NewLine + "WARNING: If you do not manually Save your Feature File, you may loose your work if you close out of Ginger.", eUserMsgOption.YesNoCancel, eUserMsgSelection.Yes));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GherkinScenariosGenerated, new UserMsg(eUserMsgType.INFO, "Scenarios generated", "{0} Scenarios generated successfully", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GeneralErrorOccured, new UserMsg(eUserMsgType.ERROR, "Error Occurred", "Application error occurred." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingImplementation, new UserMsg(eUserMsgType.WARN, "Missing Implementation", "The {0} functionality hasn't been implemented yet.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingImplementation2, new UserMsg(eUserMsgType.WARN, "Missing Implementation", "The functionality hasn't been implemented yet.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingImplementationForPlatform, new UserMsg(eUserMsgType.WARN, "Missing Implementation", "Functionality hasn't been implemented yet for {0} platform.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ApplicationInitError, new UserMsg(eUserMsgType.ERROR, "Application Initialization Error", "Error occurred during application initialization." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.PageLoadError, new UserMsg(eUserMsgType.ERROR, "Page Load Error", "Failed to load the page '{0}'." + Environment.NewLine + "Error Details: '{1}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UserProfileLoadError, new UserMsg(eUserMsgType.ERROR, "User Profile Load Error", "Error occurred during user profile loading." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ItemToSaveWasNotSelected, new UserMsg(eUserMsgType.WARN, "Save", "Item to save was not selected.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RecommendNewVersion, new UserMsg(eUserMsgType.WARN, "Upgrade required", "You are not using the latest version of Ginger. Please go to http://cmitechint1srv:8089/ to get the latest build.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ToSaveChanges, new UserMsg(eUserMsgType.QUESTION, "Save Changes?", "Do you want to save changes?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UnsupportedFileFormat, new UserMsg(eUserMsgType.ERROR, "Save Changes", "File format not supported.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.CtrlSsaveEnvApp, new UserMsg(eUserMsgType.WARN, "Save Environment", "Please select the environment to save.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CtrlSMissingItemToSave, new UserMsg(eUserMsgType.WARN, "Missing Item to Save", "Please select individual item to save or use menu toolbar to save all the items.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.StaticErrorMessage, new UserMsg(eUserMsgType.ERROR, "Error", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.StaticWarnMessage, new UserMsg(eUserMsgType.WARN, "Warning", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.StaticInfoMessage, new UserMsg(eUserMsgType.INFO, "Info", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.StaticQuestionsMessage, new UserMsg(eUserMsgType.QUESTION, "Question", "{0}", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfSureWantToClose, new UserMsg(eUserMsgType.QUESTION, "Close Ginger", "Are you sure you want to close Ginger?" + Environment.NewLine + Environment.NewLine + "Notice: Unsaved changes won't be saved.", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfSureWantToRestart, new UserMsg(eUserMsgType.QUESTION, "Restart Ginger", "Are you sure you want to Restart Ginger?" + Environment.NewLine + Environment.NewLine + "Notice: Unsaved changes won't be saved.", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.BusinessFlowNeedTargetApplication, new UserMsg(eUserMsgType.WARN, "Target Application Not Selected", "Target Application Not Selected! Please Select at least one Target Application", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfSureWantToUndoChange, new UserMsg(eUserMsgType.WARN, "Undo Changes", "Are you sure you want to undo all changes?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            #endregion General Application Messages

            #region Settings
            Reporter.UserMsgsPool.Add(eUserMsgKey.SettingsChangeRequireRestart, new UserMsg(eUserMsgType.INFO, "Settings Change", "For the settings change to take affect you must '{0}' restart Ginger.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ChangesRequireRestart, new UserMsg(eUserMsgType.INFO, "Restart to apply", "The changes will be applied only after save and restart Ginger", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion Settings

            #region Repository
            Reporter.UserMsgsPool.Add(eUserMsgKey.ItemNameExistsInRepository, new UserMsg(eUserMsgType.WARN, "Add Item to Repository", "Item with the name '{0}' already exist in the repository." + Environment.NewLine + Environment.NewLine + "Please change the item name to be unique and try to upload it again.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ItemExistsInRepository, new UserMsg(eUserMsgType.WARN, "Add Item to Repository", "The item '{0}' already exist in the repository (item name in repository is '{1}')." + Environment.NewLine + Environment.NewLine + "Do you want to continue and overwrite it?" + Environment.NewLine + Environment.NewLine + "Note: If you select 'Yes', backup of the existing item will be saved into 'PreVersions' folder.", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ItemExternalExistsInRepository, new UserMsg(eUserMsgType.WARN, "Add Item to Repository", "The item '{0}' is mapped to the same external item like the repository item '{1}'." + Environment.NewLine + Environment.NewLine + "Do you want to continue and overwrite the existing repository item?" + Environment.NewLine + Environment.NewLine + "Note: If you select 'Yes', backup of the existing repository item will be saved into 'PreVersions' folder.", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ItemParentExistsInRepository, new UserMsg(eUserMsgType.WARN, "Add Item to Repository", "The item '{0}' source is from the repository item '{1}'." + Environment.NewLine + Environment.NewLine + "Do you want to overwrite the source repository item?" + Environment.NewLine + Environment.NewLine + "Note:" + Environment.NewLine + "If you select 'No', the item will be added as a new item to the repository." + Environment.NewLine + "If you select 'Yes', backup of the existing repository item will be saved into 'PreVersions' folder.", eUserMsgOption.YesNoCancel, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToAddItemToSharedRepository, new UserMsg(eUserMsgType.ERROR, "Add Item to Repository", "Failed to add the item '{0}' to shared repository." + Environment.NewLine + Environment.NewLine + "Error Details: {1}.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfWantsToUpdateRepoItemInstances, new UserMsg(eUserMsgType.WARN, "Update Repository Item Usages", "The item '{0}' has {1} instances." + Environment.NewLine + Environment.NewLine + "Do you want to review them and select which one to get updated as well?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfWantsToChangeeRepoItem, new UserMsg(eUserMsgType.WARN, "Change Repository Item", "The item '{0}' is been used in {1} places." + Environment.NewLine + Environment.NewLine + "Are you sure you want to {2} it?" + Environment.NewLine + Environment.NewLine + "Note: Anyway the changes won't affect the linked instances of this item", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfWantsToChangeLinkedRepoItem, new UserMsg(eUserMsgType.WARN, "Change Repository Item", "The item '{0}' may be used as Link in many places. Modifying it will auto update all Linked instances." + Environment.NewLine + Environment.NewLine + "Are you sure you want to {1} it?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfWantsToUpdateAllLinkedRepoItem, new UserMsg(eUserMsgType.WARN, "Change Repository Items", "These repository items may be used as Link in many places. Modifying it will auto update all Linked instances." + Environment.NewLine + Environment.NewLine + "Are you sure you want to save it?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GetRepositoryItemUsagesFailed, new UserMsg(eUserMsgType.ERROR, "Repository Item Usages", "Failed to get the '{0}' item usages." + Environment.NewLine + Environment.NewLine + "Error Details: {1}.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UpdateRepositoryItemUsagesSuccess, new UserMsg(eUserMsgType.INFO, "Update Repository Item Usages", "Finished to update the repository items usages." + Environment.NewLine + Environment.NewLine + "Note: Updates were not saved yet.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfSureWantToDeLink, new UserMsg(eUserMsgType.WARN, "De-Link to Shared Repository", "Are you sure you want to de-link the item from it Shared Repository source item?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.OfferToUploadAlsoTheActivityGroupToRepository, new UserMsg(eUserMsgType.QUESTION, "Add the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to Repository", "The " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " '{0}' is part of the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " '{1}', do you want to add the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to the shared repository as well?" + System.Environment.NewLine + System.Environment.NewLine + "Note: If you select Yes, only the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " will be added to the repository and not all of it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + ".", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.PublishRepositoryInfo, new UserMsg(eUserMsgType.INFO, "Item Repository Publish Info", "Repository item is published to selected BusinessFlow.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToPublishRepositoryInfo, new UserMsg(eUserMsgType.ERROR, "Failed to publish Repository Item", "Failed to publish in one or more Business flows.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingErrorString, new UserMsg(eUserMsgType.ERROR, "Missing Error String details", "Error String is missing for one or more row." + Environment.NewLine + "Please add error string for missing rows", eUserMsgOption.OK, eUserMsgSelection.None));

            #endregion Repository

            #region Analyzer
            Reporter.UserMsgsPool.Add(eUserMsgKey.AnalyzerFoundIssues, new UserMsg(eUserMsgType.WARN, "Issues Detected By Analyzer", "Critical/High Issues were detected, please handle them before execution.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AnalyzerSaveRunSet, new UserMsg(eUserMsgType.WARN, "Issues Detected By Analyzer", "Please save the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " first", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion Analyzer

            #region Registry Values Check Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.RegistryValuesCheckFailed, new UserMsg(eUserMsgType.ERROR, "Run Registry Values Check", "Failed to check if all needed registry values exist.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AddRegistryValue, new UserMsg(eUserMsgType.QUESTION, "Missing Registry Value", "The required registry value for the key: '{0}' is missing or wrong. Do you want to add/fix it?", eUserMsgOption.YesNo, eUserMsgSelection.Yes));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AddRegistryValueSucceed, new UserMsg(eUserMsgType.INFO, "Add Registry Value", "Registry value added successfully for the key: '{0}'." + Environment.NewLine + "Please restart the application.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AddRegistryValueFailed, new UserMsg(eUserMsgType.ERROR, "Add Registry Value", "Registry value add failed for the key: '{0}'." + Environment.NewLine + "Please restart the application as administrator and try again.", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion Registry Values Check Messages

            #region SourceControl Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoItemWasSelected, new UserMsg(eUserMsgType.WARN, "No item was selected", "Please select an item to proceed.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskToAddCheckInComment, new UserMsg(eUserMsgType.WARN, "Check-In Changes", "Please enter check-in comments.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToGetProjectsListFromSVN, new UserMsg(eUserMsgType.ERROR, "Source Control Error", "Failed to get the solutions list from the source control." + Environment.NewLine + "Error Details: '{0}'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskToSelectSolution, new UserMsg(eUserMsgType.WARN, "Select Solution", "Please select solution.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlFileLockedByAnotherUser, new UserMsg(eUserMsgType.WARN, "Source Control File Locked", "The file '{0}' was locked by: {1} " + Environment.NewLine + "Locked comment {2}." + Environment.NewLine + Environment.NewLine + " Are you sure you want to unlock the file?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DownloadedSolutionFromSourceControl, new UserMsg(eUserMsgType.INFO, "Download Solution", "The solution '{0}' was downloaded successfully." + Environment.NewLine + "Do you want to open the downloaded Solution?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UpdateToRevision, new UserMsg(eUserMsgType.INFO, "Update Solution", "The solution was updated successfully to revision: {0}.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CommitedToRevision, new UserMsg(eUserMsgType.INFO, "Commit Solution", "The changes were committed successfully, Revision: {0}.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GitUpdateState, new UserMsg(eUserMsgType.INFO, "Update Solution", "The solution was updated successfully, Update status: {0}.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoPathForCheckIn, new UserMsg(eUserMsgType.ERROR, "No Path for Check-In", "Missing Path", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlConnFaild, new UserMsg(eUserMsgType.ERROR, "Source Control Connection", "Failed to establish connection to the source control." + Environment.NewLine + "Error Details: '{0}'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlRemoteCannotBeAccessed, new UserMsg(eUserMsgType.ERROR, "Source Control Connection", "Failed to establish connection to the source control." + Environment.NewLine + "The remote repository cannot be accessed, please check your internet connection and your proxy configurations" + Environment.NewLine + Environment.NewLine + "Error details: '{0}'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlUnlockFaild, new UserMsg(eUserMsgType.ERROR, "Source Control Unlock", "Failed to unlock remote file, File locked by different user." + Environment.NewLine + "Locked by: '{0}'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlConnSucss, new UserMsg(eUserMsgType.INFO, "Source Control Connection", "Succeed to establish connection to the source control", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlLockSucss, new UserMsg(eUserMsgType.INFO, "Source Control Lock", "Succeed to Lock the file in the source control", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlUnlockSucss, new UserMsg(eUserMsgType.INFO, "Source Control Unlock", "Succeed to unlock the file in the source control", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlConnMissingConnInputs, new UserMsg(eUserMsgType.WARN, "Source Control Connection", "Missing connection inputs, please set the source control URL, user name and password.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlConnMissingLocalFolderInput, new UserMsg(eUserMsgType.WARN, "Download Solution", "Missing local folder input, please select local folder to download the solution into.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlUpdateFailed, new UserMsg(eUserMsgType.ERROR, "Update Solution", "Failed to update the solution from source control." + Environment.NewLine + "Error Details: '{0}'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlCommitFailed, new UserMsg(eUserMsgType.ERROR, "Commit Solution", "Failed to commit the solution from source control." + Environment.NewLine + "Error Details: '{0}'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlChkInSucss, new UserMsg(eUserMsgType.QUESTION, "Check-In Changes", "Check-in process ended successfully." + Environment.NewLine + Environment.NewLine + "Do you want to do another check-in?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlChkInConflictHandled, new UserMsg(eUserMsgType.QUESTION, "Check-In Results", "The check in process ended, please notice that conflict was identified during the process and may additional check in will be needed for pushing the conflict items." + Environment.NewLine + Environment.NewLine + "Do you want to do another check-in?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlChkInConflictHandledFailed, new UserMsg(eUserMsgType.WARN, "Check-In Results", "Check in process ended with unhandled conflict please notice that you must handled them prior to the next check in of those items", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlGetLatestConflictHandledFailed, new UserMsg(eUserMsgType.WARN, "Get Latest Results", "Get latest process encountered conflicts which must be resolved for successful Solution loading", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlCheckInLockedByAnotherUser, new UserMsg(eUserMsgType.WARN, "Locked Item Check-In", "The item: '{0}'" + Environment.NewLine + "is locked by the user: '{1}'" + Environment.NewLine + "The locking comment is: '{2}' ." + Environment.NewLine + Environment.NewLine + "Do you want to unlock the item and proceed with check in process?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlCheckInLockedByMe, new UserMsg(eUserMsgType.WARN, "Locked Item Check-In", "The item: '{0}'" + Environment.NewLine + "is locked by you please noticed that during check in the lock will need to be removed please confirm to continue", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlCheckInUnsavedFileChecked, new UserMsg(eUserMsgType.QUESTION, "Unsaved Item Checkin", "The item: '{0}' contains unsaved changes which must be saved prior to the check in process." + Environment.NewLine + Environment.NewLine + "Do you want to save the item and proceed with check in process?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToUnlockFileDuringCheckIn, new UserMsg(eUserMsgType.QUESTION, "Locked Item Check-In Failure", "The item: '{0}' unlock operation failed on the URL: '{1}'." + Environment.NewLine + Environment.NewLine + "Do you want to proceed with the check in  process for rest of the items?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlChkInConfirmtion, new UserMsg(eUserMsgType.QUESTION, "Check-In Changes", "Checking in changes will effect all project users, are you sure you want to continue and check in those {0} changes?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlMissingSelectionToCheckIn, new UserMsg(eUserMsgType.WARN, "Check-In Changes", "Please select items to check-in.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlResolveConflict, new UserMsg(eUserMsgType.QUESTION, "Source Control Conflicts", "Source control conflicts has been identified for the path: '{0}'." + Environment.NewLine + "You probably won't be able to use the item which in the path till conflicts will be resolved." + Environment.NewLine + Environment.NewLine + "Do you want to automatically resolve the conflicts and keep your local changes for all conflicts?" + Environment.NewLine + Environment.NewLine + "Select 'No' for accepting server updates for all conflicts or select 'Cancel' for canceling the conflicts handling.", eUserMsgOption.YesNoCancel, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SureWantToDoRevert, new UserMsg(eUserMsgType.QUESTION, "Undo Changes", "Are you sure you want to revert changes?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SureWantToDoCheckIn, new UserMsg(eUserMsgType.QUESTION, "Check-In Changes", "Are you sure you want to check-in changes?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UploadSolutionInfo, new UserMsg(eUserMsgType.INFO, "Upload Solution", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UploadSolutionToSourceControl, new UserMsg(eUserMsgType.QUESTION, "Upload Solution", "The solution was created and loaded successfully.\nPlease make a note of the encryption key provided from solution details page." + Environment.NewLine + "Do you want to upload the Solution: '{0}' to Source Control?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UploadSolutionFailed, new UserMsg(eUserMsgType.ERROR, "Upload Solution", "Failed to Upload Solution to Source Control" + Environment.NewLine + "'{0}'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlBranchNameEmpty, new UserMsg(eUserMsgType.ERROR, "Upload Solution", "Branch name cannot be empty.", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion SourceControl Messages

            #region Validation Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.PleaseStartAgent, new UserMsg(eUserMsgType.WARN, "Start Agent", "Please start agent in order to run validation.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskToSelectValidation, new UserMsg(eUserMsgType.WARN, "Select Validation", "Please select validation to edit.", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion Validation Messages

            #region DataBase Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.ErrorConnectingToDataBase, new UserMsg(eUserMsgType.ERROR, "Cannot connect to database", "DB Connection error." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ErrorClosingConnectionToDataBase, new UserMsg(eUserMsgType.ERROR, "Error in closing connection to database", "DB Close Connection error" + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DbTableNameError, new UserMsg(eUserMsgType.ERROR, "Invalid DB Table Name", "Table with the name '{0}' already exist in the Database. Please try another name.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DbTableError, new UserMsg(eUserMsgType.ERROR, "DB Table Error", "Error occurred while trying to get the {0}." + Environment.NewLine + "Error Details: '{1}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DbQueryError, new UserMsg(eUserMsgType.ERROR, "DB Query Error", "The DB Query returned error, please double check the table name and field name." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DbConnFailed, new UserMsg(eUserMsgType.ERROR, "DB Connection Status", "Connect to the DB failed.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DbConnSucceed, new UserMsg(eUserMsgType.INFO, "DB Connection Status", "Connect to the DB succeeded.", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion DataBase Messages

            #region Environment Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingUnixCredential, new UserMsg(eUserMsgType.ERROR, "Unix credential is missing or invalid", "Unix credential is missing or invalid in Environment settings, please open Environment tab to check.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.EnvironmentItemLoadError, new UserMsg(eUserMsgType.ERROR, "Environment Item Load Error", "Failed to load the {0}." + Environment.NewLine + "Error Details: '{1}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ShareEnvAppWithAllEnvs, new UserMsg(eUserMsgType.INFO, "Share Environment Applications", "The selected application/s were added to the other solution environments." + Environment.NewLine + Environment.NewLine + "Note: The changes were not saved, please perform save for all changed environments.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ShareEnvAppParamWithAllEnvs, new UserMsg(eUserMsgType.INFO, "Share Environment Application Parameter", "The selected parameter/s were added to the other solution environments matching applications." + Environment.NewLine + Environment.NewLine + "Note: The changes were not saved, please perform save for all changed environments.", eUserMsgOption.OK, eUserMsgSelection.None));

            #endregion Environment Messages

            #region Agents/Drivers Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.MobileDriverNotConnected, new UserMsg(eUserMsgType.INFO, "Mobile Device Error", "The connection to the mobile device has failed, please try again.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.TestagentSucceed, new UserMsg(eUserMsgType.INFO, "Test Agent", "Agent starting test ended successfully!", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToConnectAgent, new UserMsg(eUserMsgType.ERROR, "Connect to Agent", "Failed to load '{0}' Agent." + Environment.NewLine + "Error Details: '{1}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SshCommandError, new UserMsg(eUserMsgType.ERROR, "Ssh Command Error", "The Ssh Run Command returns error, please double check the connection info and credentials." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GoToUrlFailure, new UserMsg(eUserMsgType.ERROR, "Go To URL Error", "Failed to go to the URL: '{0}'." + Environment.NewLine + "Error Details: '{1}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.HookLinkEventError, new UserMsg(eUserMsgType.ERROR, "Hook Link Event Error", "The link type is unknown.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskToStartAgent, new UserMsg(eUserMsgType.WARN, "Missing Agent", "Please start/select agent.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RestartAgent, new UserMsg(eUserMsgType.WARN, "Agent Restart needed", "Please restart agent.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ASCFNotConnected, new UserMsg(eUserMsgType.ERROR, "Not Connected to ASCF", "Please Connect first.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SetDriverConfigTypeNotHandled, new UserMsg(eUserMsgType.ERROR, "Set Driver configuration", "Unknown Type {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DriverConfigUnknownDriverType, new UserMsg(eUserMsgType.ERROR, "Driver Configuration", "Unknown Driver Type {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SetDriverConfigTypeFieldNotFound, new UserMsg(eUserMsgType.ERROR, "Driver Configuration", "Unknown Driver Parameter {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UnknownConsoleCommand, new UserMsg(eUserMsgType.ERROR, "Unknown Console Command", "Command {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DOSConsolemissingCMDFileName, new UserMsg(eUserMsgType.ERROR, "DOS Console Driver Error", "DOSConsolemissingCMDFileName", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CannontFindBusinessFlow, new UserMsg(eUserMsgType.ERROR, "Cannot Find " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), "Missing flow in solution {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AgentNotFound, new UserMsg(eUserMsgType.ERROR, "Cannot Find Agent", "Missing Agent from Run Config- {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingNewAgentDetails, new UserMsg(eUserMsgType.WARN, "Missing Agent Details", "The new Agent {0} is missing.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingNewTableDetails, new UserMsg(eUserMsgType.ERROR, "Missing Table Details", "The new Table {0} is missing.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.InvalidTableDetails, new UserMsg(eUserMsgType.ERROR, "InValid Table Details", "The Table Name provided is Invalid. It cannot contain spaces", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingNewDSDetails, new UserMsg(eUserMsgType.WARN, "Missing Data Source Details", "The new Data Source {0} is missing.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DuplicateDSDetails, new UserMsg(eUserMsgType.ERROR, "Duplicate DataSource Details", "The Data Source with the File Path {0} already Exist. Please use another File Path", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GingerKeyNameError, new UserMsg(eUserMsgType.ERROR, "InValid Ginger Key Name", "The Ginger Key Name cannot be Duplicate or NULL. Please fix before continuing.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GingerKeyNameDuplicate, new UserMsg(eUserMsgType.ERROR, "InValid Ginger Key Name", "The Ginger Key Name cannot be Duplicated. Please change the Key Name : {0} in Table : {1}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingNewColumn, new UserMsg(eUserMsgType.WARN, "Missing Column Details", "The new Column {0} is missing.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingApplicationPlatform, new UserMsg(eUserMsgType.WARN, "Missing Application Platform Info", "The default Application Platform Info is missing, please go to Solution level to add at least one Target Application.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ConnectionCloseWarning, new UserMsg(eUserMsgType.WARN, "Connection Close", "Closing this window will cause the connection to {0} to be closed, to continue and close?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ChangingAgentDriverAlert, new UserMsg(eUserMsgType.WARN, "Changing Agent Driver", "Changing the Agent driver type will cause all driver configurations to be reset, to continue?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.InvalidCharactersWarning, new UserMsg(eUserMsgType.WARN, "Invalid Details", "Name can't contain any of the following characters: " + Environment.NewLine + " /, \\, *, :, ?, \", <, >, |", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.InvalidValueExpression, new UserMsg(eUserMsgType.WARN, "Invalid Value Expression", "{0} - Value Expression has some Error. Do you want to continue?", eUserMsgOption.YesNo, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoOptionalAgent, new UserMsg(eUserMsgType.WARN, "No Optional Agent", "No optional Agent was found." + Environment.NewLine + "Please configure new Agent under the Solution to be used for this application.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DriverNotSupportingWindowExplorer, new UserMsg(eUserMsgType.WARN, "Open Window Explorer", "The driver '{0}' is not supporting the Window Explorer yet.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AgentNotRunningAfterWaiting, new UserMsg(eUserMsgType.WARN, "Running Agent", "The Agent '{0}' failed to start running after {1} seconds.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ApplicationAgentNotMapped, new UserMsg(eUserMsgType.WARN, "Agent Not Mapped to Target Application", "The Agent Mapping for Target Application  '{0}' , Please map Agent.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WindowClosed, new UserMsg(eUserMsgType.WARN, "Invalid target window", "Target window is either closed or no longer available. \n\n Please Add switch Window action by selecting correct target window on Window Explorer", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.TargetWindowNotSelected, new UserMsg(eUserMsgType.WARN, "Target Window not Selected", "Please choose the target window from available list", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ChangingEnvironmentParameterValue, new UserMsg(eUserMsgType.WARN, "Changing Environment Variable Name", "Changing the Environment variable name will cause rename this environment variable name in every " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", do you want to continue?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SaveLocalChanges, new UserMsg(eUserMsgType.QUESTION, "Save Local Changes?", "Your Local Changes will be saved. Do you want to Continue?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.LooseLocalChanges, new UserMsg(eUserMsgType.WARN, "Loose Local Changes?", "Your Local Changes will be Lost. Do you want to Continue?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.IFSaveChangesOfBF, new UserMsg(eUserMsgType.WARN, "Save Current" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Before Change?", "Do you want to save the changes made in the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + "?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.RecordingStopped, new UserMsg(eUserMsgType.ERROR, "Recording Stopped", "Recording Stopped for the {0} agent." + Environment.NewLine + "Error Details: '{1}'.", eUserMsgOption.OK, eUserMsgSelection.None));

            #endregion Agents/Drivers Messages

            #region Actions Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingActionPropertiesEditor, new UserMsg(eUserMsgType.ERROR, "Action Properties Editor", "No Action properties Editor yet for '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskToSelectItem, new UserMsg(eUserMsgType.WARN, "Select Item", "Please select item.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskToSelectValidItem, new UserMsg(eUserMsgType.WARN, "Select Item", "Please select valid item.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskToSelectAction, new UserMsg(eUserMsgType.WARN, "Select Action", "Please select action.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ImportSeleniumScriptError, new UserMsg(eUserMsgType.ERROR, "Import Selenium Script Error", "Error occurred while trying to import Selenium Script, please make sure the html file is a Selenium script generated by Selenium IDE.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WarnAddLegacyAction, new UserMsg(eUserMsgType.WARN, "Legacy Action Warning", "This Action is part of the Legacy Actions which will be deprecated soon and not recommended to be used anymore." + Environment.NewLine + "Do you still interested adding it?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WarnAddLegacyActionAndOfferNew, new UserMsg(eUserMsgType.WARN, "Legacy Action Warning", "This Action is part of the Legacy Actions which will be deprecated soon and not recommended to be used anymore." + Environment.NewLine + "Do you prefer to add the replacement Action ({0}) for it?", eUserMsgOption.YesNoCancel, eUserMsgSelection.Yes));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WarnAddSwingOrWidgetElement, new UserMsg(eUserMsgType.WARN, "Add Element Type Warning", "Do you want to add widget element?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            #endregion Actions Messages

            #region Runset Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.RunsetNoGingerPresentForBusinessFlow, new UserMsg(eUserMsgType.WARN, "No Ginger in " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Please add at least one Ginger to your " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " before choosing a " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ".", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ResetBusinessFlowRunVariablesFailed, new UserMsg(eUserMsgType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Variables) + " Reset Failed", "Failed to reset the " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " to original configurations." + System.Environment.NewLine + "Error Details: {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfToGenerateAutoRunDescription, new UserMsg(eUserMsgType.QUESTION, "Automatic Description Creation", "Do you want to automatically populate the Run Description?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.RunsetAutoRunResult, new UserMsg(eUserMsgType.INFO, "Runset Auto Run Configuration Result", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RunsetAutoConfigBackWarn, new UserMsg(eUserMsgType.WARN, "Runset Auto Run Configuration Changes", "{0}", eUserMsgOption.YesNo, eUserMsgSelection.No));

            #endregion Runset Messages

            #region Excel Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExcelInvalidFieldData, new UserMsg(eUserMsgType.WARN, "Missing file or data", "Field data is missing or file is being used by another process. please check.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExcelNoWorksheetSelected, new UserMsg(eUserMsgType.WARN, "Missing worksheet", "Please select a worksheet", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExcelBadWhereClause, new UserMsg(eUserMsgType.WARN, "Problem with WHERE clause", "Please check your WHERE clause. Do all column names exist and are they spelled correctly?", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion Excel Messages

            #region Variables Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskToSelectVariable, new UserMsg(eUserMsgType.WARN, "Select " + GingerDicser.GetTermResValue(eTermResKey.Variable), "Please select " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ".", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.VariablesAssignError, new UserMsg(eUserMsgType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Variables) + " Assign Error", "Failed to assign " + GingerDicser.GetTermResValue(eTermResKey.Variables) + "." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SetCycleNumError, new UserMsg(eUserMsgType.ERROR, "Set Cycle Number Error", "Failed to set the cycle number." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.VariablesParentNotFound, new UserMsg(eUserMsgType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Variables) + " Parent Error", "Failed to find the " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " parent.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CantStoreToVariable, new UserMsg(eUserMsgType.ERROR, "Store to " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Failed", "Cannot Store to " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " '{0}'- " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " not found", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WarnRegradingMissingVariablesUse, new UserMsg(eUserMsgType.WARN, "Unassociated " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " been Used in " + GingerDicser.GetTermResValue(eTermResKey.Activity), GingerDicser.GetTermResValue(eTermResKey.Variables) + " which are not part of the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " list are been used in it." + Environment.NewLine + "For the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " to work as a standalone you will need to add those " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: ".") + Environment.NewLine + Environment.NewLine + "Missing " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: ":") + Environment.NewLine + "{1}" + Environment.NewLine + Environment.NewLine + "Do you want to automatically add the missing " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: "?") + Environment.NewLine + "Note: Clicking 'No' will cancel the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Upload.", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NotAllMissingVariablesWereAdded, new UserMsg(eUserMsgType.WARN, "Unassociated " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " been Used in " + GingerDicser.GetTermResValue(eTermResKey.Activity), "Failed to find and add the following missing " + GingerDicser.GetTermResValue(eTermResKey.Variables, suffixString: ":") + Environment.NewLine + "{0}" + Environment.NewLine + Environment.NewLine + "Please add those " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " manually for allowing the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Upload.", eUserMsgOption.OK, eUserMsgSelection.No));
            #endregion Variables Messages

            #region Solution Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.BeginWithNoSelectSolution, new UserMsg(eUserMsgType.INFO, "No solution is selected", "You have not selected any existing solution, please open an existing one or create a new solution by pressing the New button.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskToSelectSolutionFolder, new UserMsg(eUserMsgType.WARN, "Missing Folder Selection", "Please select the folder you want to perform the action on.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DeleteRepositoryItemAreYouSure, new UserMsg(eUserMsgType.WARN, "Delete", "Are you sure you want to delete '{0}' item?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DeleteTreeFolderAreYouSure, new UserMsg(eUserMsgType.WARN, "Delete Folder", "Are you sure you want to delete the '{0}' folder and all of it content?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RenameRepositoryItemAreYouSure, new UserMsg(eUserMsgType.WARN, "Rename", "Are you sure you want to rename '{0}'?", eUserMsgOption.YesNoCancel, eUserMsgSelection.Yes));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SaveBusinessFlowChanges, new UserMsg(eUserMsgType.QUESTION, "Save Changes", "Save Changes to - {0}", eUserMsgOption.YesNoCancel, eUserMsgSelection.Yes));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SolutionLoadError, new UserMsg(eUserMsgType.ERROR, "Solution Load Error", "Failed to load the solution." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingAddSolutionInputs, new UserMsg(eUserMsgType.WARN, "Add Solution", "Missing solution inputs, please set the solution name, folder, encryption key and main application details.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SolutionAlreadyExist, new UserMsg(eUserMsgType.WARN, "Add Solution", "The solution already exist, please select different name/folder.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AddSolutionSucceed, new UserMsg(eUserMsgType.INFO, "Add Solution", "The solution was created and loaded successfully.\nPlease make a note of the encryption key provided from solution details page", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AddSolutionFailed, new UserMsg(eUserMsgType.ERROR, "Add Solution", "Failed to create the new solution. " + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RefreshTreeGroupFailed, new UserMsg(eUserMsgType.ERROR, "Refresh", "Failed to perform the refresh operation." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToDeleteRepoItem, new UserMsg(eUserMsgType.ERROR, "Delete", "Failed to perform the delete operation." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.InvalidEncryptionKey, new UserMsg(eUserMsgType.WARN, "Encryption Key", "Encryption key must be 8-16 in lenght and should contain atleast 1 cap, 1 small, 1 digit and 1 special char.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SolutionOpenedOnNewerVersion, new UserMsg(eUserMsgType.QUESTION, "Load Solution", "Note: Ginger version is higher than the loaded Solution version which will make it incompatible with older versions of Ginger if you will make any changes on the Solution, are you sure you want to continue with loading the Solution?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.FolderExistsWithName, new UserMsg(eUserMsgType.WARN, "Folder Creation Failed", "Folder with same name already exists. Please choose a different name for the folder.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UpdateApplicationNameChangeInSolution, new UserMsg(eUserMsgType.WARN, "Target Application Name Change", "Do you want to automatically update the Target Application name in all Solution items?" + Environment.NewLine + Environment.NewLine + "Note: If you choose 'Yes', changes won't be saved, for saving them please click 'SaveAll'", eUserMsgOption.YesNo, eUserMsgSelection.Yes));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SaveRunsetChanges, new UserMsg(eUserMsgType.QUESTION, "Save Changes", "There are unsaved changes in runset, Do you want to save it?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SolutionEncryptionKeyUpgrade, new UserMsg(eUserMsgType.INFO, "Solution Passwords encryption key updated", "'{0}'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ForgotKeySaveChanges, new UserMsg(eUserMsgType.QUESTION, "Confirm to set new key", "All password values in the solution will be cleared and need to be entered again. Are you Sure you want to set a new key ?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SaveRunsetChangesWarn, new UserMsg(eUserMsgType.WARN, "Save Changes", "Runset execution will be reset on clicking on Yes.", eUserMsgOption.YesNo, eUserMsgSelection.No));

            #endregion Solution Messages

            #region Activities
            Reporter.UserMsgsPool.Add(eUserMsgKey.ActionsDependenciesLoadFailed, new UserMsg(eUserMsgType.ERROR, "Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies Load Failed", "Failed to load the Actions-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies data." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ActivitiesDependenciesLoadFailed, new UserMsg(eUserMsgType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Dependencies Load Failed", "Failed to load the " + GingerDicser.GetTermResValue(eTermResKey.Activities) + "-" + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies data." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DependenciesMissingActions, new UserMsg(eUserMsgType.INFO, "Missing Actions", "Actions not found." + System.Environment.NewLine + "Please add Actions to the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + ".", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DependenciesMissingVariables, new UserMsg(eUserMsgType.INFO, "Missing " + GingerDicser.GetTermResValue(eTermResKey.Variables), GingerDicser.GetTermResValue(eTermResKey.Variables) + " not found." + System.Environment.NewLine + "Please add " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " from type 'Selection List' to the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + ".", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DuplicateVariable, new UserMsg(eUserMsgType.WARN, "Duplicated " + GingerDicser.GetTermResValue(eTermResKey.Variable), "The " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " name '{0}' and value '{1}' exist more than once." + System.Environment.NewLine + "Please make sure only one instance exist in order to set the " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingActivityAppMapping, new UserMsg(eUserMsgType.WARN, "Missing " + GingerDicser.GetTermResValue(eTermResKey.Activity) + "-Application Mapping", "Target Application was not mapped to the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " so the required Actions platform is unknown." + System.Environment.NewLine + System.Environment.NewLine + "Map Target Application to the Activity by double clicking the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " record and select the application you want to test using it.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.LegacyActionsCleanup, new UserMsg(eUserMsgType.INFO, "Legacy Actions Cleanup", "Legacy Actions cleanup was ended." + Environment.NewLine + Environment.NewLine + "Cleanup Statistics:" + Environment.NewLine + "Number of Processed " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + ": {0}" + Environment.NewLine + "Number of Deleted " + GingerDicser.GetTermResValue(eTermResKey.Activities) + ": {1}" + Environment.NewLine + "Number of Deleted Actions: {2}", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion Activities

            #region Support Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.AddSupportValidationFailed, new UserMsg(eUserMsgType.WARN, "Add Support Request Validation Error", "Add support request validation failed." + Environment.NewLine + "Failure Reason:" + Environment.NewLine + "'{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UploadSupportRequestSuccess, new UserMsg(eUserMsgType.INFO, "Upload Support Request", "Upload was completed successfully.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UploadSupportRequestFailure, new UserMsg(eUserMsgType.ERROR, "Upload Support Request", "Failed to complete the upload." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion Support Messages

            #region SharedFunctions Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.FunctionReturnedError, new UserMsg(eUserMsgType.ERROR, "Error Occurred", "The {0} returns error. please check the provided details." + Environment.NewLine + "Error Details: '{1}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ShowInfoMessage, new UserMsg(eUserMsgType.INFO, "Message", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion SharedFunctions Messages

            #region CustomeFunctions Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.LogoutSuccess, new UserMsg(eUserMsgType.INFO, "Logout", "Logout completed successfully- {0}.", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion CustomeFunctions Messages

            #region ALM
            Reporter.UserMsgsPool.Add(eUserMsgKey.QcConnectSuccess, new UserMsg(eUserMsgType.INFO, "QC/ALM Connection", "QC/ALM connection successful!", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.QcConnectFailure, new UserMsg(eUserMsgType.WARN, "QC/ALM Connection Failed", "QC/ALM connection failed." + System.Environment.NewLine + "Please make sure that the credentials you use are correct and that QC/ALM Client is registered on your machine." + System.Environment.NewLine + System.Environment.NewLine + "For registering QC/ALM Client- please follow below steps:" + System.Environment.NewLine + "1. Launch Internet Explorer as Administrator" + System.Environment.NewLine + "2. Go to http://<QCURL>/qcbin" + System.Environment.NewLine + "3. Click on 'Add-Ins Page' link" + System.Environment.NewLine + "4. In next page, click on 'HP ALM Client Registration'" + System.Environment.NewLine + "5. In next page click on 'Register HP ALM Client'" + System.Environment.NewLine + "6. Restart Ginger and try to reconnect", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ALMConnectFailureWithCurrSettings, new UserMsg(eUserMsgType.WARN, "ALM Connection Failed", "ALM Connection Failed, Please make sure credentials are correct.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ALMConnectFailure, new UserMsg(eUserMsgType.WARN, "ALM Connection Failed", "ALM connection failed." + System.Environment.NewLine + "Please make sure that the credentials you use are correct and that ALM Client is registered on your machine.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.QcLoginSuccess, new UserMsg(eUserMsgType.INFO, "Login Success", "QC/ALM Login successful!", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ALMLoginFailed, new UserMsg(eUserMsgType.WARN, "Login Failed", "ALM Login Failed - {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.QcNeedLogin, new UserMsg(eUserMsgType.WARN, "Not Connected to QC/ALM", "You Must Log Into QC/ALM First.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.TestCasesUpdatedSuccessfully, new UserMsg(eUserMsgType.INFO, "TestCase Update", "TestCases Updated Successfully.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.TestCasesUploadedSuccessfully, new UserMsg(eUserMsgType.INFO, "TestCases Uploaded", "TestCases Uploaded Successfully.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.TestSetsImportedSuccessfully, new UserMsg(eUserMsgType.INFO, "Import ALM Test Set", "ALM Test Set/s import process ended successfully.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.TestSetExists, new UserMsg(eUserMsgType.WARN, "Import Exiting Test Set", "The Test Set '{0}' was imported before and already exists in current Solution." + System.Environment.NewLine + "Do you want to delete the existing mapped " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " and import the Test Set again?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ErrorInTestsetImport, new UserMsg(eUserMsgType.ERROR, "Import Test Set Error", "Error Occurred while exporting the Test Set '{0}'." + System.Environment.NewLine + "Error Details:{1}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ErrorWhileExportingExecDetails, new UserMsg(eUserMsgType.ERROR, "Export Execution Details Error", "Error occurred while exporting the execution details to QC/ALM." + System.Environment.NewLine + "Error Details:{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExportedExecDetailsToALM, new UserMsg(eUserMsgType.INFO, "Export Execution Details", "Export execution details result: {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExportAllItemsToALMSucceed, new UserMsg(eUserMsgType.INFO, "Export All Items to ALM", "All items has been successfully exported to ALM.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExportAllItemsToALMFailed, new UserMsg(eUserMsgType.INFO, "Export All Items to ALM", "While exporting to ALM One or more items failed to export.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExportItemToALMSucceed, new UserMsg(eUserMsgType.INFO, "Export ALM Item", "Exporting item to ALM process ended successfully.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ALMOperationFailed, new UserMsg(eUserMsgType.ERROR, "ALM Operation Failed", "Failed to perform the {0} operation." + Environment.NewLine + Environment.NewLine + "Error Details: '{1}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ActivitiesGroupAlreadyMappedToTC, new UserMsg(eUserMsgType.WARN, "Export " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to QC/ALM", "The " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " '{0}' is already mapped to the QC/ALM '{1}' Test Case, do you want to update it?" + Environment.NewLine + Environment.NewLine + "Select 'Yes' to update or 'No' to create new Test Case.", eUserMsgOption.YesNoCancel, eUserMsgSelection.Cancel));
            Reporter.UserMsgsPool.Add(eUserMsgKey.BusinessFlowAlreadyMappedToTC, new UserMsg(eUserMsgType.WARN, "Export " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to QC/ALM", "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " '{0}' is already mapped to the QC/ALM '{1}' Test Set, do you want to update it?" + Environment.NewLine + Environment.NewLine + "Select 'Yes' to update or 'No' to create new Test Set.", eUserMsgOption.YesNoCancel, eUserMsgSelection.Cancel));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExportQCNewTestSetSelectDiffFolder, new UserMsg(eUserMsgType.INFO, "Export QC Item - Creating new Test Set", "Please select QC folder to export to that the Test Set does not exist there.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExportItemToALMFailed, new UserMsg(eUserMsgType.ERROR, "Export to ALM Failed", "The {0} '{1}' failed to be exported to ALM." + Environment.NewLine + Environment.NewLine + "Error Details: {2}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfToSaveBFAfterExport, new UserMsg(eUserMsgType.QUESTION, "Save Links to QC/ALM Items", "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " '{0}' must be saved for keeping the links to QC/ALM items." + Environment.NewLine + "To perform the save now?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfToLoadExternalFields, new UserMsg(eUserMsgType.QUESTION, "Load/Refresh ALM External Fields", "The activity will run in the background for several hours." + Environment.NewLine + "Please do not close Ginger until operation is complete." + Environment.NewLine + "Would you like to continue?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            #endregion QC

            #region UnitTester Messages
            Reporter.UserMsgsPool.Add(eUserMsgKey.TestCompleted, new UserMsg(eUserMsgType.INFO, "Test Completed", "Test completed successfully.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CantFindObject, new UserMsg(eUserMsgType.INFO, "Missing Object", "Cannot find the {0}, the ID is: '{1}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion UnitTester Messages

            #region CommandLineParams
            Reporter.UserMsgsPool.Add(eUserMsgKey.UnknownParamInCommandLine, new UserMsg(eUserMsgType.ERROR, "Unknown Parameter", "Parameter not recognized {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion CommandLineParams Messages

            #region Tree / Grid / List
            Reporter.UserMsgsPool.Add(eUserMsgKey.ConfirmToAddTreeItem, new UserMsg(eUserMsgType.QUESTION, "Add New Item to Tree", "Are you Sure you want to add new item to tree?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToAddTreeItem, new UserMsg(eUserMsgType.ERROR, "Add Tree Item", "Failed to add the tree item '{0}'." + Environment.NewLine + "Error Details: '{1}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SureWantToDeleteAll, new UserMsg(eUserMsgType.QUESTION, "Delete All", "Are you sure you want to delete all?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SureWantToDeleteSelectedItems, new UserMsg(eUserMsgType.QUESTION, "Delete Selected", "Are you sure you want to delete selected {0} like '{1}'?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SureWantToContinue, new UserMsg(eUserMsgType.QUESTION, "Replace Existing", "Are you sure you want to overwrite the existing '{1}' ?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.BaseAPIWarning, new UserMsg(eUserMsgType.WARN, "Merged API Not found", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MultipleMatchingAPI, new UserMsg(eUserMsgType.WARN, "Overwrite Matching API Error", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SureWantToDelete, new UserMsg(eUserMsgType.QUESTION, "Delete", "Are you sure you want to delete '{0}'?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoItemToDelete, new UserMsg(eUserMsgType.WARN, "Delete All", "Didn't found item to delete", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SelectItemToDelete, new UserMsg(eUserMsgType.WARN, "Delete", "Please select items to delete", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SelectItemToAdd, new UserMsg(eUserMsgType.WARN, "Add New Item", "Please select an Activity on which you want to add a new Action", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToloadTheGrid, new UserMsg(eUserMsgType.ERROR, "Load Grid", "Failed to load the grid." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DragDropOperationFailed, new UserMsg(eUserMsgType.ERROR, "Drag and Drop", "Drag & Drop operation failed." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SelectValidColumn, new UserMsg(eUserMsgType.WARN, "Column Not Found", "Please select a valid column", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SelectValidRow, new UserMsg(eUserMsgType.WARN, "Row Not Found", "Please select a valid row", eUserMsgOption.OK, eUserMsgSelection.None));
            #endregion Tree / Grid

            #region ActivitiesGroup
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoActivitiesGroupWasSelected, new UserMsg(eUserMsgType.WARN, "Missing " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " Selection", "No " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " was selected.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ActivitiesGroupActivitiesNotFound, new UserMsg(eUserMsgType.WARN, "Import " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " " + GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Activities) + " to import were not found in the repository.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.PartOfActivitiesGroupActsNotFound, new UserMsg(eUserMsgType.WARN, "Import " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " " + GingerDicser.GetTermResValue(eTermResKey.Activities), "The following " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to import were not found in the repository:" + System.Environment.NewLine + "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SureWantToDeleteGroup, new UserMsg(eUserMsgType.QUESTION, "Delete Group", "Are you sure you want to delete the '{0}' group?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            #endregion ActivitiesGroup

            #region Mobile
            Reporter.UserMsgsPool.Add(eUserMsgKey.MobileConnectionFailed, new UserMsg(eUserMsgType.ERROR, "Mobile Connection Failed", "Failed to connect to the mobile device." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MobileRefreshScreenShotFailed, new UserMsg(eUserMsgType.WARN, "Mobile Screen Image", "Failed to refresh the mobile device screen image." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MobileShowElementDetailsFailed, new UserMsg(eUserMsgType.WARN, "Mobile Elements Inspector", "Failed to locate the details of the selected element." + Environment.NewLine + "Error Details: '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MobileActionWasAdded, new UserMsg(eUserMsgType.INFO, "Add Action", "Action was added.", eUserMsgOption.OK, eUserMsgSelection.None));
            //Reporter.UserMessagesPool.Add(eUserMsgKey.MobileActionWasAdded, new UserMessage(eMessageType.INFO, "Add Action", "Action was added." + System.Environment.NewLine + System.Environment.NewLine + "Do you want to run the Action?", MessageBoxButton.YesNo, MessageBoxResult.No));
            #endregion Mobile

            #region Reports
            Reporter.UserMsgsPool.Add(eUserMsgKey.ReportTemplateNotFound, new UserMsg(eUserMsgType.ERROR, "Report Template", "Report Template '{0}' not found", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.AutomationTabExecResultsNotExists, new UserMsg(eUserMsgType.WARN, "Execution Results are not existing", "Results from last execution are not existing (yet). Nothing to report, please wait for execution finish and click on report creation.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.FolderNamesAreTooLong, new UserMsg(eUserMsgType.WARN, "Folders Names Are Too Long", "Provided folders names are too long. Please change them to be less than 100 characters", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.FolderSizeTooSmall, new UserMsg(eUserMsgType.WARN, "Folders Size is Too Small", "Provided folder size is Too Small. Please change it to be bigger than 50 Mb", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.DefaultTemplateCantBeDeleted, new UserMsg(eUserMsgType.WARN, "Default Template Can't Be Deleted", "Default Template Can't Be Deleted. Please change it to be a non-default", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ExecutionsResultsProdIsNotOn, new UserMsg(eUserMsgType.WARN, "Executions Results Producing Should Be Switched On", "In order to perform this action, executions results producing should be switched On", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ExecutionsResultsNotExists, new UserMsg(eUserMsgType.WARN, "Executions Results Are Not Existing Yet", "In order to get HTML report, please, perform executions before", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ExecutionsResultsToDelete, new UserMsg(eUserMsgType.QUESTION, "Delete Executions Results", "Are you sure you want to delete selected Executions Results?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.AllExecutionsResultsToDelete, new UserMsg(eUserMsgType.QUESTION, "Delete All Executions Results", "Are you sure you want to delete all Executions Results?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.HTMLReportAttachment, new UserMsg(eUserMsgType.WARN, "HTML Report Attachment", "HTML Report Attachment already exists, please delete existing one.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ImageSize, new UserMsg(eUserMsgType.WARN, "Image Size", "Image Size should be less than 30 Kb", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.BFNotExistInDB, new UserMsg(eUserMsgType.INFO, "Run " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), "Business Flow data don't exist in LiteDB, Please run to generate report", eUserMsgOption.OK, eUserMsgSelection.None));

            #endregion Reports


            Reporter.UserMsgsPool.Add(eUserMsgKey.RemoteExecutionResultsCannotBeAccessed, new UserMsg(eUserMsgType.INFO, "Remote Data deletion", "Remote Execution Results will not be deleted.", eUserMsgOption.OK, eUserMsgSelection.OK));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ApplicationNotFoundInEnvConfig, new UserMsg(eUserMsgType.ERROR, "Application Not Found In EnvConfig", "Application = {0}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ExecutionReportSent, new UserMsg(eUserMsgType.INFO, "Publish", "Execution report sent by email", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ErrorReadingRepositoryItem, new UserMsg(eUserMsgType.ERROR, "Error Reading Repository Item", "Repository Item {0}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.EnvNotFound, new UserMsg(eUserMsgType.ERROR, "Env not found", "Env not found {0}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.CannotAddGinger, new UserMsg(eUserMsgType.ERROR, "Cannot Add Ginger", "Number of Gingers is limited to 12 Gingers.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ShortcutCreated, new UserMsg(eUserMsgType.INFO, "New Shortcut created", "Shortcut created on selected path - {0}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ShortcutCreationFailed, new UserMsg(eUserMsgType.ERROR, "Shortcut creation Failed", "Cannot create shortcut.Please avoid special characters in the Name/Description. Details: -{0}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.CannotRunShortcut, new UserMsg(eUserMsgType.ERROR, "Shortcut Execution Failed", "Cannot execute shortcut. Details: -{0}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.SolutionFileNotFound, new UserMsg(eUserMsgType.ERROR, "Solution File Not Found", "Cannot find Solution File at - {0}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.PlugInFileNotFound, new UserMsg(eUserMsgType.ERROR, "Plugin Configuration File Not Found", "Cannot find Plugin Configuration File at - {0}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.PluginDownloadInProgress, new UserMsg(eUserMsgType.WARN, "Plugins Download is in Progress", "Missing Plugins download is in progress, please wait for it to finish and then try to load page again.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ActionIDNotFound, new UserMsg(eUserMsgType.ERROR, "Action ID Not Found", "Cannot find action with ID - {0}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ActivityIDNotFound, new UserMsg(eUserMsgType.ERROR, GingerDicser.GetTermResValue(eTermResKey.Activity) + " ID Not Found", "Cannot find " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " with ID - {0}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToSendEmail, new UserMsg(eUserMsgType.ERROR, "Failed to Send E-mail", "Failed to send the {0} over e-mail using {1}." + Environment.NewLine + Environment.NewLine + "Error Details: {2}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToExportBF, new UserMsg(eUserMsgType.ERROR, "Failed to export BusinessFlow to CSV", "Failed to export BusinessFlow to CSV file. Got error {0}" + Environment.NewLine, eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.FoundDuplicateAgentsInRunSet, new UserMsg(eUserMsgType.ERROR, "Found Duplicate Agents in " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Agent name '{0}' is defined more than once in the " + GingerDicser.GetTermResValue(eTermResKey.RunSet), eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.FileNotExist, new UserMsg(eUserMsgType.WARN, "XML File Not Exist", "XML File has not been found on the specified path, either deleted or been re-named on the define XML path", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.FilterNotBeenSet, new UserMsg(eUserMsgType.QUESTION, "Filter Not Been Set", "No filtering criteria has been set, Retrieving all elements from page can take long time to complete, Do you want to continue?", eUserMsgOption.OKCancel, eUserMsgSelection.Cancel));

            Reporter.UserMsgsPool.Add(eUserMsgKey.CloseFilterPage, new UserMsg(eUserMsgType.QUESTION, "Confirmation", "'{0}' Elements Found, Close Filtering Page?", eUserMsgOption.YesNo, eUserMsgSelection.Yes));

            Reporter.UserMsgsPool.Add(eUserMsgKey.RetreivingAllElements, new UserMsg(eUserMsgType.QUESTION, "Retrieving all elements", "Retrieving all elements from page can take long time to complete, Do you want to continue?", eUserMsgOption.OKCancel, eUserMsgSelection.Cancel));

            Reporter.UserMsgsPool.Add(eUserMsgKey.WhetherToOpenSolution, new UserMsg(eUserMsgType.QUESTION, "Open Downloaded Solution?", "Do you want to open the downloaded Solution?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.ClickElementAgain, new UserMsg(eUserMsgType.INFO, "Please Click", "Please click on the desired element again", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.CurrentActionNotSaved, new UserMsg(eUserMsgType.INFO, "Current Action Not Saved", "Before using the 'next/previous action' button, Please save the current action", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.LoseChangesWarn, new UserMsg(eUserMsgType.WARN, "Save Changes", "The operation may result with lost of un-saved local changes." + Environment.NewLine + "Please make sure all changes were saved before continue." + Environment.NewLine + Environment.NewLine + "To perform the operation?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.CompilationErrorOccured, new UserMsg(eUserMsgType.ERROR, "Compilation Error Occurred", "Compilation error occurred." + Environment.NewLine + "Error Details: " + Environment.NewLine + " '{0}'.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.CopiedVariableSuccessfully, new UserMsg(eUserMsgType.INFO, "Info Message", "'{0}'" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " Affected." + Environment.NewLine + Environment.NewLine + "Notice: Un-saved changes won't be saved.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RenameItemError, new UserMsg(eUserMsgType.ERROR, "Rename", "Failed to rename the Item. Error: '{0}'?", eUserMsgOption.OK, eUserMsgSelection.OK));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfShareVaribalesInRunner, new UserMsg(eUserMsgType.QUESTION, "Share" + GingerDicser.GetTermResValue(eTermResKey.Variables), "Are you sure you want to share selected " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Values to all the similar " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " and " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " across all Runners?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CompilationSucceed, new UserMsg(eUserMsgType.INFO, "Compilation Succeed", "Compilation Passed successfully.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FileExtensionNotSupported, new UserMsg(eUserMsgType.ERROR, "File Extension Not Supported", "The selected file extension is not supported by this editor." + Environment.NewLine + "Supported extensions: '{0}'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NotifyFileSelectedFromTheSolution, new UserMsg(eUserMsgType.ERROR, "File Already Exists", "File - '{0}'." + Environment.NewLine + "Selected From The Solution folder hence its already exist and cannot be copy to the same place" + Environment.NewLine + "Please select another File to continue.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FileImportedSuccessfully, new UserMsg(eUserMsgType.INFO, "File imported successfully", "The File was imported successfully", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfToUndoChanges, new UserMsg(eUserMsgType.QUESTION, "Undo Changes?", "Do you want to undo changes (in case changes were done) and close?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfToUndoItemChanges, new UserMsg(eUserMsgType.QUESTION, "Undo Changes?", "Do you want to undo changes for '{0}'?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfToImportFile, new UserMsg(eUserMsgType.QUESTION, "Import File?", "Do you want to import the file into the following folder: '{0}'?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.InvalidIndexValue, new UserMsg(eUserMsgType.ERROR, "Invalid index value", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FileOperationError, new UserMsg(eUserMsgType.ERROR, "Error occurred during file operation", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FolderOperationError, new UserMsg(eUserMsgType.ERROR, "Error occurred during folder operation", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ObjectUnavailable, new UserMsg(eUserMsgType.ERROR, "Object Unavailable", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.PatternNotHandled, new UserMsg(eUserMsgType.ERROR, "Pattern not handled", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.LostConnection, new UserMsg(eUserMsgType.ERROR, "Lost Connection", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskToSelectBusinessflow, new UserMsg(eUserMsgType.INFO, "Select Businessflow", "Please select " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingFileLocation, new UserMsg(eUserMsgType.INFO, "Missing file location", "Please select a location for the file.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ScriptPaused, new UserMsg(eUserMsgType.INFO, "Script Paused", "Script is paused!", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ElementNotFound, new UserMsg(eUserMsgType.ERROR, "Element Not Found", "Element not found", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.TextNotFound, new UserMsg(eUserMsgType.ERROR, "Text Not Found", "Text not found", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ProvideSearchString, new UserMsg(eUserMsgType.ERROR, "Provide Search String", "Please provide search string", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoTextOccurrence, new UserMsg(eUserMsgType.ERROR, "No Text Occurrence", "No more text occurrence", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToInitiate, new UserMsg(eUserMsgType.ERROR, "Failed to initiate", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToCreateRequestResponse, new UserMsg(eUserMsgType.ERROR, "Failed to create Request / Response", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ActionNotImplemented, new UserMsg(eUserMsgType.ERROR, "Action is not implemented yet for control type ", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RunSetNotExecuted, new UserMsg(eUserMsgType.ERROR, "Runset Not Executed", "Please execute the Runset before running Send Email Report Operation", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.OperationNotSupported, new UserMsg(eUserMsgType.INFO, "Operation not supported ", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ValueIssue, new UserMsg(eUserMsgType.ERROR, "Value Issue", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.JSExecutionFailed, new UserMsg(eUserMsgType.ERROR, "Error Executing JS", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingTargetApplication, new UserMsg(eUserMsgType.ERROR, "Missing target application", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ThreadError, new UserMsg(eUserMsgType.ERROR, "Thread Exception", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ParsingError, new UserMsg(eUserMsgType.ERROR, "Error while parsing", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SpecifyUniqueValue, new UserMsg(eUserMsgType.WARN, "Please specify unique value", "Optional value already exists", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ParameterAlreadyExists, new UserMsg(eUserMsgType.WARN, "Cannot upload the selected parameter", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DeleteNodesFromRequest, new UserMsg(eUserMsgType.WARN, "Delete nodes from request body?", "Do you want to delete also nodes from request body that contain those parameters?", eUserMsgOption.YesNo, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ParameterMerge, new UserMsg(eUserMsgType.WARN, "Models Parameters Merge", "Do you want to update the merged instances on all model configurations?", eUserMsgOption.YesNo, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ParameterEdit, new UserMsg(eUserMsgType.WARN, "Global Parameter Edit", "Global Parameter Place Holder cannot be edit.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ParameterUpdate, new UserMsg(eUserMsgType.WARN, "Update Global Parameter Value Expression Instances", "{0}", eUserMsgOption.YesNo, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ParameterDelete, new UserMsg(eUserMsgType.WARN, "Delete Parameter", "{0}", eUserMsgOption.YesNo, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SaveAll, new UserMsg(eUserMsgType.WARN, "Save All", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SaveSelected, new UserMsg(eUserMsgType.WARN, "Save Selected", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SaveAllModifiedItems, new UserMsg(eUserMsgType.WARN, "Save All", "{0}", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CopiedErrorInfo, new UserMsg(eUserMsgType.INFO, "Copied Error information", "Error Information copied to Clipboard", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RepositoryNameCantEmpty, new UserMsg(eUserMsgType.WARN, "QTP to Ginger Converter", "Object Repository name cannot be empty", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExcelProcessingError, new UserMsg(eUserMsgType.ERROR, "Excel processing error", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.EnterValidBusinessflow, new UserMsg(eUserMsgType.WARN, "Enter valid businessflow", "Please enter a Valid " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Name", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DeleteItem, new UserMsg(eUserMsgType.WARN, "Delete Item", "Are you sure you want to delete '{0}' item ?", eUserMsgOption.YesNo, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RefreshFolder, new UserMsg(eUserMsgType.WARN, "Refresh Folder", "Un saved items changes under the refreshed folder will be lost, to continue with refresh?", eUserMsgOption.YesNo, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RefreshFailed, new UserMsg(eUserMsgType.ERROR, "Refresh Failed", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ReplaceAll, new UserMsg(eUserMsgType.QUESTION, "Replace All", "{0}", eUserMsgOption.OKCancel, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ItemSelection, new UserMsg(eUserMsgType.WARN, "Item Selection", "{0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DifferentItemType, new UserMsg(eUserMsgType.WARN, "Not Same Item Type", "The source item type do not match to the destination folder type", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CopyCutOperation, new UserMsg(eUserMsgType.INFO, "Copy/Cut Operation", "Please select Copy/Cut operation first.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ObjectLoad, new UserMsg(eUserMsgType.ERROR, "Object Load", "Not able to load the object details", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.InitializeBrowser, new UserMsg(eUserMsgType.INFO, "Initialize Browser", "Initialize Browser action automatically added." + Environment.NewLine + "Please continue to spy the required element.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.GetModelItemUsagesFailed, new UserMsg(eUserMsgType.ERROR, "Model Item Usages", "Failed to get the '{0}' item usages." + Environment.NewLine + Environment.NewLine + "Error Details: {1}.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SolutionSaveWarning, new UserMsg(eUserMsgType.WARN, "Save", "Note: save will include saving also changes which were done to: {0}." + System.Environment.NewLine + System.Environment.NewLine + "To continue with Save operation?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SaveItemParentWarning, new UserMsg(eUserMsgType.WARN, "Item Parent Save", "Save process will actually save the item {0} parent which called '{1}'." + System.Environment.NewLine + System.Environment.NewLine + "To continue with save?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SaveAllItemsParentWarning, new UserMsg(eUserMsgType.WARN, "Items Parent Save", "Save process will actually save item\\s parent\\s (" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ")" + System.Environment.NewLine + System.Environment.NewLine + "To continue with save?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlConflictResolveFailed, new UserMsg(eUserMsgType.ERROR, "Resolve Conflict Failed", "Ginger failed to resolve the conflicted file" + Environment.NewLine + "File Path: {0}" + Environment.NewLine + Environment.NewLine + "It seems like the SVN conflict content (e.g. '<<<<<<< .mine') has been updated on the remote repository", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SourceControlItemAlreadyLocked, new UserMsg(eUserMsgType.INFO, "Source Control File Locked", "The file is already locked" + Environment.NewLine + "Please do Get info for more details", eUserMsgOption.OK, eUserMsgSelection.OK));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SoruceControlItemAlreadyUnlocked, new UserMsg(eUserMsgType.INFO, "Source Control File not Locked", "The file is not locked" + Environment.NewLine + "Please do Get info for more details", eUserMsgOption.OK, eUserMsgSelection.OK));



            Reporter.UserMsgsPool.Add(eUserMsgKey.RefreshWholeSolution, new UserMsg(eUserMsgType.QUESTION, "Refresh Solution", "Do you want to Refresh the whole Solution to get the Latest changes?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.OracleDllIsMissing, new UserMsg(eUserMsgType.ERROR, "DB Connection Status", "Connect to the DB failed." + Environment.NewLine + "The file Oracle.ManagedDataAccess.dll is missing," + Environment.NewLine + "Please download the file, place it under the below folder, restart Ginger and retry:" + Environment.NewLine + "{0}" + Environment.NewLine + "Do you want to download the file now?", eUserMsgOption.YesNo, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingExcelDetails, new UserMsg(eUserMsgType.ERROR, "Missing Export Path Details", "The Export Excel File Path is missing", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.InvalidExcelDetails, new UserMsg(eUserMsgType.ERROR, "InValid Export Sheet Details", "The Export Excel can be *.xlsx only.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExportFailed, new UserMsg(eUserMsgType.ERROR, "Export Failed", "Error Occurred while exporting the {0}: {1}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExportDetails, new UserMsg(eUserMsgType.INFO, "Export Details", "Export execution ended successfully", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ParamExportMessage, new UserMsg(eUserMsgType.QUESTION, "Param Export to Data Source", "Special Characters will be removed from Parameter Names while exporting to Data Source. Do you wish to Continue?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CreateTableError, new UserMsg(eUserMsgType.ERROR, "Create Table Error", "Failed to Create the Table. Error: {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MappedtoDataSourceError, new UserMsg(eUserMsgType.ERROR, "Output Param Mapping Error ", "Failed to map the Output Params to Data Source", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.InvalidDataSourceDetails, new UserMsg(eUserMsgType.ERROR, "Invalid Data Source Details", "The Data Source Details provided are Invalid.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DeleteDSFileError, new UserMsg(eUserMsgType.WARN, "Delete DataSource File", "The Data Source File with the File Path '{0}' Could not be deleted. Please Delete file Manually", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.InvalidDSPath, new UserMsg(eUserMsgType.ERROR, "Invalid DataSource Path", "The Data Source with the File Path {0} is not valid. Please use correct File Path", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.InvalidColumnName, new UserMsg(eUserMsgType.ERROR, "Invalid Column Details", "The Column Name is invalid.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CreateRunset, new UserMsg(eUserMsgType.WARN, "Create Runset", "No runset found, Do you want to create new runset", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DeleteRunners, new UserMsg(eUserMsgType.WARN, "Delete Runners", "Are you sure you want to delete all runners", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DeleteRunner, new UserMsg(eUserMsgType.WARN, "Delete Runner", "Are you sure you want to delete selected runner", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DeleteBusinessflows, new UserMsg(eUserMsgType.WARN, "Delete " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), "Are you sure you want to delete all" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DeleteBusinessflow, new UserMsg(eUserMsgType.WARN, "Delete " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), "Are you sure you want to delete selected " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RunsetBuinessFlowWasChanged, new UserMsg(eUserMsgType.WARN, GingerDicser.GetTermResValue(eTermResKey.RunSet) + " " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Changed", "One or more of the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " were changed/deleted." + Environment.NewLine + "You must reload the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " for changes to be viewed/executed." + Environment.NewLine + Environment.NewLine + "Do you want to reload the " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + "?" + Environment.NewLine + Environment.NewLine + "Note: Reload will cause un-saved changes to be lost.", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RunSetReloadhWarn, new UserMsg(eUserMsgType.WARN, "Reload " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Reload process will cause all un-saved changes to be lost." + Environment.NewLine + Environment.NewLine + " To continue with reload?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.CantDeleteRunner, new UserMsg(eUserMsgType.WARN, "Delete Runner", "You can't delete last Runner, you must have at least one.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DuplicateRunsetName, new UserMsg(eUserMsgType.WARN, "Duplicate Runset Name", "'{0}' already exists, please use different name", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WarnOnDynamicActivities, new UserMsg(eUserMsgType.QUESTION, "Dynamic " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " Warning", "The dynamically added Shared Repository " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " will not be saved (but they will continue to appear on the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString: ")") + System.Environment.NewLine + System.Environment.NewLine + "To continue with Save?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.WarnOnLinkSharedActivities, new UserMsg(eUserMsgType.QUESTION, "Link Shared " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " Warning", "The Link Shared " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " are readonly and any changes done will not be saved." + System.Environment.NewLine + System.Environment.NewLine + "Do you want to save other changes?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WarnOnEditLinkSharedActivities, new UserMsg(eUserMsgType.QUESTION, "Link Shared " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " Warning", "Any updates to linked shared repository auto update the shared " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " and all it's usage in other flows." + System.Environment.NewLine + System.Environment.NewLine + "Do you want to proceed with Edit?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.EditLinkSharedActivities, new UserMsg(eUserMsgType.INFO, "Link Shared " + GingerDicser.GetTermResValue(eTermResKey.Activities), "Linked shared repository " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " are read only by default." + System.Environment.NewLine + System.Environment.NewLine + "If you want to update, open it in edit mode.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.QcConnectFailureRestAPI, new UserMsg(eUserMsgType.WARN, "QC/ALM Connection Failed", "QC/ALM connection failed." + System.Environment.NewLine + "Please make sure that the server url and the credentials you use are correct.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ExportedExecDetailsToALMIsInProcess, new UserMsg(eUserMsgType.INFO, "Export Execution Details", "Please Wait, Exporting Execution Details is in process.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskBeforeDefectProfileDeleting, new UserMsg(eUserMsgType.QUESTION, "Profiles Deleting", "After deletion there will be no way to restore deleted profiles.\nAre you sure that you want to delete the selected profiles?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissedMandatotryFields, new UserMsg(eUserMsgType.INFO, "Profiles Saving", "Please, populate value for mandatory field '{0}' of '{1}' Defect Profile", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoDefaultDefectProfileSelected, new UserMsg(eUserMsgType.INFO, "Profiles Saving", "Please, select one of the Defect Profiles to be a 'Default'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.IssuesInSelectedDefectProfile, new UserMsg(eUserMsgType.INFO, "ALM Defects Opening", "Please, revise the selected Defect Profile, current fields/values are not corresponded with ALM", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoDefectProfileCreated, new UserMsg(eUserMsgType.INFO, "Defect Profiles", "Please, create at least one Defect Profile", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WrongValueSelectedFromTheList, new UserMsg(eUserMsgType.INFO, "Profiles Saving", "Please, select one of the existed values from the list\n(Field '{0}', Defect Profile '{1}')", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WrongNonNumberValueInserted, new UserMsg(eUserMsgType.INFO, "Profiles Saving", "Please, insert numeric value\n(Field '{0}', Defect Profile '{1}')", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WrongDateValueInserted, new UserMsg(eUserMsgType.INFO, "Profiles Saving", "Please, insert Date in format 'yyyy-mm-dd'\n(Field '{0}', Defect Profile '{1}')", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ALMDefectsWereOpened, new UserMsg(eUserMsgType.INFO, "ALM Defects Opening", "{0} ALM Defects were opened", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskALMDefectsOpening, new UserMsg(eUserMsgType.QUESTION, "ALM Defects Opening", "Are you sure that you want to open {0} ALM Defects?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ALMDefectsUserInOtaAPI, new UserMsg(eUserMsgType.INFO, "ALM Defects Valid for Rest API only", "You are in ALM Ota API mode, Please change to Rest API", eUserMsgOption.OK, eUserMsgSelection.None));


            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfToDownloadPossibleValuesShortProcesss, new UserMsg(eUserMsgType.QUESTION, "ALM External Items Fields", "Would you like to download and save possible values for Categories Items? ", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfToDownloadPossibleValues, new UserMsg(eUserMsgType.QUESTION, "ALM External Items Fields", "Would you like to download and save possible values for Categories Items? " + Environment.NewLine + "This process could take up to several hours.", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SelectAndSaveCategoriesValues, new UserMsg(eUserMsgType.QUESTION, "ALM External Items Fields", "Please select values for each Category Item and click on Save", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.POMSearchByGUIDFailed, new UserMsg(eUserMsgType.WARN, "POM not found", "Previously saved POM not found. Please choose another one.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.POMElementSearchByGUIDFailed, new UserMsg(eUserMsgType.WARN, "POM Element not found", "Previously saved POM Element not found. Please choose another one.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoRelevantAgentInRunningStatus, new UserMsg(eUserMsgType.WARN, "No Relevant Agent In Running Status", "Relevant Agent In should be up and running in order to see the highlighted element.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.POMWizardFailedToLearnElement, new UserMsg(eUserMsgType.WARN, "Learn Elements Failed", "Error occurred while learning the elements." + Environment.NewLine + "Error Details:" + Environment.NewLine + "'{0}'", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.POMWizardReLearnWillDeleteAllElements, new UserMsg(eUserMsgType.WARN, "Re-Learn Elements", "Re-Learn Elements will delete all existing elements" + Environment.NewLine + "Do you want to continue?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.POMDeltaWizardReLearnWillEraseModification, new UserMsg(eUserMsgType.WARN, "Re-Learn Elements", "Re-Learn Elements will delete all existing modifications" + Environment.NewLine + "Do you want to continue?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.POMDriverIsBusy, new UserMsg(eUserMsgType.WARN, "Driver Is Busy", "Operation cannot be complete because the Driver is busy with learning operation" + Environment.NewLine + "Do you want to continue?", eUserMsgOption.OK, eUserMsgSelection.OK));
            Reporter.UserMsgsPool.Add(eUserMsgKey.POMAgentIsNotRunning, new UserMsg(eUserMsgType.WARN, "Agent is Down", "In order to perform this operation the Agent needs to be up and running." + Environment.NewLine + "Please start the agent and re-try", eUserMsgOption.OK, eUserMsgSelection.OK));
            Reporter.UserMsgsPool.Add(eUserMsgKey.POMNotOnThePageWarn, new UserMsg(eUserMsgType.WARN, "Not On the Same Page", "'{0}' Elements out of '{1}' Elements failed to be found on the page" + Environment.NewLine + "Looks like you are not on the right page" + Environment.NewLine + "Do you want to continue?", eUserMsgOption.YesNo, eUserMsgSelection.Yes));
            Reporter.UserMsgsPool.Add(eUserMsgKey.POMCannotDeleteAutoLearnedElement, new UserMsg(eUserMsgType.WARN, "Cannot Delete Auto Learned Element", "The Element you are trying to delete has been learned automatically from page and cannot be deleted", eUserMsgOption.OK, eUserMsgSelection.OK));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WizardCantMoveWhileInProcess, new UserMsg(eUserMsgType.WARN, "Process is Still Running", "Move '{0}' until the process will be finished or stopped." + Environment.NewLine + "Please wait for the process to be finished or stop it and then retry.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AskIfToCloseAgent, new UserMsg(eUserMsgType.QUESTION, "Close Agent?", "Close Agent '{0}'?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FolderNameTextBoxIsEmpty, new UserMsg(eUserMsgType.WARN, "Folders Names is Empty", "Please provide a proper folder name.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UserHaveNoWritePermission, new UserMsg(eUserMsgType.WARN, "User Have No Write Permission On Folder", "User that currently in use have no write permission on selected folder. Pay attention that attachment at shared folder may be not created.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FolderNotExistOrNotAvailible, new UserMsg(eUserMsgType.WARN, "Folder Not Exist Or Not Available", "Folder Not Exist Or Not Available. Please select another one.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.WizardSureWantToCancel, new UserMsg(eUserMsgType.QUESTION, "Cancel Wizard?", "Are you sure you want to cancel wizard and close?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ReportsTemplatesSaveWarn, new UserMsg(eUserMsgType.WARN, "Default Template Report Change", "Default change will cause all templates to be updated and saved, to continue?", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FailedToLoadPlugIn, new UserMsg(eUserMsgType.ERROR, "Failed to Load Plug In", "Ginger could not load the plug in '{0}'" + Environment.NewLine + "Error Details: {1}", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.POMElementNotExist, new UserMsg(eUserMsgType.QUESTION, "Element Not Found", "'{0}' does not exist in selected Page Object Model - '{1}'" + Environment.NewLine + "Do you want to add this Element to POM ?", eUserMsgOption.YesNo, eUserMsgSelection.Yes));
            Reporter.UserMsgsPool.Add(eUserMsgKey.UpdateExistingPOMElement, new UserMsg(eUserMsgType.QUESTION, "Updated Element Found", "An updated version of Page Object Model Element '{0}' Found." + Environment.NewLine + "Do you want to update existing POM ?", eUserMsgOption.YesNo, eUserMsgSelection.Yes));
            Reporter.UserMsgsPool.Add(eUserMsgKey.SavePOMChanges, new UserMsg(eUserMsgType.QUESTION, "Save POM Changes", "Selected POM '{0}' was updated." + Environment.NewLine + "Do you want to save changes ?", eUserMsgOption.YesNo, eUserMsgSelection.Yes));

            Reporter.UserMsgsPool.Add(eUserMsgKey.POMMoveElementFromUnmappedToMapped, new UserMsg(eUserMsgType.QUESTION, "POM Element Found in Unmapped Elements", "Selected Element '{0}' Found in Unmapped Elements list of Page Object Model '{1}'." + Environment.NewLine + "Do you want to update POM & move element into Mapped Elements list ?", eUserMsgOption.YesNo, eUserMsgSelection.Yes));

            Reporter.UserMsgsPool.Add(eUserMsgKey.Failedtosaveitems, new UserMsg(eUserMsgType.ERROR, "Failed to Save", "Failed to do Save All", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.AllItemsSaved, new UserMsg(eUserMsgType.INFO, "All Changes Saved", "All Changes Saved", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoConvertibleActionsFound, new UserMsg(eUserMsgType.INFO, "No Convertible Actions Found", "The selected " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " doesn't contain any convertible actions.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoConvertibleActionSelected, new UserMsg(eUserMsgType.WARN, "No Convertible Action Selected", "Please select the actions that you want to convert.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoActivitySelectedForConversion, new UserMsg(eUserMsgType.WARN, "No Activity Selected", "Please select an " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " that you want to convert.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ActivitiesConversionFailed, new UserMsg(eUserMsgType.WARN, "Activities Conversion Failed", "Activities Conversion Failed.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ShareVariableNotSelected, new UserMsg(eUserMsgType.INFO, "Info Message", "Please select the " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " to share.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.APIParametersListUpdated, new UserMsg(eUserMsgType.WARN, "API Model Parameters Difference", "Difference was identified between the list of parameters which configured on the API Model and the parameters exists on the Action.\n\nDo you want to update the Action parameters?", eUserMsgOption.YesNo, eUserMsgSelection.No));


            Reporter.UserMsgsPool.Add(eUserMsgKey.APIMappedToActionIsMissing, new UserMsg(eUserMsgType.WARN, "Missing Mapped API Model", "The API Model which mapped to this action is missing, please remap API Model to the action.", eUserMsgOption.OK, eUserMsgSelection.OK));
            Reporter.UserMsgsPool.Add(eUserMsgKey.NoAPIExistToMappedTo, new UserMsg(eUserMsgType.WARN, "No API Model Found", "API Models repository is empty, please add new API Models into it and map it to the action", eUserMsgOption.OK, eUserMsgSelection.OK));
            Reporter.UserMsgsPool.Add(eUserMsgKey.APIModelAlreadyContainsReturnValues, new UserMsg(eUserMsgType.WARN, "Return Values Already Exist Warning", "{0} Return Values already exist for this API Model" + Environment.NewLine + "Do you want to override them by importing form new response template file?" + Environment.NewLine + Environment.NewLine + "Please Note! - by clicking yes all {0} return values will be deleted with no option to restore.", eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.VisualTestingFailedToDeleteOldBaselineImage, new UserMsg(eUserMsgType.WARN, "Creating New Baseline Image", "Error while trying to create and save new Baseline Image.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ApplitoolsLastExecutionResultsNotExists, new UserMsg(eUserMsgType.INFO, "Show Last Execution Results", "No last execution results exists, Please run first the action, Close Applitools Eyes and then view the results.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ApplitoolsMissingChromeOrFirefoxBrowser, new UserMsg(eUserMsgType.INFO, "View Last Execution Results", "Applitools support only Chrome or Firefox Browsers, Please install at least one of them in order to browse Applitools URL results.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.ParameterOptionalValues, new UserMsg(eUserMsgType.INFO, "Parameters Optional Values", "{0} parameters were updated.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.RecoverItemsMissingSelectionToRecover, new UserMsg(eUserMsgType.WARN, "Recover Changes", "Please select valid Recover items to {0}", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingErrorHandler, new UserMsg(eUserMsgType.WARN, "Mismatch in Mapped Error Handler Found", "A mismatch has been detected in error handlers mapped with your activity." + Environment.NewLine + "Please check if any mapped error handler has been deleted or marked as inactive. ", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.BusinessFlowUpdate, new UserMsg(eUserMsgType.INFO, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " '{0}' {1} Successfully", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FindAndRepalceFieldIsEmpty, new UserMsg(eUserMsgType.WARN, "Field is Empty", "Field '{0}' cannot be empty", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FindAndReplaceListIsEmpty, new UserMsg(eUserMsgType.WARN, "List is Empty", "No items were found hence nothing can be replaced", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FindAndReplaceNoItemsToRepalce, new UserMsg(eUserMsgType.WARN, "No Suitable Items", "No suitable items selected to replace.", eUserMsgOption.OK, eUserMsgSelection.None));
            Reporter.UserMsgsPool.Add(eUserMsgKey.FindAndReplaceViewRunSetNotSupported, new UserMsg(eUserMsgType.INFO, "View " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "View " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " is not supported.", eUserMsgOption.OK, eUserMsgSelection.None));

            Reporter.UserMsgsPool.Add(eUserMsgKey.FileAlreadyExistWarn, new UserMsg(eUserMsgType.WARN, "File Already Exists", "File already exists, do you want to override?", eUserMsgOption.OKCancel, eUserMsgSelection.Cancel));

            Reporter.UserMsgsPool.Add(eUserMsgKey.SuccessfulConversionDone, new UserMsg(eUserMsgType.INFO, "Obsolete actions converted successfully", "The obsolete actions have been converted successfully" + Environment.NewLine + "Do you want to convert more actions?", eUserMsgOption.YesNo, eUserMsgSelection.No));

            Reporter.UserMsgsPool.Add(eUserMsgKey.MissingTargetPlatformForConversion, new UserMsg(eUserMsgType.WARN,
               "Missing Target Platform for Conversion", "For {0}, you need to add a Target Application of type {1}. Please add it to your " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + Environment.NewLine +
               "Do you want to continue with the conversion?",
               eUserMsgOption.YesNo, eUserMsgSelection.No));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DataSourceSheetNameHasSpace, new UserMsg(eUserMsgType.ERROR, "Sheet Name Cannot Contain Space", " Sheet Name cannot contain space, please remove space and try again", eUserMsgOption.OK, eUserMsgSelection.OK));
            Reporter.UserMsgsPool.Add(eUserMsgKey.DataSourceColumnHasSpace, new UserMsg(eUserMsgType.ERROR, "Column Name Cannot Contain Space", " Column Name [ {0} ] cannot contain space, please remove space and try again", eUserMsgOption.OK, eUserMsgSelection.OK));
        }
    }
}
