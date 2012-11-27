using System;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;

namespace WFTP.Lib
{
    /// <summary>
    /// FTPClient 的摘要說明。
    /// </summary>
    public class FTPClient
    {
        #region 建構子

        /// <summary>
        /// 預設建構子
        /// </summary>
        public FTPClient()
        {
            strRemoteHost = "192.168.100.177";
            strRemotePath = "/";
            strRemoteUser = "wftp";
            strRemotePass = "engineer53007214";
            strRemotePort = 21;
            bConnected = false;
        }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="remoteHost"></param>
        /// <param name="remotePath"></param>
        /// <param name="remoteUser"></param>
        /// <param name="remotePass"></param>
        /// <param name="remotePort"></param>
        public FTPClient(string remoteHost, string remotePath, string remoteUser, string remotePass, int remotePort)
        {
            strRemoteHost = remoteHost;
            strRemotePath = remotePath;
            strRemoteUser = remoteUser;
            strRemotePass = remotePass;
            strRemotePort = remotePort;
            Connect();
        }

        #endregion

        #region 登入

        /// <summary>
        /// FTP 伺服器 IP 位址
        /// </summary>
        private string strRemoteHost;
        public string RemoteHost
        {
            get
            {
                return strRemoteHost;
            }
            set
            {
                strRemoteHost = value;
            }
        }

        /// <summary>
        /// FTP 伺服器連接埠
        /// </summary>
        private int strRemotePort;
        public int RemotePort
        {
            get
            {
                return strRemotePort;
            }
            set
            {
                strRemotePort = value;
            }
        }

        /// <summary>
        /// 目前伺服器目錄
        /// </summary>
        private string strRemotePath;
        public string RemotePath
        {
            get
            {
                return strRemotePath;
            }
            set
            {
                strRemotePath = value;
            }
        }

        /// <summary>
        /// 登入帳號
        /// </summary>
        private string strRemoteUser;
        public string RemoteUser
        {
            set
            {
                strRemoteUser = value;
            }
        }

        /// <summary>
        /// 登入密碼
        /// </summary>
        private string strRemotePass;
        public string RemotePass
        {
            set
            {
                strRemotePass = value;
            }
        }

        /// <summary>
        /// 是否登入
        /// </summary>
        private Boolean bConnected;
        public bool Connected
        {
            get
            {
                return bConnected;
            }
        }

        #endregion

        #region 連線

        /// <summary>
        /// 建立連線
        /// </summary>
        public void Connect()
        {
            socketControl = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(RemoteHost), strRemotePort);
            // 連線
            try
            {
                socketControl.Connect(ep);
            }
            catch (Exception)
            {
                throw new IOException("Couldn't connect to remote server");
            }
            // 取得回應碼
            ReadReply();
            if (iReplyCode != 220)
            {
                DisConnect();
                throw new IOException(strReply.Substring(4));
            }
            // 登入
            SendCommand("USER " + strRemoteUser);
            if (!(iReplyCode == 331 || iReplyCode == 230))
            {
                CloseSocketConnect(); // 關閉連線
                throw new IOException(strReply.Substring(4));
            }
            if (iReplyCode != 230)
            {
                SendCommand("PASS " + strRemotePass);
                if (!(iReplyCode == 230 || iReplyCode == 202))
                {
                    CloseSocketConnect(); // 關閉連線
                    throw new IOException(strReply.Substring(4));
                }
            }
            bConnected = true;
            // 切換目錄
            ChDir(strRemotePath);
        }

        /// <summary>
        /// 關閉連線
        /// </summary>
        public void DisConnect()
        {
            if (socketControl != null)
            {
                SendCommand("QUIT");
            }
            CloseSocketConnect();
        }

        #endregion

        #region 傳送模式

        /// <summary>
        /// 傳送模式：二進位類型、ASCII 類型
        /// </summary>
        public enum TransferType { Binary, ASCII };

        /// <summary>
        /// 設定傳送模式
        /// </summary>
        /// <param name="ttType">傳送模式</param>
        public void SetTransferType(TransferType ttType)
        {
            if (ttType == TransferType.Binary)
            {
                SendCommand("TYPE I"); // 二進位類型傳送
            }
            else
            {
                SendCommand("TYPE A"); // ASCII 類型傳送
            }
            if (iReplyCode != 200)
            {
                throw new IOException(strReply.Substring(4));
            }
            else
            {
                trType = ttType;
            }
        }

