#region License
/*
Copyright © 2014-2021 European Support Limited

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
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;

namespace GingerCore.UserControls
{
    /// <summary>
    /// Interaction logic for XMLViewer.xaml
    /// </summary>

    //TODO: this is duplicate class with NextGenWPF - create new project Ginger Control and move it 
    public partial class XMLViewer : UserControl
    {
        private XmlDocument _xmldocument;
        private HtmlDocument _htmldocument;

        public XMLViewer()
        {
            InitializeComponent();
        }

        public XmlDocument xmlDocument
        {
            get { return _xmldocument; }
            set
            {
                _xmldocument = value;
                BindXMLDocument();
            }
        }

        public HtmlDocument htmlDocument
        {
            get { return _htmldocument; }
            set
            {
                _htmldocument = value;
                BindHTMLDocument();
            }
        }

        private void BindXMLDocument()
        {
            if (_xmldocument == null)
            {
                xmlTree.ItemsSource = null;
                return;
            }
 
            XmlDataProvider provider = new XmlDataProvider();
            provider.Document = _xmldocument;
            Binding binding = new Binding();
            binding.Source = provider;
            binding.XPath = "child::node()";
            xmlTree.SetBinding(TreeView.ItemsSourceProperty, binding);
        }

        private void BindHTMLDocument()
        {
            if (_htmldocument == null)
            {
                xmlTree.ItemsSource = null;
                return;
            }

            IEnumerable<HtmlNode> htmlElements = _htmldocument.DocumentNode.Descendants().Where(x => !x.Name.StartsWith("#"));

            if (htmlElements.Count() != 0)
            {
                TreeViewItem TVRoot = new TreeViewItem();
                TVRoot.Tag = _htmldocument.DocumentNode;
                TVRoot.Name = "Document";
                TVRoot.Header = "<" + _htmldocument.DocumentNode.Name + ">";

                TreeViewItem childItem;

                //foreach (HtmlNode htmlElemNode in htmlElements)
                //{
                    try
                    {
                        childItem = new TreeViewItem();
                        TVRoot.Items.Add(childItem);
                        bind(_htmldocument.DocumentNode, childItem);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Exception while generating HTML Source Doc Tree", ex);
                    }
                //}

                htmlTree.Items.Add(TVRoot);
                htmlTree.Visibility = System.Windows.Visibility.Visible;
            }

            //Binding binding = new Binding();
            //binding.Source = htmlElements;
            //binding.XPath = "child::node()";
            //xmlTree.SetBinding(TreeView.ItemsSourceProperty, binding);
        }

        public void bind(HtmlNode htmlN, TreeViewItem treeN)
        {
            StringBuilder result = new StringBuilder();
            switch (htmlN.NodeType)
            {
                case HtmlNodeType.Comment:
                    result.Append(htmlN.InnerText);
                    break;
                case HtmlNodeType.Document:
                    result.Append("root");
                    break;
                case HtmlNodeType.Element:
                    result.Append('<').Append(htmlN.Name).Append('>');
                    break;
                case HtmlNodeType.Text:
                    result.Append(htmlN.InnerText);
                    break;
                default:
                    result.Append("undefined element");
                    break;
            }

            treeN.Header = result.ToString();
            //treeN.Name = result.ToString();
            treeN.Tag = htmlN;

            TreeViewItem childTN;

            foreach (HtmlNode node in htmlN.ChildNodes)
            {
                if (node.NodeType == HtmlNodeType.Element || node.InnerText.Trim().Length > 0)
                {
                    childTN = new TreeViewItem();
                    treeN.Items.Add(childTN);
                    bind(node, childTN);
                }
            }
        }
    }
}
