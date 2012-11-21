using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WFTP.Helper
{
    public class ExtensionHelper
    {
        static string icon_folder = @"pack://application:,,,/WFTP;component/Icons/";
        Dictionary<string, string> extList;

        public ExtensionHelper()
        {
            extList = new Dictionary<string,string>();

            // Image
            extList.Add("bmp", "img.ico");
            extList.Add("jpg", "img.ico");
            extList.Add("jpeg", "img.ico");
            extList.Add("png", "img.ico");
            extList.Add("gif", "img.ico");
            extList.Add("tiff", "img.ico");
            // document
            extList.Add("doc", "doc.ico");
            extList.Add("docx", "doc.ico");
            extList.Add("xls", "xls.ico");
            extList.Add("xlsx", "xls.ico");
            extList.Add("ppt", "ppt.ico");
            extList.Add("pptx", "ppt.ico");
            extList.Add("pdf", "pdf.ico");
            extList.Add("txt", "txt.ico");
            // folder
            extList.Add("folder", "folder.ico");
            // unknown
            extList.Add("unknown", "unknown.ico");
        }

        public string GetIconPath(string extension)
        {
            if (extList.ContainsKey(extension))
            {
                return String.Format("{0}{1}", icon_folder, extList[extension]);
            }
            else
            {
                return String.Format("{0}{1}", icon_folder, extList["unknown"]);
            }
        }
    }
}
