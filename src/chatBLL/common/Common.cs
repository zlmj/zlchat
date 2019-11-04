using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace chatBLL
{
    public class Common
    {
        public const string Const_Space = " ";

        private static ILog _logger;
        /// <summary>
        /// 日志
        /// </summary>
        public static ILog Logger
        {
            get
            {
                return _logger;
            }
        }

        static Common()
        {
            try
            {
                if (_logger == null)
                {
                    var repository = LogManager.CreateRepository("NETCoreRepository");
                    //log4net从log4net.config文件中读取配置信息
                    XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
                    _logger = LogManager.GetLogger(repository.Name, "InfoLogger");
                }
            }
            catch
            {
                throw;
            }
        }


        public static string GetTimeText()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string ReadString(Stream stream, Encoding encoding)
        {
            if (stream != null)
            {
                using (StreamReader sr = new StreamReader(stream, encoding))
                {
                    return sr.ReadToEnd();
                }
            }
            return string.Empty;
        }

        public static string CustomDecode(string text)
        {
            if (text != null && text.Length > 4 && text[0] == '&')
            {
                string strTmp = text.Substring(1);
                strTmp = strTmp.Substring(strTmp.Length - 2, 2) + strTmp.Substring(3, strTmp.Length - 6) + strTmp.Substring(0, 2);
                //strTmp = strTmp.Substring(strTmp.Length - 3, 2) + (char)((int)strTmp[strTmp.Length - 1] + 1) + strTmp.Substring(0, strTmp.Length - 3);
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(strTmp));
                }
                catch { }
            }
            return text;
        }
        public static string CustomEncode(string text)
        {
            if (text != null && text.Length > 0)
            {
                string str = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
                if (str.Length > 4)
                {
                    char randomChar = GetRandomChar();
                    str = "&" + str.Substring(str.Length - 2, 2) + randomChar + str.Substring(2, str.Length - 4) + randomChar + str.Substring(0, 2);
                    //str = "&" + str.Substring(3) + str.Substring(0, 2) + (char)((int)str[2] - 1);
                }
                return str;
            }
            return text;
        }

        static char[] sCharArray = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        public static char GetRandomChar()
        {
            Random r = new Random();
            return sCharArray[r.Next(0, sCharArray.Length)];
        }

        /// <summary>
        /// 获取运行目录与路径名组合全路径
        /// </summary>
        /// <param name="strPathName"></param>
        /// <returns></returns>
        public static string GetCombineBaseDirectoryAndPath(string strPathName)
        {
            return Path.Combine(GetBaseDirectory(), strPathName);
        }

        public static string GetBaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }


        internal class ChatLogic
        {
            public static void CheckArgsOfChat(string system, string maincode, string mainid)
            {
                if (string.IsNullOrWhiteSpace(system))
                    throw new ArgumentException("system值不允许空");
                if (string.IsNullOrWhiteSpace(maincode))
                    throw new ArgumentException("maincodem值不允许空");
                if (string.IsNullOrWhiteSpace(mainid))
                    throw new ArgumentException("mainid值不允许空");
            }
            public static void CheckArgsOfUser(string uid, string uname)
            {
                if (string.IsNullOrWhiteSpace(uid))
                    throw new ArgumentException("uid");
            }
        }
    }
}