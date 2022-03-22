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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Actions;
using Ginger.Reports;

namespace Ginger.Reports
{
    class HTMLDetailedReport : HTMLReportBase 
    {
        public override string CreateReport(ReportInfo RI)
        {
            base.RI = RI;

            List<BusinessFlowReport> BizFlows = RI.BusinessFlows;

            if (BizFlows.Count == 0)
            {
                return "There are no " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " to create report";
            }
            
            string BFTableFlows = "";
            string ValFailColor = "#b01318";
            string ActFailColor = "#b01318";
            string AtvFailColor = "#b01318";
            string BFFailColor = "#D9181E";    
            double TableSize = 140;

            string bftable = "";
         

            //set fail color
            if (validationFail == 0) { ValFailColor = "#105b00"; }
            if (ActivityFail == 0) { AtvFailColor = "#105b00"; }
            if (ActionFail == 0) { ActFailColor = "#105b00"; }
            if (Failcount == 0) { BFFailColor = "#109300"; }
            
            foreach (BusinessFlowReport BFR in BizFlows)
            {
                BusinessFlow BF = BFR.GetBusinessFlow();
                bftable = "";
                string runColor = "#00000";
                TableSize = TableSize + 16.7;
                if (BF.RunStatus == BusinessFlow.eBusinessFlowRunStatus.Passed)
                {
                    runColor = "#107400";
                }
                else if (BF.RunStatus == BusinessFlow.eBusinessFlowRunStatus.Failed)
                {
                    runColor = "#D9181E";
                }               

                //format BF duration
                string FormatedDuration1 = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", 0, 0, 0); //Added by Preeti as part of resolving defect 2147
                if (BF.Elapsed != null)
                {
                    TimeSpan t1 = TimeSpan.FromMilliseconds((double)BF.Elapsed);
                    FormatedDuration1 = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t1.Hours, t1.Minutes, t1.Seconds);
                }
               
                //Added as part of resolving defect 2147
                if (BF.RunStatus == BusinessFlow.eBusinessFlowRunStatus.Running ) 
                {
                    BF.RunStatus = BusinessFlow.eBusinessFlowRunStatus.Stopped;
                }


                foreach (Activity a in BF.Activities.Where(a => a.GetType() != typeof(ErrorHandler) && a.Active == true).ToList())
                {
                    //Added as part of resolving defect 2147
                    if (a.Status == Activity.eActivityRunStatus.Pending || a.Status == Activity.eActivityRunStatus.Running)   //(a.ElapsedSecs == null) 
                    {
                        a.Status = Activity.eActivityRunStatus.Skipped;
                    }
                    
                    if (a.Status == Activity.eActivityRunStatus.Passed)
                    {
                        runColor = "#107400";
                    }
                    else if (a.Status != Activity.eActivityRunStatus.Passed)
                    {
                        runColor = "#D9181E";
                    }
                    TableSize = TableSize + 16.7;
                    bftable = bftable + @"<tr>
                            <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:13px;"">
                                " + a.ActivityName +
                            @"</td>
                                <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:13px;"">
                                " + string.Format("{0:00}h:{1:00}m:{2:00}s", a.ElapsedSecs / 3600, (a.ElapsedSecs / 60) % 60, a.ElapsedSecs % 60) +
                            @"</td>
                                <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:" + runColor + @";font-family:sans-serif;font-size:13px;"">
                                   " + a.Status.ToString() +
                            @"</td>
                            </tr>";
                }
                

                BFTableFlows = BFTableFlows + @"<tr>
                <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                    " + BFR.Name +
                @"</td>
                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                    " + FormatedDuration1 +
                @"</td>
                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:" + runColor + @";font-family:sans-serif;font-size:14px;font-weight:bold;"">
                       " + BF.RunStatus.ToString() +
                @"</td>
                </tr>";
                BFTableFlows = BFTableFlows + bftable;
                bftable = "";
            }
            TimeSpan t = RI.TotalExecutionTime;
            string FormatedDuration = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);

