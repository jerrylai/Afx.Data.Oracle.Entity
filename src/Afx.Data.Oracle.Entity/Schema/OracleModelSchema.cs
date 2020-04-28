using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Afx.Data.Entity.Schema;

namespace Afx.Data.Oracle.Entity.Schema
{
    /// <summary>
    /// 获取model信息
    /// </summary>
    public class OracleModelSchema : ModelSchema
    {
        /// <summary>
        /// 获取model属性类型string
        /// </summary>
        /// <param name="column">列信息</param>
        /// <returns>model属性类型string</returns>
        public override string GetPropertyType(ColumnInfoModel column)
        {
            if (column == null) throw new ArgumentNullException("column");
            string type = "";
            DataTypeModel m = null;
            dic.TryGetValue(column.DataType, out m);
            if (m != null)
            {
                type = column.IsNullable ? m.NullableType : m.Type;
                if(column.DataType.ToUpper() == "NUMBER")
                {
                    if(column.MinLength == 0)
                    {
                        if(column.MaxLength <= 1)
                        {
                            type = "bool";
                        }
                        else if(column.MaxLength <= 3)
                        {
                            type = "byte";
                        }
                        else if (column.MaxLength <= 5)
                        {
                            type = "short";
                        }
                        else if (column.MaxLength <= 11)
                        {
                            type = "int";
                        }
                        else
                        {
                            type = "long";
                        }
                    }
                    else
                    {
                        type = "decimal";
                    }
                }
            }

            return type;
        }

        private static Dictionary<string, DataTypeModel> dic = new Dictionary<string, DataTypeModel>(StringComparer.OrdinalIgnoreCase);

