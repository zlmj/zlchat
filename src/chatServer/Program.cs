using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using chatBLL;
using chatDAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace chatserver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Init();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static void Init()
        {
            //log4net.Config.XmlConfigurator.Configure();
            Common.Logger.Info("******启动******");

            ConfigManager.Init();
            NotifyConfig sysConfig = ConfigManager.GetSysConfig();
            if (sysConfig == null || string.IsNullOrWhiteSpace(sysConfig.db_connstring))
                throw new Exception("系统数据库未配置");
            try
            {
                ManagerBase.DbAccess = SysDbAccessFactory.Create(sysConfig.db_type);
                ManagerBase.ConnString = Common.CustomDecode(sysConfig.db_connstring);
            }
            catch (Exception ex)
            {
                throw new Exception("初始化系统数据库访问组件出错", ex);
            }
            ManagerBase.ConnCheck();

            ////debug
            //NotifyBLL.SendNotify("u001", "张三", "zlhis", "危急值1", "0001", "XX的病历缺少主诉", "http://localhost:41704/webchat/index.html?system=zlhis&maincode=病历&mainid=0001&subject=XX的病历缺少主诉&name=张三,u001&join=李四,u002", "u001", "张三", false);
            ////-->

            try
            {
                UserDAL.Instance.UpdateAllOffline();
            }
            catch (Exception ex)
            {
                Common.Logger.Error("重置在线状态出错", ex);
            }
        }
    }
}
