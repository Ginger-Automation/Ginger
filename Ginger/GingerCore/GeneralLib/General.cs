#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger;
using GingerCore.GeneralFunctions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using GingerCore.DataSource;
using System.Reflection;

namespace GingerCore
{
    public class General
    {
        public static void LoadGenericWindow(ref GenericWindow genWindow, System.Windows.Window owner, eWindowShowStyle windowStyle, string windowTitle, Page windowPage,
                                            ObservableList<Button> windowBtnsList = null, bool showClosebtn = true, string closeBtnText = "Close", EventHandler closeEventHandler = null, bool startupLocationWithOffset=false)
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
                genWindow = new GenericWindow(owner, winStyle, windowTitle, windowPage, windowBtnsList, showClosebtn, closeBtnText, closeEventHandler);
                genWindow.Title = windowPage.Title;
                if (startupLocationWithOffset)
                {
                    genWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    genWindow.Left = 50;
                    genWindow.Top = 200;
                }
                if (winStyle == eWindowShowStyle.Dialog)
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
        public static void FillComboFromEnumObj(ComboBox comboBox, Object EnumObj, List<object> values = null, bool sortValues = true, ListCollectionView valuesCollView = null)
        {
            comboBox.SelectedValuePath = "Value";
            Type Etype = EnumObj.GetType();

            if ((values == null) && (valuesCollView == null))
            {
                comboBox.Items.Clear();
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
            
            comboBox.SelectedItem = EnumObj;
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
            try
            {
                EnumValueDescriptionAttribute[] attributes = (EnumValueDescriptionAttribute[])EnumType.GetField(EnumValue.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false);
                string s;
                if (attributes.Length > 0)
                {
                    s = attributes[0].ValueDescription;
                    //for supporting multi Terminology types
                    s = s.Replace("Business Flow", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    s = s.Replace("Business Flows", GingerDicser.GetTermResValue(eTermResKey.BusinessFlows));
                    s = s.Replace("Activities Group", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));
                    s = s.Replace("Activities Groups", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups));
                    s = s.Replace("Activity", GingerDicser.GetTermResValue(eTermResKey.Activity));
                    s = s.Replace("Conversion Mechanism", GingerDicser.GetTermResValue(eTermResKey.ConversionMechanism));
                    s = s.Replace("Activities", GingerDicser.GetTermResValue(eTermResKey.Activities));
                    s = s.Replace("Variable", GingerDicser.GetTermResValue(eTermResKey.Variable));
                    s = s.Replace("Variables", GingerDicser.GetTermResValue(eTermResKey.Variables));
                    s = s.Replace("Run Set", GingerDicser.GetTermResValue(eTermResKey.RunSet));
                    s = s.Replace("Run Sets", GingerDicser.GetTermResValue(eTermResKey.RunSets));
                }
                else
                {
                    s = EnumValue.ToString();
                }
                return s;
            }
            catch
            {
                //TODO: fixme ugly catch - check why we come here and fix it, todo later
                return EnumValue.ToString();
            }
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

        // TODO: move to sperate class
        public class ComboEnumItem
        {
            public static class Fields
            {
                public static string text = "text";
                public static string Value = "Value";
            }

            public override String ToString()
            {
                return text;
            }

            public string text { get; set; }
            public object Value { get; set; }
        }

        public class ComboGroupedEnumItem
        {
            public static class Fields
            {
                public static string text = "text";
                public static string Value = "text";
                public static string Category = "Value";
            }

            public object text { get; set; }
            public object Value { get; set; }
            public string Category { get; set; }
        }

        public class XmlNodeItem
        {
            public XmlNodeItem(string p, string v, string xp)
            {
                param = p;
                value = v;
                path = xp;
            }

            public static class Fields
            {
                public static string param = "param";
                public static string value = "value";
                public static string path = "path";
            }

            public override String ToString()
            {
                return "Param:" + param + Environment.NewLine + "value:" + value + Environment.NewLine + "path:" + path;
            }

            public string param { get; set; }
            public string value { get; set; }
            public string path { get; set; }
        }

        public class ComboItem
        {

            public static class Fields
            {
                public static string text = "text";
                public static string Value = "Value";
            }

            public override String ToString()
            {
                return text;
            }

            public string text { get; set; }
            public object Value { get; set; }
        }
        #endregion ENUM

        #region Binding
        public static void ActInputValueBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, ActInputValue actInputValue, BindingMode BindingMode = BindingMode.TwoWay)
        {
            ObjFieldBinding(control, dependencyProperty, actInputValue, ActInputValue.Fields.Value, BindingMode);
        }