            string html = @"<!--[if mso]><body xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:wp=""http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:wps=""http://schemas.microsoft.com/office/word/2010/wordprocessingShape"" align=""center"" style=""padding-top: 0 !important; padding-bottom: 0 !important; padding-top: 0 !important; padding-bottom: 0 !important; margin:0 !important; width: 100% !important; -webkit-text-size-adjust: 100% !important; -ms-text-size-adjust: 100% !important; -webkit-font-smoothing: antialiased !important; padding-top: 0; padding-bottom: 0; padding-top: 0; padding-bottom: 0; background-repeat: repeat; width: 100% !important; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; -webkit-font-smoothing: antialiased;"" offset=""0"" toppadding=""0"" leftpadding=""0"" paddingwidth=""0"" paddingheight=""0"">
                                <table style=""display: block !important; outline: none !important; font-family:Helvetica, sans-serif; background: #3f4040; vertical-align: top;"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""100%"" class=""tableContent"">
                                    <tr>
                                        <td style=""vertical-align: top;"" align='center' width=""600"" class='movableContentContainer'>
                                           <!-- =============== START HEADER =============== -->
                                            <div class='movableContent'>
                                                <table border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""600"" style=""padding-top:5px;padding-bottom:5px"">
                                                    <tr>
                                                        <td style=""vertical-align:middle;"" align=""center"" width=""125"" height=""100"">
                                                            <!--<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  href=""http://www.amdocs.com/"" style=""v-text-anchor:middle;width:125px;height:100px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;"" arcsize=""10%"">
                                                                <v:fill type=""gradient"" color=""#B65C25"" color2=""#FF6E00"" color3=""#E45F0F"" angle=""0"" />
                                                                <w:anchorlock />
                                                                <left style=""v-text-anchor:middle;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;"">
                                                                    <v:image align=""left"" src=""http://www.amdocs.com/Style%20Library/Responsive/images/logo.png"" style=""width:120px;vertical-align:middle;padding:12px;"" fillcolor=""transparent""><wd:wrap wd:type=""square"" /></v:image>
                                                                </left>
                                                            </v:roundrect>-->
                                                            <div class=""contentEditable"">
                                                                <img src=""http://www.amdocs.com/Style%20Library/Responsive/images/logo.png"" alt=""Header Image"" width='100' data-default=""placeholder"" data-max-width=""120"" />
                                                            </div>
                                                        </td>
                                                        <td align=""right"" width=""475"" height=""100"">
                                                            <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  style=""v-text-anchor:middle;width:470px;height:100px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;"" arcsize=""10%"">
                                                                <v:fill type=""gradient""colors=""0% #e45f0f,41% #8c3310,42% #b65c25,100% #ff6e00"" angle=""0"" /><v:shadow on=""True"" />
                                                                <w:anchorlock />
                                                                <center style=""v-text-anchor:middle;color:#ffffff;font-family:sans-serif;font-size:34px;font-weight:bold;"">
                                                                    GINGER Automation Execution Report
                                                                </center>
                                                            </v:roundrect>
                                                        </td>
                                                    </tr>
                                                </table>
                                                <div>
                                                    <table align=""center"" cellspacing=""0"" cellpadding=""0"" style=""padding-bottom:0px;padding-top:5px;width:600px"">
                                                        <tr>
                                                            <td valign=""top"">
                                                                <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  style=""v-padding-auto:false;margin-bottom:0px;margin-top:0px;v-text-anchor:top;width:600px;height:35px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;margin:0"" arcsize=""50%"">
                                                                    <v:fill type=""gradient"" color=""#BBBABB"" color2=""#D6D4D7"" color3=""#E7E3E3"" angle=""0"" />
                                                                    <w:anchorlock />
    <center style=""v-text-anchor:top;color:#000000;font-family:sans-serif;font-size:16px;font-weight:bold;"">
        Detailed Report" +
    @"</center>
                                                               
                                                                </v:roundrect>
                                                                 <tr>
 <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  style=""v-padding-auto:false;margin-bottom:0px;margin-top:0px;v-text-anchor:top;width:600px;height:35px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;margin:0"" arcsize=""50%"">
                                                                    <v:fill type=""gradient"" color=""#BBBABB"" color2=""#D6D4D7"" color3=""#E7E3E3"" angle=""0"" />
                                                                    <w:anchorlock />
    <center style=""v-text-anchor:top;color:#000000;font-family:sans-serif;font-size:16px;font-weight:bold;"">
        Execution Environment : " + RI.ExecutionEnv + 
    @"</center>
                                                                </v:roundrect>
                                                               </tr>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style=""padding-top:15px"">
                                                                <v:line from=""0px,0px"" to=""600px,0px"">
                                                                    <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
                                                                </v:line>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </div>
                                            </div>

