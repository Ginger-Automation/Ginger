# Ginger unit test are marked with Levels and marked with AAA - Arrange, Act Assert

- Level 1: For core test of items which are stand alone classes and requires very minimal setup to test the method
	- tests only basic core test runs in less than 500 ms, can run in parallel – no dependency – no Mutex
- Level 2: Plugins, Driver, Standalone actions, GingerConsole
- Level 3: GingerWPF, include UI testing launch browser, test app etc, can have Mutex – sequential
- Level 4: Slower Longer tests – test durability, parallelism
- Level 5: Check Performance only, speed of execution, can be long test running the same action many times
- Level 6: Requires internet connection - going to public web sites, for example to test Github check-in
- Level 7: 
- Level 8:
- Level 9:
- Level 10: Running full Ginger UI and performing different activities like create new solution, add Agent, automate and more, same liek a regular user will do 
these tests include visual testing where we comapre baseline screen to current screen and doing bitmap diff with several algorithms to compare

- Level 20: Longevity Tests - these test execute run sets and keep Ginger running for hours while measuring Memory, CPU, Disk consumption.
These tests execution takes 36 hours and they are executed once a week on the developemnt branch
- Level 21: Performence Tests - 

- Level 30: Require specific component to be installed and user/pass to acces it like SVN server
- Level 31: Amdocs internal - We have a large amount of automation solution of our customers and we verify the new build is working on then without impact

# Test are executed in order, if Level 1 fails it will not continue to the next level and build will be marked as failed

## Level 1-10 are executed as part of every build, after every push.
## Level 20+ are executed only on env were the required componenets exist and setup

## Test class name should end with *Test*
## Mark the test class with the test level, do not mark specific methods
## Name the test method describing what the test does, do not use underscore '_' in test method
## Keep the test method code minimal 
## More asserts per test method is better no limit
## Assert should have comments 
## Use Test resources methods to get test data
## Output data
## Data Driver TestMethod input from CSV, or DataRow
## Run seqencial test using Mutex

For example

```cs

public class StandAlonePluginTest
{
    [ClassInitialize]
    public static void ClassInit(TestContext TC)
    {
        // put class initialization code here, will be exeucted once 
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
		// put class cleanup code here if needed 
    }

    [TestInitialize]
    public void TestInitialize()
    {
        // Code that will be executed before each test method start
    }

    [TestCleanup]
    public void TestCleanUp()
    {
        // Code which will be executed after the test method
    }

[Level1]
[TestMethod,Timeout(60000)]
public void VariableRandomNumberMin5Max10Interval1()
{
    //Arrange
    VariableRandomNumber VRN = new VariableRandomNumber();
    VRN.Min = 5;
    VRN.Max = 10;
    VRN.Interval = 1;

    //Act
    VRN.GenerateAutoValue();

    //Assert
    Assert.IsTrue(decimal.Parse(VRN.Value) >= 5, "vn.Value>=5");
    Assert.IsTrue(decimal.Parse(VRN.Value) <= 10, "vn.Value<=10");
}
```


