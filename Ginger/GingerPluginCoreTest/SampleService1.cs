using Amdocs.Ginger.Plugin.Core;
using System;

namespace GingerPluginCoreTest
{
    [GingerService(Id: "SampleService1", Description: "Sample Service 1")]
    public class SampleService1
    {        

        [GingerAction(Id: "Concat", description: "Concat two string")]        
        public void Concat(IGingerAction GA,                             
                            [Max(10)] // define s1 Max 10
                            [Mandatory] // user must fill a value
                            [Default("Default Value")]
                            string s1,

                            //[GingerParamProperty(GingerParamProperty.Mandatory)]   // define s2 is Mandatory
                            string s2)
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
                            int a,
                            [Max(10)]
                            [InvalidValue(0)] // 0 is not allowed
                            [InvalidValue(new int[] {-1,101,200})] // not allowed
                            [Label("Denominator")]
                            [Default()]
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