                                            <!-- =============== END HEADER =============== -->
                                            <!-- =============== START BODY =============== -->
                                            <div class='movableContent'>
                                                <table border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""100%"">
                                                    <tr>
                                                        <td style=""vertical-align: top;"" align=""center"">
                                                            <!--[if mso]>
                                                               <table align=""center"" cellspacing=""0"" cellpadding=""0"" style=""padding-bottom:5px"">
                                                                    <tr>
                                                                        <td align=""left"">
                                                                            <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  style=""v-text-anchor:middle;width:600px;height:60px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;"" arcsize=""10%"">
                                                                                <v:fill type=""gradient""colors=""0% #e45f0f,41% #8c3310,42% #b65c25,100% #ff6e00"" angle=""0"" /><v:shadow on=""True""/>
                                                                                <w:anchorlock />
                                                                                <center style=""v-text-anchor:middle;color:#ffffff;font-family:sans-serif;font-size:30px;font-weight:bold;"">
                                                                                    Automation Summary
                                                                                </center>
                                                                            </v:roundrect>
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            <![endif]-->
                                                            <![if !mso]>
                                                            <table width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" bgcolor=""FF6E00"" style=""-webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; display: block;"">
                                                                <tr><td style=""vertical-align: top;"" colspan=""3"" height='11'></td></tr>
                                                                <tr>
                                                                    <td style=""vertical-align: top;"" width='20'></td>
                                                                    <td style=""vertical-align: top;"">
                                                                        <div class=""contentEditableContainer contentTextEditable"">
                                                                            <div class=""contentEditable"">
                                                                                <p style=""font-size: 30px; text-align: center;font-family:sans-serif;color: #ffffff; line-height: 150%;font-weight:bold; line-height: 150%; margin:0; padding:0;"">Execution Summary:</p>
                                                                            </div>
                                                                        </div>
                                                                    </td>
                                                                    <td style=""vertical-align: top;"" width='20'></td>
                                                                </tr>
                                                                <tr><td style=""vertical-align: top;"" colspan=""3"" height='11'></td></tr>
                                                            </table>
                                                            <![endif]>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>

                                            <div class='movableContent'>
                                                <table border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""100%"">
                                                    <tr>
                                                        <td style=""vertical-align: top;"">
                                                            <table width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                                <tr>
                                                                    <td style=""vertical-align: top;"">
                                                                        <table valign=""bottom"" style=""vertical-align: bottom;"" width=""190"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                                            <tr>
                                                                                <td align=""center"">
                                                                                    <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  style=""v-text-anchor:top;width:190px;height:225px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;z-index:1"" arcsize=""10%"">
                                                                                        <v:fill type=""gradient"" color=""#8D8A8A"" color2=""#D6D4D7"" color3=""#E7E3E3"" angle=""0"" />

                                                                                        <w:anchorlock />
                                                                                        <center style=""v-text-anchor:top;color:#000000;font-family:sans-serif;font-size:24px;font-weight:bold;"">
                                                                                            EXECUTION DETAILS
                                                                                        </center>
                                                                                        <v:line from=""30px,70px"" to=""160px,70px"" style=""z-index:2"">
                                                                                            <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
                                                                                        </v:line>
                                                                                        <center style=""padding-top:40px;padding-left:10px;color:#000000;font-family:sans-serif;font-size:12px;font-weight:bold;"">
                                                                                            <p style=""padding:0px 0px 0px 0px;line-height:5px;"">
        <b style=""color:#000000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
            Execution Run Time:
        </b><br />
        <b style=""color:#000000;font-family:sans-serif;font-size:12px;font-weight:normal"">
            " + DateTime.Now.ToString() +
        @"</b><br />
        <br style=""display:block; margin-top:20px; line-height:15px;"" />
        <b style=""color:#000000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
            Execution Duration:
        </b><br />
        <b style=""color:#000000;font-family:sans-serif;font-size:12px;font-weight:normal"">
            " + FormatedDuration +
        @"</b><br />
        <br style=""display:block; margin-top:20px; line-height:15px;"" />
        <b style=""color:#000000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
            # "+GingerDicser.GetTermResValue(eTermResKey.BusinessFlows)+@":
        </b><br />
        <b style=""color:#000000;font-family:sans-serif;font-size:12px;font-weight:normal"">
            " + BizFlows.Count().ToString() +
        @"</b>
     
       
                                                                                            </p>
                                                                                            <v:line from=""50px,125px"" to=""140px,125px"" style=""z-index:2"">
                                                                                                <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
                                                                                            </v:line>
                                                                                            <v:line from=""50px,170px"" to=""140px,170px"" style=""z-index:2"">
                                                                                                <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
                                                                                            </v:line>
                                                                                        </center>
                                                                                    </v:roundrect>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                    </td>
                                                                    <td style=""vertical-align: top;"" width='15'></td>
                                                                    <td style=""vertical-align: top;"">
                                                                        <table valign=""bottom"" style=""vertical-align: bottom;"" width=""190"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                                            <tr>
                                                                                <td align=""left"">
                                                                                    <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  style=""v-text-anchor:top;width:190px;height:225px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;z-index:1"" arcsize=""10%"">
                                                                                        <v:fill type=""gradient"" color=""#66ab69"" color2=""#D6D4D7"" angle=""0"" />
                                                                                        <w:anchorlock />
        <center style=""v-text-anchor:top;color:#000000;font-family:sans-serif;font-size:24px;font-weight:bold;"">
            " +GingerDicser.GetTermResValue(eTermResKey.BusinessFlows, setToUpperCase:true)+@" PASSED
            <br /> 
        </center>
        <v:line from=""30px,70px"" to=""160px,70px"" style=""z-index:2"">
            <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
        </v:line>
        <center style=""padding-top:40px;color:#109300;font-family:Georgia;font-size:72px;font-weight:bold;text-shadow: 2px 2px #ff0000;"">
            " + Passcount.ToString() +
        @"</center>
                                                                                    </v:roundrect>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                    </td>
                                                                    <td style=""vertical-align: top;"" width='15'></td>
                                                                    <td style=""vertical-align: top;"">
                                                                        <table valign=""bottom"" style=""vertical-align: bottom;"" width=""190"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                                            <tr>
                                                                                <td align=""left"">
                                                                                    <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  style=""v-text-anchor:top;width:190px;height:225px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;"" arcsize=""10%"">
                                                                                        <!--<v:fill type=""gradient"" color=""#BBBABB"" color2=""#D6D4D7"" color3=""#E7E3E3"" angle=""0"" />-->
                                                                                        <v:fill type=""gradient"" color=""#ac6766"" color2=""#D6D4D7"" angle=""0"" />
                                                                                        <w:anchorlock />
        <center style=""v-text-anchor:top;color:#000000;font-family:sans-serif;font-size:24px;font-weight:bold;"">
            "+GingerDicser.GetTermResValue(eTermResKey.BusinessFlows, setToUpperCase:true)+ @" FAILED
            <br /> 
        </center>
        <v:line from=""30px,70px"" to=""160px,70px"" style=""z-index:2"">
            <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
        </v:line>
        <center style=""padding-top:40px;color:" + BFFailColor + @";font-family:Georgia;font-size:72px;font-weight:bold;text-shadow: 2px 2px #ff0000;""> 
            " + Failcount.ToString() +
        @"</center>
                                                                                    </v:roundrect>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </td>
                                                    </tr><tr />
                                                </table>
                                            </div>

