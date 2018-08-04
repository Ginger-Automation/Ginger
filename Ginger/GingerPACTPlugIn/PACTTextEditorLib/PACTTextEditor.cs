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

using GingerPlugIns.TextEditorLib;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using PactNet.Mocks.MockHttpService.Models;
using GingerPlugIns.TextEditorLib.Common;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using GingerPACTPlugIn.Properties;
using System.Windows.Media;

namespace GingerPACTPlugIn.PACTTextEditorLib
{
    public class PACTTextEditor : PlugInTextFileEditorBase
    {
        const string InteractionData = "<<Interactions_Data>>";
        const string ProviderData = "<<Provider>>";
        const string ConsumerData = "<<Consumer>>";

        public override List<string> Extensions
        {
            get
            {
                if (mExtensions.Count == 0)
                {
                    mExtensions.Add(".pact");                    
                }
                return mExtensions;
            }
        }

        public override byte[] HighlightingDefinition { get { return Properties.Resources.PACTHighlighting; } }
        
        public static string BitmapImageToFile(System.Drawing.Bitmap bmp)
        {
            //TODO: clanup up or where to save
            string filename = System.IO.Path.GetTempFileName();

            bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Png);

            return (filename);
        }

        public override List<TextEditorToolBarItem> Tools
        {
            get
            {
                if (mTools.Count == 0)
                {
                    ImageSourceConverter c = new ImageSourceConverter();
                    Image ToolImage1 = new Image();
                    //TODO: temp fix me to get impage from Plugin DLL
                    string file = BitmapImageToFile(Resources._JAVAdoc_16x16);

                    ToolImage1.Source = new BitmapImage(new Uri(file));

                    mTools.Add(new TextEditorToolBarItem() { Image = ToolImage1,  toolTip = "Export to Java", clickHandler = new ToolClickRoutedEventHandler(ExportToJava) });

                    Image ToolImage2 = new Image();
                    file = BitmapImageToFile(Resources._JSONdoc_16x16);
                    ToolImage2.Source = new BitmapImage(new Uri(file));
                    mTools.Add(new TextEditorToolBarItem() { Image = ToolImage2, toolTip = "Export to json", clickHandler = new ToolClickRoutedEventHandler(ExportToJson) });

                    Image ToolImage3 = new Image();
                    file = BitmapImageToFile(Resources._Compile_16x16);
                    ToolImage3.Source = new BitmapImage(new Uri(file));
                    mTools.Add(new TextEditorToolBarItem() { Image = ToolImage3, toolTip = "Compile", clickHandler = new ToolClickRoutedEventHandler(Compile) });

                    Image ToolImage4 = new Image();
                    file = BitmapImageToFile(Resources._Add_16x16);
                    ToolImage4.Source = new BitmapImage(new Uri(file));
                    mTools.Add(new TextEditorToolBarItem() { Image = ToolImage4, toolTip = "Add Interaction", clickHandler = new ToolClickRoutedEventHandler(AddNewInteractionTemplate) });
                }
                return mTools;
            }
        }

        private void AddNewInteractionTemplate(TextEditorToolRoutedEventArgs Args)
        {
            Args.txt = Args.txt.Substring(0,Args.CaretLocation) + Environment.NewLine + Resources.NewInteractionTemplate + Environment.NewLine + Args.txt.Substring(Args.CaretLocation) ;
        }

