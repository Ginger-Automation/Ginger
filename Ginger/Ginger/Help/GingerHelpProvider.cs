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

using System.Windows;
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
    }
}