                                            <div class='movableContent'>
                                                <table border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""100%"">
                                                    <tr>
                                                        <td style=""vertical-align: top;"">
                                                            <table width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                                <tr>
                                                                    <td style=""vertical-align: top;"">
                                                                        <table valign=""bottom"" style=""vertical-align: bottom;"" width=""190"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                                            <tr>
                                                                                <td align=""center"">
                                                                                    <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" style=""v-text-anchor:top;width:600px;height:175px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;z-index:1"" arcsize=""10%"">
                                                                                        <v:fill type=""gradient"" color=""#9f9ee4"" color2=""#D6D4D7"" angle=""0"" />
                                                                                        <w:anchorlock />
                                                                                        <table width=""550"" cellspacing=""0"" cellpadding=""0"" align=""center"" style=""border-collapse:collapse;vertical-align:middle"">
                                                                                            <tr>
                                                                                                <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;padding-bottom:3px;padding-top:3px"">
                                                                        
                                                                                                </td>
                                                                                                <td align=""center"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;padding-bottom:3px;padding-top:3px"">
                                                                                                    Activities
                                                                                                </td>
                                                                                                <td align=""center""  style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;"">
                                                                                                    Actions
                                                                                                </td>
                                                                                                <td align=""center"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;"">
                                                                                                    Validations
                                                                                                </td>
                                                                                            </tr>
                                                                                            <tr>
        <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;padding-bottom:3px;padding-top:3px"">
            Total
        </td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + ActivityCount.ToString() +
        @"</td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + ActionCount.ToString() +
        @"</td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + ValidationCount.ToString() +
        @"</td>
                                                                                            </tr>
                                                                                            <tr>
        <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;padding-bottom:3px;padding-top:3px"">
            Passed
        </td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#105b00;font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + ActivityPass.ToString() +
        @"</td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#105b00;font-family:sans-serif;font-size:18px;font-weight:bold;"">
           " + ActionPass.ToString() +
        @"</td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#105b00;font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + ValidationPass.ToString() +
        @"</td>
                                                                                            </tr>
                                                                                            <tr>
        <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;padding-bottom:3px;padding-top:3px"">
            Failed
        </td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:" + AtvFailColor + @";font-family:sans-serif;font-size:18px;font-weight:bold;"">
           " + ActivityFail.ToString() +
        @"</td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:" + ActFailColor + @";font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + ActionFail.ToString() +
        @"</td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:" + ValFailColor + @";font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + validationFail.ToString() +
        @"</td>
                                                                                            </tr>
                                                                                            <tr>
        <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;padding-bottom:3px;padding-top:3px"">
            Skipped
        </td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#FF7F27;;font-family:sans-serif;font-size:18px;font-weight:bold;"">
           " + ActivitySkipped.ToString() +
        @"</td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#FF7F27;;font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + ActionSkipped.ToString() +
        @"</td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#FF7F27;font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + "N/A" +
        @"</td>
                                                                                            </tr>
      <tr>
        <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;padding-bottom:3px;padding-top:3px"">
            Other
        </td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#FF7F27;;font-family:sans-serif;font-size:18px;font-weight:bold;"">
           " + ActivityOther.ToString() +
        @"</td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#FF7F27;;font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + ActionOther.ToString() +
        @"</td>
        <td align=""center"" style=""border-bottom:1pt solid black;color:#FF7F27;font-family:sans-serif;font-size:18px;font-weight:bold;"">
            " + "N/A" +
        @"</td>
                                                                                            </tr>
                                                                                        </table>
                                                                                    </v:roundrect>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>

