# Komodo Search

Information storage, search, and retrieval platform.  Komodo was written in C# and includes a RESTful server with robust APIs.

## Help, Feedback, Contribute

If you have any issues or feedback, please file an issue here in Github.  We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## New in v1.4.0

- Breaking changes
- Major refactor for traceability and performance
- Update dependencies and added support for SQL Server, MySQL, PostgreSQL, and Sqlite
- Separated projects for crawling, parsing, generating postings, database interface, index, index management, and API server
- Database for configuration instead of discrete configuration files
  
## API Documentation

Please refer to the Komodo POSTman collection in the root directory.

## Starting the Server
 
```
cd [project directory]
dotnet build -f netcoreapp3.0
dotnet publish -f netcoreapp3.0
cd bin\release\netcoreapp3.0\publish
dotnet Komodo.Server.dll
```

## Version History

Please refer to CHANGELOG.md for details.
