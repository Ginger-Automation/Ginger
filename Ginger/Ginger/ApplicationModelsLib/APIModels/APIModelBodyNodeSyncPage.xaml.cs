using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Repository;
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
        List<NodesToDelete> NodesToDeleteList = new List<NodesToDelete>();
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
            foreach (AppModelParameter paramToDelete in mParamsPendingDelete)
                if (!string.IsNullOrEmpty(paramToDelete.Path))
                {
                    switch (requestBodyType)
                    {
                        case ApplicationAPIUtils.eContentType.XML:
                            //Try first searching node using Path, if not succeed try search param using placeholder
                            XmlNode xmlNodeByXpath = XMLDocExtended.GetNodeByXpath(XMLDoc, paramToDelete.Path);
                            if (xmlNodeByXpath != null && xmlNodeByXpath.InnerText == paramToDelete.PlaceHolder)
                            {
                                NodesToDeleteList.Add(new NodesToDelete(xmlNodeByXpath.ParentNode.OuterXml));
                            }
                            else
                            {
                                XDocument xDoc = XDocument.Parse(XMLDoc.OuterXml);
                                var xmlNodeByValue = xDoc.Root.Descendants().Where(a => a.Value == paramToDelete.PlaceHolder).FirstOrDefault();
                                if(xmlNodeByValue != null)
                                    NodesToDeleteList.Add(new NodesToDelete(Regex.Replace(xmlNodeByValue.Parent.ToString(), @"\s+", string.Empty)));
                            }
                            break;
                        case ApplicationAPIUtils.eContentType.JSon:
                            JToken jNode = JsonDoc.SelectToken(paramToDelete.Path);
                            if (jNode != null && jNode.Value<String>() == paramToDelete.PlaceHolder)
                            {
                                NodesToDeleteList.Add(new NodesToDelete(jNode.Parent.Parent.ToString()));
                            }
                            else
                            {
                                List<JToken> jNodes = JsonDoc.FindTokens(paramToDelete.PlaceHolder);
                                if (jNodes.Count > 0)
                                    NodesToDeleteList.Add(new NodesToDelete(jNodes[0].Parent.Parent.ToString()));
                            }
                            break;
                    }
                }

            //2. Removing Nodes that supposed to remove the same area
            NodesToDeleteList = NodesToDeleteList.GroupBy(x => x.ParentOuterXml).Select(group => group.First()).ToList();

            //For Json only - remove spaces and new lines from string
            if (requestBodyType == ApplicationAPIUtils.eContentType.JSon)//For Json - remove spaces
            {
                foreach(NodesToDelete nodeToDelete in NodesToDeleteList)
                    nodeToDelete.ParentOuterXml = Regex.Replace(nodeToDelete.ParentOuterXml, @"\s+", string.Empty);
            }

            for (int i = 0; i < NodesToDeleteList.Count; i++)
            {
                NodesToDelete NodeToInspect = NodesToDeleteList[i];

                //3. For each node remove it id there is another node that overlap it
                List<NodesToDelete> overlappingNodeList = NodesToDeleteList.Where(x => NodeToInspect.ParentOuterXml.Contains(x.ParentOuterXml) && !NodeToInspect.ParentOuterXml.Equals(x.ParentOuterXml)).ToList();
                foreach (NodesToDelete overlappingNode in overlappingNodeList)
                    NodesToDeleteList.Remove(overlappingNode);

                //4.Find the actual node string inside the request body and save its text range
                if (requestBodyType == ApplicationAPIUtils.eContentType.XML)
                    FindXMLElementAndSaveItsTextRange(NodeToInspect);
                else if (requestBodyType == ApplicationAPIUtils.eContentType.JSon)
                    FindJSONElementAndSaveItsTextRange(NodeToInspect);
            }

            //5. Sort NodesToDelete List By text ranges in ascending order
            NodesToDeleteList = NodesToDeleteList.OrderBy(x => x.stringNodeRange.Item1).ToList(); //Sort Tuples inside NodesToDelete list
            DisplayAndColorTextRanges();
        }

        private void DisplayAndColorTextRanges()
        {
            TextBlockHelper TBH = new TextBlockHelper(xTextBlock);
            int stringIndex = 0;
            int nodeToDeleteIndex = 0;
            while (stringIndex < mApplicationAPIModel.RequestBody.Length-1 && nodeToDeleteIndex < NodesToDeleteList.Count)
            {
                if (NodesToDeleteList[nodeToDeleteIndex].stringNodeRange != null)
                {
                    if (stringIndex != NodesToDeleteList[nodeToDeleteIndex].stringNodeRange.Item1) //No color
                    {
                        TBH.AddText(mApplicationAPIModel.RequestBody.Substring(stringIndex, NodesToDeleteList[nodeToDeleteIndex].stringNodeRange.Item1 - stringIndex));
                        stringIndex = NodesToDeleteList[nodeToDeleteIndex].stringNodeRange.Item1;
                    }
                    else //With color
                    {
                        TBH.AddFormattedText(mApplicationAPIModel.RequestBody.Substring(stringIndex, NodesToDeleteList[nodeToDeleteIndex].stringNodeRange.Item2 - stringIndex), Brushes.Red, true);
                        stringIndex = NodesToDeleteList[nodeToDeleteIndex].stringNodeRange.Item2 + 1;
                        nodeToDeleteIndex++;
                    }
                }
            }

            if (stringIndex < mApplicationAPIModel.RequestBody.Length - 1)
                TBH.AddText(mApplicationAPIModel.RequestBody.Substring(stringIndex, mApplicationAPIModel.RequestBody.Length - stringIndex));
        }

        private void FindXMLElementAndSaveItsTextRange(NodesToDelete nodeToDelete)
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
                NodesToDeleteList.Remove(nodeToDelete);
        }

        private void FindJSONElementAndSaveItsTextRange(NodesToDelete nodeToDelete)
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
                NodesToDeleteList.Remove(nodeToDelete);
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

            foreach (NodesToDelete xmlNode in NodesToDeleteList)
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
            bool paramsBeenAdded = false;
            foreach (NodesToDelete xmlNode in NodesToDeleteList)
            {
                Regex regex = null;
                if (requestBodyType == ApplicationAPIUtils.eContentType.XML)
                    regex = new Regex(@"{(.*?)\}");
                else if(requestBodyType == ApplicationAPIUtils.eContentType.JSon)
                    regex = new Regex(@"<(.*?)\>");

                foreach (Match match in regex.Matches(xmlNode.ParentOuterXml))
                {
                    if (mParamsPendingDelete.Where(x => x.PlaceHolder == match.Value).FirstOrDefault() == null)
                    {
                        mParamsPendingDelete.Add(mApplicationAPIModel.AppModelParameters.Where(x => x.PlaceHolder == match.Value).FirstOrDefault());
                        paramsBeenAdded = true;
                    }
                }
            }

            if (paramsBeenAdded)
            {
                NodesToDeleteList.Clear();
                PrepareNodesPendingForDelete();
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            if (requestBodyType == ApplicationAPIUtils.eContentType.XML || requestBodyType == ApplicationAPIUtils.eContentType.JSon)
            {
                PrepareNodesPendingForDelete();

                //Scroll to first
                //if (xmlNodesRangesPendingDelete.Count > 0)
                //    xTextBlockScrollViewer.ScrollToVerticalOffset(xmlNodesRangesPendingDelete[0].Item1);

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
                System.Windows.MessageBox.Show("Can't parse API Model Request Body, please check it's syntax is valid.", "Error while parsing request body", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error, System.Windows.MessageBoxResult.OK);
            }
        }

        private class NodesToDelete
        {
            public string ParentOuterXml;
            //public JToken JsonNode;
            public Tuple<int, int> stringNodeRange;

            public NodesToDelete(string parentOuterXml)
            {
                ParentOuterXml = parentOuterXml;
            }
        }






































        //--------------------DELETE Those methods---------------------
        //private void RemoveOverlappingAreas()
        //{
        //    for (int i = 0; i < rangesToColorList.Count; i++)
        //    {
        //        Tuple<int, int> overlappingTupple = rangesToColorList.Where(x => (rangesToColorList[i].Item1 >= x.Item1 && rangesToColorList[i].Item2 < x.Item2) || (rangesToColorList[i].Item1 > x.Item1 && rangesToColorList[i].Item2 <= x.Item2)).FirstOrDefault();
        //        if (overlappingTupple != null)
        //        {
        //            rangesToColorList.Remove(overlappingTupple);
        //            i--;
        //            continue;
        //        }

        //        overlappingTupple = rangesToColorList.Where(x => (rangesToColorList[i].Item1 <= x.Item1 && rangesToColorList[i].Item2 < x.Item2) || (rangesToColorList[i].Item1 < x.Item1 && rangesToColorList[i].Item2 <= x.Item2)).FirstOrDefault();
        //        if (overlappingTupple != null)
        //        {
        //            rangesToColorList.Add(new Tuple<int, int>(overlappingTupple.Item1, rangesToColorList[i].Item2));
        //            rangesToColorList.Remove(overlappingTupple);
        //            rangesToColorList.Remove(rangesToColorList[i]);
        //            i--;
        //            continue;
        //        }

        //        overlappingTupple = rangesToColorList.Where(x => ((rangesToColorList[i].Item1 >= x.Item1 && rangesToColorList[i].Item2 > x.Item2)) || ((rangesToColorList[i].Item1 > x.Item1 && rangesToColorList[i].Item2 >= x.Item2))).FirstOrDefault();
        //        if (overlappingTupple != null)
        //        {
        //            rangesToColorList.Add(new Tuple<int, int>(rangesToColorList[i].Item1, overlappingTupple.Item2));
        //            rangesToColorList.Remove(overlappingTupple);
        //            rangesToColorList.Remove(rangesToColorList[i]);
        //            i--;
        //            continue;
        //        }
        //    }
        //}

        //private TextPointer GetTextPositionAtOffset(TextPointer position, int characterCount)
        //{
        //    while (position != null)
        //    {
        //        if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
        //        {
        //            int count = position.GetTextRunLength(LogicalDirection.Forward);
        //            if (characterCount <= count)
        //            {
        //                return position.GetPositionAtOffset(characterCount);
        //            }

        //            characterCount -= count;
        //        }

        //        TextPointer nextContextPosition = position.GetNextContextPosition(LogicalDirection.Forward);
        //        if (nextContextPosition == null)
        //            return position;

        //        position = nextContextPosition;
        //    }

        //    return position;
        //}

        //private void ColorNodesPendingForDelete2()
        //{
        //    xBodyRequestTextBlock.Selection.Text = Regex.Replace(mApplicationAPIModel.RequestBody, @"\s+", string.Empty);
        //    TextRange fullText = new TextRange(xBodyRequestTextBlock.Document.ContentStart, xBodyRequestTextBlock.Document.ContentEnd);

        //    foreach (AppModelParameter ParamNodePendingForDelete in mDeletedParams)
        //    {
        //        XmlNode node = XMLDocExtended.GetNodeByXpath(XMLDoc, ParamNodePendingForDelete.Path);
        //        int indexOfParentStart = xBodyRequestTextBlock.Selection.Text.IndexOf(node.ParentNode.OuterXml);

        //        TextPointer start = fullText.Start.GetPositionAtOffset(indexOfParentStart);
        //        TextPointer end = fullText.Start.GetPositionAtOffset(indexOfParentStart + node.ParentNode.OuterXml.Length);

        //        TextRange rangeToColor = new TextRange(start, end);
        //        rangeToColor.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Red));
        //    }
        //}
    }
}