                                            <div class='movableContent'>
                                                <table border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""100%"">
                                                    <tr><td style=""vertical-align: top;"" height='20'></td></tr>
                                                    <tr>
                                                        <td style=""vertical-align: top;"">
                                                            <table width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                                <tr>
                                                                    <td style=""vertical-align: top;"">
                                                                        <table valign=""bottom"" style=""vertical-align: bottom;"" width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                                            <tr>
                                                                                <td align=""center"">
                                                                                    <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  style=""width:600px;height:" + TableSize.ToString() + @"px;border:0;align-self:center;z-index:1;mso-wrap-mode:through;grid-column-align:center;v-text-align:center;"" arcsize=""10%"">
                                                                                        <v:fill type=""gradient"" color=""#8D8A8A"" color2=""#D6D4D7"" color3=""#E7E3E3"" angle=""0"" />
                                                                                        <w:anchorlock />
                                                                                        <table width=""550"" cellspacing=""0"" cellpadding=""0"" align=""center"" style=""border-collapse:collapse;vertical-align:middle"">
                                                                                            <tr>
                                                                                                <td colspan=""3"" style=""vertical-align: top;border-bottom:thick solid #000;padding-bottom:15px"" width=""550"">
                                                                                                    <center style=""v-text-anchor:middle;color:#FF6E00;font-family:sans-serif;font-size:36px;font-weight:bold;text-shadow:20px 12px 5px #ff0000;"">
                                                                                                        "+GingerDicser.GetTermResValue(eTermResKey.BusinessFlows, setToUpperCase:true)+@" DETAILS
                                                                                                    </center>
                                                                                                </td>
                                                                                            </tr>
                                                                                            <tr>
                                                                                                <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;padding-bottom:3px;padding-top:3px"">
                                                                                                    "+GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)+@" Name
                                                                                                </td>
                                                                                                <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:18px;font-weight:bold;"">
                                                                                                    Duration
                                                                                                </td>
                                                                                                <td align=""right"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:18px;font-weight:bold;"">
                                                                                                    Status
                                                                                                </td>
                                                                                            </tr>"
        + BFTableFlows +
                                                                                        @"</table>
                                                                                    </v:roundrect>
                                                                                </td>
                                                                            </tr>
                                                                        </table>
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>

