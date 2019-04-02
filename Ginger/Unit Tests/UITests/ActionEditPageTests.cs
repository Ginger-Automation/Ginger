#region License
/*
Copyright © 2014-2019 European Support Limited

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

using GingerCore.Actions;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UnitTests.UITests
{
    [Level2]
    [TestClass]
    public class ActionEditPageTests
    {

        [TestMethod]  [Timeout(60000)]
        public void ActionEditPagesLoadTest()
        {

            // !!!!remove hard code and use typeof
            AppDomain.CurrentDomain.Load("Ginger");


            var ActTypes =
                           from type in typeof(Act).Assembly.GetTypes()
                           where type.IsSubclassOf(typeof(Act))
                           select type;


            foreach (Type ActTyp in ActTypes)
            {

                if (ActTyp.IsAbstract)
                    continue;
                Act a = (Act)Activator.CreateInstance(ActTyp);
                if (a.ActionEditPage != null)
                {
                    string classname = "Ginger.Actions." + a.ActionEditPage;
                    Type t = Assembly.GetAssembly(typeof(Ginger.Actions.ActAgentManipulationEditPage)).GetType(classname);
                    if (t == null)
                    {
                        throw new Exception("Action edit page not found - " + classname);
                    }
                    //Page p = (Page)Activator.CreateInstance(t, a);
                }
            }
        }
    }
}
