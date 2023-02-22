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

        public TreeView XMLTree
        {
            get
            {
                return xmlTree;
            }
        }

        public XMLViewer()
        {
            InitializeComponent();
        }

        public XmlDocument xmlDocument
        {
            get { return _xmldocument; }
            set
            {
                if (_xmldocument != value)
                {
                    _xmldocument = value;
                    ClearTreeItems();
                    BindXMLDocument();
                }
            }
        }

        public void ClearTreeItems()
        {
            xmlTree.ItemsSource = null;
        }

        private void BindXMLDocument()
        {
            if (_xmldocument == null)
            {
                ClearTreeItems();
                return;
            }

            XmlDataProvider provider = new XmlDataProvider();
            provider.Document = _xmldocument;
            Binding binding = new Binding();
            binding.Source = provider;
            binding.XPath = "child::node()";
            xmlTree.SetBinding(TreeView.ItemsSourceProperty, binding);
        }

    }
}