        private void Compile(TextEditorToolRoutedEventArgs Args)
        {
            ServiceVirtualization SV = new ServiceVirtualization();
            try
            {
                List < int > BodyLines = new List<int>();
                List<int> InteractionLines = new List<int>();
                string EditorText = Args.txt;
                Args.ErrorLines = new List<int>();
                string[] lines = EditorText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length; i++)
                {
                    string st = lines[i].TrimStart();

                    if (st.StartsWith("PACT:"))
                    { }
                    else if (string.IsNullOrEmpty(st))
                    { }
                    else if (st.StartsWith("@"))
                    {
                        string[] Tags = st.Split(new string[] { " " }, StringSplitOptions.None);
                        foreach (string tag in Tags)
                        {
                            if (!tag.TrimStart().StartsWith("@"))
                            {
                                Args.ErrorLines.Add(i + 2);
                                Args.ErrorMessage = Args.ErrorMessage + "Line number: "+ (i + 2) + "-Tags must start with @" + Environment.NewLine;

                            }
                        }
                    }
                    else if (st.StartsWith("Interaction") || st.StartsWith("Interaction Outline"))
                    {
                        InteractionLines.Add(i);
                        if (lines[i + 1].TrimStart().StartsWith("Given"))
                        {
                            if (string.IsNullOrEmpty(GetStringBetween(lines[i + 1], "\"", "\"")))
                            {
                                Args.ErrorLines.Add(i + 2);
                                Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 2) + "-Given value cannot be empty" + Environment.NewLine;
                            }
                        }
                        else
                        {
                            Args.ErrorLines.Add(i + 2);
                            Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 2) + "-Given is missing." + Environment.NewLine;
                            i = i - 1;
                        }

                        if (lines[i + 2].TrimStart().StartsWith("Upon Receiving"))
                        {
                            if (string.IsNullOrEmpty(GetStringBetween(lines[i + 2], "\"", "\"")))
                            {
                                Args.ErrorLines.Add(i + 3);
                                Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 3) + "-Upon Receiving value cannot be empty" + Environment.NewLine;
                            }
                        }
                        else
                        {
                            Args.ErrorLines.Add(i + 3);
                            Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 3) + "-Upon Receiving is missing." + Environment.NewLine;
                            i = i - 1;
                        }

                        if (lines[i + 3].TrimStart().StartsWith("Method"))
                        {
                            if (string.IsNullOrEmpty(GetStringBetween(lines[i + 3], "\"", "\"")))
                            {
                                Args.ErrorLines.Add(i + 4);
                                Args.ErrorMessage = Args.ErrorMessage + "Line number: " + ( i + 4 )+ "-Method value cannot be empty" + Environment.NewLine;
                            }
                        }
                        else
                        {
                            Args.ErrorLines.Add(i + 4);
                            Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 4) + "-Method is missing." + Environment.NewLine;
                            i = i - 1;
                        }

                        if (lines[i + 4].TrimStart().StartsWith("Path"))
                        {
                            if (string.IsNullOrEmpty(GetStringBetween(lines[i + 4], "\"", "\"")))
                            {
                                Args.ErrorLines.Add(i + 5);
                                Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 5) + "-Path value cannot be empty" + Environment.NewLine;
                            }
                        }
                        else
                        {
                            Args.ErrorLines.Add(i + 5);
                            Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 5) + "-Path is missing." + Environment.NewLine;
                            i = i - 1;
                        }

                        if (lines[i + 5].TrimStart().StartsWith("Headers:"))
                        {
                            string HeadersColumns = lines[i + 6].TrimStart();
                            Dictionary<string, string> HeadersDict = GetDictBetween(HeadersColumns, "|", "|", true);
                            if (!HeadersDict.ContainsKey("Key") || HeadersDict["Key"] != "Value")
                            {
                                Args.ErrorLines.Add(i + 7);
                                Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 7) + "-Headers Columns has to be Key and Value" + Environment.NewLine;
                            }
                        }
                        else
                        {
                            Args.ErrorLines.Add(i + 7);
                            Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 7) + "-Headers definition is missing." + Environment.NewLine;
                            i = i - 1;
                        }
                        int j = i + 7;

                        while (lines[j].TrimStart().StartsWith("|"))
                        {
                            string line = lines[j].TrimStart();
                            Dictionary<string, string> ValuesDict = GetDictBetween(line, "|", "|");
                            if (ValuesDict.ContainsKey("") || ValuesDict.ContainsValue(""))
                            {
                                Args.ErrorLines.Add(j + 1);
                                Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (j + 1) + "-Headers Values have to be wrapped by quotes and cannot be empty" + Environment.NewLine;
                            }
                            j = j + 1;
                        }
                        i = j;
                        if (lines[i].TrimStart().StartsWith("Body"))
                        {
                            BodyLines.Add(i+2);
                        }
                        else
                        {
                            Args.ErrorLines.Add(i + 1);
                            Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 1) + "-Body definition is missing.";
                            i = i - 1;
                        }

                        j = i + 1;

                        while (lines[j].TrimStart().StartsWith("{"))
                        {
                            string line = lines[j].TrimStart();
                            //TODO: Add code for validating Body
                            j = j + 1;
                        }

                        i = j;

                        if (lines[i].TrimStart().StartsWith("Will Respond With"))
                        {

                        }
                        else
                        {
                            Args.ErrorLines.Add(i + 1);
                            Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 1) + "-Will Respond With is missing." + Environment.NewLine;
                            i = i - 1;
                        }

                        if (lines[i + 1].TrimStart().StartsWith("Status"))
                        {
                            if (string.IsNullOrEmpty(GetStringBetween(lines[i + 1], "\"", "\"")))
                            {
                                Args.ErrorLines.Add(i + 2);
                                Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 2) + "-Status value cannot be empty" + Environment.NewLine;
                            }
                        }
                        else
                        {
                            Args.ErrorLines.Add(i + 3);
                            Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 3) + "-Status is missing." + Environment.NewLine;
                            i = i - 1;
                        }

                        if (lines[i + 2].TrimStart().StartsWith("Headers:"))
                        {
                            string HeadersColumns = lines[i + 3].TrimStart();
                            Dictionary<string, string> HeadersDict = GetDictBetween(HeadersColumns, "|", "|", true);
                            if (!HeadersDict.ContainsKey("Key") || HeadersDict["Key"] != "Value")
                            {
                                Args.ErrorLines.Add(i + 4);
                                Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 4) + "-Headers Columns has to be Key and Value" + Environment.NewLine;
                            }
                        }
                        else
                        {
                            Args.ErrorLines.Add(i + 4);
                            Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 4) + "-Headers definition is missing." + Environment.NewLine;
                        }

                        j = i + 4;

                        while (lines[j].TrimStart().StartsWith("|"))
                        {
                            string line = lines[j].TrimStart();
                            Dictionary<string, string> ValuesDict = GetDictBetween(line, "|", "|");
                            if (ValuesDict.ContainsKey("") || ValuesDict.ContainsValue(""))
                            {
                                Args.ErrorLines.Add(j +1);
                                Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (j + 1) + "-Headers Values have to be wrapped by quotes and cannot be empty" + Environment.NewLine;
                            }
                            j = j + 1;
                        }
                        i = j;

                        if (lines[i].TrimStart().StartsWith("Body"))
                        {
                            BodyLines.Add(i+2);
                        }
                        else
                        {
                            Args.ErrorLines.Add(i + 1);
                            Args.ErrorMessage = Args.ErrorMessage + "Line number: " + (i + 1) + "-Body definition is missing." + Environment.NewLine;
                            i = i - 1;
                        }

                        j = i + 1;

                        while (lines[j].TrimStart().StartsWith("{"))
                        {
                            string line = lines[j].TrimStart();
                            //TODO: Add code for validating Body
                            j = j + 1;
                        }
                        i = j;
                    }
                }

                if (!string.IsNullOrEmpty(Args.ErrorMessage))
                {
                    SV.MockProviderService.Stop();
                    return;
                }

                List<ProviderServiceInteraction> PSIList = ParsePACT(Args.txt);
                //TODO: reuse the same code port is dummy, need to create SV constructor for creating json
                int InteractionsIndexes = 0;
                try
                {
                    foreach (ProviderServiceInteraction PSI in PSIList)
                    {
                        SV.AddInteraction(PSI);
                        InteractionsIndexes = InteractionsIndexes + 1;
                    }
                }
                catch (Exception ex)
                {
                    Args.ErrorLines.Add(InteractionLines[InteractionsIndexes]+1);
                    Args.ErrorMessage = Args.ErrorMessage +  "Failed to Build Interaction no- " + (InteractionsIndexes+1) + Environment.NewLine;
                    throw ex;
                }

                //Validate Json Body
                int BodysIndexes = 0;
                foreach (ProviderServiceInteraction PSI in PSIList)
                {
                    try
                    {
                        KeyValuePair<string, string> KVP = new KeyValuePair<string, string>("Content-Type", "application/json");

                        if (PSI.Request.Headers.Contains(KVP))
                        {
                            JObject BodyJsonObj = null;
                            Dictionary<string, string> BodydictObj = null;
                            BodyJsonObj = JObject.Parse(PSI.Request.Body);
                            BodydictObj = BodyJsonObj.ToObject<Dictionary<string, string>>();
                        }
                        BodysIndexes = BodysIndexes + 1;
                        if (PSI.Response.Headers.Contains(KVP))
                        {
                            JObject BodyJsonObj = null;
                            Dictionary<string, string> BodydictObj = null;
                            BodyJsonObj = JObject.Parse(PSI.Response.Body);
                            BodydictObj = BodyJsonObj.ToObject<Dictionary<string, string>>();
                        }
                        BodysIndexes = BodysIndexes + 1;
                    }
                    catch (Exception)
                    {
                        Args.ErrorLines.Add(BodyLines[BodysIndexes]);
                        Args.ErrorMessage = Args.ErrorMessage + "Body's Json is not in the correct Json format" + Environment.NewLine;
                    }
                }

                SV.MockProviderService.Stop();
                if (string.IsNullOrEmpty(Args.ErrorMessage))
                    Args.SuccessMessage = "Compilation Passed Successfully";
            }
            catch (Exception ex)
            {
                Args.ErrorMessage = "Compilation Failed"  +Environment.NewLine + Args.ErrorMessage + ex.Message + Environment.NewLine + ex.InnerException;
                SV.MockProviderService.Stop();
            }
        }

        public override string EditorName
        {
            get
            {
                return "PACTEditor";
            }
        }

        public override string EditorID
        {
            get
            {
                return "PACTEditorID";
            }
        }

        public override string GetTemplatesByExtensions(string plugInExtension)
        {
            return Resources.PactTemplate;
        }

        public override PlugInEditorFoldingStrategy EditorStrategy
        {
            get
            {
                List<string> FoldingTitles = new List<string>();
                FoldingTitles.Add("Interaction:");
                FoldingTitles.Add("Interaction Outline:");

                PlugInEditorFoldingStrategy PIEFS = new PlugInEditorFoldingStrategy();
                PIEFS.Tag = "@";
                PIEFS.FoldingTitles = FoldingTitles;
                return PIEFS;
            }
        }

        public override Dictionary<string, string> TableEditorPageDict
        {
            get
            {
                Dictionary<string, string> Dict = new Dictionary<string, string>();
                Dict.Add("StartKeyWord", "Headers:");
                Dict.Add("EndKeyWord", "Body");
                Dict.Add("KeyWordForTableLocationIndication", "Will Respond With");
                return Dict;
            }
        }

        public override List<string> CompletionDataKeyWords
        {
            get
            {
                List<string> KeyWords = new List<string>();
                KeyWords.Add("Interaction");
                KeyWords.Add("Given");
                KeyWords.Add("Upon Receiving");
                KeyWords.Add("Status");
                KeyWords.Add("Method");
                KeyWords.Add("Path");
                KeyWords.Add("Headers:");
                KeyWords.Add("Body:");
                KeyWords.Add("Body Examples:");
                KeyWords.Add("Will Respond With");
                return KeyWords;
            }
        }

        string PreviusSelectedFolder = string.Empty;

        private void ExportToJson(TextEditorToolRoutedEventArgs Args)
        {
            Compile(Args);
            if (!string.IsNullOrEmpty(Args.ErrorMessage))
                return;

            string SaveToPath = string.Empty;
            if (!OpenFolderDialog("Select folder for saving the created Json file", ref SaveToPath))
            {
                Args.ErrorMessage = "Export To Json Aborted";
                return;
            }

            List<ProviderServiceInteraction> PSIList = ParsePACT(Args.txt);
            string ServiceConsumer = ParseProperty(Args.txt,"Consumer");
            string HasPactWith = ParseProperty(Args.txt, "Provider");
            ServiceVirtualization SV = new ServiceVirtualization(0, SaveToPath, ServiceConsumer, HasPactWith);
            try
            {
                // TODO: use simple json save obj to file - might be faster - but will be good to comapre with below PACT lib
                //TODO: reuse the same code port is dummy, need to create SV constructor for creating json
                foreach (ProviderServiceInteraction PSI in PSIList)
                {
                    SV.AddInteraction(PSI);
                }
                SV.PactBuilder.Build();  // will save it in C:\temp\pacts - TODO: config
                SV.MockProviderService.Stop();
                Args.SuccessMessage = "Json File Exported Successfully";
                Process.Start(SaveToPath);
            }
            catch (Exception ex)
            {
                Args.ErrorMessage = "Json File Export Failed" + Environment.NewLine + ex.Message;
                SV.MockProviderService.Stop();
            }
        }

        private string ParseProperty(string txt, string property)
        {
            string[] lines = txt.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string s in lines)
            {
                string st = s.TrimStart();
                if (st.StartsWith(property))
                {
                    return GetStringBetween(s, "\"", "\"");
                }
            }
            return string.Empty;
        }
        
        private bool OpenFolderDialog(string Description,ref string SaveToPath)
        {
            var dlgf = new System.Windows.Forms.FolderBrowserDialog();
            dlgf.Description = Description;
            if (string.IsNullOrEmpty(PreviusSelectedFolder))
                dlgf.RootFolder = Environment.SpecialFolder.MyComputer;
            else
                dlgf.SelectedPath = PreviusSelectedFolder;
            dlgf.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult resultf = dlgf.ShowDialog();
            if (resultf == System.Windows.Forms.DialogResult.OK)
            {
                SaveToPath = dlgf.SelectedPath;
                PreviusSelectedFolder = SaveToPath;
            }
            else
            {
                return false;
            }
            return true;
        }

        private bool OpenFileDialog(string Description, ref string SelectedFilePath)
        {

            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "TXT Files (*.txt)|*.txt|All Files (*.*)|*.*";
            dlg.FilterIndex = 1;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SelectedFilePath = dlg.FileName;
            }
            else
            {
                return false;
            }
            return true;
        }

        private void ExportToJava(TextEditorToolRoutedEventArgs Args)
        {
            Compile(Args);
            if (!string.IsNullOrEmpty(Args.ErrorMessage))
                return;

            string SaveToPath = string.Empty;
            if (!OpenFolderDialog("Select folder for saving the created Java file",ref SaveToPath))
            {
                Args.ErrorMessage = "Export To Java Aborted";
                return;
            }
               
            string JavaTemaplate = string.Empty;
            if (MessageBox.Show("Do you want to use a default Java template?", "Java template to be used", MessageBoxButton.YesNo, MessageBoxImage.Question,MessageBoxResult.Yes) == MessageBoxResult.No)
            {
                string SelectedFilePath = string.Empty;
                if (OpenFileDialog("Select Java Temaplate File", ref SelectedFilePath))
                    JavaTemaplate = System.IO.File.ReadAllText(SelectedFilePath);
                else
                {
                    Args.ErrorMessage = "Export To Java Aborted";
                    return;
                }
            }
            else
            {
                JavaTemaplate = Resources.JavaTemplate;
            }

            List<ProviderServiceInteraction> PSIList = ParsePACT(Args.txt);
            string Consumer = ParseProperty(Args.txt, "Consumer");
            string Provider = ParseProperty(Args.txt, "Provider");
            GenerateJava(JavaTemaplate, PSIList, SaveToPath, Consumer, Provider, Args);
            if (string.IsNullOrEmpty(Args.ErrorMessage))
            {
                Process.Start(SaveToPath);
                Args.SuccessMessage = "Export to Java finished successfully";
            }
        }

        private List<ProviderServiceInteraction> ParsePACT(string txt)
        { 
            // First we parse the text to list of PSIs
            string[] lines = txt.Split(new string[] { Environment.NewLine}, StringSplitOptions.None);
            List<ProviderServiceInteraction> PSIList = new List<ProviderServiceInteraction>();
            ProviderServiceInteraction PSI = null;
            bool RequestHeadersPassed = false; 
            foreach (string s in lines)
            {
                string st = s.TrimStart();
                if (st.StartsWith("Interaction"))
                {
                    RequestHeadersPassed = false;
                    if (PSI != null)
                    {
                        PSIList.Add(PSI);
                    }
                    PSI = new ProviderServiceInteraction();
                    PSI.Request = new ProviderServiceRequest();
                    PSI.Response = new ProviderServiceResponse();
                    PSI.Request.Headers = new Dictionary<string, string>();
                    PSI.Response.Headers = new Dictionary<string, string>();
                }
                else if (st.StartsWith("Given"))
                {
                    string st2 = GetStringBetween(s, "\"", "\"");
                    PSI.ProviderState = st2;
                }
                else if (st.StartsWith("Upon Receiving"))
                {
                    string st2 = GetStringBetween(s, "\"", "\"");
                    PSI.Description = st2;

                }
                else if (st.StartsWith("Status"))
                {
                    string st2 = GetStringBetween(s, "\"", "\"").ToUpper().Trim();
                    if (st2 == "OK" || st2 == "200")
                    {
                        PSI.Response.Status = 200;
                    }
                    else if (st2 == "CONTINUE" || st2 == "100")
                    {
                        PSI.Response.Status = 100;
                    }
                    else if (st2 == "CREATED" || st2 == "201")
                    {
                        PSI.Response.Status = 201;
                    }
                    else if (st2 == "ACCEPTED" || st2 == "202")
                    {
                        PSI.Response.Status = 202;
                    }
                    else if (st2 == "NON-AUTHORITATIVE INFORMATION" || st2 == "203")
                    {
                        PSI.Response.Status = 203;
                    }
                    else if (st2 == "NO CONTENT" || st2 == "204")
                    {
                        PSI.Response.Status = 204;
                    }
                    else if (st2 == "RESET CONTENT" || st2 == "205")
                    {
                        PSI.Response.Status = 205;
                    }
                    else if (st2 == "PARTIAL CONTENT" || st2 == "206")
                    {
                        PSI.Response.Status = 206;
                    }
                    else if (st2 == "MULTI-STATUS" || st2 == "207")
                    {
                        PSI.Response.Status = 207;
                    }
                    else if (st2 == "IM USED" || st2 == "226")
                    {
                        PSI.Response.Status = 226;
                    }
                    else if (st2 == "MULTIPLE CHOICE" || st2 == "300")
                    {
                        PSI.Response.Status = 300;
                    }
                    else if (st2 == "MOVED PERMANENTLY" || st2 == "301")
                    {
                        PSI.Response.Status = 301;
                    }
                    else if (st2 == "FOUND" || st2 == "302")
                    {
                        PSI.Response.Status = 302;
                    }
                    else if (st2 == "SEE OTHER" || st2 == "303")
                    {
                        PSI.Response.Status = 303;
                    }
                    else if (st2 == "NOT MODIFIED" || st2 == "304")
                    {
                        PSI.Response.Status = 304;
                    }
                    else if (st2 == "USE PROXY" || st2 == "305")
                    {
                        PSI.Response.Status = 305;
                    }
                    else if (st2 == "UNUSED" || st2 == "306")
                    {
                        PSI.Response.Status = 306;
                    }
                    else if (st2 == "TEMPORARY REDIRECT" || st2 == "307")
                    {
                        PSI.Response.Status = 307;
                    }
                    else if (st2 == "PERMANENT REDIRECT" || st2 == "308")
                    {
                        PSI.Response.Status = 308;
                    }
                    else if (st2 == "BAD REQUEST" || st2 == "400")
                    {
                        PSI.Response.Status = 400;
                    }
                    else if (st2 == "UNAUTHORIZED" || st2 == "401")
                    {
                        PSI.Response.Status = 401;
                    }
                    else if (st2 == "PAYMENT REQUIRED" || st2 == "402")
                    {
                        PSI.Response.Status = 402;
                    }
                    else if (st2 == "FORBIDDEN" || st2 == "403")
                    {
                        PSI.Response.Status = 403;
                    }
                    else if (st2 == "NOT FOUND" || st2 == "404")
                    {
                        PSI.Response.Status = 404;
                    }
                    else if (st2 == "METHOD NOT ALLOWE" || st2 == "405")
                    {
                        PSI.Response.Status = 405;
                    }
                    else if (st2 == "NOT ACCEPTABLE" || st2 == "406")
                    {
                        PSI.Response.Status = 406;
                    }
                    else if (st2 == "PROXY AUTHENTICATION REQUIRED" || st2 == "407")
                    {
                        PSI.Response.Status = 407;
                    }
                    else if (st2 == "REQUEST TIMEOUT" || st2 == "408")
                    {
                        PSI.Response.Status = 408;
                    }
                    else if (st2 == "CONFLICT" || st2 == "409")
                    {
                        PSI.Response.Status = 409;
                    }
                    else if (st2 == "GONE" || st2 == "410")
                    {
                        PSI.Response.Status = 410;
                    }
                    else if (st2 == "LENGTH REQUIRED" || st2 == "411")
                    {
                        PSI.Response.Status = 411;
                    }
                    else if (st2 == "PRECONDITION FAILED" || st2 == "412")
                    {
                        PSI.Response.Status = 412;
                    }
                    else if (st2 == "PAYLOAD TOO LARGE" || st2 == "413")
                    {
                        PSI.Response.Status = 413;
                    }
                    else if (st2 == "URI TOO LONG" || st2 == "414")
                    {
                        PSI.Response.Status = 414;
                    }
                    else if (st2 == "UNSUPPORTED MEDIA TYPE" || st2 == "415")
                    {
                        PSI.Response.Status = 415;
                    }
                    else if (st2 == "REQUESTED RANGE NOT SATISFIABLE" || st2 == "416")
                    {
                        PSI.Response.Status = 416;
                    }
                    else if (st2 == "EXPECTATION FAILED" || st2 == "417")
                    {
                        PSI.Response.Status = 417;
                    }
                    else if (st2 == "I'M A TEAPOT" || st2 == "418")
                    {
                        PSI.Response.Status = 418;
                    }
                    else if (st2 == "MISDIRECTED REQUEST" || st2 == "421")
                    {
                        PSI.Response.Status = 421;
                    }
                    else if (st2 == "UNPROCESSABLE ENTITY" || st2 == "422")
                    {
                        PSI.Response.Status = 422;
                    }
                    else if (st2 == "LOCKED" || st2 == "423")
                    {
                        PSI.Response.Status = 423;
                    }
                    else if (st2 == "FAILED DEPENDENCY" || st2 == "424")
                    {
                        PSI.Response.Status = 424;
                    }
                    else if (st2 == "UPGRADE REQUIRED" || st2 == "426")
                    {
                        PSI.Response.Status = 426;
                    }
                    else if (st2 == "PRECONDITION REQUIRED" || st2 == "428")
                    {
                        PSI.Response.Status = 428;
                    }
                    else if (st2 == "TOO MANY REQUESTS" || st2 == "431")
                    {
                        PSI.Response.Status = 431;
                    }
                    else if (st2 == "UNAVAILABLE FOR LEGAL REASONS" || st2 == "451")
                    {
                        PSI.Response.Status = 451;
                    }
                    else if (st2 == "INTERNAL SERVER ERROR" || st2 == "500")
                    {
                        PSI.Response.Status = 500;
                    }
                    else if (st2 == "NOT IMPLEMENTED" || st2 == "501")
                    {
                        PSI.Response.Status = 501;
                    }
                    else if (st2 == "BAD GATEWAY" || st2 == "502")
                    {
                        PSI.Response.Status = 502;
                    }
                    else if (st2 == "GATEWAY TIMEOUT" || st2 == "503")
                    {
                        PSI.Response.Status = 503;
                    }
                    else if (st2 == "HTTP VERSION NOT SUPPORTED" || st2 == "505")
                    {
                        PSI.Response.Status = 505;
                    }
                    else if (st2 == "VARIANT ALSO NEGOTIATES" || st2 == "506")
                    {
                        PSI.Response.Status = 506;
                    }
                    else if (st2 == "INSUFFICIENT STORAGE" || st2 == "507")
                    {
                        PSI.Response.Status = 507;
                    }
                    else if (st2 == "LOOP DETECTED" || st2 == "508")
                    {
                        PSI.Response.Status = 508;
                    }
                    else if (st2 == "NOT EXTENDED" || st2 == "510")
                    {
                        PSI.Response.Status = 510;
                    }
                    else if (st2 == "NETWORK AUTHENTICATION REQUIRED" || st2 == "511")
                    {
                        PSI.Response.Status = 511;
                    }

                    //TODO: all other response
                }
                else if (st.StartsWith("Method"))
                {
                    string st2 = GetStringBetween(s, "\"", "\"").ToUpper();
                    switch (st2)
                    {
                        case "GET":
                            PSI.Request.Method = HttpVerb.Get;
                            break;
                        case "PUT":
                            PSI.Request.Method = HttpVerb.Put;
                            break;
                        case "NOTSET":
                            PSI.Request.Method = HttpVerb.NotSet;
                            break;
                        case "POST":
                            PSI.Request.Method = HttpVerb.Post;
                            break;
                        case "DELETE":
                            PSI.Request.Method = HttpVerb.Delete;
                            break;
                        case "HEAD":
                            PSI.Request.Method = HttpVerb.Head;
                            break;
                        case "PATCH":
                            PSI.Request.Method = HttpVerb.Patch;
                            break;
                            //TODO: add the rest !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    }
                }
                else if (st.StartsWith("Path"))
                {
                    string st2 = GetStringBetween(s, "\"", "\"");
                    PSI.Request.Path = st2;
                }
                else if (st.StartsWith("Will Respond With"))
                {
                        RequestHeadersPassed = true;
                }
                else if (st.StartsWith("|"))
                {
                    Dictionary<string, string> st2 = GetDictBetween(s, "|", "|");
                    if (!st2.ContainsKey(""))
                    {
                        foreach (KeyValuePair<string, string> KVP in st2)
                            if (!RequestHeadersPassed)
                                  PSI.Request.Headers.Add(KVP);
                            else
                                  PSI.Response.Headers.Add(KVP);
                    }
                }
                else if (st.StartsWith("{"))
                {
                    string st2 = st.Substring(0, st.LastIndexOf('}')+1);
                    if (!RequestHeadersPassed)
                         PSI.Request.Body += st2;
                    else
                        PSI.Response.Body += st2;

                }
            }
            // Add the last PSI created
            PSIList.Add(PSI);

            return PSIList;
        }

        //TODO: fix me and improve!
        private void GenerateJava(string JavaTemaplate, List<ProviderServiceInteraction> pSIList, string SaveToPath, string Consumer, string Provider, TextEditorToolRoutedEventArgs Args)
        {
            //TODO: create java code for Junit like: https://github.com/DiUS/pact-jvm/blob/master/pact-jvm-consumer-junit/src/test/java/au/com/dius/pact/consumer/examples/ExampleJavaConsumerPactRuleTest.java
            //TODO: use external file/resource with all the text and place holder to replace
            
            string txt = string.Empty;
            string tab1 = "\t";
            string tab2 = "\t";

            int i = 1;
            foreach (ProviderServiceInteraction PSI in pSIList)
            {
                txt += tab2 + "//Data setup for Interaction: <<" + PSI.ProviderState + "  /  " + PSI.Description + ">> " + Environment.NewLine;
                txt += tab1 + "Map<String, String> requestHeaders" + i + " = new HashMap<>();" + Environment.NewLine;
                foreach (var header in PSI.Request.Headers)
                {
                    txt += tab1 + "requestHeaders" + i + ".put(\"" + header.Key + "\", \"" + header.Value + "\");" + Environment.NewLine;
                }
                txt += tab1 + "Map<String, String> responseHeaders" + i + " = new HashMap<>();" + Environment.NewLine;
                foreach (var header in PSI.Response.Headers)
                {
                    txt += tab1 + "responseHeaders" + i + ".put(\"" + header.Key + "\", \"" + header.Value + "\");" + Environment.NewLine;
                }
                txt += Environment.NewLine;
                i = i + 1;
            }

            txt += tab2 + "return builder" + Environment.NewLine;
            i = 1;
            foreach (ProviderServiceInteraction PSI in pSIList)
            {
                txt += tab2 + "//Add Interaction: <<" + PSI.Description + ">>" + Environment.NewLine;
                txt += tab2 + ".given(\"" + PSI.ProviderState  + "\")" + Environment.NewLine;
                txt += tab1 + tab2 + ".uponReceiving(\"" + PSI.Description + "\")" + Environment.NewLine;
                txt += tab1 + tab2 + ".path(\"" + PSI.Request.Path + "\")" + Environment.NewLine;
                txt += tab1 + tab2 + ".method(\"" + PSI.Request.Method.ToString()  + "\")" + Environment.NewLine;
                txt += tab1 + tab2 + ".headers(requestHeaders" + i + ")" + Environment.NewLine;
                txt += tab1 + tab2 + ".body(\"" + PSI.Request.Body + "\")" + Environment.NewLine;
                txt += tab2 + ".willRespondWith()" + Environment.NewLine;
                txt += tab1 + tab2 + ".status(" + PSI.Response.Status + ")" + Environment.NewLine;   //TODO: get status convert OK...
                txt += tab1 + tab2 + ".headers(responseHeaders" + i + ")" + Environment.NewLine;
                txt += tab1 + tab2 + ".body(\""+ PSI.Response.Body + "\")" + Environment.NewLine;
                i = i + 1;
            }

            bool IsPlaceHolderExist = JavaTemaplate.Contains(InteractionData);

            if (!IsPlaceHolderExist)
            {
                Args.ErrorMessage = Args.ErrorMessage + "Place Holder "+ InteractionData + " is missing from the customized Java template." + Environment.NewLine + "Export to Java Process Aborted.";
                return;
            }

            string JavaFileContent = JavaTemaplate.Replace(InteractionData, txt);
            JavaFileContent = JavaFileContent.Replace(ProviderData, Provider);
            JavaFileContent = JavaFileContent.Replace(ConsumerData, Consumer);

            String timeStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");

            System.IO.File.WriteAllText(SaveToPath + @"\PactToJava" + timeStamp + ".Java", JavaFileContent);
        }

        public string GetStringBetween(string STR, string FirstString, string LastString = null)
        {
            string str = "";
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2;
            if (LastString != null)
            {
                Pos2 = STR.IndexOf(LastString, Pos1);
            }
            else
            {
                Pos2 = STR.Length;
            }

            if ((Pos2 - Pos1) > 0)
            {
                str = STR.Substring(Pos1, Pos2 - Pos1);
                return str;
            }
            else
            {
                return "";
            }
        }

        public Dictionary<string,string> GetDictBetween(string STR, string FirstString, string LastString = null,bool HeaderNeededOnly = false)
        {
            Dictionary<string, string> Dict = new Dictionary<string, string>();
            string str = "";
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2;
            if (LastString != null)
            {
                Pos2 = STR.LastIndexOf(LastString);
            }
            else
            {
                Pos2 = STR.Length;
            }

            if ((Pos2 - Pos1) > 0)
            {
                str = STR.Substring(Pos1, Pos2 - Pos1);
            }
            else
            {
                str = "";
            }
            string[] result = str.Split('|');
            if (!HeaderNeededOnly)
                for (int i = 0;i<result.Length;i++)
                    result[i] = GetStringBetween(result[i], "\"", "\"");
            Dict.Add(result[0].Trim(), result[1].Trim());
            return Dict;
        }

        public override string Title()
        {

            return "PACT Content Editor";
        }
    }
}
