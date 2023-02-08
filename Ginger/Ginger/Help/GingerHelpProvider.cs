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

using Amdocs.Ginger.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Ginger.Help
{
    static class GingerHelpProvider
    {
        public static readonly DependencyProperty HelpStringProperty =
           DependencyProperty.RegisterAttached("HelpString", typeof(string), typeof(GingerHelpProvider));

        static GingerHelpProvider()
        {
            CommandManager.RegisterClassCommandBinding(typeof(FrameworkElement),
            new CommandBinding(
            ApplicationCommands.Help,
            new ExecutedRoutedEventHandler(Executed),
            new CanExecuteRoutedEventHandler(CanExecute)));
        }

        static private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // We check which element have Help key once found we return true
            // Click F1 on text box, if help exist it will show if not it will go to the parent control and up until found or done.

            FrameworkElement senderElement = sender as FrameworkElement;
            if (GingerHelpProvider.GetHelpString(senderElement) != null)
                e.CanExecute = true;
        }

        static private void Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Once help key is found it call execute to show the help
            string HS = GingerHelpProvider.GetHelpString(sender as FrameworkElement);
            General.ShowGingerHelpWindow(HS);
        }

        public static string GetHelpString(FrameworkElement frameworkElement)
        {
            return (string)frameworkElement.GetValue(HelpStringProperty);
        }

        public static void SetHelpString(DependencyObject obj, string value)
        {
            obj.SetValue(HelpStringProperty, value);
        }

        public static void ShowHelpLibrary(string searchText="")
        {
            if (!ShowOfflineHelpLibrary(searchText))
            {
                ShowOnlineHelpLibrary(searchText);
            }
        }

        public static bool ShowOfflineHelpLibrary(string searchText="", bool silent=true)
        {
            try
            {
                string helpLibIndex = System.IO.Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Help", "Library", "index.html");
                if (File.Exists(helpLibIndex))
                {
                    //start local help lib
                    ProcessStartInfo processStartInfo = new ProcessStartInfo();
                    processStartInfo.FileName = helpLibIndex;
                    processStartInfo.UseShellExecute = true;
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        string htmWithSearch = System.IO.Path.GetTempPath() + "OfflineGingerHelpWithSearch.htm";
                        if (File.Exists(htmWithSearch))
                        {
                            File.Delete(htmWithSearch);
                        }
                        using (StreamWriter sw = File.CreateText(htmWithSearch))
                        {
                            sw.WriteLine(string.Format("<!DOCTYPE html><html><head><meta http-equiv=\"refresh\" content=\"0; url='{0}'\" /></head></html>", helpLibIndex + "?rhsearch=" + Uri.EscapeUriString(searchText)));
                        }
                        processStartInfo.FileName = htmWithSearch;
                    }
                    Process.Start(processStartInfo);
                }
                else
                {
                    if (!silent)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Offline help library was not found");
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, string.Format("Error Occurred while loading offline help library, error: {0}", ex.Message));
                Reporter.ToLog(eLogLevel.ERROR, "Error Occurred while loading help library", ex);
                return false;
            }
        }

        public static void ShowOnlineHelpLibrary(string searchText="")
        {
            try
            {

                string publicLibURI = @"https://ginger-automation.github.io/Ginger-Web-Help";

                if (!string.IsNullOrEmpty(searchText))
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo() { FileName = publicLibURI + "/?rhsearch=" + Uri.EscapeUriString(searchText), UseShellExecute = true });
                }
                else
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo() { FileName = publicLibURI, UseShellExecute = true });                  
                }

            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, string.Format("Error Occurred while loading online help library, error: {0}", ex.Message));
                Reporter.ToLog(eLogLevel.ERROR, "Error Occurred while loading help library", ex);
            }
        }
    }
}
