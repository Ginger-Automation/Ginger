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

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Ginger.Actions.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.ScreenCapture;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.GeneralLib;
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

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSetTextValueTextBox.ValueTextBox, TextBox.TextProperty, Act, nameof(ActSikuli.SetTextValue), BindingMode.TwoWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xShowSikuliCheckBox, CheckBox.IsCheckedProperty, Act, nameof(ActSikuli.ShowSikuliConsole), BindingMode.TwoWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSetSimilarityTextBox.ValueTextBox, TextBox.ToolTipProperty, Act, nameof(ActSikuli.PatternSimilarity), BindingMode.TwoWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSetSimilarityTextBox.ValueTextBox, TextBox.TextProperty, Act, nameof(ActSikuli.PatternSimilarity), BindingMode.TwoWay);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSikuliOperationComboBox, ComboBox.TextProperty, Act, nameof(ActSikuli.ActSikuliOperation), BindingMode.TwoWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xActiveProcessesTitlesComboBox, ComboBox.TextProperty, Act, nameof(ActSikuli.ProcessNameForSikuliOperation), BindingMode.TwoWay);


            xSetTextValueTextBox.BindControl(Context.GetAsContext(actSikuli.Context), actSikuli, nameof(ActSikuli.SetTextValue));
            xSetSimilarityTextBox.BindControl(Context.GetAsContext(actSikuli.Context), actSikuli, nameof(ActSikuli.PatternSimilarity));
            xSetTextValueTextBox.Init(Context.GetAsContext(actSikuli.Context), actSikuli.GetOrCreateInputParam(nameof(actSikuli.SetTextValue),
                (Context.GetAsContext(actSikuli.Context)).BusinessFlow.CurrentActivity.ActivityName), true, false);
            xPatternImageLocationTextBox.Init(Context.GetAsContext(actSikuli.Context), actSikuli.GetOrCreateInputParam(nameof(actSikuli.PatternPath),
                (Context.GetAsContext(actSikuli.Context)).BusinessFlow.CurrentActivity.ActivityName), true, false);
            xSetSimilarityTextBox.Init(Context.GetAsContext(actSikuli.Context), actSikuli.GetOrCreateInputParam(nameof(actSikuli.PatternSimilarity),
                (Context.GetAsContext(actSikuli.Context)).BusinessFlow.CurrentActivity.ActivityName), true, false);

            ChangeAppScreenSizeComboBox.Init(actSikuli.GetOrCreateInputParam(ActSikuli.Fields.ChangeAppWindowSize,
                ActSikuli.eChangeAppWindowSize.None.ToString()), typeof(ActSikuli.eChangeAppWindowSize), false, new SelectionChangedEventHandler(ChangeAppWindowSize_Changed));

            JavaPathTextBox.Init(Context.GetAsContext(actSikuli.Context), actSikuli.GetOrCreateInputParam(nameof(ActSikuli.CustomJavaPath)));


            xPatternImageLocationTextBox.ValueTextBox.TextChanged -= ValueTextBox_TextChanged;
            xPatternImageLocationTextBox.ValueTextBox.TextChanged += ValueTextBox_TextChanged;
            xPatternImageLocationTextBox.ValueTextBox.Text = actSikuli.PatternPath;

            if (!string.IsNullOrEmpty(actSikuli.PatternSimilarity))
            {
                xSetSimilarityTextBox.ValueTextBox.Text = actSikuli.PatternSimilarity.ToString();
            }
            else
            {
                xSetSimilarityTextBox.ValueTextBox.Text = "70";
            }

            ElementImageSourceChanged(true);
            SetJavaRelatedDetails();
            xProcessValueEditor.ShowTextBox(false);
            xProcessValueEditor.Init(Context.GetAsContext(actSikuli.Context), actSikuli.GetOrCreateInputParam(nameof(actSikuli.ProcessNameForSikuliOperation),
               actSikuli.ProcessNameForSikuliOperation), true, false);
            xProcessValueEditor.ValueTextBox.TextChanged -= ProcessValueTextBox_TextChanged;
            xProcessValueEditor.ValueTextBox.TextChanged += ProcessValueTextBox_TextChanged;
        }

        private void SetJavaRelatedDetails()
        {
            JavaPathHomeRdb.Content = new TextBlock { Text = "Use JAVA_HOME Environment Variable (" + CommonLib.GetJavaHome() + ")" };
            if (actSikuli.UseCustomJava)
            {
                JavaPathHomeRdb.IsChecked = false;
                JavaPathOtherRdb.IsChecked = true;
                JavaPathTextBox.ValueTextBox.Text = actSikuli.CustomJavaPath;
            }
            else
            {
                JavaPathHomeRdb.IsChecked = true;
                JavaPathOtherRdb.IsChecked = false;
                JavaPathTextBox.ValueTextBox.Text = string.Empty;
            }
        }

        private void ProcessValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(xProcessValueEditor.ValueTextBox.Text))
            {
                for (int i = 0; i < xActiveProcessesTitlesComboBox.Items.Count; i++)
                {
                    ComboEnumItem item = xActiveProcessesTitlesComboBox.Items[i] as ComboEnumItem;
                    if (item.Value.Equals(actSikuli.ProcessNameForSikuliOperation))
                    {
                        xActiveProcessesTitlesComboBox.SelectedIndex = i;
                        return;
                    }
                }
                ComboEnumItem newItem = new ComboEnumItem()
                {
                    text = actSikuli.ProcessNameForSikuliOperation,
                    Value = actSikuli.ProcessNameForSikuliOperation
                };
                int index = xActiveProcessesTitlesComboBox.Items.Add(newItem);
                xActiveProcessesTitlesComboBox.SelectedIndex = index;
            }
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(xPatternImageLocationTextBox.ValueTextBox.Text)
                && File.Exists(xPatternImageLocationTextBox.ValueTextBox.Text))
            {
                actSikuli.PatternPath = xPatternImageLocationTextBox.ValueTextBox.Text;
                xRefreshPatternImage.DoClick();
            }
        }

        private void CaptureLocatorImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(actSikuli.ProcessNameForSikuliOperation))
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please select valid application instance to proceed with image capture.");
                return;
            }

            actSikuli.PatternPath = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(GetPathToExpectedImage());
            xPatternImageLocationTextBox.ValueTextBox.Text = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(actSikuli.PatternPath);

            App.MainWindow.WindowState = WindowState.Minimized;
            System.Threading.Tasks.Task.Run(() => actSikuli.SetFocusToSelectedApplicationInstance()).ContinueWith((result) =>
            {

                System.Threading.Tasks.Task.Run(() => OpenSnippingTool()).ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        xPatternImageLocationTextBox.Dispatcher.Invoke(ElementImageSourceChanged, false);
                    }
                });
            });
        }

        private string GetPathToExpectedImage()
        {
            string screenImageName = Guid.NewGuid().ToString() + ".JPG";
            string imagePath = @"Documents\SikuliImages\";
            string screenImageDirectory = Path.Combine("~", amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.SolutionFolder,
                                                        imagePath);
            if (!Directory.Exists(screenImageDirectory))
            {
                Directory.CreateDirectory(screenImageDirectory);
            }
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
                xSetTextRow.Visibility = Visibility.Hidden;
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
                fileName = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(fileName);
                actSikuli.PatternPath = fileName;
                xPatternImageLocationTextBox.ValueTextBox.Text = fileName;
                xRefreshPatternImage.DoClick();
            }
        }

        void ElementImageSourceChanged(bool IsFirstCall = false)
        {
            string calculateValue = actSikuli.ValueExpression.Calculate(xPatternImageLocationTextBox.ValueTextBox.Text);
            if (string.IsNullOrEmpty(calculateValue))
            {
                calculateValue = xPatternImageLocationTextBox.ValueTextBox.Text;
            }
            if (!string.IsNullOrEmpty(calculateValue)
                && File.Exists(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(calculateValue)))
            {
                try
                {
                    ScreenShotViewPage screenShotPage = new ScreenShotViewPage(calculateValue, calculateValue, 0.5);
                    xScreenShotsViewFrame.Content = screenShotPage;
                }
                catch (Exception exc)
                {
                    actSikuli.PatternPath = string.Empty;
                    Reporter.ToLog(eLogLevel.ERROR, exc.Message, exc);
                    xScreenShotsViewFrame.Content = null;
                }
            }
            else
            {
                if (!IsFirstCall)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Valid Image file found. Please enter a valid Image path.");
                }
                actSikuli.PatternPath = string.Empty;
                xScreenShotsViewFrame.Content = null;
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
            ElementImageSourceChanged();
        }
        private void ChangeAppWindowSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (actSikuli.ChangeAppWindowSize != ActSikuli.eChangeAppWindowSize.None)
            {
                actSikuli.SetCustomResolution = true;
            }
            else
            {
                actSikuli.SetCustomResolution = false;
            }
        }

        private void JavaPathOtherRdb_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (rb.IsChecked.Value)
            {
                actSikuli.UseCustomJava = true;
                JavaPathTextBox.IsEnabled = true;
                BrowseJavaPath.IsEnabled = true;
            }
            else
            {
                actSikuli.UseCustomJava = false;
                JavaPathTextBox.IsEnabled = false;
                JavaPathTextBox.ValueTextBox.Clear();
                BrowseJavaPath.IsEnabled = false;
            }
        }
        private void BrowseJavaPath_Click(object sender, RoutedEventArgs e)
        {
            string folderName = General.SetupBrowseFolder(new System.Windows.Forms.FolderBrowserDialog(), false);
            if (!string.IsNullOrEmpty(folderName))
            {
                actSikuli.UseCustomJava = true;
                actSikuli.CustomJavaPath = folderName;
                JavaPathTextBox.ValueTextBox.Text = actSikuli.CustomJavaPath;
            }
        }


    }
}
