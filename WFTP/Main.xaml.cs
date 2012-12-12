using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using WFTP.Pages;
using WFTP.Lib;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Threading;
using System.Collections.ObjectModel;

namespace WFTP
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class Main : MetroWindow
    {
        public DataTable _progressList;
        public static string _LISTPATH = @"C:\test.json";

        public Main()
        {
            InitializeComponent();

            // 初始化各頁面
            Switcher.query = new Query();
            Switcher.progress = new Progress();
            Switcher.upload = new Upload();
            Switcher.manage = new Manage();

            // 登入前隱藏功能
            btnQuery.Visibility = Visibility.Hidden;
            btnManage.Visibility = Visibility.Hidden;
            btnUpload.Visibility = Visibility.Hidden;
            btnDownload.Visibility = Visibility.Hidden;

            // 初始化檔案處裡進度清單
            _progressList = new DataTable("Progress");
            _progressList.Columns.Add("Type", typeof(string));
            _progressList.Columns.Add("RemoteFilePath", typeof(string));
            _progressList.Columns.Add("LocalFilePath", typeof(string));
            _progressList.Columns.Add("FileSize", typeof(long));
            _progressList.Columns.Add("Percent", typeof(double));

            // 初始化Switcher
            Switcher.main = this;
            Switcher.Switch(new Login()); //載入 Login
        }

        private void CloseButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void MaximizeButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void ChangeViewButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void MinimizeButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void DragableGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            //Switcher.Switch(new Query());
            Switcher.Switch(Switcher.query); 
        }

        private void btnManage_Click(object sender, RoutedEventArgs e)
        {
            //Switcher.Switch(new Manage());
            Switcher.Switch(Switcher.manage);
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            //Switcher.Switch(new Upload());
            Switcher.Switch(Switcher.upload);
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            //Switcher.Switch(new Download());
            Switcher.Switch(Switcher.progress);
        }

        #region Method

        public void UpdateProgressList(string type, string remoteFilePath, string localFilePath)
        {
            // Create fileId
            string fileId = "Download_" + Guid.NewGuid().ToString().Replace('-', '_');

            // Get remote file size
            FTPClient client = new FTPClient();
            long fileSize = client.GetFileSize(remoteFilePath);

            // Read progress list
            List<ProgressInfo> progressList = JsonConvert.DeserializeObject<List<ProgressInfo>>(
                File.ReadAllText(_LISTPATH)).Select(c => (ProgressInfo)c).ToList();

            var existFile = progressList.Where(o => 
                o.Type == "Download"
                && o.RemoteFilePath == remoteFilePath
                && o.LocalFilePath == localFilePath).FirstOrDefault();
            int indexOfFile = progressList.IndexOf(existFile);

            if (indexOfFile != -1)
            {
                progressList.RemoveAt(indexOfFile);
            }

            // Create new progress info
            ProgressInfo progressInfo = new ProgressInfo()
            {
                Type = type,
                RemoteFilePath = remoteFilePath,
                LocalFilePath = localFilePath,
                FileSize = fileSize,
                FileId = fileId
            };
            
            progressList.Add(progressInfo);

            // Serialize progress list to json format
            string jsonList = JsonConvert.SerializeObject(progressList, Formatting.Indented);

            // Overwrite progress list
            File.WriteAllText(_LISTPATH, jsonList, Encoding.UTF8);

            if (type.Equals("Download"))
            {
                // Add file to download list
                Switcher.progress._dataDownloadFiles.Add(new FileProgressItem {
                    Name = System.IO.Path.GetFileName(localFilePath),
                    Progress = 0,
                    FileId = fileId
                });

                // Download file from FTP server
                Dictionary<string, string> fileInfo = new Dictionary<string, string>();
                fileInfo.Add("FileId", fileId);
                fileInfo.Add("RemoteFilePath", remoteFilePath);
                fileInfo.Add("LocalFilePath", System.IO.Path.GetDirectoryName(localFilePath));
                fileInfo.Add("LocalFileName", System.IO.Path.GetFileName(localFilePath));
                fileInfo.Add("RemoteFileSize", Convert.ToString(fileSize));

                DownloadFile(fileInfo);
            }
            else
            {
                // Upload file to FTP server

            }
        }

        public void DownloadFile(Dictionary<string,string> fileInfo)
        {
            BackgroundWorker bgworkerDownload = new BackgroundWorker();
            bgworkerDownload.DoWork += bgworkerDownload_DoWorkHandler;
            bgworkerDownload.RunWorkerCompleted += bgworkerDownload_RunWorkerCompleted;
            bgworkerDownload.RunWorkerAsync(fileInfo);

            //FileInfo localFile = new FileInfo(String.Format(@"{0}\{1}", fileInfo["LocalFilePath"], fileInfo["LocalFileName"]));
            //long remoteFileSize = Convert.ToInt64(fileInfo["RemoteFileSize"]);

            Switcher.progress.UpdateProgress(fileInfo);
        }

        public void bgworkerDownload_DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            Dictionary<string, string> fileInfo = (Dictionary<string, string>)e.Argument;

            FTPClient client = new FTPClient();
            client.Get(fileInfo["RemoteFilePath"], fileInfo["LocalFilePath"], fileInfo["LocalFileName"], true);

            //Switcher.download.UpdateProgress(fileInfo);
        }

        private void bgworkerDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private void UploadFile(Dictionary<string, string> fileInfo)
        {
            BackgroundWorker bgworkerUpload = new BackgroundWorker();
            bgworkerUpload.DoWork += bgworkerUpload_DoWorkHandler;
            bgworkerUpload.RunWorkerCompleted += bgworkerUpload_RunWorkerCompleted;
            bgworkerUpload.RunWorkerAsync(fileInfo);

            Switcher.progress.UpdateProgress(fileInfo);
        }

        public void bgworkerUpload_DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            Dictionary<string, string> fileInfo = (Dictionary<string, string>)e.Argument;

            FTPClient client = new FTPClient();
            client.Get(fileInfo["RemoteFilePath"], fileInfo["LocalFilePath"], fileInfo["LocalFileName"], true);
        }

        private void bgworkerUpload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        #endregion

        #region Data Model

        public class ProgressInfo
        {
            public string Type;
            public string RemoteFilePath;
            public string LocalFilePath;
            public long FileSize;
            public string FileId;
        }

        #endregion

        #region Switcher

        public void Navigate(UserControl nextPage)
        {
            if (nextPage is Pages.Query)
            {
                btnQuery.Visibility = Visibility.Visible;
                btnManage.Visibility = Visibility.Visible;
                btnUpload.Visibility = Visibility.Visible;
                btnDownload.Visibility = Visibility.Visible;
            }
            this.transitioning.Content = nextPage;
        }

        public void Navigate(UserControl nextPage, object state)
        {
            this.Content = nextPage;
            ISwitchable s = nextPage as ISwitchable;

            if (s != null)
            {
                s.UtilizeState(state);
            }
            else
            {
                throw new ArgumentException("NextPage is not ISwitchable! "
                  + nextPage.Name.ToString());
            }
        }

        #endregion
    }
}
