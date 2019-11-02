using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace chatBLL
{
    internal class NotifyConfigArray
    {
        public NotifyConfig[] configs { get; set; }
    }
    /// <summary>
    /// 配置信息
    /// </summary>
    internal class NotifyConfig
    {
        public string code { get; set; }
        public string db_type { get; set; }
        public string db_connstring { get; set; }

        public NotifyConfigItem[] items { get; set; }
    }
    /// <summary>
    /// 配置信息
    /// </summary>
    internal class NotifyConfigItem
    {
        public string name { get; set; }
        public string type { get; set; } //oracle.proc,webapi
        public string filter { get; set; }

        public string args { get; set; } //,:user_in:{user},url_in:{url},system_in:{system},maincode_in:{maincode},mainid_in:{mainid},subject_in:{subject},senduser_in:{senduser}

        public string db_connstring { get; set; }
        public string db_proc { get; set; } //todo:可固定

        public string webapi_url { get; set; }
        public string webapi_method { get; set; } //post,get
        public string webapi_auth { get; set; }
        public string webapi_bh_tasktypeid { get; set; }
    }
}