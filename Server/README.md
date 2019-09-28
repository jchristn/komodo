# Komodo Search - Server

API server instance for Komodo to expose APIs for information storage, search, and retrieval. 

## APIs

Use the following APIs to exercise the platform.

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

## Create an Index
```
POST /indices
Request body:
{
  "IndexName": "First",
  "RootDirectory": "First",
  "Options": {
    "SplitCharacters": [
      "]","[",",","."," ","'","\"",";","<",">",".","/","\\","|","{","}","(",")","@","_","-"
    ],
    "NormalizeCase": true,
    "RemovePunctuation": true,
    "RemoveStopWords": true,
    "PerformStemming": true,
    "MinTokenLength": 3,
    "MaxTokenLength": 32,
    "StopWords": []
  },
  "DocumentsDatabase": {
    "Type": "Sqlite",
    "Filename": "First/Documents.db",
    "Hostname": "localhost",
    "Port": 8100,
    "DatabaseName": "Komodo",
    "InstanceName": "Komodo",
    "Username": "komodo",
    "Password": "komodo",
    "Debug": false
  },
  "PostingsDatabase": {
    "Type": "Sqlite",
    "Filename": "First/Postings.db",
    "Hostname": "localhost",
    "Port": 8100,
    "DatabaseName": "Komodo",
    "InstanceName": "Komodo",
    "Username": "komodo",
    "Password": "komodo",
    "Debug": false
  },
  "StorageSource": {
    "Type": "Disk",
    "Disk": {
      "Directory": "SourceDocuments"
    }
  },
  "StorageParsed": {
    "Type": "Disk",
    "Disk": {
      "Directory": "ParsedDocuments"
    }
  }
}
Response:
201/Created
```

The object for ```DocumentsDatabase``` and ```PostingsDatabase``` supports the following types:
- Sqlite
- Mssql

The object for ```StorageSource``` and ```StorageParsed``` supports the following types:
- Disk
- AwsS3
- Azure
- Kvpbase

### Storage Settings

The object structure for ```Type == StorageType.Disk``` is shown above.

For ```AwsS3```:
```
{
  "Hostname": "<hostname>",       // leave null if using AWS S3, set if using other S3-compatible storage
  "Ssl": true,                    // enable or disable SSL
  "AccessKey": "<access key>",    // AWS access key
  "SecretKey": "<secret key>",    // AWS secret key
  "Region": "<aws region>",       // AWS region, see below, e.g. USWest1
  "Bucket": "<bucket name>"       // name of the S3 bucket
}
```

Supported AWS S3 regions:
```
APNortheast1 = 0,
APNortheast2 = 1,
APNortheast3 = 2,
APSoutheast1 = 3,
APSoutheast2 = 4,
APSouth1 = 5,
CACentral1 = 6,
CNNorth1 = 7,
EUCentral1 = 8,
EUNorth1 = 9,
EUWest1 = 10,
EUWest2 = 11,
EUWest3 = 12,
SAEast1 = 13,
USEast1 = 14,
USEast2 = 15,
USGovCloudEast1 = 16,
USGovCloudWest1 = 17,
USWest1 = 18,
USWest2 = 19
```

For ```Azure```:
```
{
  "AccountName": "<azure account name>",
  "AccessKey": "<access key>",
  "Endpoint": "<azure BLOB URL>",         // of the form https://[accountname].blob.core.windows.net/
  "Container": "<azure container name>"
}
```

For ```Kvpbase```:
```
{
  "Endpoint": "<kvpbase URL>",      // of the form http[s]://[hostname]:[port]/
  "UserGuid": "<kvpbase user GUID>",
  "Container": "<kvpbase container name>",
  "ApiKey": "<kvpbase API key>"
}
```

## Retrieve List of Indices
```
GET /indices
Repsonse:
[
  "First"
]
```

## Retrieve an Index
```
GET /First
Response:
{
  "IndexName": "First",
  "RootDirectory": "First",
  "Options": {
    "SplitCharacters": [
      "]","[",",","."," ","'","\"",";","<",">",".","/","\\","|","{",""}","(",")","@","_","-"
    ],
    "NormalizeCase": true,
    "RemovePunctuation": true,
    "RemoveStopWords": true,
    "PerformStemming": true,
    "MinTokenLength": 3,
    "MaxTokenLength": 32,
    "StopWords": []
  },
  "DocumentsDatabase": {
    ...
  }
  ...
}
```

