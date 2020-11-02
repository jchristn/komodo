![alt tag](https://github.com/jchristn/komodo/blob/master/Assets/komodo-icon.ico)

| Package | Description | Version | Downloads | Komodo Dependencies |
|---|---|---|---|---|
| Komodo.Classes | Core classes | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.Classes.svg?style=flat)](https://www.nuget.org/packages/Komodo.Classes/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.Classes.svg)](https://www.nuget.org/packages/Komodo.Classes) | none |
| Komodo.Sdk | C# client SDK | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.Sdk.svg?style=flat)](https://www.nuget.org/packages/Komodo.Sdk/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.Sdk.svg)](https://www.nuget.org/packages/Komodo.Sdk) | none |
| Komodo.Crawler | FTP, HTTP, HTTPS, disk crawlers | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.Crawler.svg?style=flat)](https://www.nuget.org/packages/Komodo.Crawler/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.Crawler.svg)](https://www.nuget.org/packages/Komodo.Crawler) | Classes |
| Komodo.Parser | JSON, XML, HTML, SQL, and text parsers | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.Parser.svg?style=flat)](https://www.nuget.org/packages/Komodo.Parser/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.Parser.svg)](https://www.nuget.org/packages/Komodo.Parser) | Crawler |
| Komodo.Postings | Postings generator | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.Postings.svg?style=flat)](https://www.nuget.org/packages/Komodo.Postings/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.Postings.svg)](https://www.nuget.org/packages/Komodo.Postings) | Parser |
| Komodo.IndexClient | Index client | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.IndexClient.svg?style=flat)](https://www.nuget.org/packages/Komodo.IndexClient/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.IndexClient.svg)](https://www.nuget.org/packages/Komodo.IndexClient) | Postings |
| Komodo.IndexManager | Index manager | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.IndexManager.svg?style=flat)](https://www.nuget.org/packages/Komodo.IndexManager/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.IndexManager.svg)](https://www.nuget.org/packages/Komodo.IndexManager) | IndexClient |
| Komodo.MetadataManager | Metadata management | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.MetadataManager.svg?style=flat)](https://www.nuget.org/packages/Komodo.MetadataManager/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.MetadataManager.svg)](https://www.nuget.org/packages/Komodo.MetadataManager) | IndexManager |
| Komodo.Daemon | In-process version of Komodo | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.Daemon.svg?style=flat)](https://www.nuget.org/packages/Komodo.Daemon/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.Daemon.svg)](https://www.nuget.org/packages/Komodo.Daemon) | IndexManager |
| Komodo.Server | Standalone API server | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.Server.svg?style=flat)](https://www.nuget.org/packages/Komodo.Server/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.Server.svg)](https://www.nuget.org/packages/Komodo.Server) | Daemon |
 
# Komodo Search

Information storage, search, metadata, and retrieval platform.  Komodo was written in C# and includes a host of libraries for data management, data storage, search, and information retrieval, including a standalone RESTful API server and in-process daemon that you can include within your own application.

## Help, Feedback, Contribute

If you have any issues or feedback, please file an issue here in Github.  We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## New in v1.7.0

- Breaking changes
- Changes to crawl result and parse result classes (consolidation of classes)
- Changes to HTML parser
  - Separate tokens for head and body
  - Consolidated tokens including both head and body
  - Ability to strip and ignore sections while parsing
- Performance improvements in both parsing and generating postings
- Better control over configurable parameters within parsing and generating postings
- Dependency updates

## API Documentation

Please refer to the Komodo POSTman collection in the root directory.

## Starting the Server
 
```
cd [project directory]
dotnet build -f netcoreapp3.1
dotnet publish -f netcoreapp3.1
cd bin\release\netcoreapp3.1\publish
dotnet Komodo.Server.dll
```

## Deploying Komodo.Server with Docker

Refer to ```Docker.md``` for details.

## Version History

Please refer to CHANGELOG.md for details.
