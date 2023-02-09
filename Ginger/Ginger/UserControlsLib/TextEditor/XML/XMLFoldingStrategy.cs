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

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace Ginger.UserControlsLib.TextEditor.XML
{
	/// <summary>
	/// Allows producing foldings from a document based on Gherkin -> Scenario.
	/// </summary>
	public class XMLFoldingStrategy : IFoldingStrategy
    {
        XmlFoldingStrategy foldingStrategy = new XmlFoldingStrategy();

        public void UpdateFolding(FoldingManager manager, TextDocument document)
        {                        
            foldingStrategy.UpdateFoldings(manager, document);
        }
    }
}