        static OracleModelSchema()
        {
            dic.Add("NUMBER", new DataTypeModel() { DbDataType = "NUMBER", Type = "int", NullableType = "Nullable<int>" });

            dic.Add("BINARY_FLOAT", new DataTypeModel() { DbDataType = "BINARY_FLOAT", Type = "float", NullableType = "Nullable<float>" });
            dic.Add("BINARY_DOUBLE", new DataTypeModel() { DbDataType = "BINARY_DOUBLE", Type = "double", NullableType = "Nullable<double>" });

            dic.Add("CHAR", new DataTypeModel() { DbDataType = "CHAR", Type = "string", NullableType = "string" });
            dic.Add("NCHAR", new DataTypeModel() { DbDataType = "NCHAR", Type = "string", NullableType = "string" });
            dic.Add("VARCHAR", new DataTypeModel() { DbDataType = "VARCHAR", Type = "string", NullableType = "string" });
            dic.Add("VARCHAR2", new DataTypeModel() { DbDataType = "VARCHAR2", Type = "string", NullableType = "string" });
            dic.Add("NVARCHAR2", new DataTypeModel() { DbDataType = "NVARCHAR2", Type = "string", NullableType = "string" });
            dic.Add("CLOB", new DataTypeModel() { DbDataType = "CLOB", Type = "string", NullableType = "string" });
            dic.Add("NCLOB", new DataTypeModel() { DbDataType = "NCLOB", Type = "string", NullableType = "string" });

            dic.Add("TIMESTAMP", new DataTypeModel() { DbDataType = "TIMESTAMP", Type = "DateTime", NullableType = "Nullable<DateTime>" });
            dic.Add("TIMESTAMP WITH TIME ZONE", new DataTypeModel() { DbDataType = "TIMESTAMP WITH TIME ZONE", Type = "DateTimeOffset", NullableType = "Nullable<DateTimeOffset>" });
            dic.Add("TIMESTAMP WITH LOCAL TIME ZONE", new DataTypeModel() { DbDataType = "TIMESTAMP WITH LOCAL TIME ZONE", Type = "DateTimeOffset", NullableType = "Nullable<DateTimeOffset>" });

            dic.Add("RAW", new DataTypeModel() { DbDataType = "RAW", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("LONG RAW", new DataTypeModel() { DbDataType = "LONG RAW", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("BLOB", new DataTypeModel() { DbDataType = "BLOB", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("BFILE", new DataTypeModel() { DbDataType = "BFILE", Type = "byte[]", NullableType = "byte[]" });
            dic.Add("CFILE", new DataTypeModel() { DbDataType = "CFILE", Type = "byte[]", NullableType = "byte[]" });

            //====================

            dic.Add("BINARY ROWID", new DataTypeModel() { DbDataType = "BINARY ROWID", Type = "int", NullableType = "Nullable<int>" });
            
            //dic.Add("CANONICAL", new DataTypeModel() { DbDataType = "CANONICAL", Type = "byte", NullableType = "byte" });
            
            dic.Add("CONTIGUOUS ARRAY", new DataTypeModel() { DbDataType = "CONTIGUOUS ARRAY", Type = "byte[]", NullableType = "byte[]" });

            dic.Add("DATE", new DataTypeModel() { DbDataType = "DATE", Type = "DateTime", NullableType = "Nullable<DateTime>" });
            
            dic.Add("DECIMAL", new DataTypeModel() { DbDataType = "DECIMAL", Type = "decimal", NullableType = "Nullable<decimal>" });

            dic.Add("DOUBLE PRECISION", new DataTypeModel() { DbDataType = "DOUBLE PRECISION", Type = "double", NullableType = "Nullable<double>" });

            dic.Add("FLOAT", new DataTypeModel() { DbDataType = "FLOAT", Type = "float", NullableType = "Nullable<float>" });

            dic.Add("INTEGER", new DataTypeModel() { DbDataType = "INTEGER", Type = "int", NullableType = "Nullable<int>" });

            dic.Add("INTERVAL DAY TO SECOND", new DataTypeModel() { DbDataType = "INTERVAL DAY TO SECOND", Type = "int", NullableType = "Nullable<int>" });

            dic.Add("INTERVAL YEAR TO MONTH", new DataTypeModel() { DbDataType = "INTERVAL YEAR TO MONTH", Type = "int", NullableType = "Nullable<int>" });

            dic.Add("LOB POINTER", new DataTypeModel() { DbDataType = "LOB POINTER", Type = "int", NullableType = "Nullable<int>" });

            //dic.Add("NAMED COLLECTION", new DataTypeModel() { DbDataType = "NAMED COLLECTION", Type = "int", NullableType = "Nullable<int>" });
            //dic.Add("NAMED OBJECT", new DataTypeModel() { DbDataType = "NAMED OBJECT", Type = "int", NullableType = "Nullable<int>" });


            dic.Add("OCTET", new DataTypeModel() { DbDataType = "OCTET", Type = "long", NullableType = "Nullable<long>" });

            dic.Add("OID", new DataTypeModel() { DbDataType = "OID", Type = "int", NullableType = "Nullable<int>" });

            dic.Add("POINTER", new DataTypeModel() { DbDataType = "POINTER", Type = "int", NullableType = "Nullable<int>" });

            dic.Add("LONG", new DataTypeModel() { DbDataType = "LONG", Type = "long", NullableType = "Nullable<long>" });

            dic.Add("ROWID", new DataTypeModel() { DbDataType = "ROWID", Type = "int", NullableType = "Nullable<int>" });
            
            dic.Add("REAL", new DataTypeModel() { DbDataType = "REAL", Type = "float", NullableType = "Nullable<float>" });

            //dic.Add("REF", new DataTypeModel() { DbDataType = "REF", Type = "float", NullableType = "Nullable<float>" });

            dic.Add("SIGNED BINARY INTEGER", new DataTypeModel() { DbDataType = "SIGNED BINARY INTEGER", Type = "int", NullableType = "Nullable<int>" });

            dic.Add("SMALLINT", new DataTypeModel() { DbDataType = "SMALLINT", Type = "short", NullableType = "Nullable<short>" });

            //dic.Add("TABLE", new DataTypeModel() { DbDataType = "TABLE", Type = "float", NullableType = "Nullable<float>" });

            dic.Add("TIME", new DataTypeModel() { DbDataType = "TIME", Type = "DateTime", NullableType = "Nullable<DateTime>" });

            dic.Add("TIME WITH TZ", new DataTypeModel() { DbDataType = "TIME WITH TZ", Type = "DateTime", NullableType = "Nullable<DateTime>" });

            dic.Add("TIMESTAMP WITH LOCAL TZ", new DataTypeModel() { DbDataType = "TIMESTAMP WITH TIME ZONE", Type = "DateTimeOffset", NullableType = "Nullable<DateTimeOffset>" });

            dic.Add("TIMESTAMP WITH TZ", new DataTypeModel() { DbDataType = "TIMESTAMP WITH LOCAL TIME ZONE", Type = "DateTimeOffset", NullableType = "Nullable<DateTimeOffset>" });

            dic.Add("UNSIGNED BINARY INTEGER", new DataTypeModel() { DbDataType = "UNSIGNED BINARY INTEGER", Type = "int", NullableType = "Nullable<int>" });

            dic.Add("UROWID", new DataTypeModel() { DbDataType = "UROWID", Type = "uint", NullableType = "Nullable<uint>" });

            dic.Add("VARYING ARRAY", new DataTypeModel() { DbDataType = "VARYING ARRAY", Type = "byte[]", NullableType = "byte[]" });
        }
    }
}

