using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace chatBLL
{
    internal class ConfigFileMonitor
    {
        public delegate void FileChangeEventHandler(string strOldName, string strName);
        public static event FileChangeEventHandler FileChange;

        public static void Init()
        {
            //文件监控
            var strPath = Common.GetCombineBaseDirectoryAndPath("webchat");
            Directory.CreateDirectory(strPath);
            var fileSystemWatcher = new System.IO.FileSystemWatcher(strPath, "*.json");
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
            fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private static void FileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                CallFileChange(null, e.Name);
            }
            catch (Exception ex)
            {
                Common.Logger.Error("监视文件改变出错。", ex);
            }
        }

        private static void FileSystemWatcher_Renamed(object sender, System.IO.RenamedEventArgs e)
        {
            try
            {
                CallFileChange(e.OldName, e.Name);
            }

            catch (Exception ex)
            {
                Common.Logger.Error("监视文件改变出错。", ex);
            }
        }

        private static void FileSystemWatcher_Deleted(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                CallFileChange(null, e.Name);
            }
            catch (Exception ex)
            {
                Common.Logger.Error("监视文件改变出错。", ex);
            }
        }

        private static void FileSystemWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                CallFileChange(null, e.Name);
            }
            catch (Exception ex)
            {
                Common.Logger.Error("监视文件改变出错。", ex);
            }
        }

        private static void CallFileChange(string strOldName, string strName)
        {
            if (FileChange != null)
            {
                new TaskFactory().StartNew(() =>
                {
                    Thread.Sleep(1000);
                    try
                    {
                        FileChange(strOldName, strName);
                    }
                    catch (Exception ex)
                    {
                        Common.Logger.Error("监视文件改变出错。", ex);
                    }
                });
            }
        }
    }
}