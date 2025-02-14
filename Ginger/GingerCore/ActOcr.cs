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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.Actions;
using GingerCore.GingerOCR;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;

namespace GingerCore
{
    public class ActOcr : ActWithoutDriver
    {
        public override string ActionType
        {
            get
            {
                return "OCR Action to Read Text from PDF or Images";
            }
        }

        public override string ActionDescription
        {
            get
            {
                return "Read Text Using OCR";
            }
        }

        public override eImageType Image { get { return eImageType.PDFFile; } }

        public override bool ObjectLocatorConfigsNeeded
        {
            get
            {
                return false;
            }
        }

        public override bool ValueConfigsNeeded
        {
            get
            {
                return false;
            }
        }

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

        public override string ActionEditPage
        {
            get
            {
                return "ActOcrEditPage";
            }
        }

        public override string ActionUserDescription
        {
            get
            {
                return "OCR Action to Read Text from PDF or Images";
            }
        }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to read text from PDF or Images");
            TBH.AddLineBreak();
            TBH.AddText("This action provides the capability to read text from screenshots, images or PDFs");
        }

        public enum eActOcrFileType
        {
            [EnumValueDescription("Image")]
            ReadTextFromImage = 0,
            [EnumValueDescription("PDF")]
            ReadTextFromPDF = 1
        }

        public enum eActOcrImageOperations
        {
            [EnumValueDescription("Read Text After Label")]
            ReadTextAfterLabel = 0,
            [EnumValueDescription("Read Text Between Two Strings")]
            ReadTextBetweenTwoStrings = 1,
            [EnumValueDescription("Read All Text")]
            ReadAllText = 2
        }

        public enum eActOcrPdfOperations
        {
            [EnumValueDescription("Read Text After Label")]
            ReadTextAfterLabel = 0,
            [EnumValueDescription("Read Text Between Two Strings")]
            ReadTextBetweenTwoStrings = 1,
            [EnumValueDescription("Read Text From PDF")]
            ReadTextFromPDFSinglePage = 2,
            [EnumValueDescription("Read Text From Table In Pdf")]
            ReadTextFromTableInPdf = 3
        }

        public enum eDPILevel
        {
            [EnumValueDescription("150")]
            e150 = 150,
            [EnumValueDescription("300")]
            e300 = 300,
            [EnumValueDescription("450")]
            e450 = 450,
            [EnumValueDescription("600")]
            e600 = 600,
            [EnumValueDescription("750")]
            e750 = 750,
        }
        public eDPILevel SelectedOcrDPIOperation
        {
            get => GetOrCreateInputParam(nameof(SelectedOcrDPIOperation), eDPILevel.e300);
            set => AddOrUpdateInputParamValue(nameof(SelectedOcrDPIOperation), value.ToString());
        }
        public eActOcrImageOperations SelectedOcrImageOperation
        {
            get => GetOrCreateInputParam(nameof(SelectedOcrImageOperation), eActOcrImageOperations.ReadTextAfterLabel);
            set => AddOrUpdateInputParamValue(nameof(SelectedOcrImageOperation), value.ToString());
        }
        public eActOcrPdfOperations SelectedOcrPdfOperation
        {
            get => GetOrCreateInputParam(nameof(SelectedOcrPdfOperation), eActOcrPdfOperations.ReadTextAfterLabel);
            set => AddOrUpdateInputParamValue(nameof(SelectedOcrPdfOperation), value.ToString());
        }
        public eActOcrFileType SelectedOcrFileType
        {
            get => GetOrCreateInputParam(nameof(SelectedOcrFileType), eActOcrFileType.ReadTextFromImage);
            set => AddOrUpdateInputParamValue(nameof(SelectedOcrFileType), value.ToString());
        }

        public string OcrFilePath
        {
            get
            {
                return GetOrCreateInputParam(nameof(OcrFilePath)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(OcrFilePath), value);
            }
        }

        public string OcrPassword
        {
            get
            {
                return GetOrCreateInputParam(nameof(OcrPassword)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(OcrPassword), value);
            }
        }

        public string PageNumber
        {
            get
            {
                return GetOrCreateInputParam(nameof(PageNumber)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(PageNumber), value);
            }
        }

