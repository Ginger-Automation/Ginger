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

using Amdocs.Ginger.Plugin.Core;
using System;

namespace GingerPluginCoreTest
{
    [GingerService(Id: "SampleService1", Description: "Sample Service 1")]
    public class SampleService1
    {        

        [GingerAction(Id: "Concat", description: "Concatenate two string")]        
        public void Concat(IGingerAction GA,                                                          
                            [MinLength(10)]// define s1 Min 10
                            [Mandatory] // user must fill a value
                            [Default("webSiteURL")]
                            [Label("webSiteURL")]
                            [MaxLength(15)]
                            string s1,

                            [ValidValue(new int[]{10, 20, 30})]
                            [ValidValue(123)]
                            //[GingerParamProperty(GingerParamProperty.Mandatory)]   // define s2 is Mandatory                                  
                            string s2,
                            [Browse(true)]
                            [FileType("txt")]
                            [BrowseType(BrowseTypeAttribute.eBrowseType.File)]
                            string JavaHomePath,
                            [Browse(true)]
                            [BrowseType(BrowseTypeAttribute.eBrowseType.Folder)]
                            [FolderType(Environment.SpecialFolder.NetworkShortcuts)]
                            string Folder)
        {
            Console.WriteLine(DateTime.Now + "> Concat: " + s1 + "+" + s2);
            //In

            //Act
            string txt = string.Concat(s1, s2);

            //Out
            GA.AddOutput("s1", s1);
            GA.AddOutput("s2", s2);
            GA.AddOutput("txt", txt);

            GA.AddExInfo(s1 + "+" + s2 + "=" + txt);
        }


        [GingerAction(Id: "Divide", description: "Divide two numbers")]
        public void Divide(IGingerAction GA,                            
                            [Mandatory] // user must fill a value
                            [Label("Numerator")]
                            [MaxValue(10)]
                            [MinValue(5)]
                            int a,                            
                            [InvalidValue(0)] // 0 is not allowed
                            [InvalidValue(new int[] {-1,101,200})] // not allowed
                            [Label("Denominator")]
                            [Default(1)]
                            [Tooltip("Enter the Denominator value")]
                            int b)
        {
            Console.WriteLine(DateTime.Now + "> Divide: " + a + "/" + b);
            //In

            //Act
            int result = a / b;

            //Out
            GA.AddOutput("a", a);
            GA.AddOutput("b", b);
            GA.AddOutput("result", result);

            GA.AddExInfo(a + "/" + b + "=" + result);
        }   
        

    }
}