## Retrieve Index Stats
```
GET /First/stats
Response:
{
    "IndexName": "first",
    "SourceDocuments": {
        "Count": 0,
        "SizeBytes": 0
    },
    "ParsedDocuments": {
        "Count": 0,
        "SizeBytesParsed": 0,
        "SizeBytesSource": 0
    },
    "TermsCount": 0
}
```

## Preview Parse a Document Without Indexing
```
POST /_parse?type=json
[
  {
    "_id": "5c99acba82e8738d61dd091e",
    "index": 0,
    "guid": "69efa262-c510-4f6b-9b9c-2d1432c94035",
    "isActive": false,
    "balance": "$1,348.47",
    "picture": "http://placehold.it/32x32",
    "age": 34,
    "eyeColor": "brown",
    "name": "Fernandez Howell",
    "gender": "male",
    "company": "ROBOID",
    "email": "fernandezhowell@roboid.com",
    "phone": "+1 (930) 509-3204",
    "address": "310 Dahl Court, Wawona, Ohio, 1142",
    "about": "Ex mollit sunt reprehenderit elit laborum ullamco voluptate id. Exercitation minim adipisicing ut consectetur nisi quis aliqua dolore veniam ea ullamco in. Reprehenderit quis proident elit mollit excepteur sit non cillum dolor voluptate do consequat. Qui culpa aliquip cupidatat voluptate amet aliqua non ullamco duis. Occaecat occaecat commodo est magna nulla anim minim in do commodo est occaecat. Laborum dolor dolore ipsum minim ut incididunt sit deserunt adipisicing elit adipisicing est duis irure.\r\n",
    "registered": "2017-12-30T04:55:56 +08:00",
    "latitude": -62.599274,
    "longitude": 98.830868,
    "tags": [
      "consectetur",
      "ad",
      "ea",
      "est",
      "incididunt",
      "tempor",
      "magna"
    ],
    "friends": [
      {
        "id": 0,
        "name": "Tracie Hahn"
      },
      {
        "id": 1,
        "name": "Alyssa Fitzgerald"
      },
      {
        "id": 2,
        "name": "Bell Stark"
      }
    ],
    "greeting": "Hello, Fernandez Howell! You have 4 unread messages.",
    "favoriteFruit": "apple"
  }
]

Response body:
{
    "Schema": {
        "undefined": "Array",
        "_id": "String",
        "index": "Integer",
        "guid": "String",
        "isActive": "Boolean",
        ... 
    },
    "MaxDepth": 2,
    "ArrayCount": 3,
    "NodeCount": 39,
    "Flattened": [
        {
            "Key": "undefined",
            "Type": "Array"
        },
        {
            "Key": "_id",
            "Data": "5c99acba82e8738d61dd091e",
            "Type": "String"
        }, ... 
    ],
    "Tokens": [
        "5c99acba82e8738d61dd091e",
        "69efa262-c510-4f6b-9b9c-2d1432c94035",
        "false",
        ... 
    ]
}
```

## Preview Index a Document without Adding to Index
```
POST /_index?type=json
[
  {
    "_id": "5c99acba82e8738d61dd091e",
    "index": 0,
    "guid": "69efa262-c510-4f6b-9b9c-2d1432c94035",
    "isActive": false,
    "balance": "$1,348.47",
    "picture": "http://placehold.it/32x32",
    "age": 34,
    "eyeColor": "brown",
    "name": "Fernandez Howell",
    "gender": "male",
    "company": "ROBOID",
    "email": "fernandezhowell@roboid.com",
    "phone": "+1 (930) 509-3204",
    "address": "310 Dahl Court, Wawona, Ohio, 1142",
    "about": "Ex mollit sunt reprehenderit elit laborum ullamco voluptate id. Exercitation minim adipisicing ut consectetur nisi quis aliqua dolore veniam ea ullamco in. Reprehenderit quis proident elit mollit excepteur sit non cillum dolor voluptate do consequat. Qui culpa aliquip cupidatat voluptate amet aliqua non ullamco duis. Occaecat occaecat commodo est magna nulla anim minim in do commodo est occaecat. Laborum dolor dolore ipsum minim ut incididunt sit deserunt adipisicing elit adipisicing est duis irure.\r\n",
    "registered": "2017-12-30T04:55:56 +08:00",
    "latitude": -62.599274,
    "longitude": 98.830868,
    "tags": [
      "consectetur",
      "ad",
      "ea",
      "est",
      "incididunt",
      "tempor",
      "magna"
    ],
    "friends": [
      {
        "id": 0,
        "name": "Tracie Hahn"
      },
      {
        "id": 1,
        "name": "Alyssa Fitzgerald"
      },
      {
        "id": 2,
        "name": "Bell Stark"
      }
    ],
    "greeting": "Hello, Fernandez Howell! You have 4 unread messages.",
    "favoriteFruit": "apple"
  }
]

Response body:
{
    "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd",
    "DocumentType": "Json", 
    "Schema": {
        "undefined": "Array",
        "_id": "String",
        "index": "Integer",
        "guid": "String",
        ... 
    },
    "Postings": [
        {
            "Term": "5c99acba82e8738d61dd091e", 
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "69efa262 c510 4f6b 9b9c 2d1432c94035", 
            "Frequency": 1,
            "Positions": [
                0
            ]
        }, ... 
    ],
    "Terms": [
        "5c99acba82e8738d61dd091e",
        "69efa262 c510 4f6b 9b9c 2d1432c94035",
        "False",
        "348",
        "http",
        "placehold",
        "32x32",
        "brown",
        ... 
    ],
    "Json": {
        "Schema": {
            "undefined": "Array",
            "_id": "String",
            "index": "Integer",
            "guid": "String",
            "isactive": "Boolean",
            ... 
        },
        "MaxDepth": 2,
        "ArrayCount": 3,
        "NodeCount": 39,
        "Flattened": [
            {
                "Key": "undefined",
                "Type": "Array"
            },
            {
                "Key": "_id",
                "Data": "5c99acba82e8738d61dd091e",
                "Type": "String"
            },
            {
                "Key": "index",
                "Data": "0",
                "Type": "Integer"
            },
            ... 
        ],
        "Tokens": [
            "5c99acba82e8738d61dd091e",
            "0",
            "69efa262 c510 4f6b 9b9c 2d1432c94035",
            "false",
            ... 
        ]
    }
}
```

