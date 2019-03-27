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

## Parse a Document
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
        "balance": "String",
        "picture": "String",
        "age": "Integer",
        "eyeColor": "String",
        "name": "String",
        "gender": "String",
        "company": "String",
        "email": "String",
        "phone": "String",
        "address": "String",
        "about": "String",
        "registered": "String",
        "latitude": "Decimal",
        "longitude": "Decimal",
        "tags": "Array",
        "friends": "Array",
        "friends.id": "Integer",
        "friends.name": "String",
        "greeting": "String",
        "favoriteFruit": "String"
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
        {
            "Key": "guid",
            "Data": "69efa262-c510-4f6b-9b9c-2d1432c94035",
            "Type": "String"
        },
        {
            "Key": "isActive",
            "Data": "False",
            "Type": "Boolean"
        },
        {
            "Key": "balance",
            "Data": "$1,348.47",
            "Type": "String"
        },
        {
            "Key": "picture",
            "Data": "http://placehold.it/32x32",
            "Type": "String"
        },
        {
            "Key": "age",
            "Data": "34",
            "Type": "Integer"
        },
        {
            "Key": "eyeColor",
            "Data": "brown",
            "Type": "String"
        },
        {
            "Key": "name",
            "Data": "Fernandez Howell",
            "Type": "String"
        },
        {
            "Key": "gender",
            "Data": "male",
            "Type": "String"
        },
        {
            "Key": "company",
            "Data": "ROBOID",
            "Type": "String"
        },
        {
            "Key": "email",
            "Data": "fernandezhowell@roboid.com",
            "Type": "String"
        },
        {
            "Key": "phone",
            "Data": "+1 (930) 509-3204",
            "Type": "String"
        },
        {
            "Key": "address",
            "Data": "310 Dahl Court, Wawona, Ohio, 1142",
            "Type": "String"
        },
        {
            "Key": "about",
            "Data": "Ex mollit sunt reprehenderit elit laborum ullamco voluptate id. Exercitation minim adipisicing ut consectetur nisi quis aliqua dolore veniam ea ullamco in. Reprehenderit quis proident elit mollit excepteur sit non cillum dolor voluptate do consequat. Qui culpa aliquip cupidatat voluptate amet aliqua non ullamco duis. Occaecat occaecat commodo est magna nulla anim minim in do commodo est occaecat. Laborum dolor dolore ipsum minim ut incididunt sit deserunt adipisicing elit adipisicing est duis irure.\r\n",
            "Type": "String"
        },
        {
            "Key": "registered",
            "Data": "2017-12-30T04:55:56 +08:00",
            "Type": "String"
        },
        {
            "Key": "latitude",
            "Data": "-62.599274",
            "Type": "Decimal"
        },
        {
            "Key": "longitude",
            "Data": "98.830868",
            "Type": "Decimal"
        },
        {
            "Key": "tags",
            "Type": "Array"
        },
        {
            "Key": "tags",
            "Data": "consectetur",
            "Type": "String"
        },
        {
            "Key": "tags",
            "Data": "ad",
            "Type": "String"
        },
        {
            "Key": "tags",
            "Data": "ea",
            "Type": "String"
        },
        {
            "Key": "tags",
            "Data": "est",
            "Type": "String"
        },
        {
            "Key": "tags",
            "Data": "incididunt",
            "Type": "String"
        },
        {
            "Key": "tags",
            "Data": "tempor",
            "Type": "String"
        },
        {
            "Key": "tags",
            "Data": "magna",
            "Type": "String"
        },
        {
            "Key": "friends",
            "Type": "Array"
        },
        {
            "Key": "friends",
            "Type": "Object"
        },
        {
            "Key": "friends.id",
            "Data": "0",
            "Type": "Integer"
        },
        {
            "Key": "friends.name",
            "Data": "Tracie Hahn",
            "Type": "String"
        },
        {
            "Key": "friends",
            "Type": "Object"
        },
        {
            "Key": "friends.id",
            "Data": "1",
            "Type": "Integer"
        },
        {
            "Key": "friends.name",
            "Data": "Alyssa Fitzgerald",
            "Type": "String"
        },
        {
            "Key": "friends",
            "Type": "Object"
        },
        {
            "Key": "friends.id",
            "Data": "2",
            "Type": "Integer"
        },
        {
            "Key": "friends.name",
            "Data": "Bell Stark",
            "Type": "String"
        },
        {
            "Key": "greeting",
            "Data": "Hello, Fernandez Howell! You have 4 unread messages.",
            "Type": "String"
        },
        {
            "Key": "favoriteFruit",
            "Data": "apple",
            "Type": "String"
        }
    ],
    "Tokens": [
        "5c99acba82e8738d61dd091e",
        "0",
        "69efa262-c510-4f6b-9b9c-2d1432c94035",
        "false",
        "$1,348.47",
        "http//placehold.it/32x32",
        "34",
        "brown",
        "fernandez howell",
        "male",
        "roboid",
        "fernandezhowell@roboid.com",
        "+1 (930) 509-3204",
        "310 dahl court, wawona, ohio, 1142",
        "ex mollit sunt reprehenderit elit laborum ullamco voluptate id. exercitation minim adipisicing ut consectetur nisi quis aliqua dolore veniam ea ullamco in. reprehenderit quis proident elit mollit excepteur sit non cillum dolor voluptate do consequat. qui culpa aliquip cupidatat voluptate amet aliqua non ullamco duis. occaecat occaecat commodo est magna nulla anim minim in do commodo est occaecat. laborum dolor dolore ipsum minim ut incididunt sit deserunt adipisicing elit adipisicing est duis irure.",
        "2017-12-30t045556 +0800",
        "-62.599274",
        "98.830868",
        "consectetur",
        "ad",
        "ea",
        "est",
        "incididunt",
        "tempor",
        "magna",
        "0",
        "tracie hahn",
        "1",
        "alyssa fitzgerald",
        "2",
        "bell stark",
        "hello, fernandez howell! you have 4 unread messages.",
        "apple"
    ]
}
```

## Index a Document
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
        "isactive": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.495ad7c1-19db-46bb-ad8c-b4b450f0145f",
        "balance": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e0489162-6545-4120-b9b1-b7bfc8849722",
        "picture": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.873d0185-7e4f-403f-a35b-0f659d6f99db",
        "age": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.8de45277-fe50-4404-a7a8-6258e991f20a",
        "eyecolor": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.97a16b64-5dab-488a-8efe-190d07f5db7c",
        "name": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.bd7a9915-4d45-49db-9b3a-98ecbaa020ab",
        "gender": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.a55342c5-51d9-4546-98ba-88ab7b81ad3c",
        "company": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.327b9c10-efd7-449f-bd21-f3bf2e28e14a",
        "email": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.fb8686cc-31a4-42ac-b108-b0930db3d7f7",
        "phone": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.27e24990-697a-47f0-89fc-8ec4af70fa6e",
        "address": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.a85e2b26-da89-4026-81a3-826609a763d9",
        "about": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
        "registered": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.383f2573-b9cd-4bd5-9c3e-1fcf0dedf754",
        "latitude": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.cc9c5291-625c-4cde-a3bc-cea74e2a3115",
        "longitude": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.86d9264b-d1d7-4a45-a9d2-5e7a80965cf6",
        "tags": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.86f3c9bb-278f-4513-8651-da620fd2320f",
        "friends": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.6fa192f2-2d23-407d-8fee-7e07631d15ab",
        "friends.id": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.8fa5abd6-46f0-4ac6-b200-a883da77c153",
        "friends.name": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e9d468ed-8ec5-48cd-92a8-afaccf795cb6",
        "greeting": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e979753a-6551-4893-ad98-17a9e89c131c",
        "favoritefruit": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.b8c42ecd-e545-47ee-8301-f5edaeb37d69"
    },
    "Schema": {
        "undefined": "Array",
        "_id": "String",
        "index": "Integer",
        "guid": "String",
        "isactive": "Boolean",
        "balance": "String",
        "picture": "String",
        "age": "Integer",
        "eyecolor": "String",
        "name": "String",
        "gender": "String",
        "company": "String",
        "email": "String",
        "phone": "String",
        "address": "String",
        "about": "String",
        "registered": "String",
        "latitude": "Decimal",
        "longitude": "Decimal",
        "tags": "Array",
        "friends": "Array",
        "friends.id": "Integer",
        "friends.name": "String",
        "greeting": "String",
        "favoritefruit": "String"
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
        },
        {
            "Term": "False",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.495ad7c1-19db-46bb-ad8c-b4b450f0145f",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "348",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e0489162-6545-4120-b9b1-b7bfc8849722",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "http",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.873d0185-7e4f-403f-a35b-0f659d6f99db",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "placehold",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.873d0185-7e4f-403f-a35b-0f659d6f99db",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "32x32",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.873d0185-7e4f-403f-a35b-0f659d6f99db",
            "Frequency": 1,
            "Positions": [
                2
            ]
        },
        {
            "Term": "brown",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.97a16b64-5dab-488a-8efe-190d07f5db7c",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "fernandez",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.bd7a9915-4d45-49db-9b3a-98ecbaa020ab",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "howell",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.bd7a9915-4d45-49db-9b3a-98ecbaa020ab",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "male",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.a55342c5-51d9-4546-98ba-88ab7b81ad3c",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "roboid",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.327b9c10-efd7-449f-bd21-f3bf2e28e14a",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "fernandezhowell roboid",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.fb8686cc-31a4-42ac-b108-b0930db3d7f7",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "com",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.fb8686cc-31a4-42ac-b108-b0930db3d7f7",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "930",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.27e24990-697a-47f0-89fc-8ec4af70fa6e",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "509 3204",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.27e24990-697a-47f0-89fc-8ec4af70fa6e",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "310",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.a85e2b26-da89-4026-81a3-826609a763d9",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "dahl",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.a85e2b26-da89-4026-81a3-826609a763d9",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "court",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.a85e2b26-da89-4026-81a3-826609a763d9",
            "Frequency": 1,
            "Positions": [
                2
            ]
        },
        {
            "Term": "wawona",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.a85e2b26-da89-4026-81a3-826609a763d9",
            "Frequency": 1,
            "Positions": [
                3
            ]
        },
        {
            "Term": "ohio",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.a85e2b26-da89-4026-81a3-826609a763d9",
            "Frequency": 1,
            "Positions": [
                4
            ]
        },
        {
            "Term": "1142",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.a85e2b26-da89-4026-81a3-826609a763d9",
            "Frequency": 1,
            "Positions": [
                5
            ]
        },
        {
            "Term": "sunt",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "exercitation",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                7
            ]
        },
        {
            "Term": "consectetur",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                10
            ]
        },
        {
            "Term": "nisi",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                11
            ]
        },
        {
            "Term": "veniam",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                15
            ]
        },
        {
            "Term": "reprehenderit",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                2,
                17
            ]
        },
        {
            "Term": "quis",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                12,
                18
            ]
        },
        {
            "Term": "proident",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                19
            ]
        },
        {
            "Term": "mollit",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                0,
                21
            ]
        },
        {
            "Term": "excepteur",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                22
            ]
        },
        {
            "Term": "cillum",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                25
            ]
        },
        {
            "Term": "consequat",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                28
            ]
        },
        {
            "Term": "qui",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                29
            ]
        },
        {
            "Term": "culpa",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                30
            ]
        },
        {
            "Term": "aliquip",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                31
            ]
        },
        {
            "Term": "cupidatat",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                32
            ]
        },
        {
            "Term": "voluptate",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 3,
            "Positions": [
                6,
                27,
                33
            ]
        },
        {
            "Term": "amet",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                34
            ]
        },
        {
            "Term": "aliqua",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                13,
                35
            ]
        },
        {
            "Term": "non",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                24,
                36
            ]
        },
        {
            "Term": "ullamco",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 3,
            "Positions": [
                5,
                16,
                37
            ]
        },
        {
            "Term": "magna",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                43
            ]
        },
        {
            "Term": "nulla",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                44
            ]
        },
        {
            "Term": "anim",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                45
            ]
        },
        {
            "Term": "commodo",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                41,
                47
            ]
        },
        {
            "Term": "occaecat",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 3,
            "Positions": [
                39,
                40,
                49
            ]
        },
        {
            "Term": "laborum",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                4,
                50
            ]
        },
        {
            "Term": "dolor",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                26,
                51
            ]
        },
        {
            "Term": "dolore",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                14,
                52
            ]
        },
        {
            "Term": "ipsum",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                53
            ]
        },
        {
            "Term": "minim",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 3,
            "Positions": [
                8,
                46,
                54
            ]
        },
        {
            "Term": "incididunt",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                55
            ]
        },
        {
            "Term": "sit",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                23,
                56
            ]
        },
        {
            "Term": "deserunt",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                57
            ]
        },
        {
            "Term": "elit",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 3,
            "Positions": [
                3,
                20,
                59
            ]
        },
        {
            "Term": "adipisicing",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 3,
            "Positions": [
                9,
                58,
                60
            ]
        },
        {
            "Term": "est",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 3,
            "Positions": [
                42,
                48,
                61
            ]
        },
        {
            "Term": "duis",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 2,
            "Positions": [
                38,
                62
            ]
        },
        {
            "Term": "irure",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.d569de6b-595f-407a-ad13-74fac842b1cc",
            "Frequency": 1,
            "Positions": [
                63
            ]
        },
        {
            "Term": "2017 12 30t04 55 56",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.383f2573-b9cd-4bd5-9c3e-1fcf0dedf754",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "08 00",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.383f2573-b9cd-4bd5-9c3e-1fcf0dedf754",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "599274",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.cc9c5291-625c-4cde-a3bc-cea74e2a3115",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "830868",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.86d9264b-d1d7-4a45-a9d2-5e7a80965cf6",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "consectetur",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.86f3c9bb-278f-4513-8651-da620fd2320f",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "est",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.86f3c9bb-278f-4513-8651-da620fd2320f",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "incididunt",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.86f3c9bb-278f-4513-8651-da620fd2320f",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "tempor",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.86f3c9bb-278f-4513-8651-da620fd2320f",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "magna",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.86f3c9bb-278f-4513-8651-da620fd2320f",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "tracie",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e9d468ed-8ec5-48cd-92a8-afaccf795cb6",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "hahn",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e9d468ed-8ec5-48cd-92a8-afaccf795cb6",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "alyssa",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e9d468ed-8ec5-48cd-92a8-afaccf795cb6",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "fitzgerald",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e9d468ed-8ec5-48cd-92a8-afaccf795cb6",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "bell",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e9d468ed-8ec5-48cd-92a8-afaccf795cb6",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "stark",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e9d468ed-8ec5-48cd-92a8-afaccf795cb6",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "hello",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e979753a-6551-4893-ad98-17a9e89c131c",
            "Frequency": 1,
            "Positions": [
                0
            ]
        },
        {
            "Term": "fernandez",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e979753a-6551-4893-ad98-17a9e89c131c",
            "Frequency": 1,
            "Positions": [
                1
            ]
        },
        {
            "Term": "howell",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e979753a-6551-4893-ad98-17a9e89c131c",
            "Frequency": 1,
            "Positions": [
                2
            ]
        },
        {
            "Term": "you",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e979753a-6551-4893-ad98-17a9e89c131c",
            "Frequency": 1,
            "Positions": [
                3
            ]
        },
        {
            "Term": "have",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e979753a-6551-4893-ad98-17a9e89c131c",
            "Frequency": 1,
            "Positions": [
                4
            ]
        },
        {
            "Term": "unread",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e979753a-6551-4893-ad98-17a9e89c131c",
            "Frequency": 1,
            "Positions": [
                5
            ]
        },
        {
            "Term": "messages",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.e979753a-6551-4893-ad98-17a9e89c131c",
            "Frequency": 1,
            "Positions": [
                6
            ]
        },
        {
            "Term": "apple",
            "DocumentId": "c52b075d-69ff-4edb-a8f6-51e5f6b20ecd.b8c42ecd-e545-47ee-8301-f5edaeb37d69",
            "Frequency": 1,
            "Positions": [
                0
            ]
        }
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
        "fernandez",
        "howell",
        "male",
        "roboid",
        "fernandezhowell roboid",
        "com",
        "930",
        "509 3204",
        "310",
        "dahl",
        "court",
        "wawona",
        "ohio",
        "1142",
        "mollit",
        "sunt",
        "reprehenderit",
        "elit",
        "laborum",
        "ullamco",
        "voluptate",
        "exercitation",
        "minim",
        "adipisicing",
        "consectetur",
        "nisi",
        "quis",
        "aliqua",
        "dolore",
        "veniam",
        "proident",
        "excepteur",
        "sit",
        "non",
        "cillum",
        "dolor",
        "consequat",
        "qui",
        "culpa",
        "aliquip",
        "cupidatat",
        "amet",
        "duis",
        "occaecat",
        "commodo",
        "est",
        "magna",
        "nulla",
        "anim",
        "ipsum",
        "incididunt",
        "deserunt",
        "irure",
        "2017 12 30t04 55 56",
        "08 00",
        "599274",
        "830868",
        "tempor",
        "tracie",
        "hahn",
        "alyssa",
        "fitzgerald",
        "bell",
        "stark",
        "hello",
        "you",
        "have",
        "unread",
        "messages",
        "apple"
    ],
    "Json": {
        "Schema": {
            "undefined": "Array",
            "_id": "String",
            "index": "Integer",
            "guid": "String",
            "isactive": "Boolean",
            "balance": "String",
            "picture": "String",
            "age": "Integer",
            "eyecolor": "String",
            "name": "String",
            "gender": "String",
            "company": "String",
            "email": "String",
            "phone": "String",
            "address": "String",
            "about": "String",
            "registered": "String",
            "latitude": "Decimal",
            "longitude": "Decimal",
            "tags": "Array",
            "friends": "Array",
            "friends.id": "Integer",
            "friends.name": "String",
            "greeting": "String",
            "favoritefruit": "String"
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
            {
                "Key": "guid",
                "Data": "69efa262-c510-4f6b-9b9c-2d1432c94035",
                "Type": "String"
            },
            {
                "Key": "isactive",
                "Data": "False",
                "Type": "Boolean"
            },
            {
                "Key": "balance",
                "Data": "$1,348.47",
                "Type": "String"
            },
            {
                "Key": "picture",
                "Data": "http://placehold.it/32x32",
                "Type": "String"
            },
            {
                "Key": "age",
                "Data": "34",
                "Type": "Integer"
            },
            {
                "Key": "eyecolor",
                "Data": "brown",
                "Type": "String"
            },
            {
                "Key": "name",
                "Data": "fernandez howell",
                "Type": "String"
            },
            {
                "Key": "gender",
                "Data": "male",
                "Type": "String"
            },
            {
                "Key": "company",
                "Data": "roboid",
                "Type": "String"
            },
            {
                "Key": "email",
                "Data": "fernandezhowell@roboid.com",
                "Type": "String"
            },
            {
                "Key": "phone",
                "Data": "+1 (930) 509-3204",
                "Type": "String"
            },
            {
                "Key": "address",
                "Data": "310 dahl court, wawona, ohio, 1142",
                "Type": "String"
            },
            {
                "Key": "about",
                "Data": "ex mollit sunt reprehenderit elit laborum ullamco voluptate id. exercitation minim adipisicing ut consectetur nisi quis aliqua dolore veniam ea ullamco in. reprehenderit quis proident elit mollit excepteur sit non cillum dolor voluptate do consequat. qui culpa aliquip cupidatat voluptate amet aliqua non ullamco duis. occaecat occaecat commodo est magna nulla anim minim in do commodo est occaecat. laborum dolor dolore ipsum minim ut incididunt sit deserunt adipisicing elit adipisicing est duis irure.\r\n",
                "Type": "String"
            },
            {
                "Key": "registered",
                "Data": "2017-12-30t04:55:56 +08:00",
                "Type": "String"
            },
            {
                "Key": "latitude",
                "Data": "-62.599274",
                "Type": "Decimal"
            },
            {
                "Key": "longitude",
                "Data": "98.830868",
                "Type": "Decimal"
            },
            {
                "Key": "tags",
                "Type": "Array"
            },
            {
                "Key": "tags",
                "Data": "consectetur",
                "Type": "String"
            },
            {
                "Key": "tags",
                "Data": "ad",
                "Type": "String"
            },
            {
                "Key": "tags",
                "Data": "ea",
                "Type": "String"
            },
            {
                "Key": "tags",
                "Data": "est",
                "Type": "String"
            },
            {
                "Key": "tags",
                "Data": "incididunt",
                "Type": "String"
            },
            {
                "Key": "tags",
                "Data": "tempor",
                "Type": "String"
            },
            {
                "Key": "tags",
                "Data": "magna",
                "Type": "String"
            },
            {
                "Key": "friends",
                "Type": "Array"
            },
            {
                "Key": "friends",
                "Type": "Object"
            },
            {
                "Key": "friends.id",
                "Data": "0",
                "Type": "Integer"
            },
            {
                "Key": "friends.name",
                "Data": "tracie hahn",
                "Type": "String"
            },
            {
                "Key": "friends",
                "Type": "Object"
            },
            {
                "Key": "friends.id",
                "Data": "1",
                "Type": "Integer"
            },
            {
                "Key": "friends.name",
                "Data": "alyssa fitzgerald",
                "Type": "String"
            },
            {
                "Key": "friends",
                "Type": "Object"
            },
            {
                "Key": "friends.id",
                "Data": "2",
                "Type": "Integer"
            },
            {
                "Key": "friends.name",
                "Data": "bell stark",
                "Type": "String"
            },
            {
                "Key": "greeting",
                "Data": "hello, fernandez howell! you have 4 unread messages.",
                "Type": "String"
            },
            {
                "Key": "favoritefruit",
                "Data": "apple",
                "Type": "String"
            }
        ],
        "Tokens": [
            "5c99acba82e8738d61dd091e",
            "0",
            "69efa262 c510 4f6b 9b9c 2d1432c94035",
            "false",
            " 1 348 47",
            "http  placehold it 32x32",
            "34",
            "brown",
            "fernandez howell",
            "male",
            "roboid",
            "fernandezhowell roboid com",
            " 1  930  509 3204",
            "310 dahl court  wawona  ohio  1142",
            "ex mollit sunt reprehenderit elit laborum ullamco voluptate id  exercitation minim adipisicing ut consectetur nisi quis aliqua dolore veniam ea ullamco in  reprehenderit quis proident elit mollit excepteur sit non cillum dolor voluptate do consequat  qui culpa aliquip cupidatat voluptate amet aliqua non ullamco duis  occaecat occaecat commodo est magna nulla anim minim in do commodo est occaecat  laborum dolor dolore ipsum minim ut incididunt sit deserunt adipisicing elit adipisicing est duis irure ",
            "2017 12 30t045556  0800",
            " 62 599274",
            "98 830868",
            "consectetur",
            "ad",
            "ea",
            "est",
            "incididunt",
            "tempor",
            "magna",
            "0",
            "tracie hahn",
            "1",
            "alyssa fitzgerald",
            "2",
            "bell stark",
            "hello  fernandez howell  you have 4 unread messages ",
            "apple"
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
