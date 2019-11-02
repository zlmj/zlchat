using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace chatModel
{
    public class ChatRoom
    {
        public string SystemID { get; set; }

        public string MainCode { get; set; }

        public string MainID { get; set; }

        public string Subject { get; set; }

        public string LastUname { get; set; }
        public string LastUid { get; set; }
        public int Unread { get; set; }

        public string Uname { get; set; }
        public string LastTime { get; set; }

        public string CreateTime { get; set; }

        public string CreatorID { get; set; }

        public string Creator { get; set; }
        /// <summary>
        /// 最后消息记录
        /// </summary>
        public string LastMsg { get; set; }
    }
}