        /// <summary>
        /// 取得傳送模式
        /// </summary>
        /// <returns>傳送模式</returns>
        public TransferType GetTransferType()
        {
            return trType;
        }

        #endregion

        #region 檔案操作

        /// <summary>
        /// 取得檔案清單
        /// </summary>
        /// <param name="strMask">檔案名稱匹配字串</param>
        /// <returns></returns>
        public string[] Dir(string strMask)
        {
            // 建立連線
            if (!bConnected)
            {
                Connect();
            }
            // 建立進行資料連線的 Socket
            Socket socketData = CreateDataSocket();

            // 傳送指令
            SendCommand("NLST " + strMask);
            // 解析回應代碼
            if (!(iReplyCode == 150 || iReplyCode == 125 || iReplyCode == 226))
            {
                throw new IOException(strReply.Substring(4));
            }
            // 取得結果
            strMsg = "";
            while (true)
            {
                int iBytes = socketData.Receive(buffer, buffer.Length, 0);
                //strMsg += ASCII.GetString(buffer, 0, iBytes);
                strMsg += UTF8.GetString(buffer, 0, iBytes);
                
                if (iBytes < buffer.Length)
                {
                    break;
                }
            }
            //char[] seperator = { '\n' };
            //string[] strsFileList = strMsg.Split(seperator);
            char[] seperator = { '\r', '\n' };
            string[] strsFileList = strMsg.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            socketData.Close();
            // 資料 Socket 關閉時也會有回應代碼
            if (iReplyCode != 226)
            {
                ReadReply();
                if (iReplyCode != 226)
                {
                    throw new IOException(strReply.Substring(4));
                }
            }
            return strsFileList;
        }

        /// <summary>
        /// 取得檔案大小
        /// </summary>
        /// <param name="strFileName">檔案名稱</param>
        /// <returns>檔案大小</returns>
        private long GetFileSize(string strFileName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("SIZE " + Path.GetFileName(strFileName));
            long lSize = 0;
            if (iReplyCode == 213)
            {
                lSize = Int64.Parse(strReply.Substring(4));
            }
            else
            {
                throw new IOException(strReply.Substring(4));
            }
            return lSize;
        }

        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="strFileName">欲刪除檔案名稱</param>
        public void Delete(string strFileName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("DELE " + strFileName);
            if (iReplyCode != 250)
            {
                throw new IOException(strReply.Substring(4));
            }
        }

        /// <summary>
        /// 重新命名(如果新檔案名稱與原有檔案名稱重複，將覆蓋原有檔案)
        /// </summary>
        /// <param name="strOldFileName">舊檔案名稱</param>
        /// <param name="strNewFileName">新檔案名稱</param>
        public void Rename(string strOldFileName, string strNewFileName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("RNFR " + strOldFileName);
            if (iReplyCode != 350)
            {
                throw new IOException(strReply.Substring(4));
            }
            // 如果新檔案名稱與原有檔案名稱重複，將覆蓋原有檔案
            SendCommand("RNTO " + strNewFileName);
            if (iReplyCode != 250)
            {
                throw new IOException(strReply.Substring(4));
            }
        }

        #endregion

        #region 上傳和下載

        /// <summary>
        /// 下載一批檔案
        /// </summary>
        /// <param name="strFileNameMask">檔案名稱的匹配字串</param>
        /// <param name="strFolder">本地端目錄(不得以 \ 結束)</param>
        public void Get(string strFileNameMask, string strFolder)
        {
            if (!bConnected)
            {
                Connect();
            }
            string[] strFiles = Dir(strFileNameMask);
            foreach (string strFile in strFiles)
            {
                if (!strFile.Equals("")) // 一般來說 strFiles 的最後一個元素可能是空字串
                {
                    Get(strFile, strFolder, strFile);
                }
            }
        }

