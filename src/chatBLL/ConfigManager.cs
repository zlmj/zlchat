using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace chatBLL
{
    internal class ConfigManager
    {
        private const string Const_SysConfig = "_sys_";
        private const string Const_ConfigFileName = "notifyConfig.json";
        private static string sConfigFileFullName = Path.Combine(Common.GetBaseDirectory(), "webchat", Const_ConfigFileName);

        public static Dictionary<string, NotifyConfig> sConfigs;
        public static Dictionary<string, NotifyExectorContainer> sExectors;
        private static object sLock = new object();


        public static void Init()
        {
            try
            {
                ConfigFileMonitor.FileChange += ConfigFileMonitor_FileChange;
                UpdateConfig();
            }
            catch (Exception ex)
            {
                Common.Logger.Error("初始化配置文件监听器出错", ex);
            }

            StringBuilder str = new StringBuilder();
            foreach (NotifyConfig item in sConfigs.Values)
            {
                if (item.code == Const_SysConfig)
                    continue;
                if (item.items != null && item.items.Length > 0)
                {
                    if (item.items.Length == 1)
                    {
                        str.AppendFormat("[{0}:{1}]", item.code, item.items[0].type);
                    }
                    else
                    {
                        str.AppendFormat("[{0}:", item.code);
                        for (int i = 0; i < item.items.Length; i++)
                        {
                            if (item.items[i] != null)
                                str.AppendFormat("{0},", item.items[i].type);
                        }
                        str.Remove(str.Length - 1, 1);
                        str.Append("]");
                    }
                }
                else
                {
                    str.Append($"[{item.code}:<空>]");
                }
            }
            Common.Logger.Info($"启动日志-以配置的消息通知:{str.ToString()}");
        }

        public static NotifyConfig GetSysConfig()
        {
            return GetConfig(Const_SysConfig);
        }

        public static NotifyConfig GetConfig(string system)
        {
            if (sConfigs != null && !string.IsNullOrWhiteSpace(system))
            {
                string key = system.Trim().ToLower();
                lock (sLock)
                {
                    NotifyConfig val;
                    if (sConfigs != null && sConfigs.TryGetValue(key, out val))
                        return val;
                }
            }
            return null;
        }

        public static NotifyExector[] GetExectors(string system, string maincode)
        {
            if (sExectors != null && !string.IsNullOrWhiteSpace(system))
            {
                string key = system.Trim().ToLower();
                lock (sLock)
                {
                    NotifyExectorContainer val;
                    if (sExectors != null && sExectors.TryGetValue(key, out val))
                        return val.GetAllMatches(maincode);
                }
            }
            return null;
        }

        private static void ConfigFileMonitor_FileChange(string strOldName, string strName)
        {
            if (string.Equals(strName, Const_ConfigFileName, StringComparison.OrdinalIgnoreCase) || string.Equals(strOldName, Const_ConfigFileName, StringComparison.OrdinalIgnoreCase))
                UpdateConfig();
        }

        private static void UpdateConfig()
        {
            NotifyConfigArray array = ReadConfigInfo();
            if (array != null && array.configs != null && array.configs.Length > 0)
            {
                Dictionary<string, NotifyConfig> configs = new Dictionary<string, NotifyConfig>();
                Dictionary<string, NotifyExectorContainer> exectors = new Dictionary<string, NotifyExectorContainer>();
                foreach (NotifyConfig cfg in array.configs)
                {
                    if (string.IsNullOrWhiteSpace(cfg.code))
                        continue;

                    //将编码关键字全部处理为小写，方便后续做匹配查询
                    cfg.code = cfg.code.Trim().ToLower();
                    if (cfg.code == Const_SysConfig)
                    {
                        //todo:有效性检查
                    }
                    else
                    {
                        if (cfg.items == null || cfg.items.Length == 0)
                            continue;
                        //todo:有效性检查

                        List<NotifyExector> list = new List<NotifyExector>();
                        foreach (NotifyConfigItem cfgItem in cfg.items)
                        {
                            cfgItem.type = cfgItem.type.Trim().ToLower();

                            NotifyExector exec = NotifyExector.GetExector(cfgItem);
                            if (exec != null) list.Add(exec);
                        }
                        exectors[cfg.code] = list == null ? null : new NotifyExectorContainer(list);
                    }
                    configs[cfg.code] = cfg;
                }


                lock (sLock)
                {
                    sConfigs = configs;
                    sExectors = exectors;
                }
            }
            else
            {
                sConfigs = null;
                sExectors = null;
            }
        }
        private static NotifyConfigArray ReadConfigInfo()
        {
            try
            {
                if (!File.Exists(sConfigFileFullName))
                    return null;

                var strContent = string.Empty;
                using (var stream = File.Open(sConfigFileFullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        strContent = reader.ReadToEnd();
                        reader.Close();
                    }
                }
                var list = JsonConvert.DeserializeObject<NotifyConfigArray>(strContent);
                if (list == null || list.configs == null || list.configs.Length == 0)
                    return null;

                return list;
            }
            catch (Exception ex)
            {
                Common.Logger.Error(string.Format("获取推送配置失败。【{0}】", sConfigFileFullName), ex);
                return null;
            }
        }
    }
}