        public static void ObjFieldBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property, BindingMode BindingMode = BindingMode.TwoWay)
        {
            //TODO: add Inotify on the obj.attr - so code changes to property will be reflected
            //TODO: check perf impact + reuse exisitng binding on same obj.prop
            try
            {
                Binding b = new Binding();
                b.Source = obj;
                b.Path = new PropertyPath(property);
                b.Mode = BindingMode;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                b.NotifyOnValidationError = true;
                control.SetBinding(dependencyProperty, b);
            }
            catch (Exception ex)
            {
                //it is possible we load an old enum or something else which will cause the binding to fail
                // Can happen also if the bind field name is incorrect
                // mark the control in red, instead of not openning the Page
                // Set a tool tip with the error
                
                control.Style = null; // remove style so red will show
                control.Background = System.Windows.Media.Brushes.LightPink;
                control.BorderThickness = new Thickness(2);
                control.BorderBrush = System.Windows.Media.Brushes.Red;

                control.ToolTip = "Error binding control to property: " + Environment.NewLine + property + " Please open a defect with all information,  " + Environment.NewLine + ex.Message;
            }
        }

        public static void ObjFieldBinding(TextBlock textBlockControl, DependencyProperty dependencyProperty, object obj, string property, BindingMode BindingMode = BindingMode.TwoWay)
        {
            //TODO: add Inotify on the obj.attr - so code changes to property will be reflected
            //TODO: check perf impact + reuse exisitng binding on same obj.prop
            try
            {
                Binding b = new Binding();
                b.Source = obj;
                b.Path = new PropertyPath(property);
                b.Mode = BindingMode;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                textBlockControl.SetBinding(dependencyProperty, b);
            }
            catch (Exception ex)
            {
                //it is possible we load an old enum or something else which will cause the binding to fail
                // Can happen also if the bind field name is incorrect
                // mark the control in red, instead of not openning the Page
                // Set a tool tip with the error
                
                textBlockControl.Style = null; // remove style so red will show
                textBlockControl.Background = System.Windows.Media.Brushes.LightPink;
                textBlockControl.ToolTip = "Error binding control to property: " + Environment.NewLine + property + " Please open a defect with all information,  " + Environment.NewLine + ex.Message;
            }
        }
        #endregion Binding

