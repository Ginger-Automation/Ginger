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

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Amdocs.Ginger.Common;
using GingerCore.Actions;
using GingerCore.Actions.ScreenCapture;
using ScreenSnipApplication;

namespace Ginger.Actions
{
    public partial class ActSikuliEditPage : Page
    {
        private ActSikuli actSikuli;

        public ActSikuliEditPage(ActSikuli Act)
        {
            InitializeComponent();

            this.actSikuli = Act;
            RefreshProcessesCombo();
            GingerCore.General.FillComboFromEnumObj(xSikuliOperationComboBox, Act.ActSikuliOperation);

            //TODO: fix hard coded ButtonAction use Fields: Fixed
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xPatternImageLocationTextBox.ValueTextBox, TextBox.TextProperty, Act, nameof(ActSikuli.PatternPath), BindingMode.TwoWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSetTextValueTextBox.ValueTextBox, TextBox.TextProperty, Act, nameof(ActSikuli.SetTextValue), BindingMode.TwoWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xShowSikuliCheckBox, CheckBox.IsCheckedProperty, Act, nameof(ActSikuli.ShowSikuliConsole), BindingMode.TwoWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xPatternImageLocationTextBox.ValueTextBox, TextBox.ToolTipProperty, Act, nameof(ActSikuli.PatternPath), BindingMode.TwoWay);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSikuliOperationComboBox, ComboBox.TextProperty, Act, nameof(ActSikuli.ActSikuliOperation), BindingMode.TwoWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xActiveProcessesTitlesComboBox, ComboBox.TextProperty, Act, nameof(ActSikuli.ProcessNameForSikuliOperation), BindingMode.TwoWay);

            xPatternImageLocationTextBox.ValueTextBox.TextChanged -= ValueTextBox_TextChanged;
            xPatternImageLocationTextBox.ValueTextBox.TextChanged += ValueTextBox_TextChanged;
            xPatternImageLocationTextBox.ValueTextBox.Text = actSikuli.PatternPath;
            ElementImageSourceChanged();
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(xPatternImageLocationTextBox.ValueTextBox.Text)
                && File.Exists(xPatternImageLocationTextBox.ValueTextBox.Text))
            {
                xRefreshPatternImage.DoClick();
            }
        }

        private void CaptureLocatorImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (actSikuli.ProcessIDForSikuliOperation == -1)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please select valid application instance to proceed with image capture.");
                return;
            }

            actSikuli.PatternPath = GetPathToExpectedImage();
            xPatternImageLocationTextBox.ValueTextBox.Text = GetPathToExpectedImage();

            App.MainWindow.WindowState = WindowState.Minimized;

            System.Threading.Tasks.Task.Run(() => OpenSnippingTool()).ContinueWith(t =>
            {
                if (t.Result)
                {
                    ElementImageSourceChanged();
                }
            }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
            actSikuli.SetFocusToSelectedApplicationInstance();
        }

        private string GetPathToExpectedImage()
        {
            string screenImageName = Guid.NewGuid().ToString() + ".JPG";
            string imagePath = @"Documents\SikuliImages\";
            string screenImageDirectory = Path.Combine("~", amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.SolutionFolder,
                                                        imagePath);
            return Path.Combine(screenImageDirectory, screenImageName);
        }

        private bool OpenSnippingTool()
        {
            System.Threading.Thread.Sleep(300);
            return SnippingTool.Snip(actSikuli.PatternPath);
        }

        private void xSikuliOperationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xSikuliOperationComboBox.SelectedItem != null && xSikuliOperationComboBox.SelectedValue.ToString() == ActSikuli.eActSikuliOperation.SetValue.ToString())
            {
                xSetTextRow.Visibility = Visibility.Visible;
            }
            else
            {
                xSetTextRow.Visibility = Visibility.Collapsed;
            }
        }

        private void xBrowsePatternButton_Click(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = "*.jpg or .jpeg or .png",
                Filter = "Image Files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png"
            }, false) is string fileName)
            {
                actSikuli.PatternPath = fileName;
                xPatternImageLocationTextBox.ValueTextBox.Text = fileName;
                xRefreshPatternImage.DoClick();
            }
        }

        void ElementImageSourceChanged()
        {
            if (!string.IsNullOrEmpty(xPatternImageLocationTextBox.ValueTextBox.Text)
                && File.Exists(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(xPatternImageLocationTextBox.ValueTextBox.Text)))
            {
                try
                {
                    var imgSrc = File.ReadAllBytes(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(xPatternImageLocationTextBox.ValueTextBox.Text));
                    var bitmapSource = new BitmapImage();
                    bitmapSource.BeginInit();
                    bitmapSource.StreamSource = new MemoryStream(imgSrc);
                    bitmapSource.EndInit();

                    xElementImage.Source = bitmapSource;
                    xElementImageCanvas.Visibility = Visibility.Visible;
                }
                catch (Exception exc)
                {
                    Reporter.ToLog(eLogLevel.ERROR, exc.Message, exc);
                    xElementImageCanvas.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                xElementImageCanvas.Visibility = Visibility.Collapsed;
            }
        }

        private void xRefreshActiveWindows_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcessesCombo();
        }

        void RefreshProcessesCombo()
        {
            GingerCore.General.FillComboFromList(xActiveProcessesTitlesComboBox, actSikuli.ActiveProcessWindows);
        }

        private void xRefreshPatternImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(xPatternImageLocationTextBox.ValueTextBox.Text)
                || !File.Exists(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(xPatternImageLocationTextBox.ValueTextBox.Text)))
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Valid Image file found. Please enter a valid Image path.");
                return;
            }

            ElementImageSourceChanged();
        }
    }
}
