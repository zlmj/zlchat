using chatModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace chatDAL
{
    public class RecordDAL : ManagerBase
    {
        public static RecordDAL Instance { get; } = new RecordDAL();

        private RecordDAL()
        {
        }

        public void InsertData(Record record)
        {
            string sql = string.Format("insert into record(content,name,date,systemid,maincode,mainid) values('{0}','{1}','{2}','{3}','{4}','{5}')", record.Content, record.Name, record.Date, record.System, record.MainCode, record.MainID);
            DbAccess.ExecuteNonQuery(ConnString, CommandType.Text, sql, null);
        }

        public IList<Record> GetAll(string system, string maincode, string mainid)
        {
            string sql = string.Format("select * from record where systemid='{0}' and maincode='{1}' and mainid='{2}'", system, maincode, mainid);
            DataSet dataSet = DbAccess.ExecuteQuery(ConnString, CommandType.Text, sql, null);
            IList<Record> patInfos = ModelConvert.DataSetToIList<Record>(dataSet, 0);

            return patInfos;
        }





        public bool IsChatExist(string system, string maincode, string mainid)
        {
            string sql = string.Format("select count(*) from record where systemid='{0}' and maincode='{1}' and mainid='{2}'", system, maincode, mainid);
            object objVal = DbAccess.ExecuteScalar(ConnString, CommandType.Text, sql, null);
            if (objVal != null && objVal != DBNull.Value)
            {
                int nVal;
                if (int.TryParse(objVal.ToString(), out nVal) && nVal > 0)
                    return true;

                sql = string.Format("select count(*) from \"user\" where systemid='{0}' and maincode='{1}' and mainid='{2}'", system, maincode, mainid);
                objVal = DbAccess.ExecuteScalar(ConnString, CommandType.Text, sql, null);
                if (objVal != null && objVal != DBNull.Value)
                {
                    if (int.TryParse(objVal.ToString(), out nVal) && nVal > 0)
                        return true;
                }
            }
            return false;
        }

        public bool IsUserOnline(string system, string maincode, string mainid, string uid)
        {
            string sql = string.Format("select count(*) from \"user\" where systemid='{0}' and maincode='{1}' and mainid='{2}' and \"id\"='{3}' and status=1", system, maincode, mainid, uid);
            object objVal = DbAccess.ExecuteScalar(ConnString, CommandType.Text, sql, null);
            if (objVal != null && objVal != DBNull.Value)
            {
                int nVal;
                if (int.TryParse(objVal.ToString(), out nVal))
                    return nVal > 0;
            }
            return false;
        }

        public Dictionary<string, string> GetOfflineUsers(string system, string maincode, string mainid)
        {
            string sql = string.Format("select \"id\",name from \"user\" where systemid='{0}' and maincode='{1}' and mainid='{2}' and status=0", system, maincode, mainid);
            DataSet ds = DbAccess.ExecuteQuery(ConnString, CommandType.Text, sql, null);
            if (ds != null && ds.Tables.Count > 0)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                string id, name;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    id = row["ID"].ToString();
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        id = id.Trim();
                        name = row["name"].ToString().Trim();
                        dict[id] = name;
                    }
                }
                return dict;
            }
            return null;
        }

        public string GetAllChats()
        {
            string sql = "select DISTINCT systemid,maincode,mainid from record union select DISTINCT systemid, maincode, mainid from \"user\"";
            StringBuilder str = new StringBuilder();
            DataSet dataSet = DbAccess.ExecuteQuery(ConnString, CommandType.Text, sql, null);
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                str.AppendLine($"讨论组 * {dataSet.Tables[0].Rows.Count.ToString()}<br>");
                string system, maincode, mainid;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    system = row["systemid"].ToString();
                    maincode = row["maincode"].ToString();
                    mainid = row["mainid"].ToString();
                    str.AppendLine($"[系统:{system}, 主体编码:{maincode}, 主体ID:{mainid}]chat/info?system={system}&maincode={maincode}&mainid={mainid}<br>");
                }
            }
            return str.ToString();
        }


        public string GetChatInfo(string system, string maincode, string mainid)
        {
            string sql = string.Format("select count(*) from record where systemid='{0}' and maincode='{1}' and mainid='{2}'", system, maincode, mainid);
            object objVal = DbAccess.ExecuteScalar(ConnString, CommandType.Text, sql, null);
            int nRecordCount = 0;
            if (objVal != null && objVal != DBNull.Value)
                int.TryParse(objVal.ToString(), out nRecordCount);

            StringBuilder str = new StringBuilder($"[系统:{system}, 主体编码:{maincode}, 主体ID:{mainid}]");
            sql = string.Format("select \"id\",name,status from \"user\" where systemid='{0}' and maincode='{1}' and mainid='{2}'", system, maincode, mainid);
            DataSet dataSet = DbAccess.ExecuteQuery(ConnString, CommandType.Text, sql, null);
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                str.AppendLine($"参与人员 * {dataSet.Tables[0].Rows.Count.ToString()}<br>");
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    str.AppendLine($">>姓名:{row["name"].ToString()}, ID:{row["ID"].ToString()}, 状态:{(row["status"].ToString() == "1" ? "在线" : "离线")}<br>");
                }
            }

            str.Append($"讨论消息数 * {nRecordCount.ToString()}");

            return str.ToString();
        }


        public string GetChatOfUser(string uid)
        {
            string sql = $"select DISTINCT systemid, maincode, mainid from \"user\" where \"id\"='{uid}'";
            StringBuilder str = new StringBuilder();
            DataSet dataSet = DbAccess.ExecuteQuery(ConnString, CommandType.Text, sql, null);
            int count = 0;
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                count = dataSet.Tables[0].Rows.Count;
                string system, maincode, mainid;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    system = row["systemid"].ToString();
                    maincode = row["maincode"].ToString();
                    mainid = row["mainid"].ToString();
                    str.AppendLine($"[系统:{system}, 主体编码:{maincode}, 主体ID:{mainid}]chat/info?system={system}&maincode={maincode}&mainid={mainid}<br>");
                }
            }
            str.Insert(0, $"人员[{uid}]的讨论组共计 {count} 个<br>");
            return str.ToString();
        }
        public string GetChatOfMaincode(string system1, string maincode1)
        {
            string sql = $"select DISTINCT systemid, maincode, mainid from \"user\" where systemid='{system1}' and maincode='{maincode1}'";
            StringBuilder str = new StringBuilder();
            DataSet dataSet = DbAccess.ExecuteQuery(ConnString, CommandType.Text, sql, null);
            int count = 0;
            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                count = dataSet.Tables[0].Rows.Count;
                string system, maincode, mainid;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    system = row["systemid"].ToString();
                    maincode = row["maincode"].ToString();
                    mainid = row["mainid"].ToString();
                    str.AppendLine($"[系统:{system}, 主体编码:{maincode}, 主体ID:{mainid}]chat/info?system={system}&maincode={maincode}&mainid={mainid}<br>");
                }
            }
            str.Insert(0, $"主体[{system1}.{maincode1}]的讨论组共计 {count} 个<br>");
            return str.ToString();
        }
    }
}
