# Ginger Projects

# Ginger.exe  - Ginger IDE - The Main UI for users
.NET 4.6.1
## Gigner WPF UI forms goes here
Include only Ginger core IDE Windows/Pages requires for Ginger basics operation
Edit Pages for: Business flow, Activity, Actions etc.
Window Explorer
Wizards pages
TreeView Items and edit pages
Images ?

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
- BusinessFlow, Activity, Act, RunSet, ProjEnvironemnt, Agent, Variables. Flow Control etc.
- Analyzer
- Models: API, UI, DB, Batch
- Tags
- Upgrader
- General Enums communly used
 Generic types and interface: IObservableList, ObservableList

# GingerApp
## Items shared when Ginger.exe/GingerConsole is up 
- WorkSpace
- Reporter
- Find/Replace

# GingerAutoPilot
* .NET Standard 2.0 *

# GingerCoreNET.dll  --> # GingerExecution.dll, GingerEngine.dll?
*.NET Standard 2.0 *
## Ginger components requires for execution 

- GingerRunner, GingerRunners, 
- GingerGrid
- Source control
- ValueExpression - calculate 

# GingerUtils.dll - Stanalone utils which doesn't require any other Ginger project/ref
*.NET Standard 2.0 *

- IO LongPath - handle long path add '//?/' when needed
- MRUManager - Most Recently used
- Email
- SMS
- Encryption
- JSONHelper
- StringCompressor
- XPath
- DeleteFolderContentBySizeLimit
- HttpUtilities
- XML Utils

# GingerControls.dll - Ginger User Controls
.NET 4.6.1

ucGrid
ucTreeView

# GingerReports.dll
.NET Standards 2.0

# GingerDriverWindow.exe - Ginger driver window
.NET 4.6.1


# GingerPlugins.dll - Ginger plugins dll
.NET Standards 2.0
- all plugins reference this projects

# GingerConsole.exe
.NET Core 2.0

- Console Menus

# GingerExecutionLogger - Save execution/operation/user stat and upload to centralized web for reports on the web
- AutoLog to capture data in JSON
- Upload to Web service


# GingerTestHelper.dll - Common function for testing
*.NET Standard 2.0 *

- GetTestResource()


- running Ginger on Linux/Mac/Windows without console UI, enable execution on CI/CD machines and docker

# Each project have a macthing Test project


TBD - ALM, SourceControl
See pic !!