                                            <!-- =============== END BODY =============== -->
                                            <!-- =============== START FOOTER =============== -->
                                            <div class='movableContent'>
                                                <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" class='bgItem'>
                                                    <tr>
                                                        <td style=""vertical-align: top;"">
                                                            <table width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                                <tr><td style=""vertical-align: top;"" height='20'></td></tr>
                                                                <tr>
                                                                    <td style=""vertical-align: top;"" align='center'>
                                                                        <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" style=""width:200px;height:100px;border:0;align-self:center;z-index:1;mso-wrap-mode:through;grid-column-align:center;v-text-align:center;"" arcsize=""10%"">
                                                                            <v:fill type=""gradient"" color=""#8D8A8A"" color2=""#D6D4D7"" color3=""#E7E3E3"" angle=""0"" />
                                                                            <w:anchorlock />
                                                                            <p style=""color:#00000;text-align:center;font-size:14px;line-height:19px; margin:0; padding:0;"">
                                                                                Contact Us:
                                                                                <br/> 
                                                                                <a style=""text-decoration: none; color:#00000;""  href=""Ginger@amdocs.com"">Ginger@amdocs.com</a>
                                                                                <br />
                                                                                Amdocs BEAT Ginger Automation <br />
                                                                            </p>
                                                                        </v:roundrect>
                                                                    </td>
                                                                </tr>
                                                                <tr><td style=""vertical-align: top;"" height='20'></td></tr>
                                                            </table>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </div>
                                            <!-- =============== END FOOTER =============== -->
                                       </td>
                                    </tr>
                                </table>
                            </body><![endif]-->
<![if !mso]><body xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:wp=""http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" xmlns:a=""http://schemas.openxmlformats.org/drawingml/2006/main"" xmlns:wps=""http://schemas.microsoft.com/office/word/2010/wordprocessingShape"" align=""center"" style=""padding-top: 0 !important; padding-bottom: 0 !important; padding-top: 0 !important; padding-bottom: 0 !important; margin:0 !important; width: 100% !important; -webkit-text-size-adjust: 100% !important; -ms-text-size-adjust: 100% !important; -webkit-font-smoothing: antialiased !important; background: #3f4040; padding-top: 0; padding-bottom: 0; padding-top: 0; padding-bottom: 0; background-repeat: repeat; width: 100% !important; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; -webkit-font-smoothing: antialiased;"" offset=""0"" toppadding=""0"" leftpadding=""0"" paddingwidth=""0"" paddingheight=""0"">
    <table style=""display: block !important; outline: none !important; font-family:Helvetica, sans-serif; background: #3f4040; vertical-align: top;"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""100%"" class=""tableContent"">
        <tr>
            <td style=""vertical-align: top;"" align='center' width=""600"" class='movableContentContainer'>
                <div class='movableContent'>
                    <table border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""600"" style=""padding-top:5px;padding-bottom:5px"">
                        <tr>
                            <td style=""vertical-align:middle;"" align=""center"" width=""125"" height=""100"">
                                <div class=""contentEditable"">
                                    <img src=""http://www.amdocs.com/Style%20Library/Responsive/images/logo.png"" alt=""Header Image"" width='100' data-default=""placeholder"" data-max-width=""120"" />
                                </div>
                            </td>
                            <td align=""right"" width=""475"" height=""100"">
                                <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" style=""v-text-anchor:middle;width:470px;height:100px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;"" arcsize=""10%"">
                                    <v:fill type=""gradient"" color=""#B65C25"" color2=""#FF6E00"" color3=""#E45F0F"" angle=""0"" /><v:shadow on=""True"" />
                                    <w:anchorlock />
                                    <center style=""v-text-anchor:middle;color:#ffffff;font-family:sans-serif;font-size:34px;font-weight:bold;"">
                                        GINGER Automation Execution Report
                                    </center>
                                </v:roundrect>
                            </td>
                        </tr>
                    </table>
                    <div>
                        <table align=""center"" cellspacing=""0"" cellpadding=""0"" style=""padding-bottom:0px;padding-top:5px;width:600px"">
                            <tr>
                                <td valign=""top"">
                                    <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" style=""v-padding-auto:false;margin-bottom:0px;margin-top:0px;v-text-anchor:top;width:600px;height:35px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;margin:0"" arcsize=""50%"">
                                        <v:fill type=""gradient"" color=""#BBBABB"" color2=""#D6D4D7"" color3=""#E7E3E3"" angle=""0"" />
                                        <w:anchorlock />
                                        <center style=""v-text-anchor:top;color:#000000;font-family:sans-serif;font-size:16px;font-weight:bold;"">
                                            Report Title - Run Set Long Name of Biz Flow
                                        </center>
                                    </v:roundrect>
                                </td>
                            </tr>
                            <tr>
                                <td style=""padding-top:15px"">
                                    <v:line from=""0px,0px"" to=""600px,0px"">
                                        <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
                                    </v:line>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
                <div class='movableContent'>
                    <table border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""100%"">
                        <tr>
                            <td style=""vertical-align: top;"" align=""center"">
                                <table width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" style=""background: -webkit-linear-gradient(90deg,  rgba(228,95,15,1) 0%,rgba(140,51,16,1) 41%,rgba(182,92,37,1) 42%,rgba(255,110,0,1) 100%); -webkit-border-radius: 5px; -moz-border-radius: 5px; border-radius: 5px; display: block;"">
                                    <tr><td style=""vertical-align: top;"" colspan=""3"" height='11'></td></tr>
                                    <tr>
                                        <td style=""vertical-align: top;"" width='20'></td>
                                        <td style=""vertical-align: top;"">
                                            <div class=""contentEditableContainer contentTextEditable"">
                                                <div class=""contentEditable"" align=""center"">
                                                    <p style=""font-size: 30px; text-align: center;font-family:sans-serif;color: #ffffff; line-height: 150%;font-weight:bold; line-height: 150%; margin:0; padding:0;text-shadow: 1px 1px 0 black;"">Execution Summary:</p>
                                                </div>
                                            </div>
                                        </td>
                                        <td style=""vertical-align: top;"" width='20'></td>
                                    </tr>
                                    <tr><td style=""vertical-align: top;"" colspan=""3"" height='11'></td></tr>
                                </table>

                            </td>
                        </tr>
                    </table>
                </div>

