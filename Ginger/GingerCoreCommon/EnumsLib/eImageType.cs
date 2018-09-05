using System;
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
        #endregion


        #region Repository Items Images
        //############################## Repository Items Images:
        Solution,
        BusinessFlow,
        Activity,
        Action,
        Agent,
        RunSet,
        APIModel32,
        APIModel16,
        ApplicationPOM,
        Runner,
        Operations,
        Environment,
        Parameter,
        HtmlReport,
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
        Expand,
        Collapse,
        ExpandAll,
        CollapseAll,
        ActiveAll,
        Info,
        Exchange,
        Export,
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
        ListGroup,
        FlowDiagram,
        EllipsisH,
        Sitemap,
        Clock,
        File,
        Link,
        Search,
        Remove,
        Report,
        Active,
        InActive,
        History,
        Pointer,
        Camera,

        Power,
        ExcelExport,

        #endregion


        #region Source control Images        
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
        Download,
        Application,
        OpenFolder,
        Check,
        Bug,
        DataSource,
        PluginPackage,
        PlusSquare,
        Th,
        ThLarge
    }
}
