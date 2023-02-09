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

using Ginger;
using GingerCore.GeneralLib;
using GingerTest;
using GingerTest.POMs.Common;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GingerWPFUnitTest.POMs
{
    public abstract class GingerPOMBase
    {
        public static Dispatcher Dispatcher;

        public VisualCompare VisualCompare = new VisualCompare();


        /// <summary>
        /// Recursive method to find element by AutomationID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="automationID"></param>
        /// <returns></returns>
        public DependencyObject FindElementByAutomationID<T>(DependencyObject context, string automationID)
        {            
            foreach (object o in LogicalTreeHelper.GetChildren(context))
            {                
                if (o is DependencyObject)
                {
                    DependencyObject dependencyObject = (DependencyObject)o;
                   
                    if (dependencyObject is T)  // the type we are searching
                    {
                        if (AutomationProperties.GetAutomationId(dependencyObject) == automationID)
                        {
                            return dependencyObject;
                        }
                    }

                    //Drill down the tree
                    DependencyObject childDependencyObject = FindElementByAutomationID<T>(dependencyObject, automationID);
                    if (childDependencyObject != null)
                    {
                        return childDependencyObject;
                    }
                }
            }
            return null;
        }


        internal DependencyObject FindElementByName(DependencyObject context, string name)
        {            
            DependencyObject d = null;
            Execute(() => {                
                // try up to 10 seconds
                Stopwatch st = Stopwatch.StartNew();
                while (d == null && st.ElapsedMilliseconds < 10000)
                {
                    d = (DependencyObject)LogicalTreeHelper.FindLogicalNode(context, name);
                    if (d != null) break;                    
                    SleepWithDoEvents(100);                        
                }                
            });
            if (d != null)
            {
                return d;
            }
            else
            {
                throw new Exception("Element not found: " + name);
            }
        }


        public DependencyObject FindElementByText<T>(DependencyObject context, string text)
        {
            foreach (object o in LogicalTreeHelper.GetChildren(context))
            {
                if (o is DependencyObject)
                {
                    DependencyObject dependencyObject = (DependencyObject)o;

                    if (dependencyObject is T)  // the type we are searching
                    {                        
                        ContentControl cc = (ContentControl)dependencyObject;
                        if (cc.Content.ToString() == text)
                        {
                            return dependencyObject;
                        }                        
                    }

                    //Drill down the tree
                    DependencyObject childDependencyObject = FindElementByText<T>(dependencyObject, text);
                    if (childDependencyObject != null)
                    {
                        return childDependencyObject;
                    }
                }
            }
            return null;
        }


        public void SleepWithDoEvents(int Milliseconds)
        {
            Stopwatch st = Stopwatch.StartNew();
            while (st.ElapsedMilliseconds < Milliseconds)
            {
                DoEvents();
                Thread.Sleep(1);
            }

        }

        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        private object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        /// <summary>
        /// Execute action on the UI thread + include 'Sleep with Doevents' of 100ms to let the UI refresh if needed
        /// </summary>
        /// <param name="action"></param>
        public void Execute(Action action)
        {
            Dispatcher.Invoke(() =>
            {
                action.Invoke();
                SleepWithDoEvents(100);
            });
        }

      

        public bool IsWindowBitmapEquel(Page page, string ID)
        {
            bool b = false;
            Execute(() => {
                        b = VisualCompare.IsVisualEquel(page, ID);
                    });
            return b;
        }

        public InputBoxWindowPOM CurrentInputBoxWindow
        {
            get {
                int i = 0;
                while (InputBoxWindow.CurrentInputBoxWindow == null && i < 100)
                {
                    SleepWithDoEvents(100);
                    i++;
                }
                if (InputBoxWindow.CurrentInputBoxWindow != null)
                {
                    return new InputBoxWindowPOM();
                }
                else
                {
                    throw new Exception("Input box not found");
                }
            }
        }

        //TODO: add Check by title - so know what to expect
        public GenericWindowPOM CurrentGenericWindow
        {            
            get
            {
                if (GenericWindow.CurrentWindow != null)  return new GenericWindowPOM(GenericWindow.CurrentWindow);

                GenericWindowPOM w = null;
                Task.Factory.StartNew(()=> { 
                
                 Execute(() => { 
                    int i = 0;
                    while (GenericWindow.CurrentWindow == null && i < 100)
                    {
                        SleepWithDoEvents(100);
                        Thread.Sleep(100);
                        i++;
                    }
                    if (GenericWindow.CurrentWindow != null)
                    {
                        w = new GenericWindowPOM(GenericWindow.CurrentWindow);                        
                    }
                    else
                    {
                        throw new Exception("Generic window box not found");
                    }
                 });
                
                });
                
                Stopwatch st = Stopwatch.StartNew();
                while (w == null && st.ElapsedMilliseconds < 10000)
                {
                    Thread.Sleep(100);
                }                

                return w;
            }
        }


    }
}
