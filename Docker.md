# Running Komodo in Docker
 
Getting an ```HttpListener``` application (such as Komodo or any application using Watson Webserver, HTTP.sys, etc) up and running in Docker can be rather tricky given how 1) Docker acts as a network proxy and 2) HttpListener isn't friendly to ```HOST``` header mismatches.  Thus, it is **critical** that you run your containers using ```--user ContainerAdministrator``` (Windows) or ```--user root``` (Linux or Mac) to bypass the ```HttpListener``` restrictions.  There are likely ways around this, but I have been unable to find one.  

## Before you Begin

Komodo stores and manages a lot of persistent data.  Any data stored within a container will be lost once the container terminates.  As such, the proper precautions should be taken to configure Komodo correctly for deployment in a containerized environment in such a way that all configuration and data resides outside of the container itself.  Use the following best practices.

### Copy in Node Configuration

The ```System.json``` file which defines the configuration for your node should be either be copied in as part of your ```Dockerfile``` or overridden using either ```-v``` or ```docker-compose```.  Do not allow it to be built dynamically.

Set ```System.json``` ```EnableConsole``` to false and ```Logging.ConsoleLogging``` to false.  

Set your ```System.json``` ```Server.ListenerHostname``` to ```*```.

### Use an External Database

Komodo relies on a database for storing object and index metadata.  While Komodo is capable of using Sqlite, Sqlite databases are stored on the filesystem within the container which will be lost once the container terminates.  Use an external database such as SQL Server, MySQL, or PostgreSQL.  Modify the ```System.json``` ```Database``` section accordingly. 

Valid values for ```Type``` are: ```MySql```, ```MsSql```, ```PgSql```, and ```Sqlite```
```
  "Database": {
    "Type": "MySql",  
    "Hostname": "[database server hostname]",
    "Port": 3306,
    "DatabaseName": "komodo",
    "InstanceName": null,
    "Username": "root",
    "Password": "[password]"
  }
```

### Use External Storage

Komodo stores object data on the filesystem.  While the filesystem location could be local, this is not recommended for containerized deployments because the container filesystem is destroyed when the container is terminated.  As such, it is recommended that Kvpbase rely on an NFS export or CIFS file share (or some other underlying shared storage) when deployed in a container.

Modify the ```System.json``` ```TempStorage```, ```SourceDocuments```, ```ParsedDocuments```, and ```Postings``` section accordingly.  If necessary, modify the ```Dockerfile``` to issue the appropriate commands to establish the connection to the external or shared storage as part of the build process.

```
  "TempStorage": {
    "Disk": {
      "Directory": "/mnt/Komodo/TempStorage/"
    }
  },
  "SourceDocuments": {
    "Disk": {
      "Directory": "/mnt/Komodo/SourceDocuments/"
    }
  },
  "ParsedDocuments": {
    "Disk": {
      "Directory": "/mnt/Komodo/ParsedDocuments/"
    }
  },
  "Postings": {
    "Disk": {
      "Directory": "/mnt/Komodo/Postings/"
    }
  },
```

## Steps to Run Komodo in Docker

1) View and modify the ```Dockerfile``` as appropriate for your application.

2) Execute the Docker build process from the **solution directory**.
```
$ cd [Solution Directory]
$ docker build -f Dockerfile -t komodo-1.5 .
```

3) Verify the image exists:
```
$ docker images
REPOSITORY                              TAG                 IMAGE ID            CREATED             SIZE
komodo-1.5                              latest              047e29f37f9c        2 seconds ago       328MB
mcr.microsoft.com/dotnet/core/sdk       3.1                 abbb476b7b81        11 days ago         737MB
mcr.microsoft.com/dotnet/core/runtime   3.1                 4b555235dfc0        11 days ago         327MB
```
 
4) Execute the container:
```
Windows
$ docker run --user ContainerAdministrator -p 9090:9090 komodo-1.5

Linux or Mac 
$ docker run --user root -p 9090:9090 komodo-1.5
```

To run using a ```System.json``` from your filesystem (or external storage) use the following.  Note that the first parameter to ```-v``` is the path to the file outside of the container image and the second parameter is the path within the image.  The app is in ```/app``` so the path will need to reflect that.
```
Windows
$ docker run --user ContainerAdministrator -p 9090:9090 -v /[PathOnLocalFilesystem]/System.json:/app/System.json komodo-1.5

Linux or Mac 
$ docker run --user root -p 9090:9090 -v /[PathOnLocalFilesystem]/System.json:/app/System.json komodo-1.5
```

5) Connect to Komodo in your browser: 
```
http://localhost:9090
```

6) Get the container name:
```
$ docker ps
CONTAINER ID        IMAGE               COMMAND                  CREATED              STATUS              PORTS                    NAMES
3627b4e812fd        komodo-1.5          "dotnet Komodo.Serve…"   About a minute ago   Up About a minute   0.0.0.0:8000->8000/tcp   silly_khayyam
```

7) Kill a running container:
```
$ docker kill [CONTAINER ID]
```

8) Delete a container image:
```
$ docker rmi [IMAGE ID] -f
```

## Example System.json File

Notice in the ```System.json``` example provided below that:

- ```EnableConsole``` and ```Logging.ConsoleLogging``` are set to false
- An external ```MySql``` database is being used, so index and object metadata will persist even when the container is terminated
- External storage is used for object data and metadata storage, so object data will persist even when the container is terminated

```
{
  "EnableConsole": false,
  "Server": {
    "ListenerHostname": "*",
    "ListenerPort": 9090,
    "Ssl": false,
    "HeaderApiKey": "x-api-key",
    "AdminApiKey": "komodoadmin"
  },
  "Database": {
    "Type": "MySql",  
    "Hostname": "10.125.44.93",
    "Port": 3306,
    "DatabaseName": "komodo",
    "InstanceName": null,
    "Username": "root",
    "Password": "[password]"
  },
  "TempStorage": {
    "Disk": {
      "Directory": "/mnt/Komodo/TempStorage/"
    }
  },
  "SourceDocuments": {
    "Disk": {
      "Directory": "/mnt/Komodo/SourceDocuments/"
    }
  },
  "ParsedDocuments": {
    "Disk": {
      "Directory": "/mnt/Komodo/ParsedDocuments/"
    }
  },
  "Postings": {
    "Disk": {
      "Directory": "/mnt/Komodo/Postings/"
    }
  },
  "Logging": {
    "SyslogServerIp": "127.0.0.1",
    "SyslogServerPort": 514,
    "Header": "komodo",
    "MinimumLevel": 1,
    "ConsoleLogging": false
  }
}
```