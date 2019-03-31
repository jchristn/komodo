# Server

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
      "]","[",",","."," ","'","\"",";","<",">",".","/","\\","|","{","}","(",")"
    ],
    "NormalizeCase": true,
    "RemovePunctuation": true,
    "RemoveStopWords": true,
    "PerformStemming": true,
    "MinTokenLength": 3,
    "MaxTokenLength": 32,
    "StopWords": []
  },
  "Database": {
    "Type": "Sqlite",
    "Filename": "First.db",
    "Hostname": "localhost",
    "Port": 8100,
    "DatabaseName": "firstidx",
    "InstanceName": "firstidx",
    "Username": "joel",
    "Password": "joel",
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
      "]","[",",","."," ","'","\"",";","<",">",".","/","\\","|","{",""}","(",")"
    ],
    "NormalizeCase": true,
    "RemovePunctuation": true,
    "RemoveStopWords": true,
    "PerformStemming": true,
    "MinTokenLength": 3,
    "MaxTokenLength": 32,
    "StopWords": []
  },
  "Database": {
    "Type": "Sqlite",
    "Filename": "First.db",
    "Hostname": "localhost",
    "Port": 8100,
    "DatabaseName": "firstidx",
    "InstanceName": "firstidx",
    "Username": "joel",
    "Password": "joel",
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
    }
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
        "0",
        "69efa262-c510-4f6b-9b9c-2d1432c94035",
        "false",
        "$1,348.47",
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
    "MasterDocId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd",
    "DocumentType": "Json",
    "NodeDocIds": {
        "undefined": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.6564d937-3a22-453b-98ab-7d8e5264b2a3",
        "_id": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.80fa901e-aa1b-4b16-9b10-38b95d841ea0",
        "index": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.c81176d2-0b52-4f09-bcc1-2d60d16a9b22",
        "guid": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.7fe65a58-4e83-4f6d-adb1-799e9b8b4f53",
        ... 
    },
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
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.80fa901e-aa1b-4b16-9b10-38b95d841ea0",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "69efa262 c510 4f6b 9b9c 2d1432c94035",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.7fe65a58-4e83-4f6d-adb1-799e9b8b4f53",
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
      "MasterDocId": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7",
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

## Delete an Index
```
DELETE /First
Response:
204/No data
```

## List Documents in an Index
```
PUT /First/enumerate
Request body:
{
  GUID: "00000000-0000-0000-0000-000000000000",
  MaxResults: 100,
  StartIndex: 0,
  Filters: [],
  PostbackUrl: null
}
Response:
{
    "Query": {
        "MaxResults": 100,
        "StartIndex": 0,
        "Filters": []
    },
    "Async": false,
    "IndexName": "first",
    "StartTimeUtc": "2019-03-31T06:20:57.9129628Z",
    "EndTimeUtc": "2019-03-31T06:20:57.9334269Z",
    "TotalTimeMs": 20.4641,
    "Matches": [
        {
            "Id": 1,
            "IndexName": "first",
            "MasterDocId": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7",
            "DocType": "Json",
            "ContentLength": 483,
            "Created": "2019-03-27T06:17:47.2960956Z"
        }, ... 
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
    "MasterDocId": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7",
    "DocumentType": "Json",
    "NodeDocIds": {
        "ordernumber": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7.8db87c13-fdc0-42e9-9817-5fd3064633ac",
        "amount": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7.a7130c17-c065-42c2-aaab-b0f38d75f493",
        "customer": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7.1cf7805e-fe61-4fd2-bfac-5958ecf1483d",
        "customer.firstname": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7.0bfd62d9-8f18-4e1a-8cb1-f4aaebd11cfd",
        "customer.lastname": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7.2f7f869d-e2f3-48d8-a9d9-76aa13ec5c68",
        ... 
    },
    "Schema": {
        "ordernumber": "Integer",
        "amount": "Integer",
        "customer": "Object",
        ... 
    },
    "Postings": [
        {
            "Term": "100",
            "DocumentId": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7.a7130c17-c065-42c2-aaab-b0f38d75f493",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "joel",
            "DocumentId": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7.0bfd62d9-8f18-4e1a-8cb1-f4aaebd11cfd",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "christner",
            "DocumentId": "b7705bc9-789a-4a62-ba54-1b2043b6d1e7.2f7f869d-e2f3-48d8-a9d9-76aa13ec5c68",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        ... 
    ],
    "Terms": [
        "100",
        "joel",
        "christner",
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
