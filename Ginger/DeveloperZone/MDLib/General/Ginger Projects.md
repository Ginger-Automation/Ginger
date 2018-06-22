# Ginger Projects

# Ginger.exe  - Ginger IDE - The Main UI for users
.NET 4.6.1
## Gigner WPF UI forms goes here
Include only Ginger core IDE Windows/Pages requires for Ginger basics operation

# GingerCore.dll 
.NET 4.6.1
## This project will become obsolete once refactoring into GingerCoreCommon/NET

# GingerCoreCommon.dll
## All Ginger common objects

All Repository Items
- RepositoryItemBase, [IsSerializedForLocalRepository]
- SolutionRepository 
- Repository serializer engine
- Application Models
- BusinessFlow, Activit, Act, RunSet, ProjEnvironemnt, Agent etc.

# GingerCoreNET.dll
*.NET Standard 2.0 *
## Ginger components requires for execution 

- GingerRunner, GingerRunners, 
- GingerGrid

# GingerUtils.dll - Stanalone utils which doesn't require any other Ginger project/ref
*.NET Standard 2.0 *

- LongPath
- 

# GingerControls.dll - Ginger User Controls
.NET 4.6.1

# GingerDriverWindow.exe - Ginger driver window
.NET 4.6.1


# GingerPlugins.dll - Ginger plugins dll
.NET 4.6.1
- all plugins reference this projects

# GingerConsole.exe
.NET Core 2.0

# GingerTestHelper.dll - Common function for testing
*.NET Standard 2.0 *

- GetTestResource()


- running Ginger on Linux/Mac/Windows without console UI, enable execution on CI/CD machines and docker

# Each project have a macthing Test project


See pic !!