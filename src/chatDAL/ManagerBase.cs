using System;
using System.Collections.Generic;
using System.Text;

namespace chatDAL
{
   public class ManagerBase
    {
        public static string ConnString;// = "PORT=5432;DATABASE=zlchat;HOST=192.168.31.98;PASSWORD=zlsoft;USER ID=postgres";
        public static IDataAccessAdapter DbAccess;

        public static void ConnCheck()
        {
            try
            {
                DbAccess._ConnCheck(ConnString);
            }
            catch (Npgsql.PostgresException pgEx)
            {
                switch (pgEx.SqlState)
                {
                    case "3D000":   //3D000: database "xxx" does not exist  (INVALID CATALOG NAME)
                        throw new Exception("系统数据库访问出错，无效的数据库目录名。", pgEx);
                    case "28P01":        //28P01: password authentication failed for user "xxx"
                        throw new Exception("系统数据库访问出错，用户名密码不正确。", pgEx);
                    default:
                        throw new Exception("系统数据库访问出错，可能是配置不正确。", pgEx);
                }
            }
            catch (System.Net.Sockets.SocketException socketEx)
            {
                if (socketEx.ErrorCode == 10061)
                    throw new Exception("系统数据库访问出错，可能是地址或端口配置不正确。", socketEx);
                throw new Exception("系统数据库访问出错，可能是配置不正确。", socketEx);
            }
            catch (TimeoutException ex)
            {
                throw new Exception("系统数据库访问超时，可能是地址配置不正确。", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("初始化系统数据库访问组件出错", ex);
            }
        }
    }
}
