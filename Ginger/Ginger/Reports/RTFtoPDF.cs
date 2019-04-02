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
using System.Threading;
using Microsoft.Office.Interop.Word;

namespace Ginger.Reports
{
    public class RTFtoPDF
    {
        // Keep one ref for word
        private static Application appWord = null;

        public static void Convert(string RTFFileName, string PDFFileName)
        {
            // temp using Word to convert until we find a better faster option.

            // Cache the word app.
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            if (appWord == null)
            {
                appWord = new Microsoft.Office.Interop.Word.Application();
            }

            Document wordDocument = appWord.Documents.Open(RTFFileName);                        
            wordDocument.ExportAsFixedFormat(PDFFileName, WdExportFormat.wdExportFormatPDF, false, WdExportOptimizeFor.wdExportOptimizeForPrint);
            
            wordDocument.Close(WdSaveOptions.wdDoNotSaveChanges);
            wordDocument = null;

            //TODO: for speed do the cleanup only when Ginger is closing, so keep word app ready, report generation will be faster 4-5 secs to start word
            // However I tried cleanup at classs destroy but getting err as cretae word and destroy is done on othere thread
            Cleanup();
        }

        ~RTFtoPDF()
        {
            //TODO: make sure cleanup was called if appWord != null;
            if (appWord != null)
            {
                // Cleanup();
            }
        }


        static void Cleanup()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            appWord.Quit();
            appWord = null;

            // release when class destroyed            
            // Clean up the unmanaged COM resource.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            // GC needs to be called twice in order to get the Finalizers called  
            // - the first time in, it simply makes a list of what is to be  
            // finalized, the second time in, it actually is finalizing. Only  
            // then will the object do its automatic ReleaseComObject. 
            GC.Collect();
            GC.WaitForPendingFinalizers(); 
        }
    }
}
