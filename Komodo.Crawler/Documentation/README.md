# Komodo.Crawler

The Komodo.Crawler project contains classes used by Komodo to crawl various data repositories including HTTP, HTTPS, FTP, SQL, Sqlite, and the filesystem.

Refer to the Test.Crawler package for an example implementation.

## General

Rely on the ```Success``` parameter within the crawl result to determine if the operation succeeded.

For filesystem, FTP, HTTP/HTTPS, data will reside in the ```FileStream``` or ```DataStream``` property (see ```ContentLength``` to determine how many bytes to read), or, use the ```Data``` property to fully read the stream to a byte array.  Using ```Data``` will read the stream to the end, and in the case of FTP and HTTP/HTTPS, you will not be able to seek back to the beginning of the stream.

The ```Time``` property will provide insight into the runtime of the operation.

## Examples
```
using Komodo.Classes;
using Komodo.Crawler;
```

### Crawling the Filesystem
```
FileCrawler fc = new FileCrawler("[filename]");
FileCrawlResult fcr = fc.Get();

if (fcr.Success)
{
  Console.WriteLine("Operation successful after " + fcr.Time.TotalMs + "ms");
  Console.WriteLine("Bytes read: " + fcr.ContentLength + " bytes");
  Console.WriteLine(Encoding.UTF8.GetString(fcr.Data));
}
else
{
  Console.WriteLine("Failed");
}
```

### Crawling FTP
``` 
FtpCrawler fc = new FtpCrawler(url);
fc.Username = "[username]";
fc.Password = "[password]";
FtpCrawlResult fcr = fc.Get();

if (fcr.Success)
{
  Console.WriteLine("Operation successful after " + fcr.Time.TotalMs + "ms");
  Console.WriteLine("Bytes read: " + fcr.ContentLength + " bytes");
  Console.WriteLine(Encoding.UTF8.GetString(fcr.Data));
}
else
{
  Console.WriteLine("Failed");
}
```

### Crawling HTTP
```
HttpCrawler hc = new HttpCrawler(url);
hc.IgnoreCertificateErrors = true;
hc.Method = HttpMethod.GET;
hc.Username = "[username]";
hc.Password = "[password]";
HttpCrawlResult hcr = hc.Get();

if (hcr.Success)
{
  Console.WriteLine("Operation successful after " + hcr.Time.TotalMs + "ms");
  Console.WriteLine("Bytes read: " + hcr.ContentLength + " bytes");
  Console.WriteLine(Encoding.UTF8.GetString(hcr.Data));
}
else
{
  Console.WriteLine("Failed");
}
```

### Crawling SQL
```
DbSettings db = new DbSettings(
    DbTypes.Mysql,
    "localhost",
    3306,
    "[username]",
    "[password]",
    null,
    "[databasename]");

SqlCrawler sc = new SqlCrawler(db, "[query]");
SqlCrawlResult scr = sc.Get();

if (scr.Success)
{
  Console.WriteLine("Operation successful after " + scr.Time.TotalMs + "ms");
  Console.WriteLine("Rows returned: " + scr.DataTable.Rows.Count + " rows");
}
else
{
  Console.WriteLine("Failed");
}
```

### Crawling Sqlite
```
SqliteCrawler sc = new SqliteCrawler("[filename]", "[query]");
SqliteCrawlResult scr = sc.Get();

if (scr.Success)
{
  Console.WriteLine("Operation successful after " + scr.Time.TotalMs + "ms");  
  Console.WriteLine("Rows returned: " + scr.DataTable.Rows.Count + " rows");
}
else
{
  Console.WriteLine("Failed");
}
```