## Add a Document to an Index
```
POST /First?type=json
Request body: <json document>
Response:
{
    
  "DocumentId": "1e9fac69-07b5-4d46-ac08-0a40968864be",
  "AddTimeMs": 250
}
```

In the querystring, ```type``` can be set to ```json```, ```text```, ```xml```, or ```sql```.

You can additionally add querystring entries for ```name```, ```tags```, ```sourceurl```, and ```title```.

## Store a Document in an Index, without indexing or parsing
```
POST /First?type=json&bypass=true
Request body: <json document>
Response:
{
    
  "DocumentId": "1e9fac69-07b5-4d46-ac08-0a40968864be",
  "AddTimeMs": 16
}
```

Using ```bypass=true``` will bypass indexing and simply store the object in Komodo without creating postings.  This document will NOT appear in search results, but will appear when enumerating the index.  

## Query the Index
```
PUT /First
Request body:
{
  MaxResults: 5,
  StartIndex: null,
  Required: {
    Terms: [ "Joel" ],
    Filter: [
      {
         "Field": "lineitems.lineitemid",
         "Condition": "GreaterThan",
         "Value": "0"
      }
    ]
  },
  Optional: {
    Terms: [ ],
    Filter: [ ]
  },
  Exclude: {
    Terms: [ ],
    Filter: [ ]
  },
  IncludeContent: true,
  IncludeParsedDoc: false
}
Response body:
{
  "Query": {
    "GUID": "177b6ae6-3ca8-4d2e-b52a-710147d3d24b",
    "MaxResults": 5,
    "Required": {
      "Terms": [
        "Joel"
      ],
      "Filter": [
        {
          "Field": "lineitems.lineitemid",
          "Condition": "GreaterThan",
          "Value": "0"
        }
      ]
    },
    "Optional": {
      "Terms": [],
      "Filter": []
    },
    "Exclude": {
      "Terms": [],
      "Filter": []
    },
    "IncludeContent": true,
    "IncludeParsedDoc": false
  },
  "Async": false,
  "StartTimeUtc": "2019-03-27T06:26:22.698305Z",
  "EndTimeUtc": "2019-03-27T06:26:22.9024586Z",
  "TotalTimeMs": 204.1536,
  "MatchCount": {
    "TermsMatch": 5,
    "FilterMatch": 5
  },
  "Matches": [
    {
      "DocumentId": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7",
      "DocumentType": "Json",
      "Score": 1,
      "Errors": [],
      "Data": {
        "OrderNumber": 1,
        "Amount": 100,
        "Customer": {
          "FirstName": "Joel",
          "LastName": "Christner",
          "Address": "123 Main St",
          "City": "San Jose",
          "State": "CA",
          "Postal": "95128"
        },
        "LineItems": [
          {
            "LineItemId": 1,
            "ItemId": 1,
            "Name": "Paper clips",
            "Quantity": 5,
            "UnitPrice": 5,
            "Subtotal": 25
          }, ... 
        ]
      }
    }, ...
  ]
}
```

