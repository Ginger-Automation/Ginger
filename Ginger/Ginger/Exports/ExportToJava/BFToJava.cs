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
using GingerCore;
using GingerCore.Actions;
using System;
using System.Text;
using GingerCore.Variables;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.Exports.ExportToJava
{
    public class BFToJava
    {
        string JavaFileName;

        StringBuilder Output = new StringBuilder();

        String Missing = "======================================>";
        int indent = 0;
        public static void BrowseForFilename()
        {
        }

        public void BusinessFlowToJava(BusinessFlow BF)
        {
            //TODO: Temp fix me to have full export window with config options
            JavaFileName = @"c:\temp\java\BF1.java";

            try
            {                
                System.IO.File.WriteAllText(JavaFileName, GenerateJavafromBusinessFlow(BF), Encoding.UTF8);
            }
            catch (Exception e)
            {
                //TODO: message for Java
                Reporter.ToUser(eUserMsgKey.FailedToExportBF, e.Message);
            }
        }

        private string GenerateJavafromBusinessFlow(BusinessFlow BF)
        {
            WriteLine("// Business Flow: " + BF.Name);

            WriteLine("public class BusinessFlow_" + MakeName(BF.Name) + " (WebDriver driver)");
            OpenBrackets();
            indent++;
            AddVars("Business Flow", BF.Variables);
            
            foreach (Activity a in BF.Activities)
            {
                indent++;
                WriteLine("// Activity: " + a.ActivityName);
                WriteLine("void Activity_" + MakeName(a.ActivityName));

                OpenBrackets();

                AddVars("Activity", a.Variables);

                AddActions(a.Acts);

                CloseBrackets();
                indent--;

            }
            indent--;
            CloseBrackets();

            return Output.ToString();
        }

        private void OpenBrackets()
        {
            WriteLine("{");
            indent++;
        }

        private void CloseBrackets()
        {
            indent--;
            WriteLine("}");
        }

        private void WriteLine(string txt)
        {
            for(int i=0; i<indent; i++)
            {
                Output.Append("\t");
            }
            Output.AppendLine(txt);
        }

        void AddActions(ObservableList<IAct> acts)
        {
            WriteLine("// Actions");
            foreach (Act act in acts)
            {                
                AddActionCode(act);
                AddActionOutout(act);
            }
        }

        private void AddActionOutout(Act act)
        {
            foreach(ActReturnValue ARV in act.ActReturnValues)
            {
                AddActReturnValue(ARV);
            }
        }

        private void AddActReturnValue(ActReturnValue ARV)
        {
            WriteLine("if (!actual.Equels(" + VE(ARV.Expected) + "))");
            OpenBrackets();
            WriteLine(Missing + "  Mark step as failed - throw" );
            CloseBrackets();
        }

        private void AddActionCode(Act act)
        {
            WriteLine("// Action: " + act.Description);            
            if (act is ActGotoURL)
            {                
                WriteLine("driver.get(" + VE(act.Value)  + ");");
                return;
            }
            if (act is ActGenElement)
            {
                AddActGenElement((ActGenElement)act);
                return;
            }

            if (act is ActTextBox)
            {
                AddActTextBox((ActTextBox)act);                
                return;
            }

            if (act is ActButton)
            {
                AddActButton((ActButton)act);
                return;
            }

            if (act is ActLink)
            {
                AddActLink((ActLink)act);
                return;
            }

            if (act is ActSubmit)
            {
                AddActSubmit((ActSubmit)act);
                return;
            }

            // generic tmep
            WriteLine(Missing + act.GetType().ToString());
            return;
        }

        private void AddActSubmit(ActSubmit act)
        {
            string s = GetDriverfindElem(act.LocateBy, act.LocateValue);            
            s += ".Submit();";                    
            WriteLine(s);
        }

        private void AddActLink(ActLink act)
        {
            string s = GetDriverfindElem(act.LocateBy, act.LocateValue);

            switch (act.LinkAction )
            {
                case ActLink.eLinkAction.Click:
                    s += ".Click()";
                    break;

                default:
                    //temp to show something
                    s += "." + Missing + act.LinkAction + " " + act.Value;
                    break;
            }
            s += ";";
            WriteLine(s);
        }

        private void AddActButton(ActButton act)
        {
            string s = GetDriverfindElem(act.LocateBy, act.LocateValue);

            switch (act.ButtonAction)
            {
                case ActButton.eButtonAction.Click:
                    s += ".Click()";
                    break;

                default:
                    //temp to show something
                    s += "." + Missing + act.ButtonAction + " " + act.Value;
                    break;
            }
            s += ";";
            WriteLine(s);
        }

        private void AddActGenElement(ActGenElement act)
        {
            
            string s = GetDriverfindElem(act.LocateBy, act.LocateValue);

            switch (act.GenElementAction)
            {
                case ActGenElement.eGenElementAction.Click:
                    s += ".Click()";
                    break;
                case ActGenElement.eGenElementAction.SetValue:
                    s += ".Value = " + act.Value;
                    break;
                case ActGenElement.eGenElementAction.GetValue:
                    s = "String actual =  " + s + ".Value";
                    break;

                default:
                    //temp to show something
                    s += "." + act.GenElementAction.ToString() + " " + act.Value;
                    break;

            }
            s += ";";
            WriteLine(s);
        }

        private void AddActTextBox(ActTextBox act)
        {            
            string s = GetDriverfindElem(act.LocateBy, act.LocateValue);

            switch (act.TextBoxAction)
            {
                case ActTextBox.eTextBoxAction.SetValue:
                case ActTextBox.eTextBoxAction.SetValueFast:
                    s += ".SetValue(" + VE(act.Value) + ")";
                    break;
                case ActTextBox.eTextBoxAction.GetValue:
                    s = "String actual =  " + s + ".Value";
                    break;

                default:
                    //temp to show something
                    s += "." + Missing + act.TextBoxAction.ToString() + " " + act.Value;
                    break;

            }
            s += ";";
            WriteLine(s);
        }

        private string GetDriverfindElem(eLocateBy locateBy, string locateValue)
        {
            //TODO: per loc by put the correct locator
            string lb = "";
            switch (locateBy)
            {
                case eLocateBy.ByID:
                    lb = "By.id";
                    break;
                case eLocateBy.ByClassName:
                    lb = "By.className";
                    break;
                case eLocateBy.ByCSS:
                    lb = "By.cssSelector";
                    break;
                case eLocateBy.ByXPath:
                    lb = "By.xpath";
                    break;
                case eLocateBy.ByName:
                    lb = "By.name";
                    break;
                case eLocateBy.ByLinkText:
                    lb = "By.linkText";
                    break;
                case eLocateBy.ByValue:                    
                    string s1 = "driver.findElement(By.cssSelector(\"input[value='" + VE(locateValue) + "']\"))";
                    return s1;
                    
                //TODO: add the rest

                default:
                    lb = "By." + Missing + locateBy;
                    break;                    
            }

            string s = "driver.findElement(" + lb + "(" + VE(locateValue) + "))";
            return s;
        }

        private string VE(string value)
        {
            string s = "";
            //TODO: check for { if exist return var equivalent or TODOS if VBS etc... handle special VE vals
           
            //temp return it as string
            s = "\"" + value + "\"";
            return s;
        }

        private string MakeName(string activityName)
        {
            // Create valid function name in java from name
            //TODO: replace all special chars
            return activityName.Replace(" ", "_");
        }

        void AddVars(string section, ObservableList<VariableBase> variables)
        {
            if (variables.Count == 0) return;

            WriteLine("// " + section + " Variables");
            foreach (VariableBase v in variables)
            {
                //TODO: based on type create the relevant var
                //switch var type
                WriteLine("\t String " + MakeName(v.Name) + " = \"" + v.Value + "\";");
            }            
        }
    }
}
