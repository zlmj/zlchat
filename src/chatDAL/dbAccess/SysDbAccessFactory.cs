using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace chatDAL
{
    public class SysDbAccessFactory
    {
        public static IDataAccessAdapter Create(string dbType)
        {
            if (string.IsNullOrWhiteSpace(dbType))
                throw new ArgumentNullException("dbType");

            switch (dbType.Trim().ToLower())
            {
                case "pg":
                case "postgres":
                    return new PostgreDbAccess();
                case "ora":
                case "oracle":
                default:
                    throw new NotSupportedException($"不支持的数据库类型:{dbType}");
            }
        }
    }
}