The ```Filter``` object is a list of ```SearchFilter``` objects.  At least one term must be included in ```Required.Terms```.  Terms-based searches rely on the database, whereas Filter-based searches will iterate over the content itself.  Thus, filter-based searches will take longer as the indexed content will be evaluated against the supplied filters.  Filter-based searches can only be used on structured data (i.e. JSON, XML, or SQL) and cannot be used on text documents.  Only terms-based searches can be used on text documents since there is no structure to the data.

The structure of a ```SearchFilter``` is as follows:
```
{
  "Field": "<field name>",      // The name of the field to search
  "Condition": "<condition>",   // The match condition, i.e. GreaterThan, shown below
  "Value": "<value>"            // The value that must be matched by the field in accordance with the condition
}
```

Valid values for ```Condition``` are:
```
Equals,
NotEquals,
GreaterThan,
GreaterThanOrEqualTo,
LessThan,
LessThanOrEqualTo,
IsNull,
IsNotNull,
Contains,
ContainsNot,
StartsWith,
EndsWith
```

## Delete an Index
```
DELETE /First
Response:
204/No data
```

## Enumerate Documents in an Index

NOTE: this will enumerate both indexed documents and stored documents (when stored without indexing).

```
PUT /First/enumerate
Request body:
{
  GUID: "00000000-0000-0000-0000-000000000000",
  MaxResults: 100,
  StartIndex: 0,
  Filters: [
    {
      Field: "Id",
      Condition: "GreaterThan",
      Value: "0"
    },
    {
      Field: "Tags",
      Condition: "Contains",
      Value: "tag1"
    }
  ],
  PostbackUrl: null
}
Response:
{
    "Query": {
        "MaxResults": 100,
        "StartIndex": 0,
        "Filters": [
            {
                "Field": "Id",
                "Condition": "GreaterThan",
                "Value": "0"
            },
            {
                "Field": "Tags",
                "Condition": "Contains",
                "Value": "tag1"
            }
        ]
    },
    "Async": false,
    "IndexName": "first",
    "StartTimeUtc": "2019-04-01T04:57:30.723648Z",
    "EndTimeUtc": "2019-04-01T04:57:30.7322003Z",
    "TotalTimeMs": 8.5523,
    "Matches": [
        {
            "Id": 1,
            "IndexName": "first",
            "DocumentId": "666f4364-26b1-4969-be59-a4e191bb0987",
            "Name": "joel",
            "Tags": "tag1,tag2,tag3",
            "Title": "Hello!",
            "DocType": "Text",
            "ContentType": "text/plain",
            "ContentLength": 3697,
            "Created": "2019-04-01T04:56:55.5823126Z"
        }
    ]
}
```

## Retrieve Source Document from an Index
```
GET /First/b7705bc9-789a-4a62-ba54-1b2043b6d1e7
Response:
{
   "OrderNumber": 1,
   "Amount": 100,
   "Customer": {
      "FirstName": "Joel",
      "LastName": "Christner",
      "Address": "123 Main St",
      "City": "San Jose",
      "State": "CA",
      "Postal": "95128"
   },
   "LineItems": [
      {
         "LineItemId": 1,
         "ItemId": 1,
         "Name": "Paper clips",
         "Quantity": 5,
         "UnitPrice": 5,
         "Subtotal": 25
      }, ... 
   ]
}
```
## Retrieve Parsed Document from an Index
```
GET /First/b7705bc9-789a-4a62-ba54-1b2043b6d1e7?parsed=true
Response:
{
    "DocumentId": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7",
    "DocumentType": "Json", 
    "Schema": {
        "ordernumber": "Integer",
        "amount": "Integer",
        "customer": "Object",
        ... 
    },
    "Postings": [
        {
            "Term": "joel", 
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        ...
    ],
    "Terms": [ 
        "joel", 
        ...
    ],
    "Json": {
        "Schema": {
            "ordernumber": "Integer",
            "amount": "Integer",
            "customer": "Object",
            ...
        },
        "MaxDepth": 2,
        "ArrayCount": 1,
        "NodeCount": 31,
        "Flattened": [
            {
                "Key": "ordernumber",
                "Data": "1",
                "Type": "Integer"
            },
            {
                "Key": "amount",
                "Data": "100",
                "Type": "Integer"
            },
            {
                "Key": "customer",
                "Type": "Object"
            },
            ...
        ],
        "Tokens": [
            "1",
            "100",
            "joel",
            ...
        ]
    }
}
```
