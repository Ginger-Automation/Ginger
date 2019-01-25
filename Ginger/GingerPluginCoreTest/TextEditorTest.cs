//using Amdocs.Ginger.Plugin.Core;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using PluginExample;
//using System.Collections.Generic;
//using System.Linq;

//namespace GingerPluginCoreTest
//{
//    [TestClass]
//    public class TextEditorTest
//    {
//        [TestMethod]
//        public void CheckName()
//        {
//            //Arrange
//            ITextEditor myTextEditor = new MyTextEditor();

//            //Act
//            string name = myTextEditor.Name;

//            //assert
//            Assert.AreEqual("My Text Editor", name, "My text editor name");
//        }

//        [TestMethod]
//        public void Extensions()
//        {
//            //Arrange
//            ITextEditor myTextEditor = new MyTextEditor();

//            //Act
//            List<string> extensions = myTextEditor.Extensions;

//            //assert
//            Assert.AreEqual("txt", extensions[0], "Extension txt");
//            Assert.AreEqual(3, extensions.Count, "Extensions count");
//        }

//        [TestMethod]
//        public void Buttons()
//        {
//            //Arrange
//            ITextEditor myTextEditor = new MyTextEditor();

//            //Act
//            List <ITextEditorToolBarItem> tools = myTextEditor.Tools;
//            ITextEditorToolBarItem savetool = tools[0];

//            //assert
//            Assert.AreEqual("Save",savetool.ToolText, "tool text");            
//        }

//        [TestMethod]
//        public void ExecuteTool()
//        {
//            //Arrange
//            ITextEditor myTextEditor = new MyTextEditor();
//            ITextEditorToolBarItem savetool = (from x in myTextEditor.Tools where x.ToolText == "Save" select x).SingleOrDefault();            

//            //Act
//            savetool.Execute(myTextEditor);
//            bool isSaved = ((MyTextEditor)myTextEditor).IsSaved;

//            //assert
//            Assert.AreEqual(true, isSaved, "Tool activated");
//        }


//        [TestMethod]
//        public void Highlighting()
//        {
//            //Arrange
//            ITextEditor myTextEditor = new MyTextEditor();

//            //Act
//            byte[] b = myTextEditor.HighlightingDefinition;
//            string txt = System.Text.Encoding.UTF8.GetString(b);

//            //assert
//            Assert.IsTrue(txt.StartsWith("<?xml version=") , "Highlighting from resource");
//        }


//        [TestMethod]
//        public void ToolChangeText()
//        {
//            //Arrange
//            ITextEditor myTextEditor = new MyTextEditor();
//            myTextEditor.TextHandler.Text = "ABC";
//            ITextEditorToolBarItem lowerCaseTool = (from x in myTextEditor.Tools where x.ToolText == "Lower Case" select x).SingleOrDefault();

//            //Act
//            lowerCaseTool.Execute(myTextEditor);            

//            //assert
//            Assert.AreEqual("abc", myTextEditor.TextHandler.Text , "Tool activated and changed editor text");
//        }

//        [TestMethod]
//        public void ToolMessage()
//        {
//            //Arrange
//            ITextEditor myTextEditor = new MyTextEditor();

//            //Act
//            //myTextEditor.TextHandler.ShowMessage(MessageType.Error, "Error Occured");

//            //assert            
//            //Assert.AreEqual(MessageType.Error, ((MyTextEditor)myTextEditor).MessageType, "Error Message type");
//            //Assert.AreEqual("Error Occured", ((MyTextEditor)myTextEditor).MessageText , "Error Message type text");            
//        }
//    }
//}
