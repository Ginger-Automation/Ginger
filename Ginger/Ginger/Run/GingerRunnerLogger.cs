using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Repository;
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

            logger.LogTimeStamp();
            logger.Log("[Action] " + act.ActionDescription);
            logger.Log("[Text] " + actionLogConfig.LogText);

            // log all the input values
            if (actionLogConfig.LogInputVariables)
            {
                logger.Log("[Input Values]");                
                foreach (ActInputValue actInputValue in act.InputValues)
                {
                    logger.Log(actInputValue.ItemName + " --> " + actInputValue.Value);
                }
            }

            // log all the output variables
            if (actionLogConfig.LogOutputVariables)
            {
                logger.Log("[Return Values]");                
                foreach (ActReturnValue actReturnValue in act.ReturnValues)
                {
                    logger.Log(actReturnValue.ItemName + " --> " + actReturnValue.Expected + " --> " + actReturnValue.Actual);
                }
            }

            // action status
            if (actionLogConfig.LogRunStatus)
            {
                logger.Log("[Run Status] " + act.Status);
            }

            // action elapsed time
            if (actionLogConfig.LogElapsedTime)
            {
                logger.Log("[Elapsed Time (In Secs)] " + act.ElapsedSecs);
            }

            // action error
            if (actionLogConfig.LogError)
            {
                logger.Log("[Error] " + act.Error);
            }

            // flush value expression
            // flush flow control

        }


    }
}
