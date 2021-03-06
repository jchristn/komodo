# Change Log

## Current Version

v1.7.0

- Breaking changes
- Changes to crawl result and parse result classes (consolidation of classes)
- Changes to HTML parser
  - Separate tokens for head and body
  - Consolidated tokens including both head and body
  - Ability to strip and ignore sections while parsing
- Performance improvements in both parsing and generating postings
- Better control over configurable parameters within parsing and generating postings
- Dependency updates

## Previous Versions

v1.6.0

- Maintenance release

v1.5.1

- Dependency update
- New crawlers for AWS S3, S3 compatible storage (Minio, Less3, etc), Azure, Kvpbase
- Non-integrated version of metadata manager

v1.5.0

- Dependency update
- NuGet packages for each sub-project
- First release of Komodo.Daemon; in-process instance of Komodo for search, storage, and retrieval within your application

v1.4.2

- Changes to support multi-platform and containerization
- Dependency update

v1.4.1.x

- Async indexing API support with POSTbacks
- IncludeMetadata parameter in SearchQuery
- Amended welcome message on server start
- Fix for multi-platform

v1.4.0

- Breaking changes
- Major refactor for traceability and performance
- Update dependencies and added support for SQL Server, MySQL, PostgreSQL, and Sqlite
- Separated projects for crawling, parsing, generating postings, database interface, index, index management, and API server
- Database for configuration instead of discrete configuration files

v1.3.0

- Breaking changes
- Async APIs and dependency updates for performance and scalability
- Internally using streams for document storage and retrieval (excludes parsed documents)
- Async SDK APIs with KomodoException objects
- Improvements to SearchResult and EnumerationResult objects
- JSON/XML/SQL parsers now rely on text parser to identify terms

v1.2.x

- Support for both Sqlite and SQL Server for both documents database and postings database
- Simplified architecture using one large database vs several small databases for postings 
- Postings database settings now same structure as Documents database settings
- Settings and database query cleanup
- Source documents now have 'Title' field
- Fix URL encode/decode for querystring parameters (like title, source URL, etc)
- Index stats now includes terms count
- Added more split/stop characters to default Index configuration: (@ ._-)
- Posting positions are now trimmed if necessary to fit into database table

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
