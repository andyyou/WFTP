using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace WFTP.Helper
{
    class ApiHelper
    {
        private Encoding encoding = Encoding.UTF8;

        /// <summary>
        /// 取得遠端路徑檔案清單
        /// </summary>
        /// <param name="path">遠端路徑</param>
        /// <returns>檔案清單</returns>
        public string[] Dir(string path)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiDir, path));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                return reader.ReadToEnd().Split(',');
            }
        }

        /// <summary>
        /// 取得遠端檔案大小
        /// </summary>
        /// <param name="path">遠端路徑</param>
        /// <returns>檔案大小</returns>
        public long GetFileSize(string path)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiGetSize, path));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                return Convert.ToInt64(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// 確認指定路徑是否存在(檔案及目錄皆可)
        /// </summary>
        /// <param name="path">遠端路徑</param>
        /// <returns>路徑是否存在</returns>
        public bool CheckPath(string path)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiCheck, path));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                string result = reader.ReadToEnd();

                if (result.Equals("true"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 建立遠端目錄
        /// </summary>
        /// <param name="path">欲建立的遠端路徑</param>
        /// <returns>是否成功建立</returns>
        public bool CreateDirectory(string path)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiMkdir, path));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                string result = reader.ReadToEnd();

                if (result.Equals("true"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 刪除指定目錄(遠端伺服器會將檔案搬移至指定暫存位置)
        /// </summary>
        /// <param name="path">欲刪除的遠端路徑</param>
        /// <returns></returns>
        public bool RemoveDirectory(string path)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiRmdir, path));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                string result = reader.ReadToEnd();

                if (result.Equals("true"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 重新命名
        /// </summary>
        /// <param name="path">舊名稱(完整路徑)</param>
        /// <param name="newName">新名稱(僅檔案名稱)</param>
        /// <returns>重新命名是否成功</returns>
        public bool Rename(string path, string newName)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiRename, path, newName));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                string result = reader.ReadToEnd();

                if (result.Equals("true"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 刪除檔案
        /// </summary>
        /// <param name="path">遠端檔案路徑</param>
        /// <returns>檔案是否刪除成功</returns>
        public bool DeleteFile(string path)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiDeleteFile, path));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                string result = reader.ReadToEnd();

                if (result.Equals("true"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
