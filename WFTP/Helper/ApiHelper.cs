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

        public bool Rename(string path, string newPath)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format(GlobalHelper.ApiRename, path, newPath));
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