        public static string CorrectJSON(string WrongJson)
        {
            string CleanJson = WrongJson.Replace("\\", "");
            string CleanJson1 = CleanJson.Substring(CleanJson.IndexOf("{"));
            string CleanJson2 = CleanJson1.Substring(0, CleanJson1.LastIndexOf("}") + 1);
            return CleanJson2;
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
                    if(propertyInfo != null && propertyInfo.GetValue((comboBox.Items[i])).ToString() == value)
                        return true;
                }
            }
            return false;
        }

        public static bool GetInputWithValidation(string header, string label, ref string resultValue, char[] CharsNotAllowed, bool isMultiline = false)
        {
            bool returnWindow = GingerCore.GeneralLib.InputBoxWindow.OpenDialog(header, label, ref resultValue, isMultiline);

            if (returnWindow)
            {
                resultValue = resultValue.Trim();
                if (string.IsNullOrEmpty(resultValue.Trim()))
                {
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Value cannot be empty.");
                    return GetInputWithValidation(header, label, ref resultValue, CharsNotAllowed, isMultiline);
                }
                if (!(resultValue.IndexOfAny(CharsNotAllowed) < 0))
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    foreach (char value in CharsNotAllowed)
                    {
                        builder.Append(value);
                        builder.Append(" ");
                    }
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Value cannot contain charaters like:" + "\n" + builder.ToString());
                    return GetInputWithValidation(header, label, ref resultValue, CharsNotAllowed, isMultiline);
                }
            }
            return returnWindow;
        }

        public static bool SelectInputWithValidation(string header, string label, ref string resultValue, List<string> mValues)
        {
            bool returnWindow = GingerCore.GeneralLib.ComboBoxWindow.OpenDialog(header, label, mValues, ref resultValue);

            if (returnWindow)
            {
                resultValue = resultValue.Trim();
                if (string.IsNullOrEmpty(resultValue.Trim()))
                {
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Value cannot be empty.");
                    return SelectInputWithValidation(header, label, ref resultValue, mValues);
                }
                if (!(mValues.Contains(resultValue)))
                {
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Value must be form the list");
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
            if (string.IsNullOrEmpty(filePath))
                filePath = Path.GetTempFileName();
            else if(!CheckOrCreateDirectory(Path.GetDirectoryName(filePath)))
            {
                return string.Empty;
            }                

            bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}", ex);
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
            catch(IOException ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}", ex);
            }
            
        }
        public static Bitmap FileToBitmapImage(String path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            Bitmap bmp = (Bitmap)Bitmap.FromFile(path);
            return (bmp);
        }

        public static string GetGingerEXEPath()
        {
            string exeLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exeLocation = exeLocation.Replace("GingerCore.dll", "");
            return exeLocation;
        }

        public static bool IsNumeric(string sValue)
        {
            // simple method to check is strign is number
            // there are many other alternatives, just keep it simple and make sure it run fast as it is going to be used a lot, for every return value calc   
            // regec and other are more expensive

        foreach (char c in sValue)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    return false;
                }
            }
            return true;
        }

        public static List<GingerCore.General.ComboEnumItem> GetEnumValuesForCombo(Type Etype)
        {
            List<GingerCore.General.ComboEnumItem> list = new List<GingerCore.General.ComboEnumItem>();
            foreach (object item in Enum.GetValues(Etype))
            {
                GingerCore.General.ComboEnumItem CEI = new GingerCore.General.ComboEnumItem();
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
            string itemText = itemtoDisable.ToString();
            if (itemtoDisable.GetType().BaseType.Name == "Enum")
                itemText = GetEnumValueDescription(itemtoDisable.GetType(), itemtoDisable);
            foreach (var item in comboBox.Items)
            {
                if (item.GetType() == typeof(ComboBoxItem))
                {
                    if (((ComboBoxItem)item).Content.ToString() == itemText)
                    {
                        ((ComboBoxItem)item).IsEnabled = false;
                        return;
                    }
                }
            }
                
        }
        public static void UpdateComboItem(ComboBox comboBox, object itemtoUpdate,string newVal)
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
            string itemText = itemtoDisable.ToString();
            if (itemtoDisable.GetType().BaseType.Name == "Enum")
                itemText = GetEnumValueDescription(itemtoDisable.GetType(), itemtoDisable);
            foreach (var item in comboBox.Items)
            {                
                if (item.GetType() == typeof(ComboBoxItem))
                {
                    if (((ComboBoxItem)item).Content.ToString() == itemText)
                    {
                        ((ComboBoxItem)item).IsEnabled = true;
                        return;
                    }
                }
            }
        }
        public static Dictionary<string, object> DeserializeJson(string json)
        {
            if (json.StartsWith("["))
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();

                JArray a = JArray.Parse(json);

                int ArrayCount = 1;
                foreach (JObject o in a.Children<JObject>())
                {
                    dictionary.Add(ArrayCount.ToString(), o);
                    ArrayCount++;

                }
                return dictionary;
            }
            else
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> dictionary =
                    serializer.Deserialize<Dictionary<string, object>>(json);
                return dictionary;
            }
        }

        public static List<XmlNodeItem> GetXMLNodesItems(XmlDocument xmlDoc,bool DisableProhibitDtd = false)
        {
            List<XmlNodeItem> returnDict = new List<XmlNodeItem>();
            XmlReader rdr1 = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            XmlReader rdr = null;
            if (DisableProhibitDtd)
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;
              
                rdr = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml), settings);
            }
            else
            {
                rdr = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            }
             
            XmlReader subrdr = null;
            string Elm = "";
            
            ArrayList ls = new ArrayList();
            Dictionary<string, int> lspath = new Dictionary<string, int>();
            List<string> DeParams = new List<string>();
            while (rdr.Read())
            {
                if (rdr.NodeType == XmlNodeType.Element)
                {
                    Elm = rdr.Name;
                    if (ls.Count <= rdr.Depth)
                        ls.Add(Elm);
                    else
                        ls[rdr.Depth] = Elm;
                    foreach (var p in DeParams)
                    {
                        if (p == rdr.Name)
                        {
                            subrdr = rdr.ReadSubtree();
                            subrdr.ReadToFollowing(p);
                            returnDict.Add(new XmlNodeItem("AllDescOf" + p, subrdr.ReadInnerXml(), "/" + string.Join("/", ls.ToArray().Take(rdr.Depth))));
                            subrdr = null;
                        }
                    }
                }

                if (rdr.NodeType == XmlNodeType.Text)
                {
                    // soup req contains sub xml, so parse them 
                    if (rdr.Value.StartsWith("<?xml"))
                    {
                        XmlDocument xmlnDoc = new XmlDocument();
                        xmlnDoc.LoadXml(rdr.Value);
                        GetXMLNodesItems(xmlnDoc);
                    }
                    else
                    {
                        if (!lspath.Keys.Contains("/" + string.Join("/", ls.ToArray().Take(rdr.Depth)) + "/" + Elm))
                        {
                            returnDict.Add(new XmlNodeItem(Elm, rdr.Value, "/" + string.Join("/", ls.ToArray().Take(rdr.Depth))));
                            lspath.Add("/" + string.Join("/", ls.ToArray().Take(rdr.Depth)) + "/" + Elm, 0);
                        }
                        else if (lspath.Keys.Contains("/" + string.Join("/", ls.ToArray().Take(rdr.Depth)) + "/" + Elm))
                        {
                            returnDict.Add(new XmlNodeItem(Elm + "_" + lspath["/" + string.Join("/", ls.ToArray().Take(rdr.Depth)) + "/" + Elm], rdr.Value, "/" + string.Join("/", ls.ToArray().Take(rdr.Depth))));
                            lspath["/" + string.Join("/", ls.ToArray().Take(rdr.Depth)) + "/" + Elm]++;
                        }
                    }
                }
            }
            return returnDict;
        }

        public static void ClearDirectoryContent(string DirPath)
        {
            //clear directory
            System.IO.DirectoryInfo di = new DirectoryInfo(DirPath);
            foreach (FileInfo file in di.GetFiles())
                file.Delete();
            foreach (DirectoryInfo dir in di.GetDirectories())
                dir.Delete(true);
        }

        //HTML Report related methods added here 
        public static string TimeConvert(string s)
        {
            double seconds = Convert.ToDouble(s);
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return ts.ToString(@"hh\:mm\:ss");
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
                    myBrush = new System.Windows.Media.SolidColorBrush(GingerCore.General.makeColorN("#ED5588"));
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
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to add the required registry key 'FEATURE_BROWSER_EMULATION' value to both Local Machine and User level");
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
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to add the required registry key 'FEATURE_SCRIPTURL_MITIGATION' value to both Local Machine and User level");
                    }
                }
                //End
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to complete the registry values check", ex);
                Reporter.ToUser(eUserMsgKeys.RegistryValuesCheckFailed);
            }
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
        public static string CheckDataSource(string DataSourceVE, ObservableList<DataSourceBase> DSList)
        {
            string DSVE = DataSourceVE;
            DataSourceBase DataSource = null;
            DataSourceTable DSTable = null;
            if (DSVE.IndexOf("{DS Name=") != 0)
            {
                return "Invalid Data Source Value : '" + DataSourceVE + "'";
            }
            DSVE = DSVE.Replace("{DS Name=", "");
            DSVE = DSVE.Replace("}", "");
            if (DSVE.IndexOf(" DST=") == -1)
            {
                return "Invalid Data Source Value : '" + DataSourceVE + "'";
            }
            string DSName = DSVE.Substring(0, DSVE.IndexOf(" DST="));

            foreach (DataSourceBase ds in DSList)
                if (ds.Name == DSName)
                {
                    DataSource = ds;
                    break;
                }


            if (DataSource == null)
            {
                return "Data Source: '" + DSName + "' used in '" + DataSourceVE + "' not found in solution.";
            }

            DSVE = DSVE.Substring(DSVE.IndexOf(" DST=")).Trim();
            if (DSVE.IndexOf(" ") == -1)
            {
                return "Invalid Data Source Value : '" + DataSourceVE + "'";
            }
            string DSTableName = DSVE.Substring(DSVE.IndexOf("DST=") + 4, DSVE.IndexOf(" ") - 4);

            if (DataSource.DSType == DataSourceBase.eDSType.MSAccess)
            {
                if (DataSource.FileFullPath.StartsWith("~"))
                {
                    DataSource.FileFullPath = DataSource.FileFullPath.Replace(@"~\","").Replace("~", "");
                    DataSource.FileFullPath = Path.Combine(DataSource.ContainingFolderFullPath.Replace("DataSources", "") , DataSource.FileFullPath);
                }
                DataSource.Init(DataSource.FileFullPath);
                ObservableList<DataSourceTable> dsTables = DataSource.GetTablesList();
                foreach (DataSourceTable dst in dsTables)
                    if (dst.Name == DSTableName)
                    {
                        DSTable = dst;
                        break;
                    }
                if (DSTable == null)
                {
                    return "Data Source Table : '" + DSTableName + "' used in '" + DataSourceVE + "' not found in solution.";
                }
            }
            return "";
        }
    }
}