        /// <summary>
        /// 下載一個檔案
        /// </summary>
        /// <param name="strRemoteFileName">要下載的檔案名稱</param>
        /// <param name="strFolder">本地端目錄(不得以 \ 结束)</param>
        /// <param name="strLocalFileName">儲存在本地端時的檔案名稱</param>
        public void Get(string strRemoteFileName, string strFolder, string strLocalFileName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SetTransferType(TransferType.Binary);
            if (strLocalFileName.Equals(""))
            {
                strLocalFileName = strRemoteFileName;
            }
            if (!File.Exists(strLocalFileName))
            {
                Stream st = File.Create(strLocalFileName);
                st.Close();
            }
            FileStream output = new
            FileStream(strFolder + "\\" + strLocalFileName, FileMode.Create);
            Socket socketData = CreateDataSocket();
            SendCommand("RETR " + strRemoteFileName);
            if (!(iReplyCode == 150 || iReplyCode == 125 || iReplyCode == 226 || iReplyCode == 250))
            {
                throw new IOException(strReply.Substring(4));
            }
            while (true)
            {
                int iBytes = socketData.Receive(buffer, buffer.Length, 0);
                output.Write(buffer, 0, iBytes);
                if (iBytes <= 0)
                {
                    break;
                }
            }
            output.Close();
            if (socketData.Connected)
            {
                socketData.Close();
            }
            if (!(iReplyCode == 226 || iReplyCode == 250))
            {
                ReadReply();
                if (!(iReplyCode == 226 || iReplyCode == 250))
                {
                    throw new IOException(strReply.Substring(4));
                }
            }
        }

        /// <summary>
        /// 上傳一批檔案
        /// </summary>
        /// <param name="strFolder">本地端目錄(不得以\结束)</param>
        /// <param name="strFileNameMask">檔案名稱匹配字串(可以包含 * 和 ?)</param>
        public void Put(string strFolder, string strFileNameMask)
        {
            string[] strFiles = Directory.GetFiles(strFolder, strFileNameMask);
            foreach (string strFile in strFiles)
            {
                // strFile 是完整的檔案名稱(包含路徑)
                Put(strFile);
            }
        }

