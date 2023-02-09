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
        GingerLogoWhiteSmall,
        GingerIconWhite,
        GingerIconGray,
        GingerSplash,
        VRT,
        Applitools,
        Sealights,
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
        SharedRepositoryItemDark,
        Tag,
        DataSource,
        PluginPackage,
        Building,
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
        Mapped,
        Partial,
        UnMapped,
        #endregion

        #region Operations Images
        //############################## Operations Images:        
        Refresh,
        Add,
        AddCircle,//needed?
        Stop,
        Close,
        Run,
        RunSingle,
        Continue,
        Save,
        SaveLightGrey,
        SaveGradient,
        SaveAll,
        SaveAllGradient,
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
        StopAll,// needed?
        RunAll,// needed?
        Config,
        Edit,
        Reply,
        ShareSquareOutline,
        Reorder,
        Convert,
        Retweet,
        Automate,
        Minimize,
        MoveRight,
        MoveLeft,
        MoveUp,
        MoveDown,
        MoveUpDown,
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
        Upload,
        CheckIn,
        Download,
        Fix,
        Expand,
        Collapse,
        ExpandAll,
        CollapseAll,
        ActiveAll,
        ExpandToFullScreen,
        Exchange,
        Export,
        Filter,
        ImportFile,
        Upgrade,
        Recover,
        Approve,
        Reject,
        Retry,
        Warn,
        HighWarn,
        MediumWarn,
        LowWarn,
        EditWindow,
        UserDefined,
        Spy,
        Undo,
        Simulate,
        Copy,
        Cut,
        Paste,
        WindowRestore,
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
        InstanceLinkOrange,
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
        Recording,
        Wrench,
        Settings,
        Eraser,
        Power,
        ArrowDown,
        ArrowRight,
        ExcelFile,
        User,
        UserProfile,
        Forum,
        Website,
        Beta,
        Error,
        Coffee,
        HotMag,
        MapSigns,
        Elements,
        LocationPointer,
        GitHub,
        Ping,
        Database,
        Output,
        Input,
        CodeFile,
        Rows,
        Column,
        Columns,
        Browser,
        KeyboardLayout,
        Linux,
        BatteryThreeQuarter,
        Mobile,
        Codepen,
        Code,
        Runing,
        Dos,
        Server,
        MousePointer,
        AudioFileOutline,
        ChartLine,
        Suitcase,
        Paragraph,
        Graph,
        BullsEye,
        WindowsIcon,
        SoapUI,
        Java,
        PDFFile,
        CSS3Text,
        Languages,
        MinusSquare,
        Mandatory,
        ALM,
        MapALM,
        CSV,
        Clipboard,
        ID,
        RegularExpression,
        DataManipulation,
        General,
        SignIn,
        SignOut,
        Pin,
        Square,
        Triangle,
        Circle,
        Ios,
        IosOutline,
        IosWhite,
        Android,
        AndroidOutline,
        AndroidWhite,
        AngleArrowUp,
        AngleArrowDown,
        AngleArrowLeft,
        AngleArrowRight,
        Support,
        Asterisk,
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
        ToggleOn,
        ToggleOff,
        Menu,
        Label,
        DropList,
        List,
        ListItem,
        Window,
        Text,
        CLI,
        LinkSquare,
        DatePicker,
        TreeView,
        SelfHealing
        #endregion
    }
}



