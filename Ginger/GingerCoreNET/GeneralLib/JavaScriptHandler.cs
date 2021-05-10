using Amdocs.Ginger.Common;
using NUglify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{
    static public class JavaScriptHandler
    {
        public enum eJavaScriptFile
        {
            GingerLiveSpy, InjectJavaScript, draganddrop, jquery_min, PayLoad, GingerHTMLHelper, GingerLibXPath, wgxpath_install, GingerHTMLRecorder,
            ArrayBuffer, BrowserWaitForIdle, html2canvas, HTMLSpy
        }

        static public string GetJavaScriptFileContent(eJavaScriptFile javaScriptFile, bool performManifyJS=false)
        {
            string content = string.Empty;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string jsResourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(string.Format("{0}.js", javaScriptFile.ToString())));                
                using (Stream stream = assembly.GetManifestResourceStream(jsResourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to get the content of the JavaScript file called:'{0}'", javaScriptFile.ToString()), ex);
                return null;
            }

            if (performManifyJS)
            {
                return MinifyJavaScript(content);
            }

            return content;
        }

        static public string MinifyJavaScript(string script)
        {
            try
            {
                var result = Uglify.Js(script);
                if (result.Errors.Count > 0)
                {
                    foreach (UglifyError error in result.Errors)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to Minify the JS, ErrorCode:'{0}' | ErrorMessage:'{1}'", error.ErrorCode, error.Message));
                    }
                    return null;
                }
                return result.Code + ";";
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Minify the JS",ex);
                return null;
            }
        }
    }
}