                <div class='movableContent'>
                    <table border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""100%"">
                        <tr>
                            <td style=""vertical-align: top;"">
                                <table width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                    <tr>
                                        <td style=""vertical-align: top;"">
                                            <table valign=""bottom"" style=""vertical-align: bottom;"" width=""190"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                <tr>
                                                    <td align=""center"">
                                                        <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" style=""v-text-anchor:top;width:190px;height:225px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;z-index:1"" arcsize=""10%"">
                                                            <v:fill type=""gradient"" color=""#8D8A8A"" color2=""#D6D4D7"" color3=""#E7E3E3"" angle=""0"" />
                                                            <w:anchorlock />
                                                            <center style=""v-text-anchor:top;color:#000000;font-family:sans-serif;font-size:24px;font-weight:bold;"">
                                                                EXECUTION DETAILS
                                                            </center>
                                                            <v:line from=""30px,70px"" to=""160px,70px"" style=""z-index:2"">
                                                                <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
                                                            </v:line>
                                                            <center style=""padding-top:40px;padding-left:10px;color:#000000;font-family:sans-serif;font-size:12px;font-weight:bold;"">
                                                                <p style=""padding:0px 0px 0px 0px;line-height:5px;"">
                                                                    <b style=""color:#000000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        Execution Run Time:
                                                                    </b><br />
                                                                    <b style=""color:#000000;font-family:sans-serif;font-size:12px;font-weight:normal"">
                                                                        3/26/2015 1:10:29 AM
                                                                    </b><br />
                                                                    <br style=""display:block; margin-top:20px; line-height:15px;"" />
                                                                    <b style=""color:#000000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        Execution Duration:
                                                                    </b><br />
                                                                    <b style=""color:#000000;font-family:sans-serif;font-size:12px;font-weight:normal"">
                                                                        1234
                                                                    </b><br />
                                                                    <br style=""display:block; margin-top:20px; line-height:15px;"" />
                                                                    <b style=""color:#000000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        # "+GingerDicser.GetTermResValue(eTermResKey.BusinessFlows)+@":
                                                                    </b><br />
                                                                    <b style=""color:#000000;font-family:sans-serif;font-size:12px;font-weight:normal"">
                                                                        134
                                                                    </b>
                                                                </p>
                                                                <v:line from=""50px,125px"" to=""140px,125px"" style=""z-index:2"">
                                                                    <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
                                                                </v:line>
                                                                <v:line from=""50px,170px"" to=""140px,170px"" style=""z-index:2"">
                                                                    <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
                                                                </v:line>
                                                            </center>
                                                        </v:roundrect>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td style=""vertical-align: top;"" width='15'></td>
                                        <td style=""vertical-align: top;"">
                                            <table valign=""bottom"" style=""vertical-align: bottom;"" width=""190"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                <tr>
                                                    <td align=""left"">
                                                        <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" style=""v-text-anchor:top;width:190px;height:225px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;z-index:1"" arcsize=""10%"">
                                                            <v:fill type=""gradient"" color=""#66ab69"" color2=""#D6D4D7"" angle=""0"" />
                                                            <w:anchorlock />
                                                            <center style=""v-text-anchor:top;color:#000000;font-family:sans-serif;font-size:24px;font-weight:bold;"">
                                                                " + GingerDicser.GetTermResValue(eTermResKey.Activities, setToUpperCase:true) + @" PASSED
                                                                <br /> <br />
                                                            </center>
                                                            <v:line from=""30px,70px"" to=""160px,70px"" style=""z-index:2"">
                                                                <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
                                                            </v:line>
                                                            <center style=""padding-top:40px;color:#109300;font-family:Georgia;font-size:72px;font-weight:bold;text-shadow: 2px 2px #ff0000;"">
                                                                122
                                                            </center>
                                                        </v:roundrect>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                        <td style=""vertical-align: top;"" width='15'></td>
                                        <td style=""vertical-align: top;"">
                                            <table valign=""bottom"" style=""vertical-align: bottom;"" width=""190"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                <tr>
                                                    <td align=""left"">
                                                        <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" style=""v-text-anchor:top;width:190px;height:225px;border:0;align-self:center;color:#ffffff;font-family:sans-serif;font-size:12px;font-weight:bold;"" arcsize=""10%"">
                                                            <v:fill type=""gradient"" color=""#ac6766"" color2=""#D6D4D7"" angle=""0"" />
                                                            <w:anchorlock />
                                                            <center style=""v-text-anchor:top;color:#000000;font-family:sans-serif;font-size:24px;font-weight:bold;"">
                                                                " + GingerDicser.GetTermResValue(eTermResKey.Activities, setToUpperCase: true) + @" FAILED
                                                                <br /> <br />
                                                            </center>
                                                            <v:line from=""30px,70px"" to=""160px,70px"" style=""z-index:2"">
                                                                <v:fill type=""gradient"" color=""#3f4040"" color2=""#000000"" color3=""#3f4040"" angle=""90"" />
                                                            </v:line>
                                                            <center style=""padding-top:40px;color:#D9181E;font-family:Georgia;font-size:72px;font-weight:bold;text-shadow: 2px 2px #ff0000;"">
                                                                12
                                                            </center>
                                                        </v:roundrect>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>

