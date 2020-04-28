using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Afx.Data;
using Oracle.ManagedDataAccess.Client;
using Afx.Data.Entity.Schema;

namespace Afx.Data.Oracle.Entity.Schema
{
    /// <summary>
    /// 表结构接口
    /// </summary>
    public class OracleTableSchema : TableSchema
    {
        private Database db;
        private string database;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connectionString">数据库链接字符串</param>
        public OracleTableSchema(string connectionString)
        {
            var stringBuilder = new OracleConnectionStringBuilder(connectionString);
            this.database = stringBuilder.UserID.ToUpper();
            this.db = new OracleDatabase(connectionString);
        }
        /// <summary>
        /// 执行sql logs
        /// </summary>
        public override Action<string> Log
        {
            get { return this.db.Log; }
            set { this.db.Log = value; }
        }
        /// <summary>
        /// 获取数据库所有表名
        /// </summary>
        /// <returns>数据库所有表名</returns>
        public override List<TableInfoModel> GetTables()
        {
            List<TableInfoModel> list = db.Query<TableInfoModel>("SELECT TABLE_NAME \"Name\" FROM ALL_TABLES WHERE OWNER = @p_db", new { p_db = this.database });
    
            return list;
        }
        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="indexs">索引列信息</param>
        public override void AddIndex(string table, List<IndexModel> indexs)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (indexs == null) throw new ArgumentNullException("columns");
            var list = indexs.FindAll(q => !string.IsNullOrEmpty(q.Name) && !string.IsNullOrEmpty(q.ColumnName));
            if (list.Count > 0)
            {
                var group = list.GroupBy(q => q.Name, StringComparer.OrdinalIgnoreCase);
                foreach (var item in group)
                {
                    string indexName = item.Key;
                    bool isUnique = item.Count(q => q.IsUnique) > 0;
                    List<string> columnList = new List<string>();
                    foreach (var m in item)
                    {
                        columnList.Add(m.ColumnName);
                    }
                    this.AddIndex(table, indexName, isUnique, columnList);
                }
            }
        }
        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="indexName">索引名称</param>
        /// <param name="isUnique">是否唯一索引</param>
        /// <param name="columns">列名</param>
        /// <returns>是否成功</returns>
        public override bool AddIndex(string table, string indexName, bool isUnique, List<string> columns)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (string.IsNullOrEmpty(indexName)) throw new ArgumentNullException("indexName");
            if (columns == null) throw new ArgumentNullException("columns");
            if (columns.Count == 0)
                return false;

            int count = 0;
            object obj = db.ExecuteScalar<object>("SELECT COUNT(1) FROM ALL_INDEXES WHERE OWNER = @p_db AND TABLE_NAME = @p_tb AND TABLE_TYPE = 'TABLE' AND INDEX_NAME = @index_name",
                new { p_db = this.database, p_tb = table, index_name = indexName });
            if (Convert.ToInt32(obj) == 0)
            {
                StringBuilder strColumns = new StringBuilder();
                foreach (var s in columns)
                {
                    strColumns.AppendFormat("\"{0}\", ", s);
                }
                strColumns.Remove(strColumns.Length - 2, 2);

                var sql = string.Format("CREATE {0} INDEX \"{1}\" ON \"{2}\" ({3});",
                    isUnique ? "UNIQUE" : "", indexName, table, strColumns.ToString());
                count = this.db.ExecuteNonQuery(sql);
            }

