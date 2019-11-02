using chatDAL;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace chatBLL
{

    internal class NotifyExectorContainer
    {
        private List<NotifyExector> m4Match;
        private List<NotifyExector> m4Dismatch;

        public NotifyExectorContainer()
        {
            m4Match = null;
            m4Dismatch = null;
        }
        public NotifyExectorContainer(List<NotifyExector> list)
        {
            m4Match = null;
            m4Dismatch = null;
            if (list != null && list.Count > 0)
            {
                foreach (NotifyExector item in list)
                {
                    Append(item);
                }
            }
        }

        public void Append(NotifyExector exec)
        {
            if (exec != null)
            {
                if (exec.IsDismatchFilter)
                {
                    if (m4Dismatch == null)
                        m4Dismatch = new List<NotifyExector>();
                    m4Dismatch.Add(exec);
                }
                else
                {
                    if (m4Match == null)
                        m4Match = new List<NotifyExector>();
                    m4Match.Add(exec);
                }
            }
        }

        public NotifyExector[] GetAllMatches(string maincode)
        {
            if (string.IsNullOrWhiteSpace(maincode))
                return null;

            string key = maincode.Trim().ToLower();
            List<NotifyExector> result = null;
            if (m4Match != null && m4Match.Count > 0)
            {
                List<NotifyExector> result1 =
                    m4Match.FindAll(o => string.IsNullOrWhiteSpace(o.Filter) || o.Filter.Contains($"&{key}&"));
                if (result1 != null && result1.Count > 0)
                    result = result1;
            }
            if (m4Dismatch != null && m4Dismatch.Count > 0)
            {
                List<NotifyExector> result2 =
                    m4Dismatch.FindAll(o => !o.Filter.Contains($"&{key}&"));
                if (result2 != null && result2.Count > 0)
                {
                    if (result != null)
                        result.AddRange(result2);
                    else
                        result = result2;
                }
            }
            return result?.ToArray();
        }
    }

    /// <summary>
    /// 通知执行器基类
    /// </summary>
    internal abstract class NotifyExector
    {
        internal string ID = Guid.NewGuid().ToString();
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Filter { get; private set; }
        public bool IsDismatchFilter { get; private set; }

        protected NotifyExector(NotifyConfigItem item)
        {
            UpdateSettingCore(item);
            Name = item?.name;
            Type = item?.type;
            Filter = item == null || string.IsNullOrWhiteSpace(item.filter) ? null : item.filter.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                if (Filter[0] == '!')
                {
                    IsDismatchFilter = true;
                    Filter = Filter.Substring(1);
                }
                else
                {
                    IsDismatchFilter = false;
                }

                if (!Filter.StartsWith("&"))
                    Filter = "&" + Filter;
                if (!Filter.EndsWith("&"))
                    Filter = Filter + "&";
            }
        }

        protected abstract void UpdateSettingCore(NotifyConfigItem item);

        internal bool GetAllMatch(string maincode)
        {


            if (Filter.Contains("[all]"))
                return true;
            if (Filter.Contains($"&{maincode.Trim().ToLower()}&"))
                return true;
            return false;
            //if (string.IsNullOrWhiteSpace)
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="uname"></param>
        /// <param name="url"></param>
        /// <param name="system"></param>
        /// <param name="maincode"></param>
        /// <param name="mainid"></param>
        /// <param name="subject"></param>
        /// <param name="creatorId"></param>
        /// <param name="creatorName"></param>
        /// <returns></returns>
        internal abstract bool ExecuteCore(string uid, string uname, string url, string system,
            string maincode, string mainid, string subject, string creatorId, string creatorName);
        
        internal static NotifyExector GetExector(NotifyConfigItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.type))
            {
                switch (item.type)
                {
                    case OracleDbProcNotifyExector.TypeString:
                        return new OracleDbProcNotifyExector(item);
                    case WebapiNotifyExector.TypeString:
                        return new WebapiNotifyExector(item);
                    default:
                        break;
                }
            }
            return null;
        }
        
        protected static bool ParseArgs4KeyValuePairs(string argsText, out Dictionary<string, string> dictArgs)
        {
            dictArgs = null;
            if (!string.IsNullOrWhiteSpace(argsText) && argsText.Length > 2 && argsText[2] == '|')
            {
                char separator1 = argsText[0];
                char separator2 = argsText[1];
                if (separator1 == separator2)
                {
                    return false;
                    //throw new ArgumentException("无效的分隔符:项目分隔符与值分隔符不能一样 ");
                }

                string[] argArray = argsText.Substring(3).Split(new char[] { separator1 }, StringSplitOptions.RemoveEmptyEntries);
                int index;
                string argName, argVal;
                dictArgs = new Dictionary<string, string>();
                foreach (string argItem in argArray)
                {
                    index = argItem.IndexOf(separator2);
                    if (index > 0)
                    {
                        argName = argItem.Substring(0, index);
                        if (!dictArgs.ContainsKey(argName))
                        {
                            argVal = argItem.Substring(index + 1);
                            dictArgs.Add(argName, argVal);
                        }
                    }
                }
                return true;
            }
            return false;
        }
        protected static string GetArgValue(string argValString, string uid, string uname, string url,
            string system, string maincode, string mainid, string subject, string creatorId, string creatorName)
        {
            if (!string.IsNullOrWhiteSpace(argValString))
            {
                List<string> list = new List<string>();
                if (ReplaceArgString(ref argValString, "user", list.Count))
                    list.Add(uid);
                if (ReplaceArgString(ref argValString, "uid", list.Count))
                    list.Add(uid);
                if (ReplaceArgString(ref argValString, "uname", list.Count))
                    list.Add(uname);
                if (ReplaceArgString(ref argValString, "url", list.Count))
                    list.Add(url);
                if (ReplaceArgString(ref argValString, "system", list.Count))
                    list.Add(system);
                if (ReplaceArgString(ref argValString, "maincode", list.Count))
                    list.Add(maincode);
                if (ReplaceArgString(ref argValString, "mainid", list.Count))
                    list.Add(mainid);
                if (ReplaceArgString(ref argValString, "subject", list.Count))
                    list.Add(subject);
                if (ReplaceArgString(ref argValString, "title", list.Count))
                    list.Add(subject);
                if (ReplaceArgString(ref argValString, "creatorId", list.Count))
                    list.Add(creatorId);
                if (ReplaceArgString(ref argValString, "creatorName", list.Count))
                    list.Add(creatorName);

                if (list.Count > 0)
                {
                    try
                    {
                        return string.Format(argValString, list.ToArray());
                    }
                    catch (Exception ex)
                    {
                        //Common.Logger.Warn($"参数串格式化错误：{ex.Message}\r\n{argValString}\r\n  {string.Join("\r\n  ", list.ToArray())}");
                        throw;
                    }
                }
            }
            return argValString;
        }
        static bool ReplaceArgString(ref string argValString, string itemName, int index)
        {
            string key = "{" + itemName + "}";
            if (argValString.Contains(key))
            {
                argValString = argValString.Replace(key, "{" + index.ToString() + "}");
                return true;
            }
            return false;
        }
    }


    internal class OracleDbProcNotifyExector : NotifyExector
    {
        internal const string TypeString = "oracle.proc";

        ODPDataAccess mDbAccess;
        string mProc;
        Dictionary<string, string> mArgs;

        internal OracleDbProcNotifyExector(NotifyConfigItem item) : base(item)
        {
        }

        protected override void UpdateSettingCore(NotifyConfigItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.db_connstring))
            {
                mDbAccess = null;
                mProc = null;
                mArgs = null;
            }
            else
            {
                mDbAccess = new ODPDataAccess(Common.CustomDecode(item.db_connstring));
                mProc = item.db_proc;
                if (!ParseArgs4KeyValuePairs(item.args, out mArgs))
                    mArgs = null;
            }
        }

        internal override bool ExecuteCore(string uid, string uname, string url, string system, string maincode, string mainid, 
            string subject, string creatorId, string creatorName)
        {
            ODPDataAccess dbAccess = mDbAccess;
            if (dbAccess != null && !string.IsNullOrWhiteSpace(mProc))
            {
                OracleParameter[] parms = null;
                Dictionary<string, string> dictArgs = mArgs;
                if (dictArgs != null && dictArgs.Count > 0)
                {
                    parms = new OracleParameter[dictArgs.Count];
                    int index = 0;
                    foreach (KeyValuePair<string, string> item in dictArgs)
                    {
                        parms[index] = new OracleParameter(item.Key, OracleDbType.Varchar2);
                        parms[index].Value = GetArgDbValue(item.Value, uid, uname, url, system,
                            maincode, mainid, subject, creatorId, creatorName);
                        index++;
                    }
                }                
                //parms = new OracleParameter[]
                //{
                //    new OracleParameter("user_in", OracleDbType.Varchar2),
                //    new OracleParameter("url_in", OracleDbType.Varchar2),
                //    new OracleParameter("system_in", OracleDbType.Varchar2),
                //    new OracleParameter("maincode_in", OracleDbType.Varchar2),
                //    new OracleParameter("mainid_in", OracleDbType.Varchar2),
                //    new OracleParameter("subject_in", OracleDbType.Varchar2),
                //    new OracleParameter("senduser_in", OracleDbType.Varchar2),
                //};
                //parms[0].Value = user;
                //parms[1].Value = url;
                //parms[2].Value = system;
                //parms[3].Value = maincode;
                //parms[4].Value = mainid;
                //parms[5].Value = title;
                //if (string.IsNullOrWhiteSpace(createUser))
                //    parms[6].Value = DBNull.Value;
                //else
                //    parms[6].Value = createUser;

                try
                { 
                    dbAccess.ExecuteNonQuery(mProc, parms, System.Data.CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    Common.Logger.Warn($"调用存储过程出错:{ex.Message}[参数个数:{parms.Length}]");
                    throw;
                }
                return true;
            }
            return false;

            #region Proc.ZLHIS
            /*
Create Or Replace Procedure Zltools.Zldiscussiongroup_Update
(
  Sysname_In    In Zltools.Zldiscussiongroup. Sysname%Type,
  Objecttype_In In Zltools.Zldiscussiongroup. Objecttype%Type,
  Objectid_In   In Zltools.Zldiscussiongroup. Objectid%Type,
  User_In       In Zltools.Zldiscussiongroup.Username%Type,
  Tilename_In   In Zltools.Zldiscussiongroup.Tilename%Type := Null,
  Url_In        In Zltools.Zldiscussiongroup.Tilename%Type := Null,
  Senduser_In   In Zltools.Zldiscussiongroup.Senduser%Type := Null
) Is
            */
            #endregion
        }

        object GetArgDbValue(string argValString, string uid, string uname, string url, string system, string maincode, 
            string mainid, string subject, string creatorId, string creatorName)
        {
            string strVal = GetArgValue(argValString, uid, uname, url, system, maincode, mainid, subject, creatorId, creatorName);
            if (string.IsNullOrWhiteSpace(strVal))
                return DBNull.Value;
            return strVal;
        }
    }

    internal class WebapiNotifyExector : NotifyExector
    {
        internal const string TypeString = "webapi";

        string mUrl, mMethod, mTaskTypeId, mUser, mPwd, mArgsTemplate;
        Dictionary<string, string> mArgs;

        internal WebapiNotifyExector(NotifyConfigItem item) : base(item)
        {
        }

        protected override void UpdateSettingCore(NotifyConfigItem item)
        {
            mUser = null;
            mPwd = null;
            if (item == null || string.IsNullOrWhiteSpace(item.webapi_url))
            {
                mUrl = null;
                mMethod = null;
                mTaskTypeId = null;
                mUser = null;
                mPwd = null;
                mArgs = null;
                mArgsTemplate = null;
            }
            else
            {
                mUrl = item.webapi_url;
                mMethod = item.webapi_method;
                if (string.IsNullOrWhiteSpace(mMethod))
                {
                    mMethod = "post";
                }
                mTaskTypeId = item.webapi_bh_tasktypeid;
                if (ParseArgs4KeyValuePairs(item.args, out mArgs))
                {
                    mArgsTemplate = null;
                }
                else
                {
                    mArgs = null;
                    mArgsTemplate = item.args;
                }

                string userAndPwd = item.webapi_auth;
                if (!string.IsNullOrWhiteSpace(userAndPwd))
                {
                    string[] strArray = userAndPwd.Trim().Split(":".ToCharArray());
                    mUser = Common.CustomDecode(strArray[0]);
                    if (strArray.Length > 1)
                    {
                        mPwd = Common.CustomDecode(strArray[1]);
                    }
                }
            }
        }

        internal override bool ExecuteCore(string uid, string uname, string url, string system, string maincode,
            string mainid, string subject, string creatorId, string creatorName)
        {
            if (!string.IsNullOrWhiteSpace(mUrl))
            {
                string realUrl = GetArgValue(mUrl, uid, uname, url, system, maincode, mainid, subject, creatorId, creatorName);

                string postData = null;
                if (mArgs != null && mArgs.Count > 0)
                {
                    Dictionary<string, string> dictTemp = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, string> item in mArgs)
                    {
                        dictTemp[item.Key] = GetArgValue(item.Value, uid, uname, url, system, maincode, mainid, subject, creatorId, creatorName);
                    }
                    postData = Newtonsoft.Json.JsonConvert.SerializeObject(dictTemp);
                }
                else if (!string.IsNullOrWhiteSpace(mArgsTemplate))
                {
                    postData = GetArgValue(mArgsTemplate, uid, uname, url, system, maincode, mainid, subject, creatorId, creatorName);
                }

                try
                {
                    CallWebAPI(realUrl, mMethod, null, postData, mUser, mPwd);
                }
                catch (Exception ex)
                {
                    Common.Logger.Warn($"调用WebAPI出错:{ex.Message}\r\n  {realUrl} | {mMethod} | {mUser},{mPwd}\r\n  {postData}");
                    throw;
                }
                return true;
            }
            return false;
        }

        public static string CallWebAPI(string url, string requestMethod, string contentType, string postData = "", string userName = "", string password = "")
        {
            if (string.IsNullOrWhiteSpace(contentType))
                contentType = "text/plain";

            HttpResponseMessage response = null;
            HttpContent content = null;
            switch (requestMethod.ToLower())
            {
                case "post":
                case "put":
                    content = new StringContent(postData, Encoding.UTF8, contentType);
                    break;
            }

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(requestMethod), url);
            request.Content = content;

            HttpClient client = new HttpClient();
            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
            {
                AuthenticationHeaderValue authenticationHeaderValue = null;
                authenticationHeaderValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + password)));
                client.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            }

            var task = client.SendAsync(request);

            response = task.Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                var strError = responseContent;
                if (string.IsNullOrWhiteSpace(responseContent))
                    response.EnsureSuccessStatusCode();
                throw new Exception(responseContent);
            }

            return responseContent;

        }
        private HttpClient CallWebAPIInitialize(string url, string user, string pwd, int requestTimeout = 3000, bool useProxy = false, string proxy = "")
        {
            var client = GetClient(useProxy, proxy);
            client.Timeout = TimeSpan.FromSeconds(requestTimeout);
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + pwd)));
            return client;
        }
        private HttpClient GetClient(bool useProxy = false, string proxy = "")
        {
            if (useProxy)
            {
                var handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxy),
                    UseProxy = true
                };
                var client = new HttpClient(handler);
                return client;
            }
            else
            {
                return new HttpClient();
            }
        }
    }
}