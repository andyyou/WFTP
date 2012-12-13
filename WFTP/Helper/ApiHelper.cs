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
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format("{0}{1}", GlobalHelper.ApiDir, path));
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
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format("{0}{1}", GlobalHelper.ApiGetSize, path));
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
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format("{0}{1}", GlobalHelper.ApiCheck, path));
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
