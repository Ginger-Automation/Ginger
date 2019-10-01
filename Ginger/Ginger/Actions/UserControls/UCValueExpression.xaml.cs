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

using System;
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for UCValueExpression.xaml
    /// </summary>
    public partial class UCValueExpression : UserControl
    {
        private object obj;
        private string AttrName;
        private string fileType;
        private string initialDirectory;
        eBrowserType mBrowserType;

                
        public static DependencyProperty ContextProperty =DependencyProperty.Register("Context", typeof(Context), typeof(UCValueExpression), new PropertyMetadata(ContextChanged));
  

        public Context mContext
        {
            get; set;
        }

        public enum eBrowserType { File, Folder }

        public UCValueExpression()
        {
            InitializeComponent();

            this.DataContextChanged += UCValueExpression_DataContextChanged;
        }

        private void UCValueExpression_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // If the VE is in Grid, we call this function:
            if (e.NewValue != null && e.NewValue.GetType() == typeof(ValueExpression))
            {
                ValueExpression ve = (ValueExpression)e.NewValue;
                Init(mContext, ve.Obj, ve.ObjAttr);
            }
        }
        private static void ContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UCValueExpression ucVE)
            {
                ucVE.mContext = (Context)e.NewValue;
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

        public void Init(Context context, object obj, string AttrName, bool isVENeeded = true, bool isBrowseNeeded = false, eBrowserType browserType = eBrowserType.File, string fileType = "*", RoutedEventHandler extraBrowserSelectionHandler = null)
        {
            this.obj = obj;
            this.AttrName = AttrName;
            mContext = context;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ValueTextBox, TextBox.TextProperty, obj, AttrName);

            if (isBrowseNeeded)
            {
                mBrowserType = browserType;
                LastCol.Width = new GridLength(55);
                BrowseButton.Visibility = Visibility.Visible;
                this.fileType = fileType;

                BrowseButton.AddHandler(Button.ClickEvent, new RoutedEventHandler(BrowseButton_Click));
                if (extraBrowserSelectionHandler != null)
                    BrowseButton.Click += extraBrowserSelectionHandler;
            }

            if (!isVENeeded)
            {
                MidCol.Width = new GridLength(0);
                OpenExpressionEditorButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Initiate and Bind the UCValue Expression Control, manage the Browse and VE buttons and configure the Extra functionality Handler.
        /// </summary>
        /// <param name="AIV">ActInputValue related to the Param configured under the Fields of the Action</param>
        /// <param name="isVENeeded">Determine whether the VE Button will be appeared or not</param>
        /// <param name="isBrowseNeeded">Determine whether the Browse Button will be appeared or not</param>
        /// <param name="browserType">Can be eBrowserType.File or eBrowserType.Folder</param>
        /// <param name="fileType">Type of the files for filter the Browser Dialog</param>
        /// <param name="extraBrowserSelectionHandler">To be used whenever extra functionality is needed after clicking OK or cancel at the Dialog window</param>
        public void Init(Context context, ActInputValue AIV, bool isVENeeded = true, bool isBrowseNeeded = false, eBrowserType browserType = eBrowserType.File, string fileType = "*", RoutedEventHandler extraBrowserSelectionHandler= null, string initialDirectory=null)
        {
            // If the VE is on stand alone form:
            this.obj = AIV;
            this.AttrName = nameof(ActInputValue.Value);
            mContext = context;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ValueTextBox, TextBox.TextProperty, obj, AttrName);

            if (isBrowseNeeded)
            {
                mBrowserType = browserType;
                LastCol.Width = new GridLength(55);
                BrowseButton.Visibility = Visibility.Visible;
                this.fileType = fileType;
                this.initialDirectory = initialDirectory;

                BrowseButton.AddHandler(Button.ClickEvent, new RoutedEventHandler(BrowseButton_Click));

                if (extraBrowserSelectionHandler != null)
                    BrowseButton.Click += extraBrowserSelectionHandler;
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
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.Filter = upperFileType + " Files (*." + fileType + ")|*." + fileType + "|All Files (*.*)|*.*";
                    dlg.FilterIndex = 1;
                    if (string.IsNullOrEmpty(initialDirectory)==false)
                    {
                        String filePath = System.IO.Path.Combine(initialDirectory, @"Documents\SQL");
                        if (!System.IO.Directory.Exists(filePath))
                        {
                            System.IO.Directory.CreateDirectory(filePath);
                        }
                        dlg.InitialDirectory = filePath;
                    }
                    System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        string FileName = General.ConvertSolutionRelativePath(dlg.FileName);
                        ValueTextBox.Text = FileName;
                    }
                    break;
                case eBrowserType.Folder:
                    var dlgf = new System.Windows.Forms.FolderBrowserDialog();
                    dlgf.Description = "Select folder";
                    dlgf.RootFolder = Environment.SpecialFolder.MyComputer;
                    dlgf.ShowNewFolderButton = true;
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

        public bool IsReadOnly
        {
            get { return ValueTextBox.IsReadOnly; }
            set { ValueTextBox.IsReadOnly = value; }
        }

    }
}