            return count != 0;
        }
        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="index">索引列信息</param>
        /// <returns>是否成功</returns>
        public override bool AddIndex(string table, IndexModel index)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (index == null) throw new ArgumentNullException("index");
            int count = 0;
            if (!string.IsNullOrEmpty(index.Name) && !string.IsNullOrEmpty(index.ColumnName))
            {
                object obj = db.ExecuteScalar<object>("SELECT COUNT(1) FROM ALL_INDEXES WHERE OWNER = @p_db AND TABLE_NAME = @p_tb AND TABLE_TYPE = 'TABLE' AND INDEX_NAME = @index_name",
                    new { p_db= this.database, p_tb = table, index_name = index.Name });
                if (Convert.ToInt32(obj) == 0)
                {
                    var sql = string.Format("CREATE {0} INDEX \"{1}\" ON \"{2}\" (\"{3}\");",
                        index.IsUnique ? "UNIQUE" : "", index.Name, table, index.ColumnName);
                    count = this.db.ExecuteNonQuery(sql);
                }
            }

            return count != 0;
        }
        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="index">索引名称</param>
        /// <returns>是否成功</returns>
        public override bool DeleteIndex(string table, string index)
        {
            int count = 0;
            if (!string.IsNullOrEmpty(index))
            {
                object obj = db.ExecuteScalar<object>("SELECT COUNT(1) FROM ALL_INDEXES WHERE OWNER = @p_db AND TABLE_NAME = @p_tb AND TABLE_TYPE = 'TABLE' AND INDEX_NAME = @index_name",
                    new { p_db = this.database, p_tb = table, index_name = index });
                if (Convert.ToInt32(obj) > 0)
                {
                    var sql = string.Format("DROP INDEX \"{0}\"", index);
                    count = this.db.ExecuteNonQuery(sql);
                }
            }

            return count != 0;
        }
        /// <summary>
        /// 创建数据库表
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="columns">列信息</param>
        /// <returns>是否成功</returns>
        public override bool CreateTable(TableInfoModel table, List<ColumnInfoModel> columns)
        {
            if (string.IsNullOrEmpty(table.Name)) throw new ArgumentNullException("table");
            if (columns == null) throw new ArgumentNullException("columns");
            if (columns.Count == 0) return false;
            int count = 0;
            StringBuilder createTableSql = new StringBuilder();
            List<ColumnInfoModel> keyColumns = columns.Where(q => q.IsKey).ToList();
            List<IndexModel> indexs = new List<IndexModel>();
            createTableSql.AppendFormat("CREATE TABLE \"{0}\"(", table.Name);
            foreach (var column in columns)
            {
                createTableSql.AppendFormat("\"{0}\" {1} {2} NULL, ", column.Name, column.DataType, column.IsNullable ? "" : "NOT");

                if (column.Indexs != null && column.Indexs.Count > 0) indexs.AddRange(column.Indexs);
            }
            createTableSql.Remove(createTableSql.Length - 2, 2);

            if (keyColumns.Count > 0)
            {
                createTableSql.AppendFormat(", CONSTRAINT \"PK_{0}\" PRIMARY KEY (", table.Name);

                foreach (var column in keyColumns)
                {
                    createTableSql.AppendFormat("\"{0}\", ", column.Name);
                }
                createTableSql.Remove(createTableSql.Length - 2, 2);
                createTableSql.Append(")");
            }

            createTableSql.Append(")");

            using (this.db.BeginTransaction())
            {
                var sql = createTableSql.ToString();
                count = this.db.ExecuteNonQuery(sql);
                foreach (var column in columns.Where(q => q.IsAutoIncrement))
                {
                    this.AddAutoIncrement(table.Name, column);
                }
                if(indexs.Count > 0) this.AddIndex(table.Name, indexs);

                this.db.Commit();
            }

            return count != 0;
        }

        private string GetSequenceName(string table, string column)
        {
            return string.Format("SQ_{0}_{1}", table, column);
        }

        private string GetTriggerName(string table, string column)
        {
            return string.Format("TR_{0}_{1}", table, column);
        }

        private int AddAutoIncrement(string table, ColumnInfoModel column)
        {
            int count = 0;
            if(column.IsAutoIncrement)
            {
                string sq_name = this.GetSequenceName(table, column.Name);
                object obj = this.db.ExecuteScalar<object>("SELECT COUNT(1) FROM ALL_SEQUENCES WHERE SEQUENCE_OWNER = @p_db AND SEQUENCE_NAME = @sq_name",
                    new { p_db = this.database, sq_name = sq_name });
                if (Convert.ToInt32(obj) == 0)
                {
                    var sql = string.Format("CREATE SEQUENCE \"{0}\" MINVALUE 1 MAXVALUE 9999999999999999999999999999 START WITH 1 INCREMENT BY 1 NOCACHE", sq_name);
                    count += this.db.ExecuteNonQuery(sql);
                }

                string tr_name = this.GetTriggerName(table, column.Name);
                obj = this.db.ExecuteScalar<object>("SELECT COUNT(1) FROM ALL_TRIGGERS WHERE OWNER = @p_db AND TRIGGERING_EVENT = 'INSERT' AND TABLE_NAME = @p_tb AND TRIGGER_NAME = @tr_name",
                    new { p_db = this.database, p_tb = table, tr_name });
                if (Convert.ToInt32(obj) == 0)
                {
                    var sql = string.Format("CREATE TRIGGER \"{0}\" BEFORE INSERT ON \"{1}\" FOR EACH ROW BEGIN SELECT \"{2}\".NEXTVAL INTO :NEW.\"{3}\" FROM DUAL; END;", tr_name, table, sq_name, column.Name);
                    count += db.ExecuteNonQuery(sql);
                }
            }

            return count;
        }

        //public override bool DeleteTable(string table)
        //{
        //    this.db.ClearParameters();
        //    this.db.CommandText = string.Format("DROP TABLE \"{0}\"", table);
        //    int count = this.db.ExecuteNonQuery();
        //    return count != 0;
        //}
        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="column">列信息</param>
        /// <returns>是否成功</returns>
        public override bool AddColumn(string table, ColumnInfoModel column)
        {
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
            if (column == null) throw new ArgumentNullException("column");
            int count = 0;
            using (this.db.BeginTransaction())
            {
                var sql = string.Format("ALTER TABLE \"{0}\" ADD (\"{1}\" {2} {3} NULL)",
                    table, column.Name, column.DataType, column.IsNullable ? "" : "NOT");
                count = this.db.ExecuteNonQuery(sql);
                if(column.IsAutoIncrement)
                {
                    count += this.AddAutoIncrement(table, column);
                }
                
                this.db.Commit();
            }

            return count != 0;
        }

        //public override bool DeleteColumn(string table, ColumnInfoModel column)
        //{
        //    int count = 0;
        //    using (var tx = this.db.BeginTransaction())
        //    {
        //        if (!string.IsNullOrEmpty(column.IndexName))
        //        {
        //            this.db.ClearParameters();
        //            this.db.CommandText = "SELECT COUNT(1) FROM ALL_INDEXES WHERE OWNER = :p_db AND TABLE_NAME = :p_tb AND TABLE_TYPE = 'TABLE' AND INDEX_NAME = :index_name";
        //            this.db.AddParameter("p_db", this.database);
        //            this.db.AddParameter("p_tb", table);
        //            this.db.AddParameter("index_name", column.IndexName);
        //            object obj = db.ExecuteScalar();
        //            if (Convert.ToInt32(obj) > 0)
        //            {
        //                this.db.ClearParameters();
        //                this.db.CommandText = string.Format("DROP INDEX \"{1}\"", table, column.IndexName);
        //                count += this.db.ExecuteNonQuery();
        //            }
        //        }
        //        if (column.IsAutoIncrement)
        //        {
        //            string sq_name = this.GetSequenceName(table, column.Name);
        //            this.db.ClearParameters();
        //            this.db.CommandText = "SELECT COUNT(1) FROM ALL_SEQUENCES WHERE SEQUENCE_OWNER = :p_db AND SEQUENCE_NAME = :sq_name";
        //            this.db.AddParameter("p_db", this.database);
        //            this.db.AddParameter("sq_name", sq_name);
        //            object obj = db.ExecuteScalar();
        //            if (Convert.ToInt32(obj) > 0)
        //            {
        //                this.db.ClearParameters();
        //                db.CommandText = string.Format("DROP SEQUENCE \"{0}\"", sq_name);
        //                count += db.ExecuteNonQuery();
        //            }

        //            string tr_name = this.GetTriggerName(table, column.Name);
        //            this.db.ClearParameters();
        //            this.db.CommandText = "SELECT COUNT(1) FROM ALL_TRIGGERS WHERE OWNER = :p_db AND TRIGGERING_EVENT = 'INSERT'AND TABLE_NAME = :p_tb AND TRIGGER_NAME = :tr_name";
        //            this.db.AddParameter("p_db", this.database);
        //            this.db.AddParameter("p_tb", table);
        //            this.db.AddParameter("tr_name", tr_name);
        //            obj = db.ExecuteScalar();
        //            if (Convert.ToInt32(obj) > 0)
        //            {
        //                this.db.ClearParameters();
        //                db.CommandText = string.Format("DROP TRIGGER \"{0}\"", tr_name);
        //                count += db.ExecuteNonQuery();
        //            }
        //        }
        //        this.db.ClearParameters();
        //        this.db.CommandText = string.Format("ALTER TABLE \"{0}\" DROP COLUMN \"{1}\"", table, column.Name);
        //        count = this.db.ExecuteNonQuery();

        //        tx.Commit();
        //    }

        //    return count != 0;
        //}

        //public override bool AlterColumn(string table, ColumnInfoModel column)
        //{
        //    //this.db.ClearParameters();
        //    //this.db.CommandText = string.Format("ALTER TABLE \"{0}\" MODIFY (\"{1}\" {2} {3} NULL)", table, column.Name, column.DataType, column.IsNullable ? "" : "NOT");
        //    //int count = this.db.ExecuteNonQuery();

        //    return false;// count > 0;
        //}

        //public override bool EqualColumn(ColumnInfoModel column1, ColumnInfoModel column2)
        //{
        //    //if (column1 != null && column2 != null)
        //    //{
        //    //    int i = column1.DataType.IndexOf('(');
        //    //    string s1 = i > 0 ? column1.DataType.Substring(0, i).Trim() : column1.DataType;
        //    //    i = column2.DataType.IndexOf('(');
        //    //    string s2 = i > 0 ? column2.DataType.Substring(0, i).Trim() : column2.DataType;
        //    //    if (!s1.Equals(s2, StringComparison.OrdinalIgnoreCase))
        //    //    {
        //    //        return false;
        //    //    }

        //    //    if ((s1.Equals("nchar", StringComparison.OrdinalIgnoreCase)
        //    //        || s1.Equals("nvarchar", StringComparison.OrdinalIgnoreCase))
        //    //        && column1.MaxLength != column2.MaxLength)
        //    //    {
        //    //        return false;
        //    //    }

        //    //    if (s1.Equals("decimal", StringComparison.OrdinalIgnoreCase)
        //    //        && column1.MaxLength != column2.MaxLength
        //    //        && column1.MinLength != column2.MinLength)
        //    //    {
        //    //        return false;
        //    //    }
        //    //}

        //    return true;
        //}

        public class OracleTabColumnModel
        {
            public int COLUMN_ID { get; set; }

            public string COLUMN_NAME { get; set; }
            public string DATA_TYPE { get; set; }
            public int? DATA_LENGTH { get; set; }
            public int? CHAR_LENGTH { get; set; }
            public int? DATA_PRECISION { get; set; }
            public int? DATA_SCALE { get; set; }
            public string NULLABLE { get; set; }
        }

        public class OracleTriggerModel
        {
            public string TRIGGER_NAME { get; set; }

            public string TRIGGER_BODY { get; set; }
        }

        public class OracleIndexModel
        {
            public string INDEX_NAME { get; set; }

            public string COLUMN_NAME { get; set; }

            public string UNIQUENESS { get; set; }
        }

        /// <summary>
        /// 获取表列信息
        /// </summary>
        /// <param name="table">表名</param>
        /// <returns>列信息</returns>
        public override List<ColumnInfoModel> GetTableColumns(string table)
        {
            List<ColumnInfoModel> list = new List<ColumnInfoModel>();
            var param = new { p_db = this.database, p_tb = table };
            // column
            var dt = db.Query<OracleTabColumnModel>("SELECT COLUMN_ID, COLUMN_NAME, DATA_TYPE, DATA_LENGTH, CHAR_LENGTH, DATA_PRECISION, DATA_SCALE, NULLABLE FROM ALL_TAB_COLUMNS WHERE OWNER = @p_db AND TABLE_NAME = @p_tb ORDER BY COLUMN_ID",
                param);

            // tr
            var tr_dt = db.Query<OracleTriggerModel>("SELECT TRIGGER_NAME, TRIGGER_BODY FROM ALL_TRIGGERS WHERE OWNER = @p_db AND TRIGGERING_EVENT = 'INSERT' AND TABLE_NAME = @p_tb",
                param);

            // key
            var key_dt = db.Query<string>("SELECT a.COLUMN_NAME FROM ALL_CONS_COLUMNS a INNER JOIN ALL_CONSTRAINTS b ON a.OWNER=b.OWNER AND a.CONSTRAINT_NAME = b.CONSTRAINT_NAME AND a.TABLE_NAME=b.TABLE_NAME WHERE b.OWNER = @p_db AND b.TABLE_NAME = @p_tb AND b.CONSTRAINT_TYPE = 'P'",
                param);

            // index
            var in_dt = db.Query<OracleIndexModel>("SELECT a.INDEX_NAME, a.COLUMN_NAME, b.UNIQUENESS FROM ALL_IND_COLUMNS a INNER JOIN ALL_INDEXES b ON a.TABLE_OWNER = b.TABLE_OWNER AND a.TABLE_NAME = b.TABLE_NAME AND a.INDEX_NAME = b.INDEX_NAME WHERE a.TABLE_OWNER = @p_db AND a.TABLE_NAME = @p_tb AND  b.TABLE_TYPE = 'TABLE' AND b.GENERATED = 'N' ORDER BY a.INDEX_NAME, a.COLUMN_POSITION",
                param);

            // sq
            var sq_dt = db.Query<string>("SELECT SEQUENCE_NAME FROM ALL_SEQUENCES WHERE SEQUENCE_OWNER = @p_db",
                new { p_db = this.database });

            foreach (var row in dt)
            {
                ColumnInfoModel m = new ColumnInfoModel();
                list.Add(m);
                m.Name = row.COLUMN_NAME;
                m.IsNullable = string.Compare(row.NULLABLE, "Y", true) == 0;
                m.Order = row.COLUMN_ID;
                m.IsKey = key_dt.Contains("m.Name", StringComparer.OrdinalIgnoreCase);

                string tr = this.GetTriggerName(table, m.Name);
                m.IsAutoIncrement = false;
                var r = tr_dt.Find(q => tr.Equals(q.TRIGGER_NAME, StringComparison.OrdinalIgnoreCase));

                if (r != null)
                {
                    string tr_body = r.TRIGGER_BODY.ToLower();
                    string sq = this.GetSequenceName(table, m.Name);

                    if (sq_dt.Contains(sq, StringComparer.OrdinalIgnoreCase))
                    {
                        m.IsAutoIncrement = tr_body.Contains(sq.ToLower()) && tr_body.Contains(m.Name.ToLower());
                        break;
                    }
                }

                var index_row = in_dt.FindAll(q=> m.Name.Equals(q.COLUMN_NAME, StringComparison.OrdinalIgnoreCase));
                if (index_row != null && index_row.Count > 0)
                {
                    m.Indexs = new List<IndexModel>(index_row.Count);
                    foreach (var ir in index_row)
                    {
                        IndexModel index = new IndexModel();
                        m.Indexs.Add(index);
                        index.ColumnName = m.Name;
                        index.Name = ir.INDEX_NAME;
                        index.IsUnique = string.Compare(ir.UNIQUENESS, "UNIQUE", true) == 0;
                    }
                }

                string type = row.DATA_TYPE;
                if (string.Compare(type, "NUMBER", true) == 0)
                {
                    m.MaxLength = row.DATA_PRECISION ?? 0;
                    m.MinLength = row.DATA_SCALE ?? 0;
                }
                else
                {
                    if (string.Compare(type, "RAW", true) == 0)
                        m.MaxLength = row.DATA_LENGTH ?? 0;
                    else
                        m.MaxLength = row.CHAR_LENGTH ?? 0;
                }
                m.DataType = type;

            }


            return list;
        }
        /// <summary>
        /// 获取列数据库类型
        /// </summary>
        /// <param name="propertyType">model 属性类型</param>
        /// <param name="maxLength">类型最大长度</param>
        /// <param name="minLength">类型最小长度</param>
        /// <returns>列数据库类型</returns>
        public override string GetColumnType(Type propertyType, int maxLength, int minLength)
        {
            string type = null;
            if (propertyType.IsEnum)
            {
                propertyType = typeof(int);
            }
            else if (typeof(byte[]) == propertyType)
            {
                if (maxLength > 2000 || maxLength == 0)
                    type = "blob";
            }
            else if (typeof(string) == propertyType)
            {
                if (maxLength > 1024)
                    type = "NCLOB";
                else if (maxLength <= 0)
                    maxLength = 50;
            }
            else if (typeof(decimal) == propertyType || typeof(decimal?) == propertyType)
            {
                if (minLength == 0 && maxLength == 0)
                {
                    maxLength = 18;
                    minLength = 7;
                }
                if (maxLength <= 0) maxLength = 38;
                if (minLength > maxLength) minLength = maxLength - 1;
            }


            if (null != type || dic.TryGetValue(propertyType, out type))
            {
                type = string.Format(type, maxLength, minLength);
            }

            return type;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this.db != null) this.db.Dispose();
            this.db = null;
            base.Dispose();
        }

        private static Dictionary<Type, string> dic = new Dictionary<Type, string>();
        static OracleTableSchema()
        {
            dic.Add(typeof(int), "NUMBER(11, 0)");
            dic.Add(typeof(int?), "NUMBER(11, 0)");

            dic.Add(typeof(IntPtr), "NUMBER(11, 0)");
            dic.Add(typeof(IntPtr?), "NUMBER(11, 0)");

            dic.Add(typeof(long), "NUMBER(19, 0)");
            dic.Add(typeof(long?), "NUMBER(19, 0)");

            dic.Add(typeof(short), "NUMBER(5, 0)");
            dic.Add(typeof(short?), "NUMBER(5, 0)");

            dic.Add(typeof(byte), "NUMBER(3, 0)");
            dic.Add(typeof(byte?), "NUMBER(3, 0)");

            dic.Add(typeof(bool), "NUMBER(1,0)");
            dic.Add(typeof(bool?), "NUMBER(1,0)");

            dic.Add(typeof(char), "CHAR(1)");
            dic.Add(typeof(char?), "CHAR(1)");
            dic.Add(typeof(char[]), "CHAR({0})");

            dic.Add(typeof(decimal), "NUMBER({0},{1})");
            dic.Add(typeof(decimal?), "NUMBER({0},{1})");

            dic.Add(typeof(float), "BINARY_FLOAT");
            dic.Add(typeof(float?), "BINARY_FLOAT");

            dic.Add(typeof(double), "BINARY_DOUBLE");
            dic.Add(typeof(double?), "BINARY_DOUBLE");

            dic.Add(typeof(DateTime), "TIMESTAMP(6)");//TIMESTAMP(6)
            dic.Add(typeof(DateTime?), "TIMESTAMP(6)");//TIMESTAMP(6)

            dic.Add(typeof(DateTimeOffset), "TIMESTAMP (7) WITH TIME ZONE");
            dic.Add(typeof(DateTimeOffset?), "TIMESTAMP (7) WITH TIME ZONE");

            dic.Add(typeof(TimeSpan), "TIMESTAMP(6)");//
            dic.Add(typeof(TimeSpan?), "TIMESTAMP(6)");//

            dic.Add(typeof(Guid), "VARCHAR2(50)");
            dic.Add(typeof(Guid?), "VARCHAR2(50)");

            dic.Add(typeof(string), "NVARCHAR2({0})");//1024
           // dic.Add("text", "NCLOB");//4GB

            dic.Add(typeof(byte[]), "RAW({0})");//2000
            //dic.Add("blob", "BLOB");//4GB
        }
    }
}
