using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Web;

namespace chatDAL
{
    public class ODPDataAccess
    {
        int blobSize = -1;
        string mConnString;

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string DBConnString
        {
            get { return mConnString; }
            set { mConnString = value; }
        }

        /// <summary>
        ///构造函数 
        /// </summary>
        public ODPDataAccess(string connString)
        {
            mConnString = connString;
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <returns></returns>
        protected OracleConnection GetConnection()
        {
            OracleConnection conn = new OracleConnection(DBConnString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// 创建Oracle参数
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        OracleParameter[] CreateParameters(object[] text)
        {
            List<OracleParameter> list = new List<OracleParameter>();
            for (int i = 0; i < text.Length; i += 2)
            {
                //这里是解决一个比较奇怪的问题，如果参数的类型是char，新建的OracleParameter的OracleType会变成byte，执行时会出错，只好在这里进行特殊处理
                //但是，编写一个小程序的里面，char类型的转换是正确，转换成OracleType.VarChar，执行结果正确。
                OracleParameter p = null;
                if (text[i + 1].GetType() == typeof(char))
                {
                    p = new OracleParameter(text[i].ToString(), OracleDbType.Char);
                    p.Value = text[i + 1];
                }
                else
                {
                    p = new OracleParameter(text[i].ToString(), text[i + 1]);
                }
                list.Add(p);
            }
            return list.ToArray();
        }

        /// <summary>
        /// 执行sql获取数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public OracleDataReader ExecuteReader(string query, params object[] parameters)
        {
            return ExecuteReader(query, CreateParameters(parameters));
        }

        /// <summary>
        /// 执行sql获取数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public OracleDataReader ExecuteReader(string query, OracleParameter[] parameters)
        {
            return ExecuteReader(query, parameters, CommandType.Text);
        }

        /// <summary>
        /// 执行sql获取数据
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public OracleDataReader ExecuteReader(string query, OracleParameter[] parameters, CommandType type)
        {
            OracleConnection conn = GetConnection();
            try
            {
                //<--链路跟踪日志
                AppendTrace(query, parameters);
                //-->
                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    cmd.CommandType = type;
                    cmd.AddToStatementCache = true;
                    cmd.BindByName = true;
                    cmd.InitialLOBFetchSize = blobSize;
                    if (parameters != null)
                    {
                        foreach (OracleParameter p in parameters)
                        {
                            cmd.Parameters.Add(p);
                        }
                    }

                    //备注:连接关闭后,Reader读取数据将报错.
                    return cmd.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }

                ClearPoolAfterEx(ex, conn);
                throw;
            }
        }

        /// <summary>
        /// 执行sql获取对象
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string query, params object[] parameters)
        {
            return ExecuteScalar(query, CreateParameters(parameters));
        }

        /// <summary>
        /// 执行sql获取对象
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar(string query, OracleParameter[] parameters)
        {
            return ExecuteScalar(query, parameters, CommandType.Text);
        }

        /// <summary>
        /// 执行sql获取对象
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object ExecuteScalar(string query, OracleParameter[] parameters, CommandType type)
        {
            OracleConnection conn = GetConnection();
            //<--链路跟踪日志
            AppendTrace(query, parameters);
            //-->
            try
            {
                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    cmd.CommandType = type;
                    cmd.AddToStatementCache = true;
                    cmd.BindByName = true;
                    cmd.InitialLOBFetchSize = blobSize;

                    if (parameters != null)
                    {
                        foreach (OracleParameter p in parameters)
                        {
                            cmd.Parameters.Add(p);
                        }
                    }

                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                ClearPoolAfterEx(ex, conn);
                throw;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public void ExecuteNonQuery(string sql, params object[] parameters)
        {
            ExecuteNonQuery(sql, CreateParameters(parameters));
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public void ExecuteNonQuery(string sql, OracleParameter[] parameters)
        {
            ExecuteNonQuery(sql, parameters, CommandType.Text);
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="type"></param>
        public void ExecuteNonQuery(string sql, OracleParameter[] parameters, CommandType type)
        {
            OracleConnection conn = GetConnection();
            //<--链路跟踪日志
            AppendTrace(sql, parameters);
            //-->
            try
            {
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    cmd.CommandType = type;
                    cmd.AddToStatementCache = true;
                    cmd.BindByName = true;
                    cmd.InitialLOBFetchSize = blobSize;
                    if (parameters != null)
                    {
                        foreach (OracleParameter p in parameters)
                        {
                            cmd.Parameters.Add(p);
                        }
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                ClearPoolAfterEx(ex, conn);
                throw;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// 执行sql获取Datatable
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string query, params object[] parameters)
        {
            return ExecuteDataTable(query, CreateParameters(parameters));
        }

        /// <summary>
        /// 执行sql获取Datatable
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string query, OracleParameter[] parameters)
        {
            return ExecuteDataTable(query, parameters, CommandType.Text);
        }

        /// <summary>
        /// 执行sql获取Datatable
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string query, OracleParameter[] parameters, CommandType type)
        {
            OracleConnection conn = GetConnection();
            //<--链路跟踪日志
            AppendTrace(query, parameters);
            //-->
            try
            {
                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    cmd.CommandType = type;
                    cmd.AddToStatementCache = true;
                    cmd.BindByName = true;
                    cmd.InitialLOBFetchSize = blobSize;

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        if (parameters != null)
                        {
                            foreach (OracleParameter p in parameters)
                            {
                                adapter.SelectCommand.Parameters.Add(p);
                            }
                        }
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                ClearPoolAfterEx(ex, conn);
                throw;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        /// <summary>
        /// 访问数据库发生异常后，检查是否需要从连接池移除连接。
        /// DataServiceHelper还有一个ClearPoolAfterEx（）
        /// </summary>
        /// 也可以在连接串参数中增加“validate connection=true”，但在打开连接时会有额外的消耗。
        ///     Use the ODP.NET connection string parameter "validate connection=true" By default, it is set to false, 
        ///     which means that no checking of the connection is done when it is retrieved from the pool as a result of a OracleConnection::Open call.
        ///     Setting it to true will cause ODP.NET to verify that the connection is still good by making a database round trip. 
        ///     If a problem with the connection is found, it is removed from the pool, and another connection is tried internally.
        static void ClearPoolAfterEx(Exception ex, OracleConnection conn)
        {
            if (ex == null)
            {
                return;
            }

            //对工作线程执行Abort后，连接已损坏，必须从线程池移除。但Abort后，捕获到的异常类型不一定是ThreadAbortException。可能是Exception类型，或者ORA-12537，12570，1013
            if (ex is ThreadAbortException
                || (Thread.CurrentThread.ThreadState & ThreadState.AbortRequested) == ThreadState.AbortRequested)
            {
                if (conn != null)
                {
                    OracleConnection.ClearPool(conn);
                    throw new Exception("工作线程Abort，已从数据库连接池移除当前连接。", ex);
                }
                else
                {
                    OracleConnection.ClearAllPools();
                    throw new Exception("工作线程Abort，已清空数据库连接池。", ex);
                }
            }

            OracleException oraEx = ex as OracleException;
            //如果不是从数据库返回的异常，可能是ODP.Net内部发生了错误，清除连接。
            if (oraEx == null)
            {
                if (conn != null)
                {
                    OracleConnection.ClearPool(conn);
                    throw new Exception("发生未知异常，已从数据库连接池移除当前连接。", ex);
                }
                else
                {
                    OracleConnection.ClearAllPools();
                    throw new Exception("发生未知异常，已清空数据库连接池。", ex);
                }
            }

            switch (oraEx.Number)
            {
                case 3113:  //ORA-03113: 通信通道的文件结尾- 可能发生于重启数据库之后重新连接时
                    OracleConnection.ClearAllPools();
                    throw new Exception("发生ORA-03113错误，已清空数据库连接池。", ex);
                case 28:    //ORA-00028: 会话己被终止 -  kill会话时可能发生00028或03111异常
                case 1012:  //ORA-01012: 没有登录(not logon) - 发生在ORA-00028后再访问数据库
                case 1013:  //ORA-01013: 用户请求取消当前的操作
                case 2396:  //ORA-02396: 超出最大空闲时间(exceeded maximum idle time)
                case 3111:  //ORA-03111: 通信通道收到中断 - kill会话时可能发生00028或03111异常
                case 3135:  //ORA-03135: 连接失去联系(connection lost contact)
                case 6508:  //ORA-06508: 无法找到正在调用的程序单元 - 存储过程用了全局变量可能出现
                case 12535: //ORA-12535: TNS操作超时(TNS:operation timed out)
                case 12537: //ORA-12537: 网络会话: 文件结束
                case 12570: //ORA-12570: 网络会话：意外的数据包读取错误
                    if (conn != null)
                    {
                        OracleConnection.ClearPool(conn);
                        throw new Exception("发生ORA-" + oraEx.Number.ToString() + "错误，已从数据库连接池移除当前连接。", ex);
                    }
                    else
                    {
                        OracleConnection.ClearAllPools();
                        throw new Exception("发生ORA-" + oraEx.Number.ToString() + "错误，已清空数据库连接池。", ex);
                    }
                default:
                    break;
            }
        }


        /// <summary>
        /// 添加跟踪日志
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        static void AppendTrace(string sql, OracleParameter[] parameters)
        {
        }
    }
}