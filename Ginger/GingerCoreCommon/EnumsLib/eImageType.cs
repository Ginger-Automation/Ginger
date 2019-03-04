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

namespace Amdocs.Ginger.Common.Enums
{

    public enum eImageType
    {
        #region General Images
        //############################## General Images:
        /// <summary>
        /// To be used in case no image is needed
        /// </summary>
        Null,
        /// <summary>
        /// to be used in case default empty image needs to be shown
        /// </summary>
        Empty,
        /// <summary>
        /// Ginger Icon image
        /// </summary>
        Ginger,
        GingerLogo,
        GingerLogoGray,
        GingerIconWhite,
        GingerIconGray,
        GingerSplash,
        #endregion


        #region Repository Items Images
        //############################## Repository Items Images:
        Solution,
        BusinessFlow,
        ActivitiesGroup,
        Activity,
        Action,
        Agent,
        RunSet,
        ApplicationModel,
        APIModel,
        ApplicationPOMModel,
        Runner,
        Operations,
        Environment,
        HtmlReport,
        SharedRepositoryItem,
        NonSharedRepositoryItem,
        Tag,
        DataSource,
        PluginPackage,
        #endregion

        #region Variable Items Images
        Variable,
        VariableList,
        Password,
        Random,
        Sequence,
        Timer,
        #endregion

        #region Execution Status Images
        Unknown,
        //############################## Execution Status Images:
        /// <summary>
        /// Show spinner 
        /// </summary>
        Processing,
        /// <summary>
        ///  Show green flag, use when action, activity, businessflows execution pass
        /// </summary>
        Passed,
        /// <summary>
        ///  Show red flag, use when action, activity, businessflows execution fail
        /// </summary>
        Failed,
        Pending,
        Ready,
        Blocked,
        Skipped,
        Stopped,
        Running,
        #endregion

        #region Operations Images
        //############################## Operations Images:        
        Refresh,
        Add,
        AddCircle,//needed?
        Stop,
        Close,
        Play,
        Continue,
        Save,
        Open,
        New,
        GoBack,
        GoNext,
        Finish,
        Cancel,
        Reset,
        Analyze,
        Delete,
        DeleteSingle,
        StopCircleOutline,// needed?
        PlayCircleOutline,// needed?
        Config,
        Edit,
        Reply,
        ShareSquareOutline,
        Reorder,
        Retweet,
        Automate,
        Minimize,
        MoveRight,
        MoveLeft,
        MoveUp,
        MoveDown,
        ParallelExecution,
        SequentialExecution,
        Duplicate,
        Merge,
        Sync,
        UnSync,
        Visible,
        Invisible,
        View,
        GetLatest,
        CheckIn,
        Download,
        Fix,
        Expand,
        Collapse,
        ExpandAll,
        CollapseAll,
        ActiveAll,
        Exchange,
        Export,
        Filter,
        ImportFile,
        Upgrade,
        Recover,
        Approve,
        Reject,
        Warn,
        EditWindow,
        UserDefined,
        #endregion

        #region Items Images
        //############################## Items Images:
        /// <summary>
        /// sample drawing image
        /// </summary>
        KidsDrawing,
        Folder,
        ItemModified,
        Email,        
        Parameter,
        ListGroup,
        FlowDiagram,
        EllipsisH,
        Sitemap,
        Clock,
        File,
        InstanceLink,
        Search,
        Remove,
        Report,
        Active,
        InActive,
        History,
        ChevronDown,
        Question,
        Help,
        Info,
        Screen,        
        Globe,
        Service,
        FileVideo,
        Ticket,        
        Pointer,
        Camera,
        Wrench,
        Power,
        ArrowDown,
        ExcelFile,
        User,
        UserProfile,
        Forum,
        Website,
        Beta,
        Error,
        Coffee,
        MapSigns,
        Elements,
        LocationPointer,
        GitHub,
        Ping,
        Database,
        #endregion

        #region Source control Images     
        SourceControl,
        SourceControlNew,
        SourceControlModified,
        SourceControlDeleted,
        SourceControlEquel,
        SourceControlLockedByAnotherUser,
        SourceControlLockedByMe,
        SourceControlError,
        #endregion

        Times_Red,
        Times,
        Share,
        ShareExternal,
        Application,
        OpenFolder,
        Check,
        Bug,
        PlusSquare,
        DataTable,

        #region Comparison Status Images
        Unchanged,
        Changed,
        Deleted,
        Added,
        Avoided,
        #endregion

        #region ElementType Images
        Button,
        TextBox,
        CheckBox,
        Link,
        RadioButton,
        Table,
        Image,
        Element,
        Toggle,
        Menu,
        Label,
        DropList,
        List,
        ListItem,
        Window,
        Text,
        #endregion
    }
}



