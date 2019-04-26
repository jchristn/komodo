# Komodo Search

Information storage, search, and retrieval libraries and server in C#.  

## Help, Feedback, Contribute

If you have any issues or feedback, please file an issue here in Github.  We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## New in v1.2.x

The following capabilities and fixes were introduced in this version:

- Support for both Sqlite and SQL Server for both documents database and postings database
- Simplified architecture using one large database vs several small databases for postings 
- Postings database settings now same structure as Documents database settings
- Settings and database query cleanup
- Source documents now have 'Title' field
- Fix URL encode/decode for querystring parameters (like title, source URL, etc)
- Index stats now includes terms count
- Added more split/stop characters to default Index configuration: (@ ._-)
- Posting positions are now trimmed if necessary to fit into database table

## Enclosed Projects

The following projects are included:

| Project       | Description                                               |
|:------------- |:--------------------------------------------------------- |
| ParserCli     | Command line interface to use parsers                     |
| Core          | Core library incl parsing and index management            |
| Server        | RESTful API server                                        |
| TestParser    | Console application to exercise parsers                   |
| Sdk           | C# class library to consume RESTful APIs                  |
| TestSdk       | Console application using the Sdk to consume API server   |

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

v1.1.x
- Major refactor and code cleanup
- Purpose-built posting manager to improve scale of indexing, search, and storage
- Removed terms table
- Searches now continue until exhaustion or until max results are met
- Collapsed retrieval of querystring items into request parameters 
- Simple storage API to store a document in an index without indexing or parsing
- Enumeration of indexed documents
- Retarget to .NET Core and .NET Framework
- Eliminate standalone sqlite DLLs
- Various bugfixes
- MIT license

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
