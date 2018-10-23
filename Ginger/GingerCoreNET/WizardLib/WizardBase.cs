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
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace GingerWPF.WizardLib
{
    public abstract class WizardBase
    {
        public ObservableList<WizardPage> Pages = new ObservableList<WizardPage>();

        public IWizardWindow mWizardWindow;

        // public List<Object> WizardArgs = new List<object>();

        // Each wizard must set the title window
        public abstract string Title { get; }

        public abstract void Finish();

        //public bool FinishVisible
        //{
        //    get
        //    {
        //        return mWizardWindow.FinishButton.Visibility == System.Windows.Visibility.Visible;
        //    }
        //    set
        //    {
        //        if (value)
        //            mWizardWindow.FinishButton.Visibility = System.Windows.Visibility.Visible;
        //        else
        //            mWizardWindow.FinishButton.Visibility = System.Windows.Visibility.Collapsed;
        //    }
        // }

        //public bool NextVisible
        //{
        //    get
        //    {
        //        return mWizardWindow.NextButton.Visibility == System.Windows.Visibility.Visible;
        //    }
        //    set
        //    {
        //        if (value)
        //            mWizardWindow.NextButton.Visibility = System.Windows.Visibility.Visible;
        //        else
        //            mWizardWindow.NextButton.Visibility = System.Windows.Visibility.Collapsed;
        //    }
        //}

        //public bool PrevVisible
        //{
        //    get
        //    {
        //        return mWizardWindow.PrevButton.Visibility == System.Windows.Visibility.Visible;
        //    }
        //    set
        //    {
        //        if (value)
        //            mWizardWindow.PrevButton.Visibility = System.Windows.Visibility.Visible;
        //        else
        //            mWizardWindow.PrevButton.Visibility = System.Windows.Visibility.Collapsed;
        //    }
        //}




        public void ProcessStarted()
        {        
            mWizardWindow.ProcessStarted();
        }

        public void ProcessEnded()
        {
            // mWizardWindow.xProcessingImage.Visibility = Visibility.Collapsed;
            mWizardWindow.ProcessEnded();
        }

        //public bool FinishEnabled
        //{
        //    get
        //    {
        //        return mWizardWindow.FinishButton.IsEnabled;
        //    }
        //    set
        //    {
        //        mWizardWindow.FinishButton.IsEnabled = value;
        //    }
        //}

        //public bool NextEnabled
        //{
        //    get
        //    {
        //        return mWizardWindow.NextButton.IsEnabled;
        //    }
        //    set
        //    {
        //        mWizardWindow.NextButton.IsEnabled = value;
        //    }
        //}

        //public bool PrevEnabled
        //{
        //    get
        //    {
        //        return mWizardWindow.PrevButton.IsEnabled;
        //    }
        //    set
        //    {
        //        mWizardWindow.PrevButton.IsEnabled = value;
        //    }
        //}

                

        public void AddPage(string Name, String Title, string SubTitle, IWizardPage Page)
        {
            WizardPage wp1 = new WizardPage() { Name = Name, Title = Title, SubTitle = SubTitle, Page = Page };            
            Pages.Add(wp1);
        }        

        //internal void ShowWizard(int width = 800)
        //{
        //    WizardEventArgs WizardEventArgs = new WizardEventArgs(this, EventType.Init);
        //    foreach (WizardPage wp in Pages)
        //    {
        //        wp.Page.WizardEvent(WizardEventArgs);
        //    }

        //    mWizardWindow.ShowDialog(width);                          // /TODO: pass the width
        //}

        public WizardPage GetCurrentPage()
        {
            if (Pages.CurrentItem == null && Pages.Count > 0)
            {
                Pages.CurrentItem = Pages[0];
            }
            return (WizardPage)Pages.CurrentItem;
        }

        //public void UpdateButtons()
        //{
        //    WizardPage p = (WizardPage)Pages.CurrentItem;
        //    if (p.HasErrors)
        //    {
        //        mWizardWindow.NextButton(false);
        //    }
        //    else
        //    {
        //        mWizardWindow.NextButton(true);
        //    }

        //    //TODO: enable disable finish button based on all pages
        //}

        public void Next()
        {            
            WizardEventArgs WizardEventArgsLeavingForNextPage = new WizardEventArgs(this, EventType.LeavingForNextPage);
            GetCurrentPage().Page.WizardEvent(WizardEventArgsLeavingForNextPage);            
            //TODO: add check if can move next 
            if (!WizardEventArgsLeavingForNextPage.CancelEvent)
            {
                Pages.MoveNext();
                GetCurrentPage().Page.WizardEvent(new WizardEventArgs(this, EventType.Active));                
            }            
        }
        
        public void Prev()
        {
            WizardEventArgs WizardEventArgs = new WizardEventArgs(this, EventType.Prev);
            foreach (WizardPage wp in Pages)
            {
                wp.Page.WizardEvent(WizardEventArgs);                
            }

            Pages.MovePrev();

            GetCurrentPage().Page.WizardEvent(new WizardEventArgs(this, EventType.Active));            
        }

        public virtual void Cancel()
        {
            WizardEventArgs WizardEventArgs = new WizardEventArgs(this, EventType.Cancel);
            foreach (WizardPage wp in Pages)
            {
                wp.Page.WizardEvent(WizardEventArgs);                
            }
        }

        public bool IsLastPage()
        {
            if (Pages.CurrentItem == Pages[Pages.Count-1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsFirstPage()
        {
            if (Pages.CurrentItem == Pages[0])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ProcessFinish()
        {

            foreach (WizardPage p in Pages)
            {
                // TODO: call validate on page to run the rules
                //WizardEventArgs WizardEventArgs = new WizardEventArgs(this, EventType.Validate);
                //p.Page.WizardEvent(WizardEventArgs);
                // as soon as we have errors exist.
                if (p.HasErrors)
                {
                    Pages.CurrentItem = p;
                    return;
                }
            }

            //foreach (WizardPage p in Pages)
            //{
            //    WizardEventArgs WizardEventArgs2 = new WizardEventArgs(this, EventType.Finish);
            //    p.Page.WizardEvent(WizardEventArgs2);                
            //}

            // all went OK!            
            Finish();
            Pages.Clear();
            mWizardWindow.Close();
            mWizardWindow = null;
            // GC.Collect();            
        }



        




    }
}
