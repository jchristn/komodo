# Komodo Search

Information storage, search, and retrieval platform.  Komodo was written in C# and includes a RESTful server with robust APIs.

## Help, Feedback, Contribute

If you have any issues or feedback, please file an issue here in Github.  We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## New in v1.3.0

- Breaking changes
- Async APIs and dependency updates for performance and scalability
- Internally using streams for document storage and retrieval (excludes parsed documents)
- Async SDK APIs with KomodoException objects
- Improvements to SearchResult and EnumerationResult objects
- JSON/XML/SQL parsers now rely on text parser to identify terms

The following capabilities and fixes were introduced in this version:

## Enclosed Projects

The following projects are included:

| Project       | Description                                               |
|:------------- |:--------------------------------------------------------- |
| Core          | Core library incl parsing and index management            |
| Server        | RESTful API server                                        |
| Sdk           | C# class library to consume RESTful APIs                  |
| TestSdk       | Console application using the Sdk to consume API server   |
| KParse        | Command line interface to use parsers                     |
| TestParser    | Console application to exercise parsers                   |

## API Documentation

Please refer to the ```README.md``` file in the ```Server``` project for API documentation. 

## Starting the Server

### Using .NET Core
```
cd bin\debug\netcoreapp2.0
dotnet Komodo.Server.dll
```

### Using .NET Framework

Windows
```
cd bin\debug
Komodo.Server.exe
```
Mono - use the Mono ahead of time compiler first (otherwise weird things will happen and you will find yourself in gdb hell)
```
sudo mono --aot=nrgctx-trampolines=8096,nimt-trampolines=8096,ntrampolines=4048 --server KomodoServer.exe
sudo mono --server Komodo.Server.exe
```

## Project Roadmap

The following items are on the horizon for this project.  No timeline has been established, and no commitment is being made to implementing these items:

- Implement support for MySQL, and PostgreSQL as an alternative to Sqlite or MSSQL for index metadata

## Version History

Please refer to CHANGELOG.md for details.
