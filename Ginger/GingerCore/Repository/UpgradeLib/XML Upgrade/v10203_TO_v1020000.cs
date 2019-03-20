#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using GingerCore.Repository;

namespace GingerCore.XMLConverters
{
    class v10203_TO_v1020000 : XMLConverterBase
    {
        /// <summary>
        /// Makes following changes to convert Ginger XML files from v1 to v2: 
        ///     1) Global
        ///         a) Updates Ginger version #
        ///         b) Replaces "NextGenCoreWPF nodes w "GingerCore" nodes
        ///         c) Replaces "NextGen" nodes w "Ginger" nodes
        ///     2) Business Flows
        ///         a) Converts validations in Asserts nodes into GenWebElement actions
        ///         b) add ExternalID attribute
        ///         c) Add v2 attributes to all actions
        ///             i) Platform
        ///             ii) TakeScreenShot
        ///             iii) GenElementAction
        ///         d) Removes Asserts nodes
        ///     3) Solutions
        ///         a) add v2 attribute (MainPlatform)
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public override void Convert()
        {
            /* This takes 2 passes at doing replacements: one using Linq and then one using string functions.
             * In the future there might be things that are a lot easeir to do using string functions, so the 2 pass setup makes sense I think.
             * The <Asserts> nodes are being commented out instead of deleted to minimize risk of lost data (e.g., if someone accidentally opens the a project 
             * before noting whichs assertions in which activities need to be re-added after conversion). */

            string outputxml = "";

            // UPDATE VERSION IN COMMENT SO THAT FILE DOESN'T GET PARSED OVER AND OVER.
            /* Expecting the 1st comment in file to contain build info and be like this: <!--Ginger Repository Item created with version: 0.1.2.3 -->
             * Setting version to 0.2.0.0/ */
            UpdateXMLVersion("1.2.0.0");

            // 1ST PASS - UPDATE XML BASED ON FILE TYPE USING LINQ
            #region parse using Linq
            switch (RepoType)
            {
                #region case GingerFileType.Solution:
                case eGingerFileType.Solution:
                    break;
                #endregion
                #region case GingerFileType.BusinessFlow:
                case eGingerFileType.BusinessFlow:
                    #region case GingerFileType.BusinessFlow:
                    #region notes
                    /* This does the following to convert validations in BusinessFlow into the new format:
                     *  1) Loop through all activities containing validations
                     *  2) For each old validation add ActGenElement action to the activity w following
                     *      <InputValues>
                     *      <ReturnValues>
                     *          ActReturnValue 
                     *  3) Delete old validation from <Asserts> once it's been handled.
                     *  4) Remove deprecated <Asserts> once all validations have been handled. 
                     *  5) Adds a ExternalID attribute
                     */
                    /* OLD BF XML:
                     *  BusinessFlow / Activities / Activity / Asserts / ValidationUI 
                     *  BusinessFlow / Activities / Activity / Asserts / ValidationDB */

                    /* NEW BF XML:
                     *  BusinessFlow / Activities / Activity / Asserts [REMOVED] 
                     *  BusinessFlow / Activities / Activity / Acts / ActGenElement 
                     *  BusinessFlow / Activities / Activity / Acts / ActGenElement  / InputValues [EMPTY]
                     *  BusinessFlow / Activities / Activity / Acts / ActGenElement  / ReturnValues / ActReturnValue
                     * */
                    #endregion


                    // ADD ExternalID ATTRIBUTE TO BF NODE
                    var qBF = (from e in xmlDoc.Descendants("NextGenCoreWPF.BusinessFlow")  select e);
                    XElement el_BF = (XElement)qBF.First();
                    if (el_BF != null) 
                    { 
                        el_BF.Add(new XAttribute("ExternalID", "0"));
                    }
                    else 
                    {
                        /* The NextGenCoreWPF.BusinessFlow node is missing  */ 
                    }
                    
                    #region ADD EMPTY <InputValues> & <ReturnValues> NODES TO ALL ACTIONS IN ORIGINAL XML
                    IEnumerable<XElement> qAllElements= (from el in xmlDoc.Descendants() select el);
                    Regex ActionNameRegex = new Regex(@"NextGenCoreWPF\.Actions\.Act.*"); 
                    foreach (XElement x in qAllElements)
                    {
                        if ((ActionNameRegex.Match(x.Name.ToString()).Success))
                        {
                            //add nodes if absent 
                            if(x.Element("InputValues")==null) x.Add(new XElement("InputValues"));
                            

                            // IF THE V1 ACTION HAD A VALUE ATTRIBUTE, ADD A GingerCore.Actions.ActInputValue CHILD NODE 
                            // The old action VALUE attribute is now a child ActInputValue node
                            if (x.Attribute("Value") != null)
                            {
                                XElement ActInputValueToBeAdded = new XElement("GingerCore.Actions.ActInputValue");
                                ActInputValueToBeAdded.Add(new XAttribute("Param", "Value"));
                                ActInputValueToBeAdded.Add(new XAttribute("Value", x.Attribute("Value").Value));
                                ActInputValueToBeAdded.Add(new XAttribute("Created", x.Attribute("Created").Value));
                                ActInputValueToBeAdded.Add(new XAttribute("CreatedBy", x.Attribute("CreatedBy").Value));
                                ActInputValueToBeAdded.Add(new XAttribute("Guid", "00000000-0000-0000-0000-00000000"));
                                ActInputValueToBeAdded.Add(new XAttribute("ParentGuid", "00000000-0000-0000-0000-00000000"));
                                ActInputValueToBeAdded.Add(new XAttribute("LastUpdate", x.Attribute("LastUpdate").Value));
                                ActInputValueToBeAdded.Add(new XAttribute("Version", "0"));
                                x.Element("InputValues").Add(ActInputValueToBeAdded);
                                break;
                            }
                        }
                    }
                    #endregion //ADD EMPTY <InputValues> & <ReturnValues> NODES TO ALL ACTIONS IN ORIGINAL XML

                    #region HANDLE FUNCTIONS
                    var qActFunctions = (from e in xmlDoc.Descendants("NextGenCoreWPF.Actions.ActFunction")  select e);
                    foreach (XElement xFunc in qActFunctions)
                    {
                        // copy & rename old function & tweak attributes(add a few attribus & remove 1)
                        XElement nEl = new XElement("GingerCore.Actions.ActFunctions");
                        foreach (XAttribute a in xFunc.Attributes()){ nEl.Add(a);}
                        nEl.Attribute("Value").Remove();
                        nEl.Add(new XAttribute("Platform", "Null"));
                        nEl.Add(new XAttribute("Wait", "0"));
                        // add InputValues node
                        nEl.Add(new XElement("InputValues"));

                        #region get this function's FunctionParam nodes & convert them to ActInputValue nodes
                        var oldFunctionParamNodesTBMerged = (from aValueAttribute in xFunc.Elements("NextGenCoreWPF.Functions.FunctionParam") 
                                                select aValueAttribute);
                        foreach(XElement FunctionParamNode in oldFunctionParamNodesTBMerged)
                        {
                            XElement ActInputValueToBeAddedValue = new XElement("GingerCore.Actions.ActInputValue");
                            foreach (XAttribute currAttribs in FunctionParamNode.Attributes()){ ActInputValueToBeAddedValue.Add(currAttribs);} //copy all of old node's attributes
                            ActInputValueToBeAddedValue.Add(new XAttribute("Param",FunctionParamNode.Attribute("Name").Value.ToString())); //add Param attribute using old Name attribute
                            ActInputValueToBeAddedValue.Attribute("Name").Remove(); 
                            xFunc.Element("InputValues").Add(ActInputValueToBeAddedValue);
                        }
                        #endregion //get this function's FunctionParam nodes & convert them to ActInputValue nodes

                        nEl.Add(new XElement("ReturnValues"));

                        #region ADD PARAMS FOR Excel Functions
                        if (xFunc.Attribute("LocateValue").Value.ToString() == "ExcelFunctions")
                        {
                            switch (xFunc.Attribute("Value").Value.ToString())
                            {
                                case "fXLSReadDataToVariables":

                                    #region get values
                                    var vExcelFileName = (from ExcelNode in xFunc.Element("FunctionParams").Elements("NextGenCoreWPF.Functions.FunctionParam")
                                                          where ExcelNode.Attribute("Name").Value.ToString() == "ExcelFileName"
                                                          select ExcelNode.Attribute("Value").Value).SingleOrDefault();
                                    var vSheetName = (from SheetNameNode in xFunc.Element("FunctionParams").Elements("NextGenCoreWPF.Functions.FunctionParam")
                                                      where SheetNameNode.Attribute("Name").Value.ToString() == "SheetName"
                                                      select SheetNameNode.Attribute("Value").Value).SingleOrDefault();
                                    var vSelectRowsWhere = (from SelectRowsWhereNode in xFunc.Element("FunctionParams").Elements("NextGenCoreWPF.Functions.FunctionParam")
                                                            where SelectRowsWhereNode.Attribute("Name").Value.ToString() == "SelectRowsWhere"
                                                            select SelectRowsWhereNode.Attribute("Value").Value).SingleOrDefault();
                                    var vPrimaryKeyColumn = (from PrimaryKeyColumnNode in xFunc.Element("FunctionParams").Elements("NextGenCoreWPF.Functions.FunctionParam")
                                                             where PrimaryKeyColumnNode.Attribute("Name").Value.ToString() == "PrimaryKeyColumn"
                                                             select PrimaryKeyColumnNode.Attribute("Value").Value).Single();
                                    var vSetDataUsed = (from SetDataUsedNode in xFunc.Element("FunctionParams").Elements("NextGenCoreWPF.Functions.FunctionParam")
                                                        where SetDataUsedNode.Attribute("Name").Value.ToString() == "SetDataUsed"
                                                        select SetDataUsedNode.Attribute("Value").Value).Single();                                    
                                    var vOutputCols = (from OutputColsNode in xFunc.Element("FunctionParams").Elements("NextGenCoreWPF.Functions.FunctionParam")
                                                       where OutputColsNode.Attribute("Name").Value.ToString() == "VarCols"
                                                       select OutputColsNode.Attribute("Value").Value).Single();                                    
                                    string sOutputCols=(string)vOutputCols;
                                    string sDataColumnName = sOutputCols.Substring(sOutputCols.IndexOf(",") + 1);
                                    string sVariableName = sOutputCols.Substring(0,sOutputCols.IndexOf(","));



                                    #endregion

                                    XElement NewElementToBeAdded = new XElement("GingerCore.Actions.ActExcel"); //
                                    foreach(XAttribute xa in xFunc.Attributes()) NewElementToBeAdded.Add(xa);
                                    #region attributes
                                    NewElementToBeAdded.Add(new XAttribute("ExcelActionType", "ReadData"));
                                    NewElementToBeAdded.Add(new XAttribute("ExcelFileName", (string)vExcelFileName));
                                    NewElementToBeAdded.Add(new XAttribute("SheetName", (string)vSheetName));
                                    NewElementToBeAdded.Add(new XAttribute("SelectRowsWhere", (string)vSelectRowsWhere));
                                    NewElementToBeAdded.Add(new XAttribute("PrimaryKeyColumn", (string)vPrimaryKeyColumn));
                                    NewElementToBeAdded.Add(new XAttribute("SetDataUsed", (string)vSetDataUsed));
                                    NewElementToBeAdded.Attribute("LocateBy").Value = "NA";
                                    NewElementToBeAdded.Attribute("LocateValue").Remove();
                                    NewElementToBeAdded.Attribute("Value").Remove();

                                    NewElementToBeAdded.Add(new XElement("InputValues"));
                                    NewElementToBeAdded.Add(new XElement("ReturnValues"));

                                    XElement ElActInputValue = new XElement("GingerCore.Actions.ActInputValue");
                                    ElActInputValue.Add(new XAttribute("Active",xFunc.Attribute("Active").Value.ToString()));
                                    ElActInputValue.Add(new XAttribute("Created",xFunc.Attribute("Created").Value.ToString()));
                                    ElActInputValue.Add(new XAttribute("CreatedBy",xFunc.Attribute("CreatedBy").Value.ToString()));
                                    ElActInputValue.Add(new XAttribute("Guid",xFunc.Attribute("Guid").Value.ToString()));
                                    ElActInputValue.Add(new XAttribute("LastUpdate",xFunc.Attribute("LastUpdate").Value.ToString()));
                                    ElActInputValue.Add(new XAttribute("Param","Value"));
                                    ElActInputValue.Add(new XAttribute("ParentGuid",xFunc.Attribute("ParentGuid").Value.ToString()));
                                    ElActInputValue.Add(new XAttribute("Value",""));
                                    ElActInputValue.Add(new XAttribute("Version",xFunc.Attribute("Version").Value.ToString()));
                                    NewElementToBeAdded.Element("InputValues").Add(ElActInputValue);

                                    XElement ElActReturnValue = new XElement("GingerCore.Actions.ActReturnValue");
                                    ElActReturnValue.Add(new XAttribute("Active", xFunc.Attribute("Active").Value.ToString()));
                                    ElActReturnValue.Add(new XAttribute("Actual", "blah"));
                                    ElActReturnValue.Add(new XAttribute("Created",xFunc.Attribute("Created").Value.ToString()));
                                    ElActReturnValue.Add(new XAttribute("CreatedBy",xFunc.Attribute("CreatedBy").Value.ToString()));
                                    ElActReturnValue.Add(new XAttribute("Guid",xFunc.Attribute("Guid").Value.ToString()));
                                    ElActReturnValue.Add(new XAttribute("LastUpdate",xFunc.Attribute("LastUpdate").Value.ToString()));                                    
                                    ElActReturnValue.Add(new XAttribute("ParentGuid",xFunc.Attribute("ParentGuid").Value.ToString()));
                                    ElActReturnValue.Add(new XAttribute("Value",""));
                                    ElActReturnValue.Add(new XAttribute("Version",xFunc.Attribute("Version").Value.ToString()));
                                    ElActReturnValue.Add(new XAttribute("Param", sDataColumnName)); 
                                    ElActReturnValue.Add(new XAttribute("StoreToVariable", sVariableName)); 
                                    NewElementToBeAdded.Element("ReturnValues").Add(ElActReturnValue);

                                    xFunc.AddAfterSelf(NewElementToBeAdded);
                                    #endregion
                                    break;
                                case "fXLSWriteDataFromVariables":

                                    break;
                            }
                        }
                        #endregion //ADD PARAMS FOR Excel Functions
                    } //HANDLE FUNCTIONS

                    var qActFunctionsTbRemoved = (from e in xmlDoc.Descendants("NextGenCoreWPF.Actions.ActFunction")
                                                  where e.Attribute("Value").ToString()=="fXLSReadDataToVariables"
                                                  select e);
                    qActFunctionsTbRemoved.Remove();

                    //delete the old functions now that they have been converted
                    qActFunctions.Remove(); 
                    #endregion //HANDLE FUNCTIONS


                    // ### Loop through <NextGenCoreWPF.Activity> nodes containing an <Asserts> node ###
                    IEnumerable<XElement> qAssertsNodes = (from el in xmlDoc.Descendants("NextGenCoreWPF.Activity").Elements("Asserts") select el);
                    foreach (XElement el_AssertsNode in qAssertsNodes)
                    {
                        int assertreached = 0;
                        // ### Step 1. handle this activity's <NextGenCoreWPF.Asserts.ValidationUI> nodes ###  
                        #region Step 1. handle this activity's <NextGenCoreWPF.Asserts.ValidationUI> nodes
                        var qryValidationUINodes = el_AssertsNode.Descendants("NextGenCoreWPF.Asserts.ValidationUI");
                        foreach (XElement el_OldValidationUI in qryValidationUINodes)
                        {

                            //### create a the new action to hold the old validation's info ###
                            XElement ActGenElementToBeAdded = new XElement("GingerCore.Actions.ActGenElement");

                            //### Get the HTML object choice selected in V1 (NOT USED)###
                            #region determine HTML object type  selected (NOT USED)
                            string Version1UIValidationObjType = el_OldValidationUI.Attribute("UIObjType").Value;
                            switch (Version1UIValidationObjType)
                            {
                                case "TEXTBOX":
                                    break;
                                case "LABEL":
                                    break;
                                case "SELECT":
                                    break;
                                case "LINK":
                                    break;
                                case "CHECKBOX":
                                    break;
                                case "BUTTON":
                                    break;
                                case "PASSWORD":
                                    break;
                                case "RADIO":
                                    break;
                                case "GEN":
                                    break;
                            }
                            #endregion

                            // Check whether we're interested this validation. 
                            #region Check whether we're interested this validation based on validation type 
                            //Only keeping UI validations that used Value and Visible. The rest are discarded
                            string Version1UIValidationType = el_OldValidationUI.Attribute("UIValidationType").Value;
                            string NewValidationType = "";
                            switch (Version1UIValidationType)
                            {
                                case "Value": NewValidationType = "GetInnerText"; break; //GetValue
                                case "Displayed": NewValidationType = "Visible"; break;
                                case "NotDisplayed": break;
                                case "Enabled": break;
                                case "Disabled": break;
                                case "FontSize": break;
                                case "ValidValues": break;
                            }
                            
                            #endregion

                            //populate the new action
                            #region populate the new action
                            // step through old ValidatoinUI attributes to give new ActGenElement its initial properties
                            foreach (XAttribute x in el_OldValidationUI.Attributes()) { ActGenElementToBeAdded.Add(new XAttribute(x.Name.ToString(), x.Value)); }
                            // remove attributes that don't belong in ActGenElement
                            if (ActGenElementToBeAdded.Attribute("Expected")!=null) ActGenElementToBeAdded.Attribute("Expected").Remove();
                            if (ActGenElementToBeAdded.Attribute("UIValidationType") != null) ActGenElementToBeAdded.Attribute("UIValidationType").Remove();
                            if (ActGenElementToBeAdded.Attribute("UIObjType") != null) ActGenElementToBeAdded.Attribute("UIObjType").Remove();                            
                            // add remaining attributes needed by ActGenElement 
                            ActGenElementToBeAdded.Add(new XAttribute("Platform", "Null"));
                            ActGenElementToBeAdded.Add(new XAttribute("TakeScreenShot", "False"));
                            ActGenElementToBeAdded.Add(new XAttribute("GenElementAction", NewValidationType));

                            //CREATE <InputValues> NODE
                            ActGenElementToBeAdded.Add(new XElement("InputValues"));
                            // create <GingerCore.Actions.ActInputValue> node for this <InputValues> node 
                            XElement ActInputValueToBeAdded = new XElement("GingerCore.Actions.ActInputValue");
                            ActInputValueToBeAdded.Add(new XAttribute("Param", "Value"));
                            ActInputValueToBeAdded.Add(new XAttribute("Value", ""));
                            ActInputValueToBeAdded.Add(new XAttribute("Created", el_OldValidationUI.Attribute("Created").Value));
                            ActInputValueToBeAdded.Add(new XAttribute("CreatedBy", el_OldValidationUI.Attribute("CreatedBy").Value));
                            ActInputValueToBeAdded.Add(new XAttribute("Guid", "00000000-0000-0000-0000-00000000"));
                            ActInputValueToBeAdded.Add(new XAttribute("ParentGuid", "00000000-0000-0000-0000-00000000"));
                            ActInputValueToBeAdded.Add(new XAttribute("LastUpdate", el_OldValidationUI.Attribute("LastUpdate").Value)); //String.Format("MMMM/dd/yyyy H:mm:ss tt", DateTime.Now)));
                            ActInputValueToBeAdded.Add(new XAttribute("Version", "0"));
                            // now add it to the action 
                            ActGenElementToBeAdded.Element("InputValues").Add(ActInputValueToBeAdded);

                            //CREATE <ReturnValues> NODE
                            ActGenElementToBeAdded.Add(new XElement("ReturnValues"));
                            // create <GingerCore.Actions.ActReturnValue> node for this <ReturnValues> node 
                            XElement ActReturnValueToBeAdded = new XElement("GingerCore.Actions.ActReturnValue");
                            ActReturnValueToBeAdded.Add(new XAttribute("Active", el_OldValidationUI.Attribute("Active").Value));
                            ActReturnValueToBeAdded.Add(new XAttribute("Param", "Actual"));
                            if(el_OldValidationUI.Attribute("Expected")!=null) ActReturnValueToBeAdded.Add(new XAttribute("mExpected", el_OldValidationUI.Attribute("Expected").Value));
                            ActReturnValueToBeAdded.Add(new XAttribute("Created", el_OldValidationUI.Attribute("Created").Value));
                            ActReturnValueToBeAdded.Add(new XAttribute("CreatedBy", el_OldValidationUI.Attribute("CreatedBy").Value));
                            ActReturnValueToBeAdded.Add(new XAttribute("Guid", "00000000-0000-0000-0000-00000000"));
                            ActReturnValueToBeAdded.Add(new XAttribute("ParentGuid", "00000000-0000-0000-0000-00000000"));
                            ActReturnValueToBeAdded.Add(new XAttribute("LastUpdate", el_OldValidationUI.Attribute("LastUpdate").Value)); 
                            ActReturnValueToBeAdded.Add(new XAttribute("Version", "0"));
                            // now add it to the action 
                            ActGenElementToBeAdded.Element("ReturnValues").Add(ActReturnValueToBeAdded);

                            // ADD THE NEW ACTION TO THE ACTIVITY IF WE WANT IT
                            if (NewValidationType!="") el_AssertsNode.Parent.Element("Acts").Add(ActGenElementToBeAdded);
                            #endregion
                            assertreached = assertreached++;
                        }
                        #endregion
                        #region Step 2. handle this activity's <NextGenCoreWPF.Asserts.ValidationDB> nodes (NOT USED)
                        var qryAssertsDB = el_AssertsNode.Descendants("NextGenCoreWPF.Asserts.ValidationDB");
                        foreach (XElement OldUIValidationElement in qryAssertsDB)
                        {
                            // Not doing anything for DB validations. 
                            // Use Step 1. above as template for that.
                        }
                        #endregion
                    }
                    // ### remove all <ASSERTS> nodes ###
                    qAssertsNodes.Remove();

                    // ### find all Actions & add new V2 attributes ###
                    // have to find multiple types of actions (e.g., NextGenCoreWPF.Actions.ActLink, NextGenCoreWPF.Actions.ActGotoURL) 
                    Regex NodeNameRegex = new Regex(@"NextGenCoreWPF\.Actions\.Act.*");
                    Regex ExcludedNodeNamesRegex = new Regex(@".*Act(Return|Input)Value"); 
                    var qActions =from el in xmlDoc.Descendants() select el;
                    foreach (XElement x in qActions)
                    {
                        if ((NodeNameRegex.Match(x.Name.ToString()).Success) && (!ExcludedNodeNamesRegex.Match(x.Name.ToString()).Success))
                        {
                            if (x.Attribute("Platform") == null) x.Add(new XAttribute("Platform", "Null"));
                            if (x.Attribute("Wait") == null) x.Add(new XAttribute("Wait", "0"));
                            if (x.Attribute("TakeScreenShot") == null) x.Add(new XAttribute("TakeScreenShot", "False"));
                        }
                    }

                    #region HANDLE ActGenElement NODES W VARIABLES
                    var qActGenElementNodesWVariable = from el in xmlDoc.Descendants("NextGenCoreWPF.Actions.ActGenElement")
                                                       where el.Attribute("Varb") != null
                                                       select el;
                    foreach(XElement x in qActGenElementNodesWVariable)
                    {
                        #region ActInputValue
                        XElement ActInputValueToPopulateVariable = new XElement("GingerCore.Actions.ActInputValue");
                        foreach (XAttribute xa in x.Attributes())
                        {
                            List<string> ExcludedAttributes = new List<string> { "Description", "LocateBy", "TakeScreenShot", "GenElementAction", "Varb", "LocateValue", "Platform", "Wait" };
                            if (!ExcludedAttributes.Contains(xa.Name.ToString())) { ActInputValueToPopulateVariable.Add(xa); }
                        }
                        //add new attribs
                        ActInputValueToPopulateVariable.Add(new XAttribute("Param", "Value"));
                        x.Add(new XElement("InputValues"));
                        x.Element("InputValues").Add(ActInputValueToPopulateVariable);
                        #endregion //ActInputValue

                        #region ActReturnValue
                        XElement ActReturnValueToPopulateVariable = new XElement("GingerCore.Actions.ActReturnValue");
                        string sVariableName = x.Attribute("Varb").Value.ToString();
                        foreach (XAttribute xa in x.Attributes()) 
                        {
                            List<string> ExcludedAttributes  = new List<string> { "Description", "LocateBy","TakeScreenShot", "GenElementAction", "Varb", "LocateValue", "Platform", "Wait"};
                            if (!ExcludedAttributes.Contains(xa.Name.ToString())) {     ActReturnValueToPopulateVariable.Add(xa);   }
                        }
                        //add new attribs
                        ActReturnValueToPopulateVariable.Add(new XAttribute("Param", "Actual"));
                        ActReturnValueToPopulateVariable.Add(new XAttribute("StoreToVariable", sVariableName));
                        x.Add(new XElement("ReturnValues"));
                        x.Element("ReturnValues").Add(ActReturnValueToPopulateVariable);
                        #endregion // ActReturnValue
                    }
                    #endregion //HANDLE ActGenElement NODES W VARIABLES


                    #region UPDATE VARIABLES
                    //The syntax of variables has been modified a bit (now they must always have a StringValue attribute).
                    var qVariableNodes = (from e in xmlDoc.Descendants("NextGenCoreWPF.Variables.VariableString") select e);
                    foreach (XElement x in qVariableNodes)
                    {
                        if(x.Attribute("StringValue")==null)
                        {
                            x.Add(new XAttribute("StringValue", x.Attribute("Value").Value.ToString()));
                        }
                    }
                    #endregion

                    //ADD THE PLATFORMS NODE
                    XElement PlatformsNode = new XElement("Platforms");
                    XElement PlatformNode = new XElement("GingerCore.Platforms.Platform");
                        PlatformNode.Add(new XAttribute("Active", "True"));
                        PlatformNode.Add(new XAttribute("Created", "1/1/0001 12:00:00 AM"));
                        PlatformNode.Add(new XAttribute("CreatedBy", "zzz"));
                        PlatformNode.Add(new XAttribute("Guid", "00000000-0000-0000-0000-00000000"));
                        PlatformNode.Add(new XAttribute("ParentGuid", "00000000-0000-0000-0000-00000000"));
                        PlatformNode.Add(new XAttribute("LastUpdate", "1/1/0001 12:00:00 AM"));
                        PlatformNode.Add(new XAttribute("PlatformType", "Web"));
                        PlatformNode.Add(new XAttribute("Version", "0"));
                    PlatformsNode.Add(PlatformNode);
                    xmlDoc.Element("NextGenCoreWPF.BusinessFlow").Add(PlatformsNode);
                    #endregion //case GingerFileType.BusinessFlow:
                    break;
                #endregion
                case eGingerFileType.Environment:
                    break;
                case eGingerFileType.Agent:
                    break;
                case eGingerFileType.RunSet:
                    break;
                case eGingerFileType.Action:
                    break;
                case eGingerFileType.Activity:
                    break;
                case eGingerFileType.Variable:
                    break;
                case eGingerFileType.UserProfile:
                    break;
                case eGingerFileType.Unknown:
                    break;
            }
            #endregion

            //TODO: extract XML attribute programmatically; doing it this way, Linq strips it out and you have to re-add it.
            outputxml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + xmlDoc.ToString();

            // 2ND PASS - UPDATE XML BASED ON FILE TYPE USING STRING FUNCTIONS
            // This builds on whatever was already done above using Linq.           
            switch (RepoType)
            {
                case eGingerFileType.Solution:
                    break;
                case eGingerFileType.BusinessFlow:
                    break;
                case eGingerFileType.Environment:
                    break;
                case eGingerFileType.Agent:
                    break;
                case eGingerFileType.RunSet:
                    break;
                case eGingerFileType.Action:
                    break;
                case eGingerFileType.Activity:
                    break;
                case eGingerFileType.Variable:
                    break;
                case eGingerFileType.UserProfile:
                    break;
                case eGingerFileType.Unknown:
                    break;
            }

            // GLOBAL CHANGES
            if ((outputxml.IndexOf("<NextGen") != -1))
            {
                //SPECIFIC CHANGES (must be done first)
                //agent name must be translated and moved to a different namespace 
                outputxml = outputxml.Replace("<NextGenCoreWPF.Agent", "<GingerCore.Agent");
                //GLOBAL CHANGES (must be done after specific changes)
                outputxml = outputxml.Replace("<NextGenCoreWPF.", "<GingerCore.");
                outputxml = outputxml.Replace("</NextGenCoreWPF.", "</GingerCore.");
                outputxml = outputxml.Replace("<NextGenWPF.", "<Ginger.");
                outputxml = outputxml.Replace("</NextGenWPF.", "</Ginger.");
            }

            // OTHER RENAMED OBJECTS
            outputxml = outputxml.Replace("<Ginger.Agent", "<GingerCore.Agent");
            outputxml = outputxml.Replace("Variables.VariableRandom ", "Variables.VariableRandomNumber ");

            UpdatedXML = outputxml;
        }
    }
}