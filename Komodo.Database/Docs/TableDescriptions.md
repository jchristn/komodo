# Tables

The following are tables used by Komodo.

## Important Notes

Sqlite has a limited set of datatypes (see https://www.sqlite.org/datatype3.html) compared to SQL Server, MySQL, and Postgres.  As such, ```datetime``` entries should be string with the form ```yyyy-MM-ddTHH:mm:ss.fff``` in UTC time.

## users

The ```users``` table identifies the users that are allowed to interact with Komodo.

| Column | Type  | Description  |
|------|---|---|
| id | int | Primary key |
| guid | string | Unique identifier for the user |
| name | string | Name of the user |
| email | string | Email address of the user |
| passwordmd5 | string | MD5 of the user's password |
| active | int | Indicates if the user account can be used (1) or not (0) |

## apikeys

The ```apikeys``` table identifies the credentials with which a user from the ```users``` table can interact with APIs.

| Column | Type  | Description  |
|------|---|---|
| id | int | Primary key |
| guid | string | Unique identifier for the API key, also serves as the API key itself |
| userguid | string | The GUID of the user to which this API key is mapped |
| active | int | Indicates if the API key can be used (1) or not (0) |

## metadata 

The ```metadata``` table is an unmanaged key-value store for configuration parameters that may be needed by various components in the system.

| Column | Type  | Description  |
|------|---|---|
| id | int | Primary key |
| configkey | string | Key from the metadata key-value pair |
| configval | string | Value from the metadata key-value pair |

## nodes

The ```nodes``` table identifies the various nodes that are in the deployment.  Reserved for future use.

| Column | Type  | Description  |
|------|---|---|
| id | int | Primary key |
| guid | string | Unique identifier for the node |
| hostname | string | DNS hostname or IP address |
| port | int | TCP port on which the node is listening for HTTP or HTTPS requests |
| ssl | int | Indicates if SSL is required (1) or not (0) |

## indices

The ```indices``` table is used to specify which indices are available for use.

| Column | Type  | Description  |
|------|---|---|
| id | int | Primary key |
| guid | string | Unique identifier for the index |
| ownerguid | string | The GUID of the user that owns this index |
| name | string | The name of the index |

## sourcedocs

The ```sourcedocs``` table holds metadata related to source documents that have been uploaded to Komodo.

Important: the ```guid``` within ```sourcedocs``` is often used as the master document ID within Komodo.

| Column | Type  | Description  |
|------|---|---|
| id | int | Primary key |
| guid | string | Unique identifier for the source document |
| ownerguid | string | The GUID of the user that owns this document | 
| indexguid | string | The GUID of the index where this document was uploaded |
| name | string | The name of the document |
| title | string | The title of the document |
| tags | string | CSV list containing tags associated with the document |
| doctype | string | The type of document, i.e. json, xml, etc |
| sourceurl | string | The URL from which this document was retrieved |
| contenttype | string | The content type of the document |
| contentlength | long | The content length of the document | 
| md5 | string | The MD5 hash of the document |
| created | datetime | Timestamp from when the source document was stored |
| indexed | datetime | Timestamp from when the document indexing was completed |

## parseddocs

The ```parseddocs``` table holds metadata related to storage of parse results for source documents.  ```parseddocs``` should be considered a child of ```sourcedocs```, linked to ```sourcedocs``` by the ```sourcedocguid``` column.
 
| Column | Type  | Description  |
|------|---|---|
| id | int | Primary key |
| guid | string | Unique identifier for the parsed document |
| sourcedocguid | string | The GUID of the source document in the sourcedocs table |
| ownerguid | string | The GUID of the user that owns this document | 
| indexguid | string | The GUID of the index where this document was uploaded |
| doctype | string | The type of document, i.e. json, xml, etc |
| contentlength | long | The content length of the parsed document | 
| terms | long | The number of terms in the parsed document |
| postings | long | The number of postings in the parsed document |
| created | datetime | Timestamp from when the parsed document was stored |
| indexed | datetime | Timestamp from when the document indexing was completed |

## termguids

The ```termguids``` table maps a term to an index to a GUID.  This table serves as a lookup table to identify the GUID of a term when searching a particular index, which will then be relevant to retrieve documents containing that term (using the ```termdocs``` table).

| Column | Type  | Description  |
|------|---|---|
| id | int | Primary key |
| guid | string | Unique identifier for the term |
| indexguid | string | The GUID of the index where this term can be found in documents |
| term | string | The term itself |

## termdocs 

The ```termdocs``` table maps a term, by GUID, to a document, by GUID, in an index, by GUID.  That is, if a given document stored in a certain index contains a specific term, an entry will be found in this table.  The ```parseddocs``` entry can then be consulted (and more specifically, the file containing the parse result) to examine and evaluate the postings.

| Column | Type  | Description  |
|------|---|---|
| id | int | Primary key |
| guid | string | Unique identifier for the record |
| indexguid | string | The GUID of the index where this term can be found in tje specified document |
| termguid | string | The GUID of the term |
| sourcedocguid | string | The GUID of the source document |
| parseddocguid | string | The GUID of the parsed document |
