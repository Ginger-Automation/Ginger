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

        public UCHtmlViewer()
        {
            InitializeComponent();
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

        private void BindHTMLDocument()
        {
            if (_htmldocument == null)
            {
                htmlTree.ItemsSource = null;
                return;
            }

            IEnumerable<HtmlNode> htmlElements = _htmldocument.DocumentNode.Descendants().Where(x => !x.Name.StartsWith("#"));

            if (htmlElements.Count() != 0)
            {
                TreeViewItem TVRoot = new TreeViewItem();
                //TVRoot.Tag = _htmldocument.DocumentNode;
                //TVRoot.Name = "Document";
                //TVRoot.Header = "<" + _htmldocument.DocumentNode.Name + ">";

                //TreeViewItem childItem;

                try
                {
                    //childItem = new TreeViewItem();
                    //TVRoot.Items.Add(childItem);
                    bind(_htmldocument.DocumentNode, TVRoot);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception while generating HTML Source Doc Tree", ex);
                }

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
