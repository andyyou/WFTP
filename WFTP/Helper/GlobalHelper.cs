using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WFTP.Lib;
using WFTP.Pages;

namespace WFTP.Helper
{
    public static class GlobalHelper
    {
        // The property is for binding use.
        public static AdminItem AdminItem { set; get; }
        public static string LoginUserID { set; get; }

        #region API Path

        // Return thumbnail if image exist and format supported
        public static string ApiThumb = "http://192.168.100.177:2121/thumb?p=";
        // Return true if file exist, otherwise return false
        public static string ApiCheck = "http://192.168.100.177:2121/check?p=";
        // Return files list if path exist
        public static string ApiDir = "http://192.168.100.177:2121/dir?p=";
        // Return file size if file exist
        public static string ApiGetSize = "http://192.168.100.177:2121/getsize?p=";

        #endregion
    }
}
