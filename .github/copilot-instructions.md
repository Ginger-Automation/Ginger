# Ginger Automation AI Coding Agent Guide

## Project Overview
Ginger is a comprehensive automation IDE supporting multiple platforms (Web, Mobile, APIs, Java, Windows, PowerBuilder, Linux/Unix, Mainframe). It uses a drag-and-drop approach enabling users with or without coding skills to create automation flows.

## Core Architecture Concepts

### 1. Execution Hierarchy
The fundamental execution structure follows this hierarchy:
```
Solution → RunSet → Runner → BusinessFlow → Activity → Action (Act)
```

- **BusinessFlow**: Top-level test scenario container (`GingerCoreCommon/Repository/BusinessFlowLib/BusinessFlow.cs`)
- **Activity**: Group of related actions within a BusinessFlow (`GingerCoreCommon/Repository/BusinessFlowLib/Activity.cs`)
- **Act/Action**: Individual executable step (`GingerCoreCommon/Actions/Act.cs`)
- **Agent**: Driver wrapper that connects to target applications (`GingerCoreCommon/RunLib/Agent.cs`)

### 2. Key Project Structure
- **Ginger/**: Main WPF application (UI)
- **GingerCoreCommon/**: Shared models and base classes
- **GingerCoreNET/**: .NET execution engine and CLI components
- **GingerRuntime/**: Console application entry point
- **GingerCore/**: Legacy core components (being migrated to GingerCoreNET)

### 3. Driver Architecture
Each platform has dedicated drivers under `GingerCore/Drivers/`:
- `SeleniumDriver` for web automation
- `JavaDriver` for Java applications
- `ConsoleDriverLib` for command-line interfaces
- `WindowsAutomation` for Windows applications
- Agents (`Agent.cs`) act as wrappers providing unified interface

## Development Workflows

### Building & Testing
```powershell
# Main build (Windows)
dotnet build Ginger/Ginger.sln

# Run tests
./TestDotNetFramework.ps1  # Framework tests
./CLITests.ps1             # CLI functionality tests

# Console execution
cd Ginger/GingerRuntime/bin/Release/net8.0/publish/
dotnet GingerConsole.dll run -s "path/to/solution" -e "Environment" -r "RunSet"
```

### CLI Commands
**Windows:**
- `ginger.exe help` or `dotnet ginger.dll help` - Show CLI help
- `ginger.exe run -s <solution> -e <env> -r <runset>` - Execute automation
- `ginger.exe analyze -s <solution>` - Analyze solution
- `ginger.exe grid --port <port>` - Start grid mode

**Linux:**
- `dotnet gingerruntime.dll help` - Show CLI help
- `dotnet gingerruntime.dll run -s <solution> -e <env> -r <runset>` - Execute automation
- `dotnet gingerruntime.dll analyze -s <solution>` - Analyze solution
- `dotnet gingerruntime.dll grid --port <port>` - Start grid mode

## Project Conventions

### 1. File Organization
- Actions inherit from `Act` class and implement platform-specific logic
- Repository objects use `[IsSerializedForLocalRepository]` attribute for persistence
- Test files end with `Test.cs` and use MSTest framework with `[TestClass]` and `[TestMethod]`

### 2. Naming Patterns
- Actions: `Act{PlatformType}{ActionType}` (e.g., `ActBrowserElement`, `ActConsoleCommand`)
- Drivers: `{Platform}Driver` (e.g., `SeleniumDriver`, `JavaDriver`)
- Pages/Windows: `{Feature}Page` or `{Feature}Window`

### 3. Error Handling
- Use `Reporter.ToLog(eLogLevel.ERROR, message)` for logging
- Actions set `act.Error` and `act.Status = eRunStatus.Failed` on failure
- Execution continues based on `Activity.ActionRunOption` setting

## Integration Points

### 1. Solution Structure
Solutions are XML-based with this folder structure:
```
Solution/
├── BusinessFlows/        # Test scenarios
├── Activities/          # Reusable activity groups  
├── Actions/             # Shared actions
├── Agents/              # Driver configurations
├── Environments/        # Environment settings
├── DataSources/         # Test data
└── Applications/        # Target application definitions
```

### 2. Execution Engine
- `RunsetExecutor` orchestrates execution (`GingerCoreNET/Run/RunsetExecutor.cs`)
- `GingerExecutionEngine` handles individual runner execution
- Execution logging via `ExecutionLogger` implementations
- Supports parallel execution across multiple agents

### 3. Plugin Architecture  
- Legacy drivers implement `DriverBase`
- New plugins use `Agent.eAgentType.Service` with `PluginId`/`ServiceId`
- External integrations via `ALM` (Application Lifecycle Management) connectors
- Plugin packages defined by `Ginger.PluginPackage.json` manifest
- Managed through `PluginsManager` (`GingerCoreNET/PlugInsLib/PluginsManager.cs`)

## Critical Development Notes

- **Target Framework**: .NET 8.0 with Windows 10.0.17763.0 compatibility
- **WPF Dependencies**: Main UI uses WPF; console components are cross-platform
- **XML Serialization**: Repository objects serialize to XML using custom attributes
- **Multi-Threading**: Execution engine supports parallel runners; use thread-safe patterns
- **Memory Management**: Dispose drivers properly; avoid memory leaks in long-running executions

## Testing Strategy
- Unit tests in `*Test.cs` files use dependency injection patterns
- Integration tests use `TestResources/Solutions/` for test data  
- CLI tests verify command-line interface functionality
- Use `mConsoleMessages` pattern for testing CLI output capture

When working with this codebase, focus on the execution hierarchy and understand how Actions flow through Activities within BusinessFlows, executed by Agents that wrap platform-specific drivers.