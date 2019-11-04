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
            Common.Logger.Info("******����******");

            ConfigManager.Init();
            NotifyConfig sysConfig = ConfigManager.GetSysConfig();
            if (sysConfig == null || string.IsNullOrWhiteSpace(sysConfig.db_connstring))
                throw new Exception("ϵͳ���ݿ�δ����");
            try
            {
                ManagerBase.DbAccess = SysDbAccessFactory.Create(sysConfig.db_type);
                ManagerBase.ConnString = Common.CustomDecode(sysConfig.db_connstring);
            }
            catch (Exception ex)
            {
                throw new Exception("��ʼ��ϵͳ���ݿ�����������", ex);
            }
            ManagerBase.ConnCheck();

            ////debug
            //NotifyBLL.SendNotify("u001", "����", "zlhis", "Σ��ֵ1", "0001", "XX�Ĳ���ȱ������", "http://localhost:41704/webchat/index.html?system=zlhis&maincode=����&mainid=0001&subject=XX�Ĳ���ȱ������&name=����,u001&join=����,u002", "u001", "����", false);
            ////-->

            try
            {
                UserDAL.Instance.UpdateAllOffline();
            }
            catch (Exception ex)
            {
                Common.Logger.Error("��������״̬����", ex);
            }
        }
    }
}
