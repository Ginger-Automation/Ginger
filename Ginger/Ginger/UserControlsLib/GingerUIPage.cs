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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.UserControlsLib
{
    public abstract class GingerUIPage : Page
    {
        private RepositoryItemBase mCurrentItem;
        protected RepositoryItemBase CurrentItemToSave { get { return mCurrentItem; } set { if (mCurrentItem != value) { mCurrentItem = value; WorkSpace.Instance.CurrentSelectedItem = mCurrentItem; } } }
        protected GingerUIPage()
        {
            IsVisibleChanged += IsVisibleChangedHandler;
        }

        protected virtual void IsVisibleChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (mCurrentItem != null)
            {
                if ((bool)e.NewValue)
                {
                    WorkSpace.Instance.CurrentSelectedItem = mCurrentItem;
                }
                else
                {
                    if (WorkSpace.Instance!= null && WorkSpace.Instance.CurrentSelectedItem == mCurrentItem)
                    {
                        WorkSpace.Instance.CurrentSelectedItem = null;
                    }
                }
            }
        }
    }
}
