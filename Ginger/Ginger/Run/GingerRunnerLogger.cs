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
            logger = new Logger("logfile.log", false);
            logger.LogTime();
            logger.Log("Log started");
        }

        public void LogAction(Act act)
        {
            ActionLogConfig actionLogConfig = act.actionLogConfig;
            // create a new log file if not exists and append the contents
            
            logger.LogTime();
            logger.Log(actionLogConfig.LogText);
        }
    }
}
