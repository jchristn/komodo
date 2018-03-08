# Komodo Search
Document parsing libraries and search API server in C#.

## Help, Feedback, Contribute
If you have any issues or feedback, please file an issue here in Github.  We'd love to have you help by contributing code for new features, optimization to the existing codebase, ideas for future releases, or fixes!

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
