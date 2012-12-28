using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFTP.Pages;
using DataProvider;
using System.Windows;

namespace WFTP.Helper
{
    public static class GlobalHelper
    {
        // The property is for binding use.
        public static AdminItem AdminItem { set; get; }
        public static string LoginUserID { set; get; }

        //
        public static string ProgressList = AppDomain.CurrentDomain.BaseDirectory + "progress.json";
        public static string TempDownloadFileExt = ".wftpdl";
        public static string TempUploadFileExt = ".wftpul";

        #region API Path

        // API Key, prevent unauthorized connection
        public static string ApiKey { get; set; }
        // API Host and Port
        public static string ApiHost { get; set; }
        public static int ApiPort { get; set; }
        // Return thumbnail if image exist and format supported
        public static string ApiThumb { get; set; }
        // Return true if file exist, otherwise return false
        public static string ApiCheck { get; set; }
        // Return true if file/path can be rename, otherwise return false
        public static string ApiCheckRename { get; set; }
        // Return files list if path exist
        public static string ApiDir { get; set; }
        // Return file size if file exist
        public static string ApiGetSize { get; set; }
        // Return true if mkdir success
        public static string ApiMkdir { get; set; }
        // Return true if rmdir success(the file will be move to Trash folder)
        public static string ApiRmdir { get; set; }
        // Return true if rename success(if new file already exist, return false)
        public static string ApiRename { get; set; }
        // Return true if delete file success(the file will be delete permanently)
        public static string ApiDeleteFile { get; set; }
        // Return true if lv5 single category create success
        public static string ApiAddCategorys { get; set; }
        // Return true if lv5 category create success
        public static string ApiCreateCategorys { get; set; }
        // Return true if lv5 category rename success
        public static string ApiRenameCategorys { get; set; }
        // Return true if lv5 catefory remove success
        public static string ApiRemoveCategorys { get; set; }
        // Return folder/file count(return 0 if there is no file or path not exist)
        public static string ApiGetCount { get; set; }

        #endregion

        #region FTP Information

        public static string ComponentCode { get; set; }
        public static string FtpHost { get; set; }
        public static string FtpUsername { get; set; }
        public static string FtpPasswrod { get; set; }

        #endregion

        public static void SetApiPath()
        {
            ApiThumb = String.Format("http://{0}:{1}/thumb?key={2}&p={3}", ApiHost, ApiPort, ApiKey, "{0}");
            ApiCheck = String.Format("http://{0}:{1}/check?key={2}&p={3}", ApiHost, ApiPort, ApiKey, "{0}");
            ApiCheckRename = String.Format("http://{0}:{1}/checkrename?key={2}&p={3}&n={4}", ApiHost, ApiPort, ApiKey, "{0}", "{1}");
            ApiDir = String.Format("http://{0}:{1}/dir?key={2}&p={3}", ApiHost, ApiPort, ApiKey, "{0}");
            ApiGetSize = String.Format("http://{0}:{1}/getsize?key={2}&p={3}", ApiHost, ApiPort, ApiKey, "{0}");
            ApiMkdir = String.Format("http://{0}:{1}/mkdir?key={2}&p={3}", ApiHost, ApiPort, ApiKey, "{0}");
            ApiRmdir = String.Format("http://{0}:{1}/rmdir?key={2}&p={3}", ApiHost, ApiPort, ApiKey, "{0}");
            ApiRename = String.Format("http://{0}:{1}/rename?key={2}&p={3}&n={4}", ApiHost, ApiPort, ApiKey, "{0}", "{1}");
            ApiDeleteFile = String.Format("http://{0}:{1}/deletefile?key={2}&p={3}", ApiHost, ApiPort, ApiKey, "{0}");
            ApiAddCategorys = String.Format("http://{0}:{1}/addcategory?key={2}&n={3}", ApiHost, ApiPort, ApiKey, "{0}");
            ApiCreateCategorys = String.Format("http://{0}:{1}/createcategorys?key={2}&p={3}", ApiHost, ApiPort, ApiKey, "{0}");
            ApiRenameCategorys = String.Format("http://{0}:{1}/renamecategorys?key={2}&n={3}&nn={4}", ApiHost, ApiPort, ApiKey, "{0}", "{1}");
            ApiRemoveCategorys = String.Format("http://{0}:{1}/removecategorys?key={2}&n={3}", ApiHost, ApiPort, ApiKey, "{0}");
            ApiGetCount = String.Format("http://{0}:{1}/getfoldercount?key={2}&p={3}", ApiHost, ApiPort, ApiKey, "{0}");
        }
        /// <summary>
        /// 需登入後才能呼叫 管理權限只需動這邊和Loggin
        /// </summary>
        public static void RefreshLogginUser()
        {
            WFTPDbContext db = new WFTPDbContext();
            var logger = (from user in db.GetTable<CEmployee>()
                          where user.Account == Properties.Settings.Default.Id && user.Password == Properties.Settings.Default.Pwd
                          select user).SingleOrDefault();
            int rank = Convert.ToInt32(logger.Rank);
            GlobalHelper.AdminItem.Activity = logger.Activity;
            GlobalHelper.AdminItem.IsAdmin = (rank > 5) ? true : false; // 管理權限定義
            GlobalHelper.AdminItem.Rank = rank;
           
        }

    }
}