                <div class='movableContent'>
                    <table border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" width=""100%"">
                        <tr><td style=""vertical-align: top;"" height='20'></td></tr>
                        <tr>
                            <td style=""vertical-align: top;"">
                                <table width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                    <tr>
                                        <td style=""vertical-align: top;"">
                                            <table valign=""bottom"" style=""vertical-align: bottom;"" width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                                <tr>
                                                    <td align=""center"">
                                                        <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" style=""width:600px;height:130px;border:0;align-self:center;z-index:1;mso-wrap-mode:through;grid-column-align:center;v-text-align:center;"" arcsize=""10%"">
                                                            <v:fill type=""gradient"" color=""#8D8A8A"" color2=""#D6D4D7"" color3=""#E7E3E3"" angle=""0"" />
                                                            <w:anchorlock />
                                                            <table width=""550"" cellspacing=""0"" cellpadding=""0"" align=""center"" style=""border-collapse:collapse;vertical-align:middle"">
                                                                <tr>
                                                                    <td colspan=""3"" style=""vertical-align: top;border-bottom:thick solid #000;padding-bottom:15px"" width=""550"">
                                                                        <center style=""v-text-anchor:middle;color:#FF6E00;font-family:sans-serif;font-size:36px;font-weight:bold;text-shadow:20px 12px 5px #ff0000;"">
                                                                            " +GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, setToUpperCase:true) + @" DETAILS
                                                                        </center>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:20px;font-weight:bold;padding-bottom:3px;padding-top:3px"">
                                                                        " +GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + @" Name
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:18px;font-weight:bold;"">
                                                                        Duration
                                                                    </td>
                                                                    <td align=""right"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:18px;font-weight:bold;"">
                                                                        Status
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        SCRM - Create Customer New
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        23943
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#D9181E;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        FAILED
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        SCRM - Create Customer Retention Modify Existing
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        33641
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#107400;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        PASSED
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        SCRM - Create Customer Aqcuistion Flow with Super Duper Long " +GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + @" Name for Wrapping
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        33641
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#107400;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        PASSED
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        SCRM - Create Customer Retention Modify Existing
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        33641
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#107400;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        PASSED
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        SCRM - Create Customer Retention Modify Existing
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        33641
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#107400;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        PASSED
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        SCRM - Create Customer Retention Modify Existing
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        33641
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#107400;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        PASSED
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        SCRM - Create Customer Retention Modify Existing
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        33641
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#D9181E;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        FAILED
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td align=""left"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        SCRM - Create Customer Retention Modify Existing
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#00000;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        33641
                                                                    </td>
                                                                    <td align=""center"" width=""60"" style=""border-bottom:1pt solid black;color:#107400;font-family:sans-serif;font-size:14px;font-weight:bold;"">
                                                                        PASSED
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </v:roundrect>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>
                <div class='movableContent'>
                    <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"" class='bgItem'>
                        <tr>
                            <td style=""vertical-align: top;"">
                                <table width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" align=""center"">
                                    <tr><td style=""vertical-align: top;"" height='20'></td></tr>
                                    <tr>
                                        <td style=""vertical-align: top;"" align='center'>
                                            <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"" style=""width:200px;height:100px;border:0;align-self:center;z-index:1;mso-wrap-mode:through;grid-column-align:center;v-text-align:center;"" arcsize=""10%"">
                                                <v:fill type=""gradient"" color=""#8D8A8A"" color2=""#D6D4D7"" color3=""#E7E3E3"" angle=""0"" />
                                                <w:anchorlock />
                                                <p style=""color:#00000;text-align:center;font-size:11px;line-height:19px; margin:0; padding:0;"">
                                                    <a style=""text-decoration: none; color:#00000;"" target='_blank' href=""#"">Contact us</a>
                                                    <br />
                                                        <a href=""mailto:email@address.com?subject=Hello world&body=Line one%0DLine two"">Email me</a>
                                                    Sent by GINGER AUTOMATION <br />
                                                    AMDOCS TIS AUTOMATION <br />
                                                </p>
                                            </v:roundrect>
                                        </td>
                                    </tr>
                                    <tr><td style=""vertical-align: top;"" height='20'></td></tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>
           </td>
        </tr>
    </table>
</body><![endif]>
                            ";
            return html;
        }

    }
}
