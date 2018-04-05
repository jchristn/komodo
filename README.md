# Komodo Search
Document parsing libraries and search API server in C#.

## Help, Feedback, Contribute
If you have any issues or feedback, please file an issue here in Github.  We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## New in v1.0.0.5
The following capabilities and fixes were introduced in this commit:
- Index names are now case insensitive
- Fix to sample scripts 
- Index stats API
- Fix score calculation from optional filter match

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

## Important
The current release of the Core library relies upon ```sqlite3.dll``` and stores both source and parsed documents on the local filesystem.  External storage for source documents is planned along with support for other database platforms.

## Compiling on Linux/Mac
In the ```Core``` project, remove the ```Mono.Posix``` NuGet package and add the ```Mono.Posix-4.5``` NuGet package.  Compile using Visual Studio for Mac.

## Starting the Server
On Windows, compile and run ```KomodoServer.exe``` in ```bin\debug``` or ```bin\release```.  
On Linux or Mac, compile and run the Mono Ahead-of-Time compiler before running the binary using ```mono```.
```
sudo mono --aot=nrgctx-trampolines=8096,nimt-trampolines=8096,ntrampolines=4048 --server KomodoServer.exe
sudo mono --server KomodoServer.exe
```

## Project Roadmap
The following items are on the horizon for this project.  No timeline has been established, and no commitment is being made to implementing these items:
- Implement support for MSSQL, MySQL, and PostgreSQL as an alternative to Sqlite for index metadata

## Version History
v1.0.x
- Initial release with disk support
- Tested support for Microsoft Azure BLOB storage, Kvpbase, and AWS S3 for source documents and parsed documents
- Index deletion will now delete source/parsed files if requested
- API to read source and parsed documents by ID
- C# SDK and SDK test application
- Remove Sqlite for index management (still exists for terms, planned for removal)
