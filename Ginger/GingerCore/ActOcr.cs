using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.GingerOCR;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore
{
    public class ActOcr : ActWithoutDriver
    {
        public override string ActionType
        {
            get
            {
                return "OCR Action to read text from PDF or Images";
            }
        }

        public override string ActionDescription
        {
            get
            {
                return "OCR Action to read text from PDF or Images";
            }
        }

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
                return "OCR Action to read text from PDF or Images";
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
            [EnumValueDescription("Read Text From PDF Single Page")]
            ReadTextFromPDFSinglePage = 2,
            [EnumValueDescription("Read Text From Table In Pdf")]
            ReadTextFromTableInPdf = 3
        }

        [IsSerializedForLocalRepository]
        public eActOcrImageOperations SelectedOcrImageOperation
        {
            get => GetOrCreateInputParam(nameof(SelectedOcrImageOperation), eActOcrImageOperations.ReadTextAfterLabel);
            set => AddOrUpdateInputParamValue(nameof(SelectedOcrImageOperation), value.ToString());
        }

        [IsSerializedForLocalRepository]
        public eActOcrPdfOperations SelectedOcrPdfOperation
        {
            get => GetOrCreateInputParam(nameof(SelectedOcrPdfOperation), eActOcrPdfOperations.ReadTextAfterLabel);
            set => AddOrUpdateInputParamValue(nameof(SelectedOcrPdfOperation), value.ToString());
        }

        [IsSerializedForLocalRepository]
        public eActOcrFileType SelectedOcrFileType
        {
            get => GetOrCreateInputParam(nameof(SelectedOcrFileType), eActOcrFileType.ReadTextFromImage);
            set => AddOrUpdateInputParamValue(nameof(SelectedOcrFileType), value.ToString());
        }

        [IsSerializedForLocalRepository]
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

        [IsSerializedForLocalRepository]
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

        [IsSerializedForLocalRepository]
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

        [IsSerializedForLocalRepository]
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

        [IsSerializedForLocalRepository]
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

        private void ProcessOutput(string txtOutput)
        {
            Dictionary<string, object> dctOutput = new Dictionary<string, object>();

            dctOutput.Add("Output", txtOutput);
            AddToOutputValues(dctOutput);
        }

        private void ExecuteImageOperation()
        {
            string resultText = string.Empty;
            Dictionary<string, object> dctOutput = new Dictionary<string, object>();
            switch (SelectedOcrImageOperation)
            {
                case eActOcrImageOperations.ReadTextAfterLabel:
                    resultText = GingerOcrOperations.ReadTextFromImageAfterLabel(OcrFilePath, FirstString);
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
                    resultText = GingerOcrOperations.ReadTextFromImageBetweenStrings(OcrFilePath, FirstString, SecondString);
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        ProcessOutput(resultText);
                    }
                    else
                    {
                        Error = "Unable to read text from Image";
                    }
                    break;
                case eActOcrImageOperations.ReadAllText:
                    resultText = GingerOcrOperations.ReadTextFromImage(OcrFilePath);
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
            string resultText = string.Empty;
            Dictionary<string, object> dctOutput = new Dictionary<string, object>();
            switch (SelectedOcrPdfOperation)
            {
                case eActOcrPdfOperations.ReadTextAfterLabel:
                    resultText = GingerOcrOperations.ReadTextAfterLabelPdf(OcrFilePath, FirstString, PageNumber, OcrPassword);
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
                    resultText = GingerOcrOperations.ReadTextBetweenLabelsPdf(OcrFilePath, FirstString, SecondString, PageNumber, OcrPassword);
                    if (!string.IsNullOrEmpty(resultText))
                    {
                        ProcessOutput(resultText);
                    }
                    else
                    {
                        Error = "Unable to read text from PDF";
                    }
                    break;
                case eActOcrPdfOperations.ReadTextFromPDFSinglePage:
                    resultText = GingerOcrOperations.ReadTextFromPdfSinglePage(OcrFilePath, PageNumber, OcrPassword);
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
                    resultText = GingerOcrOperations.ReadTextFromPdfTable(OcrFilePath, FirstString, PageNumber, OcrPassword);
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
