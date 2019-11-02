using chatModel;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web; 

namespace chatDAL
{
    public class ChatroomDAL : ManagerBase
    {
        public static ChatroomDAL Instance { get; } = new ChatroomDAL();

        public void Insert(string system, string maincode, string mainid, string subject,
            string time, string uid, string uname)
        {
            string sql = "insert into chatroom(systemid,maincode,mainid,subject,createtime,creatorid,creator) values(:systemid,:maincode,:mainid,:subject,:time,:uid,:uname) ON conflict(systemid,maincode,mainid) DO UPDATE SET subject=:subject,lasttime=:time,lastuid=:uid,lastuname=:uname";
            NpgsqlParameter[] parms = new NpgsqlParameter[] {
                new NpgsqlParameter(":systemid", NpgsqlDbType.Text),
                new NpgsqlParameter(":maincode", NpgsqlDbType.Text),
                new NpgsqlParameter(":mainid", NpgsqlDbType.Text),
                new NpgsqlParameter(":subject", NpgsqlDbType.Text),
                new NpgsqlParameter(":time", NpgsqlDbType.Text),
                new NpgsqlParameter(":uid", NpgsqlDbType.Text),
                new NpgsqlParameter(":uname", NpgsqlDbType.Text)
            };
            parms[0].Value = system;
            parms[1].Value = maincode;
            parms[2].Value = mainid;
            parms[3].Value = subject;
            parms[4].Value = time;
            parms[5].Value = uid;
            parms[6].Value = uname;

            //try
            //{
                DbAccess.ExecuteNonQuery(ConnString, CommandType.Text, sql, parms);
            //}
            //catch (Exception ex)
            //{
            //    sql = "insert into chatroom(systemid,maincode,mainid,subject,createtime,creatorid,creator) values(:systemid,:maincode,:mainid,:subject,:createtime,:creatorid,:creator)";
            //    parms = new NpgsqlParameter[] {
            //        new NpgsqlParameter(":systemid", NpgsqlDbType.Text),
            //        new NpgsqlParameter(":maincode", NpgsqlDbType.Text),
            //        new NpgsqlParameter(":mainid", NpgsqlDbType.Text),
            //        new NpgsqlParameter(":subject", NpgsqlDbType.Text),
            //        new NpgsqlParameter(":time", NpgsqlDbType.Text),
            //        new NpgsqlParameter(":uid", NpgsqlDbType.Text),
            //        new NpgsqlParameter(":uname", NpgsqlDbType.Text)
            //    };
            //    parms[0].Value = system;
            //    parms[1].Value = maincode;
            //    parms[2].Value = mainid;
            //    parms[3].Value = subject;
            //    parms[4].Value = time;
            //    parms[5].Value = uid;
            //    parms[6].Value = uname;
            //    PostgreHelper.ExecuteNonQuery(ConnString, CommandType.Text, sql, parms);
            //}
        }

        public void Update(string system, string maincode, string mainid, string subject, string lasttime, string lastuid, string lastuname)
        {
            string sql = "update chatroom set subject=:subject,lasttime=:lasttime,lastuid=:lastuid,lastuname=:lastuname where systemid=:systemid and maincode=:maincode and mainid=:mainid";
            NpgsqlParameter[] parms = new NpgsqlParameter[] {
                new NpgsqlParameter(":systemid", NpgsqlDbType.Text),
                new NpgsqlParameter(":maincode", NpgsqlDbType.Text),
                new NpgsqlParameter(":mainid", NpgsqlDbType.Text),
                new NpgsqlParameter(":subject", NpgsqlDbType.Text),
                new NpgsqlParameter(":lastdate", NpgsqlDbType.Text),
                new NpgsqlParameter(":lastuid", NpgsqlDbType.Text),
                new NpgsqlParameter(":lastuname", NpgsqlDbType.Text)
            };
            parms[0].Value = system;
            parms[1].Value = maincode;
            parms[2].Value = mainid;
            parms[3].Value = subject;
            parms[4].Value = lasttime;
            parms[5].Value = lastuid;
            parms[6].Value = lastuname;
            DbAccess.ExecuteNonQuery(ConnString, CommandType.Text, sql, parms);
        }

        public List<ChatRoom> GetChatRoomByUid(string uid)
        {
            string sql = $"select c.systemid,c.maincode,c.mainid,c.subject,c.creator,c.creatorid,c.createtime,c.lasttime,c.lastuid,c.lastuname,c.unread, c.name as uname, r.content as lastmsg from(select r.*, u.unread,u.name from \"user\" u, chatroom r where u.systemid = r.systemid and u.maincode = r.maincode and u.mainid = r.mainid and \"id\" = '{uid}') c left join(select * from record a where a.date = (select max(b.date) from record b where a.systemid = b.systemid and a.maincode = b.maincode and a.mainid = b.mainid)) r on c.systemid = r.systemid and c.maincode = r.maincode and c.mainid = r.mainid";
            DataSet dataSet = DbAccess.ExecuteQuery(ConnString, CommandType.Text, sql);
            return ModelConvert.DataSetToIList<ChatRoom>(dataSet, 0).ToList();
        }
    }
}