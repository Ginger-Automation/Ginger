# New Test Project

#### All Unit Test projects need to be .NET Core 2.1 - so we can run them on Windows/Linux/Mac
#### The only exception is when the test open WPF forms like GingerTest which is .Net Framework 4.6.1 - as it connect and launch WPF forms and will run only on Windows

### Steps:

- Create a new .NET Core DLL project
- Add reference to the project(should be .NET Standards 2.0) which contains the classes to be tetsed
- Add any other refences if neeeded to GingerCoreCommon or others
- Set the test Level at the top of each class - see Test levels doc
- Test Project - add Nuget: 'MSTest.TestAdapter' and 'Microsoft.NET.Test.Sdk'- no need for other test Nuget
- Create sub folder: TestResources
- Add ref to GingerTestHelper project
Test project should include ONLY the folowing Nuget: 
