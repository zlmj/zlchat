using chatModel;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;

namespace chatDAL
{
    public class UserDAL : ManagerBase
    {
        public static UserDAL Instance { get; } = new UserDAL();

        private UserDAL()
        {
        }

        public void Insert(User user)
        {
            try
            {
                string sql = string.Format("insert into \"user\"(\"id\",\"name\",status,systemid,maincode,mainid) values('{0}','{1}',{2},'{3}','{4}','{5}') ON conflict(\"id\",systemid,maincode,mainid) DO UPDATE SET \"name\"='{1}',status={2}",
                    user.ID, user.Name, user.Status, user.System, user.MainCode, user.MainID);
                DbAccess.ExecuteNonQuery(ConnString, CommandType.Text, sql, null);
            }
            catch
            {
                string sql = string.Format("insert into \"user\"(\"id\",\"name\",status,systemid,maincode,mainid) values('{0}','{1}',{2},'{3}','{4}','{5}')",
                    user.ID, user.Name, user.Status, user.System, user.MainCode, user.MainID);
                DbAccess.ExecuteNonQuery(ConnString, CommandType.Text, sql, null);
            }
        }

        public IList<User> GetUser(User user)
        {
            string sql = string.Format("select * from \"user\" where systemid='{0}' and maincode='{1}' and mainid='{2}' and \"id\"='{3}'",
                user.System, user.MainCode, user.MainID, user.ID);
            DataSet dataSet = DbAccess.ExecuteQuery(ConnString, CommandType.Text, sql, null);
            IList<User> patInfos = ModelConvert.DataSetToIList<User>(dataSet, 0);
            return patInfos;
        }

        public void UpdateStatus(User user)
        {
            string sql = string.Format("update \"user\" set status={0} where \"id\"='{1}' and systemid='{2}' and mainid='{3}' and maincode='{4}'",
                user.Status, user.ID, user.System, user.MainID, user.MainCode);
            DbAccess.ExecuteNonQuery(ConnString, CommandType.Text, sql, null);
        }

        public void UpdateUnread(string system, string maincode, string mainid, string uid, int unread)
        {
            string sql = "update \"user\" set unread=:unread where \"id\"=:id and systemid=:systemid and maincode=:maincode and mainid=:mainid";
            NpgsqlParameter[] parms = new NpgsqlParameter[] {
                new NpgsqlParameter(":systemid", NpgsqlDbType.Text),
                new NpgsqlParameter(":maincode", NpgsqlDbType.Text),
                new NpgsqlParameter(":mainid", NpgsqlDbType.Text),
                new NpgsqlParameter(":id", NpgsqlDbType.Text),
                new NpgsqlParameter(":unread", NpgsqlDbType.Integer)
            };
            parms[0].Value = system;
            parms[1].Value = maincode;
            parms[2].Value = mainid;
            parms[3].Value = uid;
            parms[4].Value = unread > 0 ? 1 : 0;
            DbAccess.ExecuteNonQuery(ConnString, CommandType.Text, sql, parms);
        }

        public bool IsUserExist(User user)
        {
            string sql = string.Format("select count(*) from \"user\" where \"id\"='{0}' and systemid='{1}' and maincode='{2}' and mainid='{3}'",
                user.ID, user.System, user.MainCode, user.MainID);
            object objVal = DbAccess.ExecuteScalar(ConnString, CommandType.Text, sql, null);
            if (objVal != null && objVal != DBNull.Value)
            {
                int nVal;
                if (int.TryParse(objVal.ToString(), out nVal))
                    return nVal > 0;
            }
            return false;
        }


        public void UpdateAllOffline()
        {
            string sql = "update \"user\" set status=0";
            DbAccess.ExecuteNonQuery(ConnString, CommandType.Text, sql, null);
        }
    }
}