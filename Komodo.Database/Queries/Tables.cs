using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DatabaseWrapper;

namespace Komodo.Database.Queries
{
    internal static class Tables
    {
        internal static void InitializeTables(DatabaseClient database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            if (!database.TableExists("users"))
                database.CreateTable("users", UsersTableColumns());

            if (!database.TableExists("apikeys"))
                database.CreateTable("apikeys", ApiKeysTableColumns());

            if (!database.TableExists("permissions"))
                database.CreateTable("permissions", PermissionsTableColumns());

            if (!database.TableExists("metadata"))
                database.CreateTable("metadata", MetadataTableColumns());

            if (!database.TableExists("nodes"))
                database.CreateTable("nodes", NodesTableColumns());

            if (!database.TableExists("indices"))
                database.CreateTable("indices", IndicesTableColumns());

            if (!database.TableExists("sourcedocs"))
                database.CreateTable("sourcedocs", SourceDocsTableColumns());

            if (!database.TableExists("parseddocs"))
                database.CreateTable("parseddocs", ParsedDocsTableColumns());

            if (!database.TableExists("termguids"))
                database.CreateTable("termguids", TermGuidsTableColumns());

            if (!database.TableExists("termdocs"))
                database.CreateTable("termdocs", TermDocsTableColumns());
        }

        private static List<Column> UsersTableColumns()
        {
            List<Column> ret = new List<Column>();
            ret.Add(new Column("id", true, DataType.Int, null, null, false));
            ret.Add(new Column("guid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("name", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("email", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("passwordmd5", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("active", false, DataType.Int, null, null, false));
            return ret;
        }

        private static List<Column> ApiKeysTableColumns()
        {
            List<Column> ret = new List<Column>();
            ret.Add(new Column("id", true, DataType.Int, null, null, false));
            ret.Add(new Column("guid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("userguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("active", false, DataType.Int, null, null, false));
            return ret;
        }

        private static List<Column> PermissionsTableColumns()
        {
            List<Column> ret = new List<Column>();
            ret.Add(new Column("id", true, DataType.Int, null, null, false));
            ret.Add(new Column("guid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("indexguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("userguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("apikeyguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("allowsearch", false, DataType.Int, null, null, false));
            ret.Add(new Column("allowcreatedoc", false, DataType.Int, null, null, false));
            ret.Add(new Column("allowdeletedoc", false, DataType.Int, null, null, false));
            ret.Add(new Column("allowcreateindex", false, DataType.Int, null, null, false));
            ret.Add(new Column("allowdeleteindex", false, DataType.Int, null, null, false));  
            return ret;
        }

        private static List<Column> MetadataTableColumns()
        {
            List<Column> ret = new List<Column>();
            // new Column("name", false, DataType.DateTime, maxLen, precision, nullable);
            ret.Add(new Column("id", true, DataType.Int, null, null, false));
            ret.Add(new Column("configkey", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("configval", false, DataType.Nvarchar, 2048, null, true));
            return ret;
        }

        private static List<Column> NodesTableColumns()
        {
            List<Column> ret = new List<Column>();
            ret.Add(new Column("id", true, DataType.Int, null, null, false));
            ret.Add(new Column("guid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("hostname", false, DataType.Nvarchar, 128, null, false));
            ret.Add(new Column("port", false, DataType.Int, null, null, false));
            ret.Add(new Column("ssl", false, DataType.Int, null, null, false));
            return ret;
        }

        private static List<Column> IndicesTableColumns()
        {
            List<Column> ret = new List<Column>();
            ret.Add(new Column("id", true, DataType.Int, null, null, false));
            ret.Add(new Column("guid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("ownerguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("name", false, DataType.Nvarchar, 64, null, false));
            return ret;
        }

        private static List<Column> SourceDocsTableColumns()
        {
            List<Column> ret = new List<Column>();
            ret.Add(new Column("id", true, DataType.Int, null, null, false));
            ret.Add(new Column("guid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("ownerguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("indexguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("name", false, DataType.Nvarchar, 128, null, true));
            ret.Add(new Column("title", false, DataType.Nvarchar, 128, null, true));
            ret.Add(new Column("tags", false, DataType.Nvarchar, 256, null, true));
            ret.Add(new Column("doctype", false, DataType.Nvarchar, 16, null, false));
            ret.Add(new Column("sourceurl", false, DataType.Nvarchar, 256, null, true));
            ret.Add(new Column("contenttype", false, DataType.Nvarchar, 128, null, true));
            ret.Add(new Column("contentlength", false, DataType.Long, null, null, false));
            ret.Add(new Column("md5", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("created", false, DataType.DateTime, null, null, false));
            ret.Add(new Column("indexed", false, DataType.DateTime, null, null, true));
            return ret;
        }

        private static List<Column> ParsedDocsTableColumns()
        {
            List<Column> ret = new List<Column>();
            ret.Add(new Column("id", true, DataType.Int, null, null, false));
            ret.Add(new Column("guid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("sourcedocguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("ownerguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("indexguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("doctype", false, DataType.Nvarchar, 16, null, false));
            ret.Add(new Column("contentlength", false, DataType.Long, null, null, false));
            ret.Add(new Column("terms", false, DataType.Long, null, null, false));
            ret.Add(new Column("postings", false, DataType.Long, null, null, false));
            ret.Add(new Column("created", false, DataType.DateTime, null, null, false));
            ret.Add(new Column("indexed", false, DataType.DateTime, null, null, true));
            return ret;
        }

        private static List<Column> TermGuidsTableColumns()
        {
            List<Column> ret = new List<Column>();
            ret.Add(new Column("id", true, DataType.Int, null, null, false));
            ret.Add(new Column("guid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("indexguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("term", false, DataType.Nvarchar, 64, null, false));
            return ret;
        }

        private static List<Column> TermDocsTableColumns()
        {
            List<Column> ret = new List<Column>();
            ret.Add(new Column("id", true, DataType.Int, null, null, false));
            ret.Add(new Column("guid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("indexguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("termguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("sourcedocguid", false, DataType.Nvarchar, 64, null, false));
            ret.Add(new Column("parseddocguid", false, DataType.Nvarchar, 64, null, false));
            return ret;
        }
    }
}
