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

using SkiaSharp.Views.Desktop;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Reports;
using GingerCore.ALM;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.GeneralFunctions;
using GingerCore.GeneralLib;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using static Amdocs.Ginger.Common.GeneralLib.General;

namespace GingerCore
{
    public class General
    {
        public static void LoadGenericWindow(ref GenericWindow genWindow, System.Windows.Window owner, eWindowShowStyle windowStyle, string windowTitle, Page windowPage,
                                            ObservableList<Button> windowBtnsList = null, bool showClosebtn = true, string closeBtnText = "Close", RoutedEventHandler closeEventHandler = null, bool startupLocationWithOffset = false, UserControl loaderElement = null)
        {
            genWindow = null;
            eWindowShowStyle winStyle;
            do
            {
                if (genWindow != null)
                {
                    winStyle = genWindow.ReShowStyle;
                    genWindow.BottomPanel.Children.Clear();
                    genWindow = null;
                }
                else
                {
                    winStyle = windowStyle;
                }
                genWindow = new GenericWindow(owner, winStyle, windowTitle, windowPage, windowBtnsList, showClosebtn, closeBtnText, closeEventHandler, loaderElement);
                genWindow.Title = windowPage.Title;
                if (startupLocationWithOffset)
                {
                    genWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    genWindow.Left = 50;
                    genWindow.Top = 200;
                }
                if (winStyle == eWindowShowStyle.Dialog || winStyle == eWindowShowStyle.OnlyDialog)
                    genWindow.ShowDialog();
                else
                    genWindow.Show();
            }
            while (genWindow.NeedToReShow);
        }

        public static bool IsListContentSame<T>(List<T> a, List<T> b)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in a)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in b)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        private static object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }



        #region ENUM
        /// <summary>
        /// 
        /// </summary>
        /// <param name="comboBox"></param>
        /// <param name="EnumObj"></param>
        /// <param name="values"> leave values empty will take all possible vals, or pass a list to limit selection </param>
        public static void FillComboFromEnumObj(ComboBox comboBox, Object EnumObj, List<object> values = null, bool sortValues = true, ListCollectionView valuesCollView = null, List<dynamic> excludeList = null)
        {
            comboBox.SelectedValuePath = "Value";
            Type Etype = EnumObj.GetType();

            if ((values == null) && (valuesCollView == null))
            {
                comboBox.Items.Clear();
                // Get all possible enum vals
                foreach (object item in Enum.GetValues(Etype))
                {
                    if (excludeList != null && excludeList.Contains(item))
                    {
                        continue;
                    }
                    ComboEnumItem CEI = new ComboEnumItem();
                    CEI.text = GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    comboBox.Items.Add(CEI);
                }
            }
            else
            {
                if ((values == null) && (valuesCollView != null))
                {
                    comboBox.ItemsSource = valuesCollView;
                }
                else
                {
                    comboBox.Items.Clear();
                    // get only subset from selected enum vals - used in Edit Action locate by to limit to valid values
                    foreach (object item in values)
                    {
                        if (excludeList != null && excludeList.Contains(item))
                        {
                            continue;
                        }
                        ComboEnumItem CEI = new ComboEnumItem();
                        CEI.text = GetEnumValueDescription(Etype, item);
                        CEI.Value = item;
                        comboBox.Items.Add(CEI);
                    }
                }
            }

            if (sortValues)
            {
                // Get the combo to be sorted
                comboBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("text", System.ComponentModel.ListSortDirection.Ascending));
            }

            //if ((values == null) && (valuesCollView != null))
            //{
            comboBox.SelectedItem = EnumObj;
            //}
            //else
            //{
            //    comboBox.SelectedValue = EnumObj;
            //}
        }

        public static void FillComboFromEnumType(ComboBox comboBox, Type Etype, List<object> values = null)
        {
            comboBox.Items.Clear();
            comboBox.SelectedValuePath = "Value";
            if (values == null)
            {
                // Get all possible enum vals
                foreach (object item in Enum.GetValues(Etype))
                {
                    ComboEnumItem CEI = new ComboEnumItem();
                    CEI.text = GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    comboBox.Items.Add(CEI);
                }
            }
            else
            {
                // get only subset from selected enum vals - used in Edit Action locate by to limit to valid values
                foreach (object item in values)
                {
                    ComboEnumItem CEI = new ComboEnumItem();
                    CEI.text = GetEnumValueDescription(Etype, item);
                    CEI.Value = item;
                    comboBox.Items.Add(CEI);
                }
            }

            // Get the combo to be sorted
            comboBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("text", System.ComponentModel.ListSortDirection.Ascending));
        }

        public static void FillComboItemsFromEnumType(ComboBox comboBox, Type Etype)
        {
            comboBox.Items.Clear();
            comboBox.SelectedValuePath = "Content";

            // Get all possible enum vals
            foreach (object item in Enum.GetValues(Etype))
            {
                ComboBoxItem CEI = new ComboBoxItem();
                CEI.Content = item;
                comboBox.Items.Add(CEI);
            }
            // Get the combo to be sorted
            comboBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("text", System.ComponentModel.ListSortDirection.Ascending));
        }
        public static List<string> GetEnumValues(Type EnumType)
        {
            List<string> l = new List<string>();
            foreach (object item in Enum.GetValues(EnumType))
            {
                l.Add(GetEnumValueDescription(EnumType, item));
            }
            return l;
        }

        public static string GetEnumValueDescription(Type EnumType, object EnumValue)
        {
            return Amdocs.Ginger.Common.GeneralLib.General.GetEnumValueDescription(EnumType, EnumValue);
        }

        public static string GetEnumDescription(Type EnumType, object EnumValue)
        {
            try
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])EnumType.GetField(EnumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                string s;
                if (attributes.Length > 0)
                {
                    s = attributes[0].Description;
                }
                else
                {
                    s = "NA";
                }
                return s;
            }
            catch
            {
                return "NA";
            }
        }







        #endregion ENUM

        public static string CorrectJSON(string WrongJson)
        {

            return Amdocs.Ginger.Common.GeneralLib.General.CorrectJSON(WrongJson);
        }
        public static void FillComboFromList(ComboBox comboBox, List<string> ls)
        {
            comboBox.Items.Clear();
            if (ls == null)
                return;

            foreach (var item in ls)
            {
                ComboEnumItem CEI = new ComboEnumItem();
                CEI.text = item.ToString();
                CEI.Value = item;
                comboBox.Items.Add(CEI);
            }
        }

        public static bool CheckComboItems(ComboBox comboBox, List<string> ls)
        {
            if (comboBox.Items.Count != ls.Count())
                return false;

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (!ls.Contains(comboBox.Items[i].ToString()))
                    return false;
            }
            return true;
        }

        public static bool CheckComboItemExist(ComboBox comboBox, string value, string specificValueField = null)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (specificValueField == null)
                {
                    if (value == comboBox.Items[i].ToString())
                        return true;
                }
                else
                {
                    PropertyInfo propertyInfo = (comboBox.Items[i]).GetType().GetProperty(specificValueField);
                    if (propertyInfo != null && propertyInfo.GetValue((comboBox.Items[i])).ToString() == value)
                        return true;
                }
            }
            return false;
        }

        public static bool GetInputWithValidation(string header, string label, ref string resultValue, char[] CharsNotAllowed = null, bool isMultiline = false, RepositoryItemBase repositoryItem = null)
        {
            bool returnWindow = GingerCore.GeneralLib.InputBoxWindow.OpenDialog(header, label, ref resultValue, isMultiline);

            if (returnWindow)
            {
                resultValue = resultValue.Trim();
                if (string.IsNullOrEmpty(resultValue.Trim()))
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Value cannot be empty.");
                    return GetInputWithValidation(header, label, ref resultValue, CharsNotAllowed, isMultiline);
                }
                if (IsNameAlreadyexists(repositoryItem, resultValue.Trim()))
                {
                    return GetInputWithValidation(header, label, ref resultValue, CharsNotAllowed, isMultiline, repositoryItem);
                }
                if (CharsNotAllowed != null && !(resultValue.IndexOfAny(CharsNotAllowed) < 0))
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    foreach (char value in CharsNotAllowed)
                    {
                        builder.Append(value);
                        builder.Append(" ");
                    }
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Value cannot contain characters like:" + "\n" + builder.ToString());
                    return GetInputWithValidation(header, label, ref resultValue, CharsNotAllowed, isMultiline, repositoryItem);
                }
            }
            return returnWindow;
        }
        public static bool IsNameAlreadyexists(RepositoryItemBase repositoryItem ,string resultValue)
        {           
                switch (repositoryItem.GetItemType())
                {     
                    case "BusinessFlow":
                    ObservableList<BusinessFlow> BFList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();                        
                        if(BFList.Any(x =>x.Name == resultValue))
                        {
                            Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Business flow with same name: " + "'" + resultValue + "'" + " already exists.");
                            return true;
                        }                                                   
                        break;
                    case "Agent":
                        ObservableList<Agent> Agentist = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
                        if (Agentist.Any(x => x.Name == resultValue))
                        {
                            Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Agent with same name: " + "'" + resultValue + "'" + " already exists.");
                            return true;
                        }                       
                        break;
                    case "ReportTemplate":
                        if ((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ReportTemplate>() where x.Name == resultValue select x).SingleOrDefault() != null)
                        {
                            Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Template with same name: " + "'" + resultValue + "'" + " already exists.");
                            return true;
                        }
                        
                        break;
                    case "ApplicationPOMModel":
                        if ((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>() where x.Name == resultValue select x).SingleOrDefault() != null)
                        {
                            Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "POM Model with same name: " + "'" + resultValue + "'" + " already exists.");
                            return true;
                        }
                      
                        break;
                    case "EnvApplication":
                        if ((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<EnvApplication>() where x.Name == resultValue select x).SingleOrDefault() != null)
                        {
                            Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Application with same name: " + "'" + resultValue + "'" + " already exists.");
                            return true;
                        }
                    
                        break;
                    case "HTMLReportTemplate":
                        if ((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportTemplate>() where x.Name == resultValue select x).SingleOrDefault() != null)
                        {
                            Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Report Template with same name: " + "'" + resultValue + "'" + " already exists.");
                            return true;
                        }
                       
                        break;
                default:
                    return false;
                }
            
            return false;
          
        }

        public static bool SelectInputWithValidation(string header, string label, ref string resultValue, List<string> mValues)
        {
            bool returnWindow = GingerCore.GeneralLib.ComboBoxWindow.OpenDialog(header, label, mValues, ref resultValue);

            if (returnWindow)
            {
                resultValue = resultValue.Trim();
                if (string.IsNullOrEmpty(resultValue.Trim()))
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Value cannot be empty.");
                    return SelectInputWithValidation(header, label, ref resultValue, mValues);
                }
                if (!(mValues.Contains(resultValue)))
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Value must be form the list");
                    return SelectInputWithValidation(header, label, ref resultValue, mValues);
                }
            }
            return returnWindow;
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        internal static ImageSource ToBitmapSource(System.Drawing.Bitmap source)
        {
            if (source == null) return null;
            BitmapSource bitSrc = null;
            var hBitmap = source.GetHbitmap();
            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                //TODO:  NativeMethods.DeleteObject(hBitmap);

                //internal static class NativeMethods
                //{
                //[DllImport("gdi32.dll")]
                //[return: MarshalAs(UnmanagedType.Bool)]
                //internal static extern bool DeleteObject(IntPtr hObject);
                //}
            }

            return bitSrc;
        }

        public static string BitmapImageToFile(Bitmap bmp, string filePath = "")
        {
            using (bmp)
            {
                if (string.IsNullOrEmpty(filePath))
                    filePath = Path.GetTempFileName();
                else if (!CheckOrCreateDirectory(Path.GetDirectoryName(filePath)))
                {
                    return string.Empty;
                }

                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            return filePath;
        }

        public static Boolean CheckOrCreateDirectory(string directoryPath)
        {
            try
            {
                if (System.IO.Directory.Exists(directoryPath))
                {
                    return true;
                }
                else
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}", ex);
                return false;
            }

        }
        public static void CleanDirectory(string folderName, bool isCleanFile = true)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderName);
            try
            {
                if (System.IO.Directory.Exists(folderName))
                {
                    if (isCleanFile)
                        foreach (System.IO.FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                    foreach (System.IO.DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
            }
            catch (IOException ex)
            {
                Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to Clean the Folder '{0}', Issue:'{1}'", folderName, ex.Message));
            }

        }


        public static string GetGingerEXEPath()
        {
            string exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exeLocation = exeLocation.Replace("GingerCore.dll", "");
            return exeLocation;
        }



        public static List<ComboEnumItem> GetEnumValuesForCombo(Type Etype)
        {
            List<ComboEnumItem> list = new List<ComboEnumItem>();
            foreach (object item in Enum.GetValues(Etype))
            {
                ComboEnumItem CEI = new ComboEnumItem();
                CEI.text = GingerCore.General.GetEnumValueDescription(Etype, item);
                CEI.Value = item;

                list.Add(CEI);
            }
            return list;
        }

        public static List<ComboEnumItem> GetEnumValuesForComboAndAddExtraValues(Type listType, List<ComboEnumItem> comboEnumItemsValues = null)
        {
            List<ComboEnumItem> comboEnumItemsList = new List<ComboEnumItem>();
            if (comboEnumItemsValues != null && comboEnumItemsValues.Count > 0)
            {
                comboEnumItemsList.AddRange(comboEnumItemsValues);
            }
            comboEnumItemsList.AddRange(GetEnumValuesForCombo(listType));
            return comboEnumItemsList;
        }
        public static List<ComboEnumItem> GetEnumValuesForComboFromList(Type Etype, List<Object> Items)
        {
            List<ComboEnumItem> list = new List<ComboEnumItem>();
            foreach (object item in Items)
            {
                ComboEnumItem CEI = new ComboEnumItem();
                CEI.text = GingerCore.General.GetEnumValueDescription(Etype, item);
                CEI.Value = item;

                list.Add(CEI);
            }
            return list;
        }



        public static void SelectComboValue(ComboBox comboBox, string Value)
        {
            string itemVal = "";
            foreach (var item in comboBox.Items)
            {
                if (item.GetType() == typeof(ComboBoxItem))
                    itemVal = ((ComboBoxItem)item).Content.ToString();
                else
                    itemVal = item.ToString();
                if (itemVal == Value)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }

        public static void AddComboItem(ComboBox comboBox, object itemtoadd)
        {
            string itemText = itemtoadd.ToString();
            if (itemtoadd.GetType().BaseType.Name == "Enum")
                itemText = GetEnumValueDescription(itemtoadd.GetType(), itemtoadd);
            foreach (var item in comboBox.Items)
                if (item.ToString() == itemText)
                {
                    return;
                }
            ComboEnumItem CEI = new ComboEnumItem();
            CEI.text = itemText;
            CEI.Value = itemtoadd;
            comboBox.Items.Add(CEI);
        }

        public static void RemoveComboItem(ComboBox comboBox, object itemtoRemove)
        {
            string itemText = itemtoRemove.ToString();
            if (itemtoRemove.GetType().BaseType.Name == "Enum")
                itemText = GetEnumValueDescription(itemtoRemove.GetType(), itemtoRemove);
            foreach (var item in comboBox.Items)
                if (item.ToString() == itemText)
                {
                    comboBox.Items.Remove(item);
                    return;
                }
        }
        public static void DisableComboItem(ComboBox comboBox, object itemtoDisable)
        {
            foreach (var item in comboBox.Items)
            {
                if (item.GetType() == typeof(ComboBoxItem))
                {
                    if (((ComboBoxItem)item).Content.ToString() == itemtoDisable.ToString())
                    {
                        ((ComboBoxItem)item).IsEnabled = false;
                        return;
                    }
                }
            }
        }

        public static void UpdateComboItem(ComboBox comboBox, object itemtoUpdate, string newVal)
        {
            string itemText = itemtoUpdate.ToString();
            if (itemtoUpdate.GetType().BaseType.Name == "Enum")
                itemText = GetEnumValueDescription(itemtoUpdate.GetType(), itemtoUpdate);
            foreach (var item in comboBox.Items)
            {
                if (item.GetType() == typeof(ComboBoxItem))
                {
                    if (((ComboBoxItem)item).Content.ToString() == itemText)
                    {
                        ((ComboBoxItem)item).Content = newVal;
                        return;
                    }
                }
            }

        }
        public static void EnableComboItem(ComboBox comboBox, object itemtoDisable)
        {
            foreach (var item in comboBox.Items)
            {
                if (item.GetType() == typeof(ComboBoxItem))
                {
                    if (((ComboBoxItem)item).Content.ToString() == itemtoDisable.ToString())
                    {
                        ((ComboBoxItem)item).IsEnabled = true;
                        return;
                    }
                }
            }
        }


        public static void ClearDirectoryContent(string DirPath)
        {
            Amdocs.Ginger.Common.GeneralLib.General.ClearDirectoryContent(DirPath);
        }

        //HTML Report related methods added here 
        public static string TimeConvert(string s)
        {
            return Amdocs.Ginger.Common.GeneralLib.General.TimeConvert(s);
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static System.Drawing.Color makeColor(string HexString)
        {
            System.Drawing.Color colour = ColorTranslator.FromHtml(HexString);
            return colour;
        }

        public static System.Windows.Media.Color makeColorN(string HexString)
        {
            System.Windows.Media.Color color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(HexString);
            return color;
        }

        public static bool CompareStringsIgnoreCase(string strOne, string strTwo)
        {
            return strOne.Equals(strTwo, StringComparison.OrdinalIgnoreCase);
        }

        public static ResourceDictionary SelectColor(string status)
        {
            ResourceDictionary r = new ResourceDictionary();
            System.Windows.Media.SolidColorBrush myBrush = null;
            if (string.IsNullOrEmpty(status))
                status = "Pending";
            switch (status)
            {
                case "Passed":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#54A81B"));
                    r.Add(status, myBrush);
                    break;
                case "Failed":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#E31123"));
                    r.Add(status, myBrush);
                    break;
                case "Fail":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#E31123"));
                    r.Add(status, myBrush);
                    break;
                case "Stopped":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#ED5588"));
                    r.Add(status, myBrush);
                    break;
                case "Pending":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#FF8C00"));
                    r.Add(status, myBrush);
                    break;
                case "Running":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#800080"));
                    r.Add(status, myBrush);
                    break;
                default:
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#1B3651"));
                    r.Add(status, myBrush);
                    break;
            }
            return r;
        }

        public static SolidColorBrush SelectColorByCollection(string status)
        {
            System.Windows.Media.SolidColorBrush myBrush = null;
            if (string.IsNullOrEmpty(status))
                status = "Pending";
            switch (status)
            {
                case "Passed":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#54A81B"));
                    
                    break;
                case "Failed":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#E31123"));
                    
                    break;
                case "Fail":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#E31123"));
                    
                    break;
                case "Stopped":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#ED5588"));
                    
                    break;
                case "Pending":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#FF8C00"));
                    
                    break;
                case "Running":
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#800080"));
                    
                    break;
                default:
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#1B3651"));
                   
                    break;
            }
            return myBrush;
        }

        /// <summary>
        /// The function will take full desktop screen shot of Primary screen or all Screens and will return Dictionary of (ScreenDeviceName, ScreenShotPath)
        /// </summary>
        /// <param name="captureAllScreens"></param>
        /// Set as True for getting screen shots from all screens
        /// <returns></returns>
        public static Dictionary<string, String> TakeDesktopScreenShot(bool captureAllScreens = false)
        {
            Dictionary<string, String> imagePaths = new Dictionary<string, String>();
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                if (captureAllScreens == false & screen.Primary == false)
                    continue;//jump over non primary screens

                System.Drawing.Rectangle bounds = screen.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(
                                           screen.Bounds.X,
                                           screen.Bounds.Y,
                                           0,
                                           0,
                                           screen.Bounds.Size,
                                           CopyPixelOperation.SourceCopy);
                    }
                    string imagePath = GingerCore.General.BitmapImageToFile(bitmap);
                    if (System.IO.File.Exists(imagePath))
                        imagePaths.Add(screen.DeviceName, imagePath);
                }
            }
            return imagePaths;
        }

        public static Bitmap GetBrowserHeaderScreenshot(System.Drawing.Point windowPosition, System.Drawing.Size windowSize, System.Drawing.Size viewportSize, double devicePixelRatio)
        {
            Bitmap browserHeaderScreenshot = new((int)(windowSize.Width * devicePixelRatio), (int)((windowSize.Height - viewportSize.Height) * devicePixelRatio));
            using (Graphics graphics = Graphics.FromImage(browserHeaderScreenshot))
            {
                System.Drawing.Point upperLeftSource = new((int)(windowPosition.X * devicePixelRatio), (int)(windowPosition.Y * devicePixelRatio));
                System.Drawing.Point upperLeftDestination = new(x: 0, y: 0);
                graphics.CopyFromScreen(upperLeftSource, upperLeftDestination, browserHeaderScreenshot.Size);
            }
            return browserHeaderScreenshot;
        }

        public static Bitmap GetTaskbarScreenshot()
        {
            System.Windows.Forms.Screen primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            Bitmap taskbarScreenshot = new(primaryScreen.Bounds.Width, primaryScreen.Bounds.Height - primaryScreen.WorkingArea.Height);
            using(Graphics graphics = Graphics.FromImage(taskbarScreenshot))
            {
                System.Drawing.Point upperLeftSource = new(x: 0, y: primaryScreen.WorkingArea.Height);
                System.Drawing.Point uppderLeftDestination = new(x: 0, y: 0);
                graphics.CopyFromScreen(upperLeftSource, uppderLeftDestination, taskbarScreenshot.Size);
            }
            return taskbarScreenshot;
        }

        public static string MergeVerticallyAndSaveBitmaps(params Bitmap[] bitmaps)
        {
            IEnumerable<SKBitmap> skBitmaps = bitmaps.Select(bitmap => bitmap.ToSKBitmap());
            return MergeVerticallyAndSaveBitmaps(skBitmaps);
        }

        private static string MergeVerticallyAndSaveBitmaps(IEnumerable<SKBitmap> bitmaps)
        {
            int maxWidth = bitmaps.Max(skBitmap => skBitmap.Width);
            int mergedHeight = bitmaps.Aggregate(0, (totalHeight, skBitmap) => totalHeight + skBitmap.Height);

            SKBitmap mergedBitmap = new(maxWidth, mergedHeight);
            using SKCanvas canvas = new(mergedBitmap);
            int currentDrawnHeight = 0;
            foreach (SKBitmap bitmap in bitmaps)
            {
                float drawingXCoordinate = (float)(maxWidth - bitmap.Width) / 2;
                float drawingYCoordinate = currentDrawnHeight;

                canvas.DrawBitmap(bitmap, drawingXCoordinate, drawingYCoordinate);

                currentDrawnHeight += bitmap.Height;
            }

            string filePath = BitmapImageToFile(mergedBitmap.ToBitmap());
            return filePath;
        }

        /// <summary>
        /// Replaces invalid XML characters in a string with their valid XML equivalent.
        /// </summary>
        /// <param name="str">The string within which to escape invalid characters.</param>
        /// <returns>The input string with invalid characters replaced.</returns>
        public static string ConvertInvalidXMLCharacters(string str)
        {
            return SecurityElement.Escape(str);
        }

        //Adding check that Registry values exist
        //FEATURE_BROWSER_EMULATION- Defines in which document mode WebBrowser Control(Internal browser) should launch (IE 11 )
        //FEATURE_SCRIPTURL_MITIGATION - feature allows the href attribute of a objects to support the javascript prototcol.
        //                               It is by default disabled for WebBrowser Control and enabled for IE
        public static void CheckRegistryValues()
        {
            bool osBitTypeIs64 = false;
            string appExeName = string.Empty;
            string registryKeyPath = string.Empty;
            string requiredValueName = string.Empty;
            object requiredValue = string.Empty;
            try
            {
                //Find out the OS bit type
                osBitTypeIs64 = Environment.Is64BitOperatingSystem;

                //Get the App name     
                appExeName = System.AppDomain.CurrentDomain.FriendlyName;

                //######################## FEATURE_BROWSER_EMULATION ###########################                               
                if (osBitTypeIs64)
                    registryKeyPath = @"SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION";
                else
                    registryKeyPath = @"SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION";
                requiredValueName = appExeName;
                requiredValue = string.Empty;
                object installedIEVersion =
                    RegistryFunctions.GetRegistryValue(eRegistryRoot.HKEY_LOCAL_MACHINE, @"Software\Microsoft\Internet Explorer", "svcUpdateVersion");
                if (installedIEVersion != null)
                {
                    if (installedIEVersion != null)
                        requiredValue = (installedIEVersion.ToString().Split(new char[] { '.' }))[0] + "000";
                }
                if (requiredValue.ToString() == string.Empty || requiredValue.ToString() == "000")
                    requiredValue = "11000"; //defualt value
                                             //write registry key to the User level if failed to write to Local Machine level

                if (!RegistryFunctions.CheckRegistryValueExist(eRegistryRoot.HKEY_LOCAL_MACHINE, registryKeyPath,
                                    requiredValueName, requiredValue, Microsoft.Win32.RegistryValueKind.DWord, true, true))
                {
                    //Try User Level
                    registryKeyPath = @"SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION";
                    if (!RegistryFunctions.CheckRegistryValueExist(eRegistryRoot.HKEY_CURRENT_USER, registryKeyPath,
                                    requiredValueName, requiredValue, Microsoft.Win32.RegistryValueKind.DWord, true, true))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to add the required registry key 'FEATURE_BROWSER_EMULATION' value to both Local Machine and User level");
                    }
                }
                //End

                //######################## FEATURE_SCRIPTURL_MITIGATION ###########################
                if (osBitTypeIs64)
                    registryKeyPath = @"SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_SCRIPTURL_MITIGATION";
                else
                    registryKeyPath = @"SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_SCRIPTURL_MITIGATION";
                requiredValueName = appExeName;
                requiredValue = 1;
                //write registry key to the User level if failed to write to Local Machine level
                if (!RegistryFunctions.CheckRegistryValueExist(eRegistryRoot.HKEY_LOCAL_MACHINE, registryKeyPath,
                                requiredValueName, requiredValue, Microsoft.Win32.RegistryValueKind.DWord, true, true))
                {
                    //Try User Level
                    registryKeyPath = @"SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_SCRIPTURL_MITIGATION";
                    if (!RegistryFunctions.CheckRegistryValueExist(eRegistryRoot.HKEY_CURRENT_USER, registryKeyPath,
                                requiredValueName, requiredValue, Microsoft.Win32.RegistryValueKind.DWord, true, true))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to add the required registry key 'FEATURE_SCRIPTURL_MITIGATION' value to both Local Machine and User level");
                    }
                }
                //End
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to complete the registry values check", ex);
                Reporter.ToUser(eUserMsgKey.RegistryValuesCheckFailed);
            }
        }

        public static string[] ReturnFilesWithDesiredExtension(string filepath, string extension)
        {
            string[] fileEntries = Directory.EnumerateFiles(filepath, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.ToLower().EndsWith(extension)).ToArray();

            return fileEntries;
        }
        public static ObservableList<T> ConvertListToObservableList<T>(List<T> List)
        {
            ObservableList<T> ObservableList = new ObservableList<T>();
            foreach (T o in List)
                ObservableList.Add(o);
            return ObservableList;
        }

        public static List<T> ConvertObservableListToList<T>(ObservableList<T> List)
        {
            List<T> ObservableList = new List<T>();
            foreach (T o in List)
                ObservableList.Add(o);
            return ObservableList;
        }

        /// <summary>
        /// Finds a parent of a given control/item on the visual tree.
        /// </summary>
        /// <typeparam name="T">Type of Parent</typeparam>
        /// <param name="child">Child whose parent is queried</param>
        /// <returns>Returns the first parent item that matched the type (T), if no match found then it will return null</returns>
        public static T TryFindParent<T>(DependencyObject child)
        where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return TryFindParent<T>(parentObject);
        }

        /// <summary>
        /// This function is used to replace the password value from the string with *****
        /// It will also remove all the spaces.
        /// </summary>
        /// <param name="dataString">The string argument which has password value</param>
        /// <returns></returns>
        public static string HidePasswordFromString(string dataString)
        {
            string passwordValue = dataString.Replace(" ", "");//remove spaces
            string passwordString = string.Empty;
            //Matching string
            if (dataString.ToLower().Contains("pwd="))
            {
                passwordString = "pwd=";
            }
            else if (dataString.ToLower().Contains("password="))
            {
                passwordString = "password=";
            }
            else
            {
                //returning origional as it does not conatain matching string
                return dataString;
            }
            //get the password value based on start and end index
            passwordValue = passwordValue.Substring(passwordValue.ToLower().IndexOf(passwordString));
            int startIndex = passwordValue.ToLower().IndexOf(passwordString) + passwordString.Length;
            int endIndex = -1;
            if (passwordValue.Contains(";"))
            {
                endIndex = passwordValue.ToLower().IndexOf(";");
            }
            if (endIndex == -1)
            {
                passwordValue = passwordValue.Substring(startIndex);
            }
            else
            {
                passwordValue = passwordValue.Substring(startIndex, endIndex - startIndex);
            }

            if (!string.IsNullOrEmpty(passwordValue))
            {
                dataString = dataString.Replace(passwordValue, "*****");
            }
            return dataString;
        }

        public static bool LoadALMSettings(string fileName, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType eALMType)
        {
            if (!ValidateConfigurationFile(fileName, eALMType))
            {
                return false;
            }
            {
                string folderPath = Path.Combine(WorkSpace.Instance.Solution.Folder, "Configurations");
                DirectoryInfo di = Directory.CreateDirectory(folderPath);
                string configPackageFolder = string.Empty;
                switch (eALMType)
                {
                    case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Jira:
                        configPackageFolder = "JiraConfigurationsPackage";
                        break;
                    case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest:
                        configPackageFolder = "QTestConfigurationsPackage";
                        break;
                    default:
                        configPackageFolder = "JiraConfigurationsPackage";
                        break;
                }
                folderPath = Path.Combine(folderPath, configPackageFolder);
                if (Directory.Exists(folderPath))
                {
                    Amdocs.Ginger.Common.GeneralLib.General.ClearDirectoryContent(folderPath);
                }
                ZipFile.ExtractToDirectory(fileName, folderPath);

                ALMCore.DefaultAlmConfig.ALMConfigPackageFolderPath = folderPath;
                ALMCore.DefaultAlmConfig.ALMConfigPackageFolderPath = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(ALMCore.DefaultAlmConfig.ALMConfigPackageFolderPath);

                if (!IsConfigPackageExists(folderPath, eALMType))
                {
                    return false;
                }
            }
            return true;
        }
        static bool ValidateConfigurationFile(string PackageFileName, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType eALMType)
        {
            bool containSettingsFile = false;
            string configPackageFile = string.Empty;
            switch (eALMType)
            {
                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Jira:
                    configPackageFile = @"JiraSettings/JiraSettings.json";
                    break;
                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest:
                    configPackageFile = @"QTestSettings/QTestSettings.json";
                    break;
                default:
                    configPackageFile = @"JiraSettings/JiraSettings.json";
                    break;
            }

            using (FileStream configPackageZipFile = new FileStream(PackageFileName, FileMode.Open))
            {
                using (ZipArchive zipArchive = new ZipArchive(configPackageZipFile))
                {
                    foreach (ZipArchiveEntry entry in zipArchive.Entries)
                    {
                        if (entry.FullName == configPackageFile)
                        {
                            containSettingsFile = true;
                            break;
                        }
                    }
                }
            }
            return containSettingsFile;
        }

        public static bool IsConfigPackageExists(string PackagePath, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType eALMType)
        {
            string settingsFolder = string.Empty;
            switch (eALMType)
            {
                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Jira:
                    settingsFolder = "JiraSettings";
                    break;
                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest:
                    settingsFolder = "QTestSettings";
                    break;
                default:
                    settingsFolder = "JiraSettings";
                    break;
            }
            if (Directory.Exists(Path.Combine(PackagePath, settingsFolder)))
            {
                return true;
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "Configuration package not exist in solution, Settings not exist at: " + Path.Combine(PackagePath, settingsFolder));
            }
            return false;
        }

        public static string GetClipboardText()
        {
            return Clipboard.GetText();
        }

    }
}


