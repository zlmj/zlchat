using chatDAL;
using chatModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace chatBLL
{
    public class NotifyBLL
    {
        private static Dictionary<string, bool> sNotifyTag = new Dictionary<string, bool>();


        public static void SetUserOffline(string uid, string system, string maincode, string mainid)
        {
            if (!string.IsNullOrWhiteSpace(uid))
            {
                string key = $"{system}{maincode}{mainid}{uid}";
                lock (sNotifyTag)
                {
                    sNotifyTag.Remove(key);
                }
            }
        }

        public static void SendNotify(string uid, string uname, string system, string maincode, string mainid, string title,
            string url, string creatorId, string creatorName, bool checkOnline)
        {
            string breakNote = null;
            bool needBreak = false;
            bool execError = false;
            string key = $"{system}{maincode}{mainid}{uid}";
            try
            {
                NotifyExector[] exectors = ConfigManager.GetExectors(system, maincode);
                if (exectors == null || exectors.Length == 0)
                {
                    needBreak = true;
                    breakNote = "未配置";
                    return;
                }

                //如果已经发送过离线通知，则不再通知。
                lock (sNotifyTag)
                {
                    if (sNotifyTag.ContainsKey(key))
                    {
                        needBreak = true;
                        breakNote = "重复通知";
                        return;
                    }
                    else
                    {
                        sNotifyTag.Add(key, false);
                    }
                }

                //如果在线是不发通知的（通常是传 checkOnline=false ，因为调用该方法时直接取的离线用户）
                if (checkOnline && RecordDAL.Instance.IsUserOnline(system, maincode, mainid, uid))
                {
                    needBreak = true;
                    breakNote = "在线";
                    return;
                }

                //UrlEncode
                if (!string.IsNullOrWhiteSpace(url))
                {
                    string nameInUrl;
                    if (uid == uname)
                        nameInUrl = UrlEncode(uid);
                    else
                        nameInUrl = $"{UrlEncode(uname)},{UrlEncode(uid)}";
                    url = $"{url}?system={UrlEncode(system)}&maincode={UrlEncode(maincode)}&mainid={UrlEncode(mainid)}&subject={UrlEncode(title)}&name={nameInUrl}";
                }

                //执行通知发送
                foreach (NotifyExector varExec in exectors)
                {
                    try
                    {
                        varExec.ExecuteCore(uid, uname, url, system, maincode, mainid, title, creatorId, creatorName);
                        //Common.Logger.Info($"调用完成[{system}&{maincode}&{mainid}\\{uname},{uid}][{varExec.Name}|{varExec.Type}]");
                    }
                    catch (Exception ex)
                    {
                        execError = true;
                        //Common.Logger.Error($"调用出错[{system}&{maincode}&{mainid}\\{uname},{uid}][{varExec.Name}|{varExec.Type}]", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                execError = true;
                //Common.Logger.Error($"发送通知:出错[{system}&{maincode}&{mainid}\\{uname},{uid}][url:{url}]", ex);
            }
            finally
            {
                //if (needBreak)
                //    Common.Logger.Info($"发送通知:跳过-{breakNote}[{system}&{maincode}&{mainid}\\{uname},{uid}][url:{url}]");
                //else
                //    Common.Logger.Info($"发送通知:完成[{system}&{maincode}&{mainid}\\{uname},{uid}][url:{url}][creator:{creatorId}]");

                if (execError)
                {
                    lock (sNotifyTag)
                    {
                        sNotifyTag.Remove(key);
                    }
                }
            }
        }
        public static void SendNotify(User user, string subject, string url, string creatorId, string creatorName, bool checkOnline)
        {
            if (user != null)
            {
                SendNotify(user.ID, user.Name, user.System, user.MainCode, user.MainID, subject, url, creatorId, creatorName, checkOnline);
            }
        }
        
        static string UrlEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return System.Web.HttpUtility.UrlEncode(str, System.Text.Encoding.UTF8);
        }
    }
}