        public string FirstString
        {
            get
            {
                return GetOrCreateInputParam(nameof(FirstString)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FirstString), value);
            }
        }

        public string SecondString
        {
            get
            {
                return GetOrCreateInputParam(nameof(SecondString)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SecondString), value);
            }
        }

        public enum eTableElementRunColOperator
        {
            [EnumValueDescription("Equals")]
            Equals,
            [EnumValueDescription("Not Equals")]
            NotEquals,
            [EnumValueDescription("Contains")]
            Contains,
            [EnumValueDescription("Not Contains")]
            NotContains,
            [EnumValueDescription("Starts With")]
            StartsWith,
            [EnumValueDescription("Not Starts With")]
            NotStartsWith,
            [EnumValueDescription("Ends With")]
            EndsWith,
            [EnumValueDescription("Not Ends With")]
            NotEndsWith
        }

        public eTableElementRunColOperator ElementLocateBy
        {
            get => GetOrCreateInputParam(nameof(ElementLocateBy), eTableElementRunColOperator.Equals);
            set => AddOrUpdateInputParamValue(nameof(ElementLocateBy), value.ToString());
        }

        private string mGetFromRowNumber { get; set; } = "0";

        public string GetFromRowNumber
        {
            get
            {
                return GetOrCreateInputParam(nameof(GetFromRowNumber), "0").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(GetFromRowNumber), value);
            }
        }

        public string ConditionColumnName
        {
            get => GetOrCreateInputParam(nameof(ConditionColumnName)).Value;
            set => AddOrUpdateInputParamValue(nameof(ConditionColumnName), value);
        }

        public string ConditionColumnValue
        {
            get => GetOrCreateInputParam(nameof(ConditionColumnValue)).Value;
            set => AddOrUpdateInputParamValue(nameof(ConditionColumnValue), value);
        }

        public string UseRowNumber
        {
            get => GetOrCreateInputParam(nameof(UseRowNumber)).Value;
            set => AddOrUpdateInputParamValue(nameof(UseRowNumber), value);
        }

        private void ProcessOutput(string txtOutput)
        {
            Dictionary<string, object> dctOutput = new Dictionary<string, object>
            {
                { "Output", txtOutput }
            };
            AddToOutputValues(dctOutput);
        }

        private void ExecuteImageOperation()
        {
            string resultText = string.Empty;
            Dictionary<string, object> dctOutput = [];
            switch (SelectedOcrImageOperation)
            {
                case eActOcrImageOperations.ReadTextAfterLabel:
                    resultText = GingerOcrOperations.ReadTextFromImageAfterLabel(ValueExpression.Calculate(OcrFilePath), ValueExpression.Calculate(FirstString));
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        ProcessOutput(resultText);
                    }
                    else
                    {
                        Error = "Unable to read text from Image";
                    }
                    break;
                case eActOcrImageOperations.ReadTextBetweenTwoStrings:
                    string err = string.Empty;

                    resultText = GingerOcrOperations.ReadTextFromImageBetweenStrings(ValueExpression.Calculate(OcrFilePath), ValueExpression.Calculate(FirstString), ValueExpression.Calculate(SecondString), ref err);
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        ProcessOutput(resultText);
                    }
                    else
                    {
                        Error = err;
                    }
                    break;
                case eActOcrImageOperations.ReadAllText:
                    resultText = GingerOcrOperations.ReadTextFromImage(ValueExpression.Calculate(OcrFilePath));
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        ProcessOutput(resultText);
                    }
                    else
                    {
                        Error = "Unable to read text from Image";
                    }
                    break;
                default:
                    //do nothing
                    break;
            }
        }

        private void ExecutePdfOperation()
        {

            // Password Can be entered in 2 ways 
            // 1. Directly Entering the password
            // 2. using ValueExpressions
            // If ValueExpression is used , Calculate returns the decrypted value , else it will return the encrypted value
            // Hence before decrpyting , checking if the string is encrypted is required otherwise an error is thrown.
            string password = ValueExpression.Calculate(OcrPassword);
            string decryptedPassword = (EncryptionHandler.IsStringEncrypted(password)) ? EncryptionHandler.DecryptwithKey(password) : password;
            string resultText = string.Empty;
            Dictionary<string, object> dctOutput = [];
            switch (SelectedOcrPdfOperation)
            {
                case eActOcrPdfOperations.ReadTextAfterLabel:
                    resultText = GingerOcrOperations.ReadTextAfterLabelPdf(ValueExpression.Calculate(OcrFilePath), ValueExpression.Calculate(FirstString), (int)SelectedOcrDPIOperation, ValueExpression.Calculate(PageNumber), decryptedPassword);
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        ProcessOutput(resultText);
                    }
                    else
                    {
                        Error = "Unable to read text from PDF";
                    }
                    break;
                case eActOcrPdfOperations.ReadTextBetweenTwoStrings:
                    string err = string.Empty;
                    resultText = GingerOcrOperations.ReadTextBetweenLabelsPdf(ValueExpression.Calculate(OcrFilePath), ValueExpression.Calculate(FirstString),
                        ValueExpression.Calculate(SecondString), ValueExpression.Calculate(PageNumber), (int)SelectedOcrDPIOperation, ref err, decryptedPassword);
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        ProcessOutput(resultText);
                    }
                    else
                    {
                        Error = err;
                    }
                    break;
                case eActOcrPdfOperations.ReadTextFromPDFSinglePage:
                    resultText = GingerOcrOperations.ReadTextFromPdfSinglePage(ValueExpression.Calculate(OcrFilePath), ValueExpression.Calculate(PageNumber), (int)SelectedOcrDPIOperation, decryptedPassword);
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        ProcessOutput(resultText);
                    }
                    else
                    {
                        Error = "Unable to read text from PDF";
                    }
                    break;
                case eActOcrPdfOperations.ReadTextFromTableInPdf:
                    if (string.IsNullOrEmpty(GetFromRowNumber))
                    {
                        GetFromRowNumber = "0";
                    }
                    resultText = GingerOcrOperations.ReadTextFromPdfTable(ValueExpression.Calculate(OcrFilePath), ValueExpression.Calculate(FirstString), ValueExpression.Calculate(PageNumber), bool.Parse(UseRowNumber),
                        int.Parse(ValueExpression.Calculate(GetFromRowNumber)), ElementLocateBy, ValueExpression.Calculate(ConditionColumnName),
                        ValueExpression.Calculate(ConditionColumnValue), decryptedPassword);
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        ProcessOutput(resultText);
                    }
                    else
                    {
                        Error = "Unable to read text from PDF";
                    }
                    break;
                default:
                    //do nothing
                    break;
            }
        }

        public override void Execute()
        {
            switch (SelectedOcrFileType)
            {
                case eActOcrFileType.ReadTextFromImage:
                    ExecuteImageOperation();
                    break;
                case eActOcrFileType.ReadTextFromPDF:
                    ExecutePdfOperation();
                    break;
                default:
                    //do nothing
                    break;
            }
        }
    }
}
