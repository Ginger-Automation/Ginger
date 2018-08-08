# Coding Standards and Guidelines


## Naming Conventions

#### General Naming Conventions

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
* Do choose readable identifier names e.g. Horizontal alignment is more English readable than alignment horizontal
 
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



#### Variables naming 
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

#### XAML Naming conventions
* Use “x” prefix all XAML control names
* Use use the type name as suffix for a xaml control names

```cs
    x:Name="xResetButton"
```

#### Ginger namespace

[Check Ginger namespaces document](https://github.com/Ginger-Automation/Ginger/blob/master/Ginger/DeveloperZone/Documents/General/Ginger%20Namespace.md)



## Common Coding practices
*	Declare all member variables at the top of a class with static variables at the very top. (Generally accepted practice that prevents the need to hunt for variable declaration)

```cs

    public class PayLoad
    {       
        //Static Members       
        public static UTF8Encoding UTF8 = new UTF8Encoding();        
        public static UnicodeEncoding UTF16 = new UnicodeEncoding();
        
        //Constants
        const byte StringType = 1;    // string
        const byte IntType = 2;       // int  
        const int cNULLStringLen = -1;    

        //private members
        private byte[] mBuffer = new byte[1024];  
        private int mBufferIndex = 4; 

        //Public members
        public string Name {get; set;}
        
        ///  constructor
        public PayLoad(string Name)
        {
            this.Name = Name;
            WriteString(Name);
        }

	   //Method
	    private int GetDataLength()
        {
            int length = mBuffer.Length;
            return length;
        }	
   }
```

  
*	Use for loop instead of foreach when working with list and add or remove from source collection is needed. Because the collection used in foreach is immutable
and it can not be used to add or remove items from the source collection 

```cs

        for(int i=0; i< this.ReturnValues.Count;i++)
        {
          ActReturnValue value = this.ReturnValues[i];
          
          if(String.IsNullOrEmpty(value.Expected))
          {
              this.ReturnValues.RemoveAt(i);
              i--;;
          }
        }
```


#### Guidelines for Exception handling
*	Use the Reporter class to show any user message instead of a message box
```cs
 Reporter.ToUser(eUserMsgKeys.MissingActivityAppMapping);
 ```
 
* Use Reporter.ToLog to add any information to Ginger logs
```cs
Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
```