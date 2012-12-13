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

        #region API Path

        // Return thumbnail if image exist and format supported
        public static string ApiThumb = "http://192.168.100.177:2121/thumb?p=";
        // Return true if file exist, otherwise return false
        public static string ApiCheck = "http://192.168.100.177:2121/check?p=";
        // Return files list if path exist
        public static string ApiDir = "http://192.168.100.177:2121/dir?p=";
        // Return file size if file exist
        public static string ApiGetSize = "http://192.168.100.177:2121/getsize?p=";
        // Return true if mkdir success
        public static string ApiMkdir = "http://192.168.100.177:2121/mkdir?p=";

        #endregion

        #region FTP Information

        public static string ComponentCode = "FTP287654321_04B9029AoH2F";
        public static string FtpHost = "192.168.100.177";
        public static string FtpUsername = "wftp";
        public static string FtpPasswrod = "engineer53007214";

        #endregion
    }
}
