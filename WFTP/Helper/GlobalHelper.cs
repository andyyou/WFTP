using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFTP.Pages;

namespace WFTP.Helper
{
    public static class GlobalHelper
    {
        // The property is for binding use.
        public static AdminItem AdminItem { set; get; }
        public static string LoginUserID { set; get; }

        //
        public static string ProgressList = "progress.json";
        public static string TempDownloadFileExt = ".wftpdl";
        public static string TempUploadFileExt = ".wftpul";

        #region API Path

        // API Key, prevent unauthorized connection
        private static string ApiKey = "15eb7a42cce1ab9822caa1f8aaa65a494d38d19654886e67c0a6b15edcdcfde7";
        // Return thumbnail if image exist and format supported
        public static string ApiThumb = String.Format("http://192.168.100.177:2121/thumb?key={0}&p={1}", ApiKey, "{0}");
        // Return true if file exist, otherwise return false
        public static string ApiCheck = String.Format("http://192.168.100.177:2121/check?key={0}&p={1}", ApiKey, "{0}");
        // Return true if file/path can be rename, otherwise return false
        public static string ApiCheckRename = String.Format("http://192.168.100.177:2121/checkrename?key={0}&p={1}&n={2}", ApiKey, "{0}", "{1}");
        // Return files list if path exist
        public static string ApiDir = String.Format("http://192.168.100.177:2121/dir?key={0}&p={1}", ApiKey, "{0}");
        // Return file size if file exist
        public static string ApiGetSize = String.Format("http://192.168.100.177:2121/getsize?key={0}&p={1}", ApiKey, "{0}");
        // Return true if mkdir success
        public static string ApiMkdir = String.Format("http://192.168.100.177:2121/mkdir?key={0}&p={1}", ApiKey, "{0}");
        // Return true if rmdir success(the file will be move to Trash folder)
        public static string ApiRmdir = String.Format("http://192.168.100.177:2121/rmdir?key={0}&p={1}", ApiKey, "{0}");
        // Return true if rename success(if new file already exist, return false)
        public static string ApiRename = String.Format("http://192.168.100.177:2121/rename?key={0}&p={1}&n={2}", ApiKey, "{0}", "{1}");
        // Return true if delete file success(the file will be delete permanently)
        public static string ApiDeleteFile = String.Format("http://192.168.100.177:2121/deletefile?key={0}&p={1}", ApiKey, "{0}");
        // Return true if lv5 single category create success
        public static string ApiAddCategorys = String.Format("http://192.168.100.177:2121/addcategory?key={0}&n={1}", ApiKey, "{0}");
        // Return true if lv5 category create success
        public static string ApiCreateCategorys = String.Format("http://192.168.100.177:2121/createcategorys?key={0}&p={1}", ApiKey, "{0}");
        // Return true if lv5 category rename success
        public static string ApiRenameCategorys = String.Format("http://192.168.100.177:2121/renamecategorys?key={0}&n={1}&nn={2}", ApiKey, "{0}", "{1}");
        // Return true if lv5 catefory remove success
        public static string ApiRemoveCategorys = String.Format("http://192.168.100.177:2121/removecategorys?key={0}&n={1}", ApiKey, "{0}");

        #endregion

        #region FTP Information

        public static string ComponentCode = "FTP287654321_04B9029AoH2F";
        public static string FtpHost = "192.168.100.177";
        public static string FtpUsername = "wftp";
        public static string FtpPasswrod = "engineer53007214";

        #endregion
    }
}
