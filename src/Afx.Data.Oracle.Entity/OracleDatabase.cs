using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Afx.Data;
using Oracle.ManagedDataAccess.Client;

namespace Afx.Data.Oracle
{
    /// <summary>
    /// Oracle数据库访问类
    /// </summary>
    public class OracleDatabase : Database
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public OracleDatabase(string connectionString)
            : base(connectionString, OracleClientFactory.Instance)
        {

        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connection">数据库链接</param>
        /// <param name="isOwnsConnection">释放资源时是否释放链接</param>
        public OracleDatabase(OracleConnection connection, bool isOwnsConnection = true)
            : base(connection, isOwnsConnection)
        {

        }
        /// <summary>
        /// 参数化查询名称加前缀
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string EncodeParameterName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            return string.Format(":{0}", name);
        }
        /// <summary>
        /// 列名转义
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string EncodeColumn(string column)
        {
            if (string.IsNullOrEmpty(column)) throw new ArgumentNullException("column");
            return string.Format("\"{0}\"", column);
        }

        //public override DateTime GetUtcNow()
        //{
        //    this.ClearParameters();
        //    this.CommandText = "SELECT SYS_EXTRACT_UTC(systimestamp) FROM DUAL";
        //    object obj = this.ExecuteScalar();
        //    var time = Convert.ToDateTime(obj);
        //    if (time.Kind == DateTimeKind.Unspecified)
        //    {
        //        time = new DateTime(time.Ticks, DateTimeKind.Utc);
        //    }

        //    return time;
        //}
    }
}
