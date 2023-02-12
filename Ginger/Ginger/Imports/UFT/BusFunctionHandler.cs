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

using System.Collections.Generic;

namespace Ginger.Imports.UFT
{
    public class BusFunctionHandler
    {
        //Class level variables
        public List<BusFunction> BusList = new List<BusFunction>();
        public List<string> GuiFunctionList = new List<string>();
        public List<string> ASAPBuiltInFunctionList = new List<string>();

        public List<BusFunction> ProcessBusScript(string sBUSfilePath)
        {
            //Creating ASAP functions list
            ASAPBuiltInFunctionList.Add("fGuiClickButton");
            ASAPBuiltInFunctionList.Add("fGuiDBCheck");
            ASAPBuiltInFunctionList.Add("fDBActivities");
            ASAPBuiltInFunctionList.Add("fGuiEnterEditField");
            ASAPBuiltInFunctionList.Add("fGuiSelectDropDownListItem");
            ASAPBuiltInFunctionList.Add("fGuiSelectDynamicDropDownListItem");
            ASAPBuiltInFunctionList.Add("fGuiClickPageActionButton");
            ASAPBuiltInFunctionList.Add("fGuiClickObject");

            List<string> GuiList = new List<string>();
            string[] CodeLines = System.IO.File.ReadAllLines(sBUSfilePath);
            string BusFuncName="" ; 
            foreach (string CodeLine in CodeLines)
            {
                string CodeLineUpper = CodeLine.ToUpper();
               
                // fetch Bus Function name
                if (CodeLineUpper.Contains("FUNCTION") && CodeLineUpper.Contains("FBUS") && (CodeLineUpper.Contains("PUBLIC") || CodeLineUpper.Contains("PRIVATE"))) BusFuncName = ProcessBUSCode(CodeLine);
                else if (CodeLineUpper.StartsWith("FUNCTION")) BusFuncName = ProcessBUSCode(CodeLine);
                else if (CodeLineUpper.Contains("SUB") && CodeLineUpper.Contains("FBUS") && (CodeLineUpper.Contains("PUBLIC") || CodeLineUpper.Contains("PRIVATE"))) BusFuncName = ProcessBUSCode(CodeLine);
                else if (CodeLineUpper.StartsWith("SUB")) BusFuncName = ProcessBUSCode(CodeLine);

                //Create a List of all GUIs in that BUS Function
                else if (CodeLine.ToUpper().Contains("FGUI") && !CodeLine.ToUpper().Contains("'")) GuiList = ProcessGUICode(CodeLine);  //&& CodeLine.ToUpper().StartsWith("\tIF")

                //To Mark the End of a Bus Function
                else if (CodeLineUpper.StartsWith("END FUNCTION") || CodeLineUpper.StartsWith("END SUB"))
                {
                    BusFunction BF = new BusFunction();
                    List<string> fGuiListPerBus = new List<string>(GuiList);
                    BF.BusFunctionName = BusFuncName;
                    BF.ListOfGuiFunctions = fGuiListPerBus; ;
                   
                    //Make sure there are no Duplicate functions
                    if (!BusList.Contains(BF))
                    {
                        BusList.Add(BF);
                    }

                    BF = null;
                    GuiList.Clear();
                }
            }
            return BusList;
        }

        private string ProcessBUSCode(string CodeLine )
        {
            string BusFunctionName;
            BusFunctionName = ProceesNewFunction(CodeLine);
            return BusFunctionName;
        }

        private List<string> ProcessGUICode(string CodeLine)
        {
          
            int start = CodeLine.IndexOf("fGui");
            int len = CodeLine.Length;
            string GuiName="";
            

            if (!CodeLine.StartsWith("'"))
            {
                //Fetch GUI function
                if (CodeLine.Contains("(")) GuiName = GetStringBetween(CodeLine," " , "(");
                else GuiName = GetStringBetween(CodeLine," " , " ");

                string GuiNameUpper = GuiName.ToUpper();

                if (GuiNameUpper.Contains("IF")) GuiName = GuiName.Replace("If", "");
                if (GuiNameUpper.Contains("THEN")) GuiName = GuiName.Replace("Then", "");
                if (GuiNameUpper.Contains("=")) GuiName = GuiName.Replace("=", "");
                if (GuiNameUpper.Contains("\"")) GuiName = GuiName.Replace("\"", "");
                if (GuiNameUpper.Contains("'")) GuiName = GuiName.Replace("'", "");
                if (GuiNameUpper.Contains(",")) GuiName = GuiName.Replace(",", "");
                if (GuiNameUpper.Contains("FALSE")) GuiName = GuiName.Replace("False", "");
                if (GuiNameUpper.Contains("TRUE")) GuiName = GuiName.Replace("True", "");
                if (GuiNameUpper.Contains("MICPASS")) GuiName = GuiName.Replace("micPass", "");
                if (GuiNameUpper.Contains("MICFAIL")) GuiName = GuiName.Replace("micFail", "");


                //Makes sure that Gui List contains Unique List and also its not a part of ASAP Built in function
                if (!GuiFunctionList.Contains(GuiName) && !ASAPBuiltInFunctionList.Contains(GuiName) && GuiName.Contains("fGui")) 
                {
                    GuiFunctionList.Add(GuiName.Trim());
                }
            }
            
            return GuiFunctionList;
        }

        private string ProceesNewFunction(string CCL)
        {
            string b = CCL;
            string FuncName;

            //Fetch the Function/Sub name from Script File
            if (b.Contains("Public") && b.Contains("Function"))
            {
                b = b.Replace("Public", "").Replace("Function", "");
            }
            else if (b.Contains("Public") && b.Contains("Sub"))
            {
                b = b.Replace("Public", "").Replace("Sub", "");
            }
            else if (b.Contains("Private") && b.Contains("Function"))
            {
                b = b.Replace("Private", "").Replace("Function", "");
            }
            else if (b.Contains("Private") && b.Contains("Sub"))
            {
                b = b.Replace("Private", "").Replace("Sub", "");
            }
            else if (b.Contains("Function"))
            {
                b = b.Replace("Function", "");
            }
            else if (b.Contains("Sub"))
            {
                b = b.Replace("Sub", "");
            }

            FuncName = b.Trim();
            return FuncName;
        }

        private string GetStringBetween(string STR, string FirstString, string LastString = null)
        {

            string str = "";
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2;
            if (LastString != null)
            {
                Pos2 = STR.IndexOf(LastString, Pos1);
            }
            else
            {
                Pos2 = STR.Length;
            }

            if (Pos2 > Pos1)
            {
                str = STR.Substring(Pos1, Pos2 - Pos1);
                return str;
            }

            return str;
        }
    }
}
