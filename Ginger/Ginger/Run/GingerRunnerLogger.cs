using Amdocs.Ginger.Common.Actions;
using Ginger.GeneralLib;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Run
{
    public class GingerRunnerLogger
    {
        Logger logger;

        public GingerRunnerLogger(string fileName)
        {
            logger = new Logger(fileName, false);
            logger.LogTime();
            logger.Log("Log started");
        }

        public void LogAction(Act act)
        {
            if (!act.EnableActionLogConfig) return;
            ActionLogConfig actionLogConfig = act.ActionLogConfig;
            // create a new log file if not exists and append the contents
            
            logger.LogTime();
            logger.Log(actionLogConfig.LogText);
        }
    }
}
