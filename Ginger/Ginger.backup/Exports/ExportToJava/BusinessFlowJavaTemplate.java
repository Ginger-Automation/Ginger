/*
Copyright © 2014-2020 European Support Limited

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

package project;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.firefox.FirefoxDriver;

public class  {
    public static void main(String[] args) {
        // declaration and instantiation of objects/variables
    	WebDriver driver ;
    	System.setProperty("webdriver.firefox.marionette","C:\\geckodriver.exe");
    	driver = new FirefoxDriver();

		//BF Variables
		%Vars%

		String CustomerID = "123";
		String rnd1 = General.GetRandomString(3,5, UpperCase);
		List<string> Activities();
		
		void main()
		{
			// Prep the activities
			List.Add("Activitiy1", Activity1);
			List.Add("Activitiy2", Activity2);
			List.Add("Activitiy3", Activity3);
			RunFlow();			
		}

		void RunFlow()
		{
		CurrentActivity = List[0];
			
			int i=0			
			bool EOBF =false;
			while (!EOBF)
			{
				RunActivity(List[i]);
			}
			// handle goto			
		}

		void Activity1()
		{			
			int step=0;
			int nextstep =1;
			bool bExit = false;

			// Vars
			String v1 = "";

			while (bExit)
			{
				switch (step)
				{
					keep = nextstep;

					case: 0
						//#1 - GOTO URL www.google.com
						driver.get("www.google.com");
					case: 1
						//#2 - Enter User ID
						Utils.Wait(2000);
						driver.FindElement.ById("ID").Value = "abc";
						Utils.TakeScreenShot("#2 - Enter User ID", AllWindows);

						//Flow Control

						// Impl the following
						// Condition - Action
						if (a=123) nextstep =4;

					case: 2
						//#3 - Enter Pass
						driver.FindElement.ById("Pass").Value = "pass";					
					case: 3
						//#3 - Click Login
						driver.FindElement.ById("Login").Click();

					case: 4
						//#4 - Validate zzz
						String Actual = driver.FindElement.ById("zzz").Value;
						//Output handling
						String Expected = "aaa";
						if (!Actual.Equels(Expected)) 
						{						
							//Fail, throw...
							// Stop Activity
							bExit =true;

							//Goto Action: 
							Step = 2;
						}					

						//Store to var
						v1 = Actual;
					case: 5
						//#5 - Use VE
						String Value = CalculateVE("{Var Name=AAA}");
						driver.FindElement.ById(Value).Click();
					if (keep=nextstep)
					{
						step++;
					}
				}
			}	
		}

		//Activities
		
		void Activity1()
		{

		}

		//Actions

		//Goto URL - www.google.com

		driver.get(baseUrl);

        String baseUrl = "http://newtours.demoaut.com";
        String expectedTitle = "Welcome: Mercury Tours";
        String actualTitle = "";

        // launch Fire fox and direct it to the Base URL
        driver.get(baseUrl);

        // get the actual value of the title
        actualTitle = driver.getTitle();

        /*
         * compare the actual title of the page with the expected one and print
         * the result as "Passed" or "Failed"
         */
        if (actualTitle.contentEquals(expectedTitle)){
            System.out.println("Test Passed!");
        } else {
            System.out.println("Test Failed");
        }
       
        //close Fire fox
        driver.close();
       
        // exit the program explicitly
        System.exit(0);
    }
}