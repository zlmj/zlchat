using System;
using System.Collections.Generic;
using System.Text;

namespace ChatModel
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum ChatType
    {
        /// <summary>
        /// 公聊
        /// </summary>
        PubChat = 0,
        /// <summary>
        //私聊
        /// </summary>
        PriChat = 1,
        /// <summary>
        /// 群聊
        /// </summary>
        GroChat = 2
    }
}
