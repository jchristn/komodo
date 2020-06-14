# Komodo.Crawler

The Komodo.Crawler project contains classes used by Komodo to crawl various data repositories including HTTP, HTTPS, FTP, SQL, Sqlite, and the filesystem.

Refer to the Test.Crawler package for an example implementation.

## Examples
```
using Komodo.Classes;
using Komodo.Crawler;
 
FileCrawler fc = new FileCrawler("[filename]");
FileCrawlResult fcr = fc.Get();
 
FtpCrawler fc = new FtpCrawler(url);
fc.Username = "[username]";
fc.Password = "[password]";
FtpCrawlResult fcr = fc.Get();
 
HttpCrawler hc = new HttpCrawler(url);
hc.IgnoreCertificateErrors = true;
hc.Method = HttpMethod.GET;
hc.Username = "[username]";
hc.Password = "[password]";
HttpCrawlResult hcr = hc.Get();
 
DbSettings db = new DbSettings(
    DbTypes.MySql,
    "localhost",
    3306,
    "[username]",
    "[password]",
    null,
    "[databasename]");

SqlCrawler sc = new SqlCrawler(db, "[query]");
SqlCrawlResult scr = sc.Get();
 
SqliteCrawler sc = new SqliteCrawler("[filename]", "[query]");
SqliteCrawlResult scr = sc.Get();
```

Rely on the ```Success``` parameter within the crawl result to determine if the operation succeeded.

Data will reside in the ```FileStream``` or ```DataStream``` property (see ```ContentLength``` to determine how many bytes to read), or, use the ```Data``` property to fully read the stream to a byte array.

The ```Time``` property will provide insight into the runtime of the operation.