![alt tag](https://github.com/jchristn/komodo/blob/master/Assets/komodo-icon.ico)

| Package | Description | Version | Downloads | Komodo Dependencies |
|---|---|---|---|---|
| Komodo.Core | Core classes | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.Classes.svg?style=flat)](https://www.nuget.org/packages/Komodo.Classes/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.Classes.svg)](https://www.nuget.org/packages/Komodo.Classes) | none |
| Komodo.Sdk | C# client SDK | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.Sdk.svg?style=flat)](https://www.nuget.org/packages/Komodo.Sdk/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.Sdk.svg)](https://www.nuget.org/packages/Komodo.Sdk) | none |
| Komodo.Server | Standalone API server | [![NuGet Version](https://img.shields.io/nuget/v/Komodo.Server.svg?style=flat)](https://www.nuget.org/packages/Komodo.Server/) | [![NuGet](https://img.shields.io/nuget/dt/Komodo.Server.svg)](https://www.nuget.org/packages/Komodo.Server) | Komodo.Core |
 
# Komodo Search

Information storage, search, metadata, and retrieval platform.  Komodo was written in C# and includes a host of libraries for data management, data storage, search, and information retrieval, including a standalone RESTful API server and in-process daemon that you can include within your own application.

## Help, Feedback, Contribute

If you have any issues or feedback, please file an issue here in Github.  We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## New in v2.0.0

- Breaking changes 
- Consolidated packages for simplicity
- Performance improvements (batch SQL INSERTs instead of discrete INSERTs)
- Support for CSV parsing
- Class enhancements including .ToJson() and JSON ordering
- Postings metadata now retained in the database and included in document metadata 
- Text parser can now eliminate stop words (not just postings)
- Dependency updates

## API Documentation

Please refer to the Komodo POSTman collection in the root directory.

## Starting the Server
 
```
cd [project directory]
dotnet build -f net5.0
dotnet publish -f net5.0
cd bin\release\net5.0\publish
dotnet Komodo.Server.dll
```

## Deploying Komodo.Server with Docker

Refer to ```Docker.md``` for details.

## Licensing

Komodo is licensed under the MIT license.  However, there are other packages used by Komodo that are licensed under different licenses, and you should be aware of such licenses if you embed Komodo into your application.

### Packages under MIT License

- Watson Webserver (https://github.com/jchristn/WatsonWebserver)
- Watson ORM (https://github.com/jchristn/watsonorm)
- BlobHelper (https://github.com/jchristn/blobhelper)
- HtmlAgilityPack (https://github.com/zzzprojects/html-agility-pack)
- SyslogLogging (https://github.com/jchristn/loggingmodule)
- XmlToPox (https://github.com/jchristn/xmltopox)
- RestWrapper (https://github.com/jchristn/restwrapper)

### Packages under Apache License 2.0 and MS-PL

- CsvHelper (https://github.com/JoshClose/CsvHelper)

## Version History

Please refer to CHANGELOG.md for details.
