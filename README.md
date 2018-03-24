# Komodo Search
Document parsing libraries and search API server in C#.

## Help, Feedback, Contribute
If you have any issues or feedback, please file an issue here in Github.  We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

## New in v1.0.0.1
The following capabilities and fixes were introduced in this commit:
- Tested support for Microsoft Azure BLOB storage for source documents and parsed documents
- Untested support for AWS S3 and Kvpbase storage for source documents and parsed documents
- Index deletion will now delete source/parsed files if requested
- API to read source and parsed documents by ID

## Enclosed Projects
Please refer to the ```README.md``` file in each project for its version history and quickstart.
The following projects are included:

| Project       | Description                                               |
|:------------- |:--------------------------------------------------------- |
| Cli           | Command line interface to use parsers                     |
| Core          | Core library incl parsing and index management            |
| Server        | RESTful API server                                        |
| Test          | Console application to exercise parsers                   |

## Important
The current release of the Core library relies upon ```sqlite3.dll``` and stores both source and parsed documents on the local filesystem.  External storage for source documents is planned along with support for other database platforms.

## Project Roadmap
The following items are on the horizon for this project.  No timeline has been established, and no commitment is being made to implementing these items:
- Testing support for AWS S3 and Kvpbase
- Implement support for MSSQL, MySQL, and PostgreSQL as an alternative to Sqlite for index metadata

## Version History
v1.0.x
- Initial release with disk support