        /// <summary>
        /// 上傳一個檔案
        /// </summary>
        /// <param name="strFileName">本地端檔案名稱</param>
        public void Put(string strFileName)
        {
            if (!bConnected)
            {
                Connect();
            }
            Socket socketData = CreateDataSocket();
            SendCommand("STOR " + Path.GetFileName(strFileName));
            if (!(iReplyCode == 125 || iReplyCode == 150))
            {
                throw new IOException(strReply.Substring(4));
            }
            FileStream input = new
            FileStream(strFileName, FileMode.Open);
            int iBytes = 0;
            while ((iBytes = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                socketData.Send(buffer, iBytes, 0);
            }
            input.Close();
            if (socketData.Connected)
            {
                socketData.Close();
            }
            if (!(iReplyCode == 226 || iReplyCode == 250))
            {
                ReadReply();
                if (!(iReplyCode == 226 || iReplyCode == 250))
                {
                    throw new IOException(strReply.Substring(4));
                }
            }
        }

        #endregion

        #region 目錄操作

        /// <summary>
        /// 建立目錄
        /// </summary>
        /// <param name="strDirName">目錄名稱</param>
        public void MkDir(string strDirName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("MKD " + strDirName);
            if (iReplyCode != 257)
            {
                throw new IOException(strReply.Substring(4));
            }
        }

        /// <summary>
        /// 刪除目錄
        /// </summary>
        /// <param name="strDirName">目錄名稱</param>
        public void RmDir(string strDirName)
        {
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("RMD " + strDirName);
            if (iReplyCode != 250)
            {
                throw new IOException(strReply.Substring(4));
            }
        }

        /// <summary>
        /// 變更目錄
        /// </summary>
        /// <param name="strDirName">新的工作目錄名稱</param>
        public void ChDir(string strDirName)
        {
            if (strDirName.Equals(".") || strDirName.Equals(""))
            {
                return;
            }
            if (!bConnected)
            {
                Connect();
            }
            SendCommand("CWD " + strDirName);
            if (iReplyCode != 250)
            {
                throw new IOException(strReply.Substring(4));
            }
            this.strRemotePath = strDirName;
        }

        #endregion

        #region 內部變數

        /// <summary>
        /// 伺服器返回的回應訊息(包含回應代碼)
        /// </summary>
        private string strMsg;

        /// <summary>
        /// 伺服器返回的回應訊息(包含回應代碼)
        /// </summary>
        private string strReply;

        /// <summary>
        /// 伺服器返回的回應代碼
        /// </summary>
        private int iReplyCode;

        /// <summary>
        /// 進行控制連線的 Socket
        /// </summary>
        private Socket socketControl;

        /// <summary>
        /// 傳送模式
        /// </summary>
        private TransferType trType;

        /// <summary>
        /// 接收和傳送資料的緩衝區
        /// </summary>
        private static int BLOCK_SIZE = 10240;
        Byte[] buffer = new Byte[BLOCK_SIZE];

        /// <summary>
        /// 編碼方式
        /// </summary>
        //Encoding ASCII = Encoding.ASCII;
        Encoding UTF8 = Encoding.UTF8;

        #endregion

        #region 內部函數
        /// <summary>
        /// 將一行回應字串記錄在 strReply 和 strMsg
        /// 回應代碼記錄在 iReplyCode
        /// </summary>
        private void ReadReply()
        {
            strMsg = "";
            strReply = ReadLine();
            iReplyCode = Int32.Parse(strReply.Substring(0, 3));
        }

        /// <summary>
        /// 建立進行資料連線的 Socket
        /// </summary>
        /// <returns>資料連線 Socket</returns>
        private Socket CreateDataSocket()
        {
            SendCommand("PASV");
            if (iReplyCode != 227)
            {
                throw new IOException(strReply.Substring(4));
            }
            int index1 = strReply.IndexOf('(');
            int index2 = strReply.IndexOf(')');
            string ipData = strReply.Substring(index1 + 1, index2 - index1 - 1);
            int[] parts = new int[6];
            int len = ipData.Length;
            int partCount = 0;
            string buf = "";
            for (int i = 0; i < len && partCount <= 6; i++)
            {
                char ch = Char.Parse(ipData.Substring(i, 1));
                if (Char.IsDigit(ch))
                {
                    buf += ch;
                }
                else if (ch != ',')
                {
                    throw new IOException("Malformed PASV strReply: " + strReply);
                }
                if (ch == ',' || i + 1 == len)
                {
                    try
                    {
                        parts[partCount++] = Int32.Parse(buf);
                        buf = "";
                    }
                    catch (Exception)
                    {
                        throw new IOException("Malformed PASV strReply: " + strReply);
                    }
                }
            }
            string ipAddress = parts[0] + "." + parts[1] + "." + parts[2] + "." + parts[3];
            int port = (parts[4] << 8) + parts[5];
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            try
            {
                s.Connect(ep);
            }
            catch (Exception)
            {
                throw new IOException("Can't connect to remote server");
            }
            return s;
        }

        /// <summary>
        /// 關閉 Socket 連線(用於登入以前)
        /// </summary>
        private void CloseSocketConnect()
        {
            if (socketControl != null)
            {
                socketControl.Close();
                socketControl = null;
            }
            bConnected = false;
        }

        /// <summary>
        /// 讀取 Socket 返回的所有字串
        /// </summary>
        /// <returns>包含回應代碼的字串</returns>
        private string ReadLine()
        {
            while (true)
            {
                int iBytes = socketControl.Receive(buffer, buffer.Length, 0);
                strMsg += Encoding.GetEncoding("utf-8").GetString(buffer, 0, iBytes);
                if (iBytes < buffer.Length)
                {
                    break;
                }
            }
            char[] seperator = { '\n' };
            string[] mess = strMsg.Split(seperator);
            if (strMsg.Length > 2)
            {
                strMsg = mess[mess.Length - 2];
                // seperator[0] 是 10，換行符號是由 13 和 0 組成的，分隔後 10 後面雖沒有字串
                // 但也會分配為空字串給後面(也是最後一個)字串陣列
                // 所以最後一個 mess 是沒用的空字串
                // 但為什麼不直接取 mess[0]，因為只有最後一行字串回應代碼與訊息之間有空格
            }
            else
            {
                strMsg = mess[0];
            }
            if (!strMsg.Substring(3, 1).Equals(" ")) // 返回字串正確的是以回應代碼(如 220 開頭，後面接一空格，再接問候字串)
            {
                return ReadLine();
            }
            return strMsg;
        }

        /// <summary>
        /// 傳送指令並取得回應代碼和最後一行回應字串
        /// </summary>
        /// <param name="strCommand">指令</param>
        private void SendCommand(String strCommand)
        {
            Byte[] cmdBytes = Encoding.ASCII.GetBytes((strCommand + "\r\n").ToCharArray());
            socketControl.Send(cmdBytes, cmdBytes.Length, 0);
            ReadReply();
        }

        #endregion
    }
}