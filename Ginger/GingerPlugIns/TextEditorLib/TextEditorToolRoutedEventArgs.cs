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


using System.Collections.Generic;
using System.Windows;

namespace GingerPlugIns.TextEditorLib
{
    public class TextEditorToolRoutedEventArgs : RoutedEventArgs
    {
        //tODO: clean

        //Input
        /// <summary>
        /// change to editor text
        /// </summary>
        // public string txt { get; set;}

        public int CaretLocation { get; set; }

        //Output
        /// <summary>
        /// Will contain all lines numbers which the errors are occures.
        /// </summary>
        public List<int> ErrorLines { get; set; }

        /// <summary>
        /// Will contain all error messages due to function failed  
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Will contain success message due to function success  
        /// </summary>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// Will contain all Wraning messages 
        /// </summary>
        public string WarnMessage { get; set; }
    }
}
