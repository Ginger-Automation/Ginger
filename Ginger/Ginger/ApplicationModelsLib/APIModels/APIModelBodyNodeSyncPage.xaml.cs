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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Ginger.ApplicationModelsLib.APIModels
{
    public partial class APIModelBodyNodeSyncPage : Page
    {
        GenericWindow _pageGenericWin = null;
        ApplicationAPIUtils.eContentType requestBodyType;
        XmlDocument XMLDoc = null;
        JsonExtended JsonDoc = null;
        List<AppModelParameter> mParamsPendingDelete = new List<AppModelParameter>();
        ApplicationAPIModel mApplicationAPIModel = null;
        List<NodeToDelete> mNodesToDeleteList = new List<NodeToDelete>();
        int RemovedCharsFromRequestBodyCounter = 0;

        public APIModelBodyNodeSyncPage(ApplicationAPIModel applicationAPIModel, List<AppModelParameter> paramsToDelete)
        {
            InitializeComponent();
            mParamsPendingDelete = paramsToDelete;
            mApplicationAPIModel = applicationAPIModel;

            if (APIConfigurationsDocumentParserBase.IsValidXML(mApplicationAPIModel.RequestBody))
            {
                requestBodyType = ApplicationAPIUtils.eContentType.XML;
                XMLDoc = new XmlDocument();
                XMLDoc.LoadXml(mApplicationAPIModel.RequestBody);
            }
            else if (APIConfigurationsDocumentParserBase.IsValidJson(mApplicationAPIModel.RequestBody))
            {
                requestBodyType = ApplicationAPIUtils.eContentType.JSon;
                JsonDoc = new JsonExtended(applicationAPIModel.RequestBody);
            }
            else
                requestBodyType = ApplicationAPIUtils.eContentType.TextPlain;
        }

        private void PrepareNodesPendingForDelete()
        {
            //1. Preparing potential nodes list for deletion
            PrepareNodesListForDeletion();

            //2. Removing Nodes that supposed to remove the same area
            mNodesToDeleteList = mNodesToDeleteList.GroupBy(x => x.ParentOuterXml).Select(group => group.First()).ToList();

            //For Json only - remove spaces and new lines from string
            if (requestBodyType == ApplicationAPIUtils.eContentType.JSon)//For Json - remove spaces
            {
                foreach(NodeToDelete nodeToDelete in mNodesToDeleteList)
                    nodeToDelete.ParentOuterXml = Regex.Replace(nodeToDelete.ParentOuterXml, @"\s+", string.Empty);
            }

            for (int i = 0; i < mNodesToDeleteList.Count; i++)
            {
                NodeToDelete NodeToInspect = mNodesToDeleteList[i];

                //3. For each node remove it if there is another node that overlap it
                List<NodeToDelete> overlappingNodeList = mNodesToDeleteList.Where(x => NodeToInspect.ParentOuterXml.Contains(x.ParentOuterXml) && !NodeToInspect.ParentOuterXml.Equals(x.ParentOuterXml)).ToList();
                foreach (NodeToDelete overlappingNode in overlappingNodeList)
                    mNodesToDeleteList.Remove(overlappingNode);

                //4.Find the actual node string inside the request body and save its text range
                if (requestBodyType == ApplicationAPIUtils.eContentType.XML)
                    FindXMLElementAndSaveItsTextRange(NodeToInspect);
                else if (requestBodyType == ApplicationAPIUtils.eContentType.JSon)
                    FindJSONElementAndSaveItsTextRange(NodeToInspect);
            }

            //5. Sort NodesToDelete List By text ranges in ascending order
            mNodesToDeleteList = mNodesToDeleteList.OrderBy(x => x.stringNodeRange.Item1).ToList(); //Sort Tuples inside NodesToDelete list
            DisplayAndColorTextRanges();
        }

        private void PrepareNodesListForDeletion()
        {
            foreach (AppModelParameter paramToDelete in mParamsPendingDelete)
                if (!string.IsNullOrEmpty(paramToDelete.Path))
                {
                    switch (requestBodyType)
                    {
                        //Try first searching node using Path, if not succeed try search param using placeholder
                        case ApplicationAPIUtils.eContentType.XML:
                            XmlNode xmlNodeByXpath = XMLDocExtended.GetNodeByXpath(XMLDoc, paramToDelete.Path);
                            if (xmlNodeByXpath != null && xmlNodeByXpath.InnerText == paramToDelete.PlaceHolder)
                            {
                                mNodesToDeleteList.Add(new NodeToDelete(xmlNodeByXpath.ParentNode.OuterXml));
                            }
                            else
                            {
                                XDocument xDoc = XDocument.Parse(XMLDoc.OuterXml);
                                var xmlNodeByValue = xDoc.Root.Descendants().Where(a => a.Value == paramToDelete.PlaceHolder).FirstOrDefault();
                                if (xmlNodeByValue != null)
                                    mNodesToDeleteList.Add(new NodeToDelete(Regex.Replace(xmlNodeByValue.Parent.ToString(), @"\s+", string.Empty)));
                            }
                            break;
                        case ApplicationAPIUtils.eContentType.JSon:
                            JToken jNode = JsonDoc.SelectToken(paramToDelete.Path);
                            if (jNode != null && jNode.Value<String>() == paramToDelete.PlaceHolder)
                            {
                                mNodesToDeleteList.Add(new NodeToDelete(jNode.Parent.Parent.ToString()));
                            }
                            else
                            {
                                List<JToken> jNodes = JsonDoc.FindTokens(paramToDelete.PlaceHolder);
                                if (jNodes.Count > 0)
                                    mNodesToDeleteList.Add(new NodeToDelete(jNodes[0].Parent.Parent.ToString()));
                            }
                            break;
                    }
                }
        }

        private void DisplayAndColorTextRanges()
        {
            TextBlockHelper TBH = new TextBlockHelper(xTextBlock);
            int stringIndex = 0;
            int nodeToDeleteIndex = 0;
            while (stringIndex < mApplicationAPIModel.RequestBody.Length-1 && nodeToDeleteIndex < mNodesToDeleteList.Count)
            {
                if (mNodesToDeleteList[nodeToDeleteIndex].stringNodeRange != null)
                {
                    if (stringIndex != mNodesToDeleteList[nodeToDeleteIndex].stringNodeRange.Item1) //No color
                    {
                        TBH.AddText(mApplicationAPIModel.RequestBody.Substring(stringIndex, mNodesToDeleteList[nodeToDeleteIndex].stringNodeRange.Item1 - stringIndex));
                        stringIndex = mNodesToDeleteList[nodeToDeleteIndex].stringNodeRange.Item1;
                    }
                    else //With color
                    {
                        TBH.AddFormattedText(mApplicationAPIModel.RequestBody.Substring(stringIndex, mNodesToDeleteList[nodeToDeleteIndex].stringNodeRange.Item2 - stringIndex), Brushes.Red, true);
                        stringIndex = mNodesToDeleteList[nodeToDeleteIndex].stringNodeRange.Item2 + 1;
                        nodeToDeleteIndex++;
                    }
                }
            }

            if (stringIndex < mApplicationAPIModel.RequestBody.Length - 1)
                TBH.AddText(mApplicationAPIModel.RequestBody.Substring(stringIndex, mApplicationAPIModel.RequestBody.Length - stringIndex));
        }

        private void FindXMLElementAndSaveItsTextRange(NodeToDelete nodeToDelete)
        {
            string[] splitedSearchText = nodeToDelete.ParentOuterXml.Split('>');
            StringBuilder regexString = new StringBuilder();
            for (int i = 0; i < splitedSearchText.Length - 1; i++)
                regexString.Append(splitedSearchText[i] + ">\\s*\\n*");
            string regexStringAfterRemovedEndSpaces = regexString.ToString().Substring(0, regexString.ToString().Length - 6);

            var regex = new Regex(regexStringAfterRemovedEndSpaces);
            Match match = regex.Match(mApplicationAPIModel.RequestBody);

            if (match.Success)
                nodeToDelete.stringNodeRange = new Tuple<int, int>(match.Index, match.Index + match.Length);
            else
                mNodesToDeleteList.Remove(nodeToDelete);
        }

        private void FindJSONElementAndSaveItsTextRange(NodeToDelete nodeToDelete)
        {
            int i = 0;
            StringBuilder regexStringBuilder = new StringBuilder();
            string regexString = Regex.Replace(nodeToDelete.ParentOuterXml, @"\s+", string.Empty);

            string[] splitedSearchText = regexString.Split('{');
            for (i = 1; i < splitedSearchText.Length; i++)
                regexStringBuilder.Append("{\\s*\\n*" + splitedSearchText[i]);

            splitedSearchText = regexStringBuilder.ToString().Split('}');
            regexStringBuilder.Clear();
            for (i = 0; i < splitedSearchText.Length; i++)
                regexStringBuilder.Append(splitedSearchText[i] + "\\s*\\n*}");

            splitedSearchText = regexStringBuilder.ToString().Substring(0, regexStringBuilder.ToString().Length - 7).Split(':');
            regexStringBuilder.Clear();
            for (i = 0; i < splitedSearchText.Length; i++)
                regexStringBuilder.Append(splitedSearchText[i] + ":\\s*\\n*");

            splitedSearchText = regexStringBuilder.ToString().Substring(0, regexStringBuilder.ToString().Length - 7).Split(',');
            regexStringBuilder.Clear();
            for (i = 0; i < splitedSearchText.Length; i++)
                regexStringBuilder.Append(splitedSearchText[i] + ",\\s*\\n*");

            splitedSearchText = regexStringBuilder.ToString().Substring(0, regexStringBuilder.ToString().Length - 7).Split('[');
            regexStringBuilder.Clear();
            for (i = 0; i < splitedSearchText.Length; i++)
                regexStringBuilder.Append(splitedSearchText[i] + "\\s*\\n*\\[\\s*\\n*");

            splitedSearchText = regexStringBuilder.ToString().Substring(0, regexStringBuilder.ToString().Length-14).Split(']');
            regexStringBuilder.Clear();
            for (i = 0; i < splitedSearchText.Length; i++)
                regexStringBuilder.Append(splitedSearchText[i] + "\\s*\\n*\\]\\s*\\n*");

            //string regexFinalString = regexStringBuilder.ToString().Substring(0, regexStringBuilder.ToString().Length - 7) + ',';
            string regexFinalString = regexStringBuilder.ToString().Substring(0, regexStringBuilder.ToString().Length - 14);
            var regex = new Regex(regexFinalString);
            Match match = regex.Match(mApplicationAPIModel.RequestBody);

            if (match.Success)
                nodeToDelete.stringNodeRange = new Tuple<int, int>(match.Index, match.Index + match.Length);
            else
                mNodesToDeleteList.Remove(nodeToDelete);
        }


        private void DeleteOnlyParamsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)xRemoveAssociatedParams.IsChecked)
                AddAssociatedParamsForDeletion();

            DeleteParams();
            _pageGenericWin.Close();
        }

        private void DeleteParamsAndBodyNodesButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)xRemoveAssociatedParams.IsChecked)
                AddAssociatedParamsForDeletion();

            foreach (NodeToDelete xmlNode in mNodesToDeleteList)
            {
                if(xmlNode.stringNodeRange.Item1 - RemovedCharsFromRequestBodyCounter > 0 )
                    mApplicationAPIModel.RequestBody = mApplicationAPIModel.RequestBody.Remove(xmlNode.stringNodeRange.Item1 - RemovedCharsFromRequestBodyCounter, xmlNode.stringNodeRange.Item2 - xmlNode.stringNodeRange.Item1);
                else
                    mApplicationAPIModel.RequestBody = mApplicationAPIModel.RequestBody.Remove(0, xmlNode.stringNodeRange.Item2 - xmlNode.stringNodeRange.Item1);

                RemovedCharsFromRequestBodyCounter += xmlNode.stringNodeRange.Item2 - xmlNode.stringNodeRange.Item1;
            }
            DeleteParams();

            _pageGenericWin.Close();
        }

        private void DeleteParams()
        {
            foreach (AppModelParameter param in mParamsPendingDelete)
                mApplicationAPIModel.AppModelParameters.Remove(param);
        }

        private void AddAssociatedParamsForDeletion()
        {
            foreach (NodeToDelete xmlNode in mNodesToDeleteList)
            {
                List<AppModelParameter> nodeParamsList = mApplicationAPIModel.AppModelParameters.Where(x => xmlNode.ParentOuterXml.Contains(x.PlaceHolder)).ToList();
                foreach(AppModelParameter param in nodeParamsList)
                    if (mParamsPendingDelete.Where(x => x.PlaceHolder == param.PlaceHolder).FirstOrDefault() == null)
                        mParamsPendingDelete.Add(param);
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            if (requestBodyType == ApplicationAPIUtils.eContentType.XML || requestBodyType == ApplicationAPIUtils.eContentType.JSon)
            {
                PrepareNodesPendingForDelete();

                Button btnDeleteParamsAndBodyNodes = new Button();
                btnDeleteParamsAndBodyNodes.Content = "Delete Parameters And Body Nodes";
                btnDeleteParamsAndBodyNodes.Click += new RoutedEventHandler(DeleteParamsAndBodyNodesButton_Click);

                Button btnDeleteOnlyParams = new Button();
                btnDeleteOnlyParams.Content = "Delete Only Parameters";
                btnDeleteOnlyParams.Click += new RoutedEventHandler(DeleteOnlyParamsButton_Click);

                GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { btnDeleteParamsAndBodyNodes, btnDeleteOnlyParams },closeBtnText: "Cancel");
            }
            else
            {                
                Reporter.ToUser(eUserMsgKeys.ParsingError, "Can't parse API Model Request Body, please check it's syntax is valid.");
            }
        }
    }
}
