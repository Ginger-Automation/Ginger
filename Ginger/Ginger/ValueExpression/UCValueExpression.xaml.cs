#region License
/*
Copyright © 2014-2025 European Support Limited

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
using GingerCore;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.BusinessFlowWindows
{
    /// <summary>
    /// Interaction logic for UCValueExpression.xaml
    /// </summary>
    public partial class UCValueExpression : UserControl
    {
        private object obj;
        private string AttrName;
        private string fileType;
        eBrowserType mBrowserType;
        Context mContext;

        public enum eBrowserType { File, Folder }

        public UCValueExpression()
        {
            InitializeComponent();
            this.DataContextChanged += UCValueExpression_DataContextChanged;
        }

        private void UCValueExpression_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If the VE is in Grid, we call this function:
            if (e.NewValue.GetType() == typeof(ValueExpression))
            {
                ValueExpression ve = (ValueExpression)e.NewValue;
                Init(mContext, ve.Obj, ve.ObjAttr);
            }
        }

        public void Init(Context context, object obj, string AttrName)
        {
            // If the VE is on stand alone form:
            this.obj = obj;
            this.AttrName = AttrName;
            mContext = context;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ValueTextBox, TextBox.TextProperty, obj, AttrName);
        }
        public void SetBrowserBtn(eBrowserType browserType = eBrowserType.File, string fileType = "*", RoutedEventHandler extraBrowserSelectionHandler = null)
        {
            mBrowserType = browserType;
            LastCol.Width = new GridLength(55);
            BrowseButton.Visibility = Visibility.Visible;
            this.fileType = fileType;

            BrowseButton.AddHandler(Button.ClickEvent, new RoutedEventHandler(BrowseButton_Click));

            if (extraBrowserSelectionHandler != null)
            {
                BrowseButton.Click += extraBrowserSelectionHandler;
            }

        }
        public void Init(Context context, object obj, string AttrName, bool isVENeeded = true, bool isBrowseNeeded = false, eBrowserType browserType = eBrowserType.File, string fileType = "*", RoutedEventHandler extraBrowserSelectionHandler = null)
        {
            this.obj = obj;
            this.AttrName = AttrName;
            mContext = context;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ValueTextBox, TextBox.TextProperty, obj, AttrName);

            if (isBrowseNeeded)
            {
                SetBrowserBtn(browserType, fileType, extraBrowserSelectionHandler);
            }

            if (!isVENeeded)
            {
                MidCol.Width = new GridLength(0);
                OpenExpressionEditorButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenExpressionEditorButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(obj, AttrName, mContext);
            w.ShowAsWindow();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            switch (mBrowserType)
            {
                case eBrowserType.File:
                    string upperFileType = fileType.ToUpper();
                    if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
                    {
                        Filter = upperFileType + " Files (*." + fileType + ")|*." + fileType + "|All Files (*.*)|*.*",
                        FilterIndex = 1
                    }) is string fileName)
                    {
                        ValueTextBox.Text = fileName;
                    }
                    break;
                case eBrowserType.Folder:
                    
                    string initialDirectory = ValueTextBox.Text;
                    if (ValueTextBox.Text != null && ValueTextBox.Text.StartsWith(@"~\"))
                    {
                        string solutionFolder = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.Folder;
                        initialDirectory = ValueTextBox.Text.Replace(@"~\", solutionFolder, StringComparison.InvariantCultureIgnoreCase);
                    }

                    var dlgf = new System.Windows.Forms.FolderBrowserDialog
                    {
                        Description = "Select folder",
                        RootFolder = Environment.SpecialFolder.MyComputer,
                        ShowNewFolderButton = true,
                        InitialDirectory = initialDirectory
                    };

                    System.Windows.Forms.DialogResult resultf = dlgf.ShowDialog();
                    if (resultf == System.Windows.Forms.DialogResult.OK)
                    {
                        ValueTextBox.Text = dlgf.SelectedPath;
                    }
                    break;
            }
        }

        public void AdjustHight(int hight)
        {
            Row.Height = new GridLength(hight);
        }

        public void HideBrowserBTN()
        {
            BrowseButton.Visibility = Visibility.Collapsed;
        }

        public bool IsReadOnly
        {
            get { return ValueTextBox.IsReadOnly; }
            set { ValueTextBox.IsReadOnly = value; }
        }
    }
}
