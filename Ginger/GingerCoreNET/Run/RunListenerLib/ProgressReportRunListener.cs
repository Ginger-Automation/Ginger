using Amdocs.Ginger.Common;
using Amdocs.Ginger.Run;
using Ginger.Reports;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    public class ProgressReportRunListener :  RunListenerBase
    {

         /// <summary>
         ///  udpate in BF start !!!!!!!!!!!!!!!!!!!!!!!
         /// </summary>
        BusinessFlow mBusinessFlow;

        enum eExecutionPhase
        {
            Start,
            End
        }

        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            mBusinessFlow = businessFlow;
            AddExecutionDetailsToLog(eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, new BusinessFlowReport(businessFlow));
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            AddExecutionDetailsToLog(eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, new BusinessFlowReport(businessFlow));
        }

        public override void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            AddExecutionDetailsToLog(eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activityGroup.Name, new ActivityGroupReport(activityGroup, mBusinessFlow));            
        }

        public override void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup)
        {
            AddExecutionDetailsToLog(eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activityGroup.Name, new ActivityGroupReport(activityGroup, mBusinessFlow));
        }

        public override void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {                        
            AddExecutionDetailsToLog(eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.Activity), activity.ActivityName, new ActivityReport(activity));
        }

        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode= false)
        {
            AddExecutionDetailsToLog(eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.Activity), activity.ActivityName, new ActivityReport(activity));
        }

        public override void ActionStart(uint eventTime, Act action)
        {
            AddExecutionDetailsToLog(eExecutionPhase.Start, "Action", action.Description, new ActionReport(action));
        }

        public override void ActionEnd(uint eventTime, Act action, bool offlineMode=false)
        {
            AddExecutionDetailsToLog(eExecutionPhase.End, "Action", action.Description, new ActionReport(action));
        }

        void AddExecutionDetailsToLog(eExecutionPhase objExecutionPhase, string objType, string objName, object obj)
        {
            if (Reporter.AppLoggingLevel == eAppReporterLoggingLevel.Debug)
            {
                StringBuilder stringBuilder = new StringBuilder("--> ");
                string prefix = string.Empty;
                switch (objExecutionPhase)
                {
                    case eExecutionPhase.Start:
                        stringBuilder.Append("Execution Started for the ");                        
                        break;
                    case eExecutionPhase.End:
                        stringBuilder.Append("Execution Ended for the ");                        
                        break;
                }
                stringBuilder.Append(objType).Append(": '").Append(objName).Append("'").AppendLine();


                //get the execution fields and their values
                if (obj != null)
                {
                    // List<KeyValuePair<string, string>> fieldsAndValues = new List<KeyValuePair<string, string>>();
                    try
                    {
                        PropertyInfo[] props = obj.GetType().GetProperties();
                        foreach (PropertyInfo prop in props)
                        {
                            try
                            {
                                FieldParamsFieldType attr = ((FieldParamsFieldType)prop.GetCustomAttribute(typeof(FieldParamsFieldType)));
                                if (attr == null)
                                {
                                    continue;
                                }
                                FieldsType ftype = attr.FieldType;
                                if (ftype == FieldsType.Field)
                                {
                                    string propName = prop.Name;
                                    string propFullName = ((FieldParamsNameCaption)prop.GetCustomAttribute(typeof(FieldParamsNameCaption))).NameCaption;
                                    object propVal = prop.GetValue(obj);
                                    string propValue;
                                    if (propVal != null)
                                    {
                                        propValue = propVal.ToString();
                                    }
                                    else
                                    {
                                        propValue = "";
                                    }
                                    
                                    stringBuilder.Append(propName).Append("= ").Append(propValue).AppendLine();
                                }
                            }
                            catch (Exception ex)
                            {
                                //TODO: !!!!!!!!!!!!!!!!!! FIXME
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //TODO: !!!!!!!!!!!!!!!!!! FIXME
                    }


                    Reporter.ToLog(eLogLevel.INFO, stringBuilder.ToString());
                }
                else
                {
                    Reporter.ToLog(eLogLevel.INFO, prefix + System.Environment.NewLine);
                }
            }

        }

    }
}
