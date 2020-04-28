using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Afx.Data;
using Oracle.ManagedDataAccess.Client;
using Afx.Data.Entity.Schema;

namespace Afx.Data.Oracle.Entity.Schema
{
    /// <summary>
    /// 获取数据库结构信息
    /// </summary>
    public class OracleDatabaseSchema : DatabaseSchema
    {
        private Database db;
        private string database;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connectionString">数据库链接字符串</param>
        public OracleDatabaseSchema(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");
            var connectionStringBuilder = new OracleConnectionStringBuilder(connectionString);
            this.database = connectionStringBuilder.UserID.ToUpper();
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
        /// 是否存在数据库
        /// </summary>
        /// <returns>true：存在，false：不存在</returns>
        public override bool Exist()
        {
            object obj = this.db.ExecuteScalar<object>("SELECT 1 FROM DUAL");
            int count = obj != null && obj != DBNull.Value ? Convert.ToInt32(obj) : 0;

            return count > 0;
        }
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <returns>true：创建成功，false：创建失败</returns>
        public override bool CreateDatabase()
        {
            object obj = this.db.ExecuteScalar<object>("SELECT 1 FROM DUAL");
            int count = obj != null && obj != DBNull.Value ? Convert.ToInt32(obj) : 0;

            return count > 0;
        }
        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <returns>true：删除成功，false：删除失败</returns>
        public override bool DeleteDatabase()
        {

            return false;
        }
        
        /// <summary>
        /// 获取数据库名称
        /// </summary>
        /// <returns>数据库名称</returns>
        public override string GetDatabase()
        {
           return this.database;
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            if (this.db != null) this.db.Dispose();
            this.db = null;
            base.Dispose();
        }
    }
}
