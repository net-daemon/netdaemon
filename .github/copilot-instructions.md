# Copilot Instructions for NetDaemon

This document provides guidance for AI assistants working on the NetDaemon project.

## .NET Version

- **Always use the .NET version specified in `global.json`**

## Development Workflow

### Building the Project
```bash
dotnet restore
dotnet build --configuration Release
```

### Running Tests
**Always verify code changes by running tests with `dotnet test`**

The project has comprehensive test coverage including:
- Unit tests for individual components
- Integration tests with Home Assistant (stable and beta versions)

Run specific test projects:
```bash
# Unit tests
dotnet test src/HassModel/NetDaemon.HassModel.Tests
dotnet test src/Extensions/NetDaemon.Extensions.Scheduling.Tests
dotnet test src/Client/NetDaemon.HassClient.Tests
dotnet test src/AppModel/NetDaemon.AppModel.Tests
dotnet test src/Runtime/NetDaemon.Runtime.Tests

# Integration tests
dotnet test tests/Integration/NetDaemon.Tests.Integration
```

### Code Quality
- Follow the existing `.editorconfig` settings
- The project uses Roslynator for additional code analysis
- Build warnings are treated as errors in CI
- Maintain test coverage for new features

### Making Changes
- Focus on minimal, surgical changes
- Maintain backward compatibility where possible
- Update tests for any functional changes
- Consider integration test coverage for Home Assistant interactions

## Key Documentation Links

### NetDaemon Documentation
- **NetDaemon User Docs**: https://netdaemon.xyz/docs/user/
- **NetDaemon Developer Site**: https://netdaemon.xyz/docs/developer

### Home Assistant Documentation
- **Home Assistant Docs**: https://www.home-assistant.io/docs/
- **Home Assistant Developer Docs**: https://developers.home-assistant.io/