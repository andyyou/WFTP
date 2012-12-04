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
        private const string _PROGRESSLIST = @"C:\test.json";

        public Main()
        {
            InitializeComponent();

            // 初始化各頁面
            Switcher.query = new Query();
            Switcher.download = new Download();
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

            // 刪除已完成檔案清單
            DeleteFinishedFileList();

            // 初始化Switcher
            Switcher.main = this;
            Switcher.Switch(new Login()); //載入 Login
        }

        ~Main()
        {
            // 刪除已完成檔案清單
            DeleteFinishedFileList();
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
            Switcher.Switch(Switcher.download);
        }

        #region Method

        // 刪除已完成檔案清單
        public void DeleteFinishedFileList()
        {
            List<ProgressInfo> progressList = JsonConvert.DeserializeObject<List<ProgressInfo>>(
                File.ReadAllText(_PROGRESSLIST)).Select(c => (ProgressInfo)c).ToList();

            var fileList = progressList.Where(o => o.Percent == 100);
            List<int> fileIndex= new List<int>();
            foreach (var file in fileList)
            {
                fileIndex.Add(progressList.IndexOf(file));
            }
            foreach (var index in fileIndex)
            {
                progressList.RemoveAt(index);
            }

            string jsonList = JsonConvert.SerializeObject(progressList, Formatting.Indented);
            File.WriteAllText(_PROGRESSLIST, jsonList, Encoding.UTF8);
        }

        public void UpdateProgressList(string type, string remoteFilePath, string localFilePath)
        {
            // Get remote file size
            FTPClient client = new FTPClient();
            long fileSize = client.GetFileSize(remoteFilePath);

            // Read progress list
            List<ProgressInfo> progressList = JsonConvert.DeserializeObject<List<ProgressInfo>>(
                File.ReadAllText(_PROGRESSLIST)).Select(c => (ProgressInfo)c).ToList();

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
                Percent = 0
            };
            
            progressList.Add(progressInfo);

            // Serialize progress list to json format
            string jsonList = JsonConvert.SerializeObject(progressList, Formatting.Indented);

            // Overwrite progress list
            File.WriteAllText(_PROGRESSLIST, jsonList, Encoding.UTF8);

            if (type.Equals("Download"))
            {
                // Add file to download list
                ListViewItem lvi = new ListViewItem();
                lvi.Name = "Download_" + Switcher.download.lvwDownloadList.Items.Count.ToString();
                var downloadFile = new ObservableCollection<FileProcessInfo>();
                downloadFile.Add(new FileProcessInfo() {
                    Name = System.IO.Path.GetFileName(localFilePath),
                    Process = 0 });
                lvi.Content = downloadFile;

                Switcher.download.lvwDownloadList.Items.Add(lvi);

                // Download file from FTP server
                Dictionary<string, string> fileInfo = new Dictionary<string, string>();
                fileInfo.Add("FileId", lvi.Name);
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

        private void DownloadFile(Dictionary<string,string> fileInfo)
        {
            BackgroundWorker bgWorkerDownload = new BackgroundWorker();

            // Wire up event handlers
            bgWorkerDownload.DoWork += new DoWorkEventHandler(bgWorkerDownload_DoWork);
            bgWorkerDownload.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerDownload_RunWorkerCompleted);

            bgWorkerDownload.RunWorkerAsync(fileInfo);
        }

        void bgWorkerDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            Dictionary<string, string> fileInfo = (Dictionary<string, string>)e.Argument;
            FTPClient client = new FTPClient();
            string fileId = fileInfo["FileId"];

            client.Get(fileInfo["RemoteFilePath"],fileInfo["LocalFilePath"], fileInfo["LocalFileName"], true);

            FileInfo localFile = new FileInfo(String.Format(@"{0}\{1}", fileInfo["LocalFilePath"], fileInfo["LocalFileName"]));
            long remoteFileSize = Convert.ToInt64(fileInfo["RemoteFileSize"]);



            //ListView lvw = Switcher.download.lvwDownloadList;

            //if (!lvw.Dispatcher.CheckAccess())
            //{
            //    lvw.Dispatcher.Invoke(
            //      System.Windows.Threading.DispatcherPriority.Normal,
            //      new Action(
            //        delegate()
            //        {
            //            ListViewItem lvi = null;
            //            foreach (ListViewItem item in lvw.Items)
            //            {
            //                if (item.Name == fileId)
            //                {
            //                    lvi = item;
            //                }
            //            }
            //            int i = 0;
            //            while (i < 100)
            //            {
            //                ObservableCollection<FileProcessInfo> downloadFile = (ObservableCollection<FileProcessInfo>)lvi.Content;
            //                downloadFile[0].Process = i;
            //                i++;
            //            }
            //        }
            //    ));
            //}
            //else
            //{
            //    ListViewItem lvi = null;
            //    foreach (ListViewItem item in lvw.Items)
            //    {
            //        if (item.Name == fileId)
            //        {
            //            lvi = item;
            //        }
            //    }
            //    int i = 0;
            //    while (i < 100)
            //    {
            //        ObservableCollection<FileProcessInfo> downloadFile = (ObservableCollection<FileProcessInfo>)lvi.Content;
            //        downloadFile[0].Process = i;
            //        i++;
            //    }
            //}

            
            
            //while (localFile.Length < remoteFileSize)
            //{
            //    // Update download progress
            //}
            //ListViewItem lvi = (ListViewItem)Switcher.download.lvwDownloadList.Items[0];

            //MessageBox.Show(lvi.Name);
            //MessageBox.Show(Switcher.download.lvwDownloadList.FindChildrenfindi[fileId].ToString());
        }

        void bgWorkerDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
            public int Percent;
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
