#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GingerCore.UserControls
{
    /// <summary>
    /// Interaction logic for UCHtmlViewer.xaml
    /// </summary>
    public partial class UCHtmlViewer : UserControl
    {
        private HtmlDocument _htmldocument;

        public TreeView HTMLTree
        {
            get
            {
                return htmlTree;
            }
        }

        public UCHtmlViewer()
        {
            InitializeComponent();
        }

        public HtmlDocument htmlDocument
        {
            get { return _htmldocument; }
            set
            {
                if (value != _htmldocument)
                {
                    _htmldocument = value;
                    ClearTreeItems();
                    BindHTMLDocument();
                }
            }
        }

        public void ClearTreeItems()
        {
            htmlTree.ItemsSource = null;
            htmlTree.Items.Clear();
        }

        public List<string> ElementsToSkip = new List<string>() { "script", "noscript", "head" };

        private async void BindHTMLDocument()
        {
            if (_htmldocument == null)
            {
                ClearTreeItems();
                return;
            }

            IEnumerable<HtmlNode> htmlElements = _htmldocument.DocumentNode.Descendants().Where(x => !x.Name.StartsWith("#") && x.NodeType == HtmlNodeType.Element && !ElementsToSkip.Contains(x.Name));

            if (htmlElements.Count() != 0)
            {
                TreeViewItem TVRoot = new TreeViewItem() { IsExpanded = true };
                //TVRoot.Tag = _htmldocument.DocumentNode;
                //TVRoot.Name = "Document";
                //TVRoot.Header = "<" + _htmldocument.DocumentNode.Name + ">";

                //TreeViewItem childItem;

                try
                {
                    //childItem = new TreeViewItem();
                    //TVRoot.Items.Add(childItem);
                    await BindTree(_htmldocument.DocumentNode, TVRoot);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception while generating HTML Source Doc Tree", ex);
                }

                htmlTree.Items.Add(TVRoot);
                htmlTree.Visibility = System.Windows.Visibility.Visible;
                //}

                //Binding binding = new Binding();
                //binding.Source = _htmldocument.DocumentNode;    // htmlElements;
                ////binding.XPath = "child::node()";
                //htmlTree.SetBinding(TreeView.ItemsSourceProperty, binding);
            }
        }

        public async Task BindTree(HtmlNode htmlN, TreeViewItem treeN)
        {
            StringBuilder result = new StringBuilder();
            switch (htmlN.NodeType)
            {
                case HtmlNodeType.Document:
                    result.Append(htmlDocument.DocumentNode.Name);
                    break;
                case HtmlNodeType.Element:
                    result.Append('<').Append(htmlN.Name).Append(' ');
                    foreach (HtmlAttribute attr in htmlN.Attributes)
                    {
                        result.Append(string.Format(" {0}='{1}' ", attr.Name, attr.Value));
                    }
                    result.Append('>');
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
                if (!ElementsToSkip.Contains(node.Name) &&
                    (node.NodeType == HtmlNodeType.Element || node.InnerText.Trim().Length > 0))
                {
                    childTN = new TreeViewItem() { IsExpanded = false };
                    treeN.Items.Add(childTN);
                    await BindTree(node, childTN);
                }
            }
        }
    }
}
