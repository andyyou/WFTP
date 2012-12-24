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
        /// <param name="nameOnly">是否僅回傳名稱</param>
        /// <returns>檔案清單</returns>
        public string[] Dir(string path, bool nameOnly = false)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiDir, path));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                if (nameOnly)
                {
                    return reader.ReadToEnd().Replace(path, String.Empty).Split(',');
                }
                else
                {
                    return reader.ReadToEnd().Split(',');
                }
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
        /// 確認指定路徑是否可重新命名(檔案及目錄皆可)
        /// </summary>
        /// <param name="path">舊名稱(完整路徑)</param>
        /// <param name="newName">新名稱(僅檔名或目錄名稱)</param>
        /// <returns></returns>
        public bool CheckRenamePath(string path, string newName)
        {
            if (path.Substring(path.LastIndexOf('/') + 1) == newName)
            {
                return true;
            }
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiCheckRename, path, newName));
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
            if (path.Substring(path.LastIndexOf('/') + 1) == newName)
            {
                return true;
            }
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

        /// <summary>
        /// 單一建立第五層分類目錄
        /// </summary>
        /// <param name="categoryName">要建立的分類目錄名稱</param>
        /// <returns>分類目錄是否建立成功</returns>
        public bool AddCategorys(string categoryName)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiAddCategorys, categoryName));
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
        /// 完整建立第五層分類目錄
        /// </summary>
        /// <param name="path">要建立分類目錄的父目錄路徑(第四層分類路徑)</param>
        /// <returns>分類目錄是否建立成功</returns>
        public bool CreateCategorys(string path)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiCreateCategorys, path));
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
        /// 重新命名第五層分類目錄
        /// </summary>
        /// <param name="oldCategoryName">舊分類名稱(僅名稱)</param>
        /// <param name="newCategoryName">新分類名稱(僅名稱)</param>
        /// <returns>回傳重新命名的資料筆數, 若新舊名稱相同回傳 0, 新名稱已存在無法更改回傳 -1</returns>
        public int RenameCategorys(string oldCategoryName, string newCategoryName)
        {
            if (oldCategoryName == newCategoryName)
            {
                return 0;
            }
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiRenameCategorys, oldCategoryName, newCategoryName));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                return Convert.ToInt32(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// 移除第五層分類目錄
        /// </summary>
        /// <param name="categoryName">要移除的分類目錄名稱</param>
        /// <returns>移除的分類目錄筆數</returns>
        public int RemoveCategorys(string categoryName)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiRemoveCategorys, categoryName));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                return Convert.ToInt32(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// 取得指定目錄之子目錄/檔案數量
        /// </summary>
        /// <param name="path">要計算子目錄/檔案數量路徑</param>
        /// <returns>實際存在資料庫及 FTP 之數量</returns>
        public int GetCount(string path)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiGetCount, path));
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                Stream responseStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream, encoding);

                return Convert.ToInt32(reader.ReadToEnd());
            }
        }
    }
}
