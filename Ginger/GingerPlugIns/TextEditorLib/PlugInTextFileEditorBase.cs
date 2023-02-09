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

//#region License
///*
//Copyright © 2014-2023 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using GingerPlugIns.TextEditorLib.Common;
//using System.Collections.Generic;

//namespace GingerPlugIns.TextEditorLib
//{
//    // Base Class for Plugin Text Editor
//    public abstract class PlugInTextFileEditorBase : PlugInCapability
//    {
//        public List<string> mExtensions = new List<string>();  // What type of extensions this editor can edit, for example: txt, json, vbs

//        public List<TextEditorToolBarItem> mTools = new List<TextEditorToolBarItem>();

//        public override eCapabilityType CapabilityType { get{ return eCapabilityType.TextEditor; } }  // TextEditor

//        public abstract string EditorName { get;}

//        public abstract string EditorID { get; }

//        public abstract PlugInEditorFoldingStrategy EditorStrategy{ get; }

//        public abstract Dictionary<string, string> TableEditorPageDict { get; }

//        public abstract List<string> CompletionDataKeyWords { get; }

//        // public abstract string Tamplate { get; }
//        public abstract List<string> Extensions { get; }

//        // the Avalon Edit highlighting xshd as byte[]
//        public abstract byte[] HighlightingDefinition { get;}

//        public abstract List<TextEditorToolBarItem> Tools { get; }
        
//        public string ExtensionsAsString
//        {
//            get
//            {
//                string res = string.Empty;
//                res = string.Join(" ,", Extensions.ToArray());
//                return res;
//            }
//        }

//        public abstract string GetTemplatesByExtensions(string plugInExtension);

//        public virtual string Title()
//        {
//            return null;
//        }
//    }
// }
