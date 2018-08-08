#Coding Standards and Guidelines


##Naming Conventions

####General Naming Conventions

* Capitalize the first letter of each word in the identifier

* Do use PascalCasing for all public member, type, and namespace names consisting of multiple words

```cs

    //Namespace
    Amdocs.Ginger.Common.Repository;

    //Type
    public class StreamReader{...}

    //Method
    public void StartDriver(){...};
```
* Do chose readable identifier names e.g. Horizontal alignment is more English readable than alignment horizontal
 
* Do favor readability over brevity e.g. CanScrollHorizontally is better than ScollableX

* Avoid using identifiers that conflict with keywords of widely used programming languages

* Do not use underscores, hyphens or any other non-alphanumeric characters to differentiate words

```cs
	// Correct	
	public DateTime ClientAppointment;
	public TimeSpan TimeLeft;
	 
	// Avoid
	public DateTime client_Appointment;
	public TimeSpan time_Left;
```
* Do not use Hungarian notation or any other type identification in identifiers.
```cs
e.g. 
//Correct 
int counter; 
//Incorrect
int iCounter;
```


* Do not use abbreviation or contractions as part of identifier names 
e.g. Use GetWindow instead of GetWin


#### Naming Classes and Interfaces

* Use Noun or noun phrases for naming classes e.g. Employee 
* Do not prefix class names with prefix “C”
* Do prefix interface names with the letter I, to indicate that the type is an interface.
* Name Source files according to their main classes



####Variables naming 
* Use pascal casing with “m” prefix for global variables
```cs
    private RunsetOperations mRunserOperations {get; set;};
```

* Use pascal casing with “c” prefix for constants

```cs
   const int cNULLStringLen = -1;
```   

* Use camel casing for method parameters and local variables

```cs
        public int AddTwoNumbers(int firstNumber, int secondNumber)
        {
            int outputValue = firstNumber - secondNumber;
            return outputValue;
        }
```   

* Do not explicitly specify a type of an enum or values of enums

```cs
	// Correct
	public enum Direction
	{
	    North,
	    East,
	    South,
	    West
	}

    // Don't
	public enum Direction : long
	{
	    North = 1,
	    East = 2,
	    South = 3,
	    West = 4
	}
```

* Do not suffix enum names with Enum


```cs	 
	// Correct
	public enum Coin
	{
	    Penny,
	    Nickel,
	    Dime,
	    Quarter,
	    Dollar
	} 
 
	// Don't
     public enum CoinEnum
	{
	    Penny,
	    Nickel,
	    Dime,
	    Quarter,
	    Dollar
	}
```

####XAML Naming conventions
* Use “x” prefix all XAML control names
* Use use the type name as suffix for a xaml control names e.g. Btn for Button. 
 So name of reset button in xaml will be "xResetBtn" 

#### Ginger namespace

#####Amdocs.Ginger.CoreNET:
- Amdocs.Ginger.Execution  
- Amdocs.Ginger.Communication (socket related stuff)
- Amdocs.Ginger.Plugins

##### Amdocs.Ginger.Common:
- Amdocs.Ginger.Repository (repository engine + cache + all objects saved to system file)
- Amdocs.Ginger.Utils (general standalone methods)
- Amdocs.Ginger.Report (HTML/mail report)
- Amdocs.Ginger.Usage (auto log)
- Amdocs.Ginger.SourceControl
- Amdocs.Ginger.Workspace (cross dlls sharing cache objects/events)


##Common Coding practices
*	Declare all member variables at the top of a class with static variables at the very top. (Generally accepted practice that prevents the need to hunt for variable declaration)
*	For iterating an observable list use for loop instead of foreach. Because if list collection is modified during foreach it throws an exception
*	


####Guidelines for Exception handling
*	Use the Reporter class to show any user message instead of a message box
```cs
 Reporter.ToUser(eUserMsgKeys.MissingActivityAppMapping);
 ```
 
* Use Reporter.ToLog to add any information to Ginger logs
```cs
Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
```