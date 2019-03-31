# Komodo Search

Information storage, search, and retrieval libraries and server in C#.  

## Help, Feedback, Contribute

If you have any issues or feedback, please file an issue here in Github.  We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## New in v1.1.x

The following capabilities and fixes were introduced in this commit:
- Minor refactor
- Enumeration of indexed documents
- Retarget to .NET Core and .NET Framework
- Eliminate standalone sqlite DLLs
- MIT license

## Enclosed Projects

Please refer to the ```README.md``` file in each project for its version history and quickstart.
The following projects are included:

| Project       | Description                                               |
|:------------- |:--------------------------------------------------------- |
| ParserCli     | Command line interface to use parsers                     |
| Core          | Core library incl parsing and index management            |
| Server        | RESTful API server                                        |
| TestParser    | Console application to exercise parsers                   |
| Sdk           | Class library to add to consume API server                |
| TestSdk       | Console application using the Sdk to consume API server   |

## Starting the Server

### Using .NET Core
```
cd bin\debug\netcoreapp2.0
dotnet KomodoServer.dll
```

### Using .NET Framework

Windows
```
cd bin\debug
KomodoServer.exe
```
Mono - use the Mono ahead of time compiler first (otherwise weird things will happen and you will find yourself in gdb hell)
```
sudo mono --aot=nrgctx-trampolines=8096,nimt-trampolines=8096,ntrampolines=4048 --server KomodoServer.exe
sudo mono --server KomodoServer.exe
```

## Project Roadmap

The following items are on the horizon for this project.  No timeline has been established, and no commitment is being made to implementing these items:
- Implement support for MySQL, and PostgreSQL as an alternative to Sqlite or MSSQL for index metadata

## Version History

v1.0.x
- Initial release with disk support
- Tested support for Microsoft Azure BLOB storage, Kvpbase, and AWS S3 for source documents and parsed documents
- Index deletion will now delete source/parsed files if requested
- API to read source and parsed documents by ID
- C# SDK and SDK test application
- Remove Sqlite for index management (still exists for terms, planned for removal)
- Index names are now case insensitive
- Fix to sample scripts 
- Index stats API
- Fix score calculation from optional filter match
- Moved storage configuration to Index object, updated scripts
- Fixed how JSON documents appear in search results (attached now as objects instead of as strings)
- Support for Sqlite, Microsoft SQL Server (and Azure) for index metadata database
- Index destroy now removes table rows and files
- Multiple bugfixes
