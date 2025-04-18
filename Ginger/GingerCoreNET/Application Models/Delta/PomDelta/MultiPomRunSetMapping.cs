﻿using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Amdocs.Ginger.Common.UIElement.ElementLocator;

namespace GingerCoreNET.Application_Models
{
    /// <summary>
    /// Represents a mapping between multiple Page Object Models (POMs) and a RunSet configuration.
    /// Used for tracking and managing updates across multiple POMs.
    /// </summary>
    public class MultiPomRunSetMapping : RepositoryItemBase
    {
        private bool mSelected = false;

        public bool Selected { get { return mSelected; } set { if (mSelected != value) { mSelected = value; OnPropertyChanged(nameof(Selected)); } } }
        
        
        /// <summary>
        /// The RunSet configuration associated with this mapping
        /// </summary>
        
        public RunSetConfig RunSetConfig { get; set; }

        /// <summary>
        /// Name of the associated RunSet
        /// </summary>
        public string RunsetName { get; set; }

        /// <summary>
        /// List of Application POM Models associated with this mapping
        /// </summary>
        public List<ApplicationPOMModel> ApplicationPOMModels { get; set; } = new List<ApplicationPOMModel>();


        /// <summary>
        /// Status of POM update operations
        /// </summary>
        public string PomUpdateStatus { get; set; } = string.Empty;

        public ApplicationPOMModel ApplicationAPIModel { get; set; }

        public string ApplicationAPIModelName { get; set; }

        public List<RunSetConfig> RunSetConfigList { get; set; } = new List<RunSetConfig>();

        public RunSetConfig? SelectedRunset { get; set; }


        public override string ItemName { get; set; }

        private Amdocs.Ginger.CoreNET.Execution.eRunStatus mRunSetStatus;
        
        /// <summary>
        /// Execution status of the RunSet
        /// </summary>
        public Amdocs.Ginger.CoreNET.Execution.eRunStatus RunSetStatus 
        { 
            get { return mRunSetStatus; } 
            set 
                { 
                    if (mRunSetStatus != value)
                    {
                        mRunSetStatus = value;
                        OnPropertyChanged(nameof(RunSetStatus));
                        OnPropertyChanged(nameof(StatusIcon));
                    }
                } 
        }
        public eImageType StatusIcon
        {
            get
            {
                return RunSetStatus switch
                {
                    Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed => eImageType.Passed,
                    Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed => eImageType.Failed,
                    Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending => eImageType.Pending,
                    Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running => eImageType.Running,
                    _ => eImageType.Unknown,
                };
            }
        }
    }
}
