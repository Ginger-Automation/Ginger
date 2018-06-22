# Ginger unit test are marked with Levels and marked with AAA - Arrange, Act Assert

for example

```cs
  [TestMethod]
        public void VariableRandomNumber_Min5_Max10_Interval_1()
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
'''

- Level 1: For core test of items which are stand alone classes and requires very minimal setup to test the method
- Level 2: 
- Level 3:
- Level 4:
- Level 5:
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

