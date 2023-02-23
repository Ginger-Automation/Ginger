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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace GingerCore
{
    public partial class ucTextDicResource : UserControl
    {
        static Dictionary<string, eTermResKey> mTermResKeyList = null;

        private string mText=string.Empty;
        public string Text
        {
            get
            {
                return txtBlock.Text.ToString();
            }
            set
            {
                if (value.ToString() != mText)
                {
                    mText = value.ToString();
                    TranslateText();
                }
            }
        }

        public ucTextDicResource()
        {
            InitializeComponent();           
        }

        private void TranslateText()
        {
            // Do it once since mTermResKeyList is static
            if (mTermResKeyList == null)
            {
                mTermResKeyList = Enum.GetValues(typeof(eTermResKey)).Cast<eTermResKey>().Select(v => v).ToDictionary(x => "[" + x.ToString() + "]", x => x);
            }

            txtBlock.Inlines.Clear();
            // terms key are like [BusinessFlow] - so if we don't find bracket then no term and skip the check
            if (mText.IndexOf("[") >= 0)
            {
                string newText = mText;
                foreach (var placeholder in mTermResKeyList.Keys)
                {
                    if (mText.IndexOf(placeholder) >= 0)
                    {
                        eTermResKey ResTermKey;
                        mTermResKeyList.TryGetValue(placeholder, out ResTermKey);
                        newText = newText.Replace(placeholder, GingerDicser.GetTermResValue(ResTermKey));
                    }
                }
                txtBlock.Inlines.Add(newText);
            }
            else
            {                
                txtBlock.Inlines.Add(mText);                
            }
        }
    }
}
