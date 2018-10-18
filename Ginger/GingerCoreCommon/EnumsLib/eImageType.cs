﻿using System;
using System.Collections.Generic;
using System.Text;

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
        GingerIconWhite,
        GingerIconGray,
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
        Variable,
        SharedRepositoryItem,
        NonSharedRepositoryItem,
        Tag,
        DataSource,
        PluginPackage,
        #endregion


        #region Execution Status Images
        //############################## Execution Status Images:
        /// <summary>
        /// Show spinner 
        /// </summary>
        Processing,
        /// <summary>
        ///  Show green flag, use when action, activity, businees flows execution pass
        /// </summary>
        Passed,
        /// <summary>
        ///  Show red flag, use when action, activity, businees flows execution fail
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
        List,
        Parameter,
        ListGroup,
        FlowDiagram,
        EllipsisH,
        Sitemap,
        Clock,
        File,
        Link,
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
        Text,
        Globe,
        Service,
        FileVideo,
        Ticket,
        Window,
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
        #endregion


        #region Source control Images     
        SourceControl,
        SourceControlNew,
        SourceControlModified,
        SourceControlDeleted,
        SourceControlEquel,
        SourceControlLockedByAnotherUser,
        SourceControlLockedByMe,
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
    }
}
