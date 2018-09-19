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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.Repository;
using GingerCore.Helpers;
using GingerCore.Platforms;
using GingerCore.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

// This class is for dummy act - good for agile, and to be replace later on when real
//  act is availble, so tester can write the step to be.
namespace GingerCore.Actions
{
    public class ActReadTextFile : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Text File Operations"; } }
        public override string ActionUserDescription { get { return "Read/Write/Append on text file"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action to perform validations Using Value Expression editor ");

        }
        public new static partial class Fields
        {
            public static string TextFilePath = "TextFilePath";
            public static string FileActionMode = "FileActionMode";
            public static string TextToWrite = "TextToWrite";
            public static string TextFileEncoding = "TextFileEncoding";
            public static readonly string AppendAt = "AppendAt";
            public static readonly string AppendLineNumber = "AppendLineNumber";

        }

        public override string ActionEditPage { get { return "ActReadTextFileEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public enum eTextFileActionMode
        {
            Read,Write,Append
        }

        public enum eAppendAt
        {
            End,Start,SpecificLine
        }
        public enum eTextFileEncodings
        {
            UTF8, Unicode, UTF32, UTF7, ASCII, BigEndianUnicode
        }
        [IsSerializedForLocalRepository]
        public eTextFileEncodings TextFileEncoding { get; set; }

        [IsSerializedForLocalRepository]
        public eAppendAt AppendAt { get; set; }

        [IsSerializedForLocalRepository]
        public string AppendLineNumber { get; set; }
        private eTextFileActionMode mFileActionMode = eTextFileActionMode.Read;
        [IsSerializedForLocalRepository]
        public eTextFileActionMode FileActionMode
        {
            get
            {
                return mFileActionMode;
            }
            set
            {
                mFileActionMode = value;
            }
        }

        [IsSerializedForLocalRepository]
        public string TextToWrite
        {
            get
            {
                return GetInputParamValue("TextToWrite");
            }
            set
            {
                AddOrUpdateInputParamValue("TextToWrite", value);
            }
        }

        [IsSerializedForLocalRepository]
        public string TextFilePath
        {
            get
            {
                return GetInputParamValue("TextFilePath");
            }
            set
            {
                AddOrUpdateInputParamValue("TextFilePath", value);
            }
        }
        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public override String ActionType
        {
            get
            {
                return "Text File Operations";
            }
        }

        public override void Execute()
        {
            string FileText = String.Empty;
            string calculatedFilePath = GetInputParamCalculatedValue(Fields.TextFilePath);
            calculatedFilePath = calculatedFilePath.Replace(@"~\", SolutionFolder);
            string filePath = Path.GetDirectoryName(calculatedFilePath);
            bool isRootedPath = Path.IsPathRooted(filePath);
            if (!isRootedPath)
            {
                calculatedFilePath = new Uri(calculatedFilePath).LocalPath;
            }

            if (FileActionMode == eTextFileActionMode.Write)
            {
                if(String.IsNullOrEmpty(calculatedFilePath)||String.IsNullOrWhiteSpace(calculatedFilePath))
                {
                    throw new Exception("Please provide a valid file path");
                }

                WriteTextWithEncoding(calculatedFilePath,GetInputParamCalculatedValue(Fields.TextToWrite));

            }
            else if (FileActionMode == eTextFileActionMode.Append)
            {
                if (String.IsNullOrEmpty(calculatedFilePath) || String.IsNullOrWhiteSpace(calculatedFilePath))
                {
                    throw new Exception("Please provide a valid file path");
                }
                if (AppendAt == eAppendAt.End)
                {
                    AppendTextWithEncoding(calculatedFilePath,GetInputParamCalculatedValue(Fields.TextToWrite));
                }
                else
                {
                    List<string> LinesfromFile = new List<string>(ReadLinesWithEncoding(calculatedFilePath));
                    int lineNum = 1;                    
                    if (AppendAt == eAppendAt.SpecificLine)
                    {
                        int.TryParse(GetInputParamCalculatedValue(Fields.AppendLineNumber), out lineNum);
                    }

                    if(lineNum>0)
                    {
                        LinesfromFile.Insert(lineNum - 1, GetInputParamCalculatedValue(Fields.TextToWrite));
                    }                        
                    else
                    {
                        LinesfromFile.Add(GetInputParamCalculatedValue(Fields.TextToWrite));
                    }
                        
                    WriteLinesWithEncoding(calculatedFilePath, (IEnumerable<string>)LinesfromFile);                    
                }
            }
            else
            {
                FileText=ReadTextWithEncoding(calculatedFilePath);
                AddOrUpdateReturnParamActual("File Content", FileText);
            }
        }

        private void WriteTextWithEncoding(String FilePath,String WriteText)
        {
            switch(TextFileEncoding)
            {
                case eTextFileEncodings.UTF8:
                    System.IO.File.WriteAllText(FilePath, WriteText,Encoding.UTF8);
                    break;
                case eTextFileEncodings.Unicode:
                    System.IO.File.WriteAllText(FilePath, WriteText,Encoding.Unicode);
                    break;
                case eTextFileEncodings.UTF32:
                    System.IO.File.WriteAllText(FilePath, WriteText, Encoding.UTF32);
                    break;
                case eTextFileEncodings.UTF7:
                    System.IO.File.WriteAllText(FilePath, WriteText, Encoding.UTF7);
                    break;
                case eTextFileEncodings.ASCII:
                    System.IO.File.WriteAllText(FilePath, WriteText, Encoding.ASCII);
                    break;
                case eTextFileEncodings.BigEndianUnicode:
                    System.IO.File.WriteAllText(FilePath, WriteText, Encoding.BigEndianUnicode);
                    break;
                default:
                    System.IO.File.WriteAllText(FilePath,WriteText, Encoding.Default);
                    break;
            }
        }


        private void AppendTextWithEncoding(String FilePath,String WriteText)
        {
            switch(TextFileEncoding)
            {
                case eTextFileEncodings.UTF8:

                    System.IO.File.AppendAllText(FilePath, WriteText,Encoding.UTF8);
                    break;
                case eTextFileEncodings.Unicode:
                    System.IO.File.AppendAllText(FilePath, WriteText,Encoding.Unicode);
                    break;
                case eTextFileEncodings.UTF32:
                    System.IO.File.AppendAllText(FilePath, WriteText, Encoding.UTF32);
                    break;
                case eTextFileEncodings.UTF7:
                    System.IO.File.AppendAllText(FilePath, WriteText, Encoding.UTF7);
                    break;
                case eTextFileEncodings.ASCII:
                    System.IO.File.AppendAllText(FilePath, WriteText, Encoding.ASCII);
                    break;
                case eTextFileEncodings.BigEndianUnicode:
                    System.IO.File.AppendAllText(FilePath, WriteText, Encoding.BigEndianUnicode);
                    break;
                default:
                    System.IO.File.AppendAllText(FilePath,WriteText, Encoding.Default);
                    break;
            }
        }

        private string  ReadTextWithEncoding(String FilePath)
        {
            String TextfromFile=String.Empty;
            switch(TextFileEncoding)
            {
                case eTextFileEncodings.UTF8:

                    TextfromFile=System.IO.File.ReadAllText(FilePath,Encoding.UTF8);
                    break;
                case eTextFileEncodings.Unicode:
                    TextfromFile=System.IO.File.ReadAllText(FilePath,Encoding.Unicode);
                    break;
                case eTextFileEncodings.UTF32:
                    TextfromFile=System.IO.File.ReadAllText(FilePath, Encoding.UTF32);
                    break;
                case eTextFileEncodings.UTF7:
                    TextfromFile=System.IO.File.ReadAllText(FilePath, Encoding.UTF7);
                    break;
                case eTextFileEncodings.ASCII:
                    TextfromFile=System.IO.File.ReadAllText(FilePath,  Encoding.ASCII);
                    break;
                case eTextFileEncodings.BigEndianUnicode:
                    TextfromFile=System.IO.File.ReadAllText(FilePath, Encoding.BigEndianUnicode);
                    break;
                default:
                    TextfromFile=System.IO.File.ReadAllText(FilePath, Encoding.Default);
                    break;

            }
            return   TextfromFile;
        }
        private IEnumerable<string> ReadLinesWithEncoding(String FilePath)
        {
            IEnumerable<string> LinesfromFile;
            switch (TextFileEncoding)
            {
                case eTextFileEncodings.UTF8:

                    LinesfromFile = System.IO.File.ReadLines(FilePath, Encoding.UTF8);
                    break;
                case eTextFileEncodings.Unicode:
                    LinesfromFile = System.IO.File.ReadLines(FilePath, Encoding.Unicode);
                    break;
                case eTextFileEncodings.UTF32:
                    LinesfromFile = System.IO.File.ReadLines(FilePath, Encoding.UTF32);
                    break;
                case eTextFileEncodings.UTF7:
                    LinesfromFile = System.IO.File.ReadLines(FilePath, Encoding.UTF7);
                    break;
                case eTextFileEncodings.ASCII:
                    LinesfromFile = System.IO.File.ReadLines(FilePath, Encoding.ASCII);
                    break;
                case eTextFileEncodings.BigEndianUnicode:
                    LinesfromFile = System.IO.File.ReadLines(FilePath, Encoding.BigEndianUnicode);
                    break;
                default:
                    LinesfromFile = System.IO.File.ReadLines(FilePath, Encoding.Default);
                    break;
            }
            return LinesfromFile;
        }
        private void WriteLinesWithEncoding(String FilePath, IEnumerable<string> writeLines)
        {            
            switch (TextFileEncoding)
            {
                case eTextFileEncodings.UTF8:
                    System.IO.File.WriteAllLines(FilePath, writeLines,Encoding.UTF8);
                    break;
                case eTextFileEncodings.Unicode:
                    System.IO.File.WriteAllLines(FilePath, writeLines, Encoding.Unicode);
                    break;
                case eTextFileEncodings.UTF32:
                    System.IO.File.WriteAllLines(FilePath, writeLines, Encoding.UTF32);
                    break;
                case eTextFileEncodings.UTF7:
                    System.IO.File.WriteAllLines(FilePath, writeLines, Encoding.UTF7);
                    break;
                case eTextFileEncodings.ASCII:
                    System.IO.File.WriteAllLines(FilePath, writeLines, Encoding.ASCII);
                    break;
                case eTextFileEncodings.BigEndianUnicode:
                    System.IO.File.WriteAllLines(FilePath, writeLines, Encoding.BigEndianUnicode);
                    break;
                default:
                    System.IO.File.WriteAllLines(FilePath, writeLines, Encoding.Default);
                    break;
            }            
        }
    }
}