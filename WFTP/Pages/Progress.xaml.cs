using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using WFTP.Helper;

namespace WFTP.Pages
{
    /// <summary>
    /// Progress.xaml 的互動邏輯
    /// </summary>
    public partial class Progress : UserControl
    {
        public BindingList<FileProgressItem> _dataDownloadFiles = new BindingList<FileProgressItem>();
        public BindingList<FileProgressItem> _dataUploadFiles = new BindingList<FileProgressItem>();
        public List<string> _cancelList = new List<string>();

        public Progress()
        {
            InitializeComponent();
            
            // Initialize listview databinding
            lvwDownloadList.ItemsSource = _dataDownloadFiles;
            lvwUploadList.ItemsSource = _dataUploadFiles;

            // Load progress list
            ReadProgressList();
        }

        #region User Control Event

        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            // Read progress list
            List<ProgressInfo> progressList = JsonConvert.DeserializeObject<List<ProgressInfo>>(
                File.ReadAllText(GlobalHelper.ProgressList)).Select(c => (ProgressInfo)c).ToList();

            if (progressList.Count > 0)
            {
                foreach (ProgressInfo info in progressList)
                {
                    if (info.Type.Equals("Download"))
                    {
                        // Download file from FTP server
                        Dictionary<string, string> fileInfo = new Dictionary<string, string>();
                        fileInfo.Add("FileId", info.FileId);
                        fileInfo.Add("RemoteFilePath", info.RemoteFilePath);
                        fileInfo.Add("LocalFilePath", System.IO.Path.GetDirectoryName(info.LocalFilePath));
                        fileInfo.Add("LocalFileName", System.IO.Path.GetFileName(info.LocalFilePath));
                        fileInfo.Add("RemoteFileSize", Convert.ToString(info.FileSize));

                        StartDownload(fileInfo);
                    }
                    else
                    {

                    }
                }
            }
        }

        private void lstCancel_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            _cancelList.Add(btn.Tag.ToString());
            btn.IsEnabled = false;
        }

        #endregion

        #region R Method

        public void ReadProgressList()
        {
            // Read progress list
            List<ProgressInfo> progressList = JsonConvert.DeserializeObject<List<ProgressInfo>>(
                File.ReadAllText(GlobalHelper.ProgressList)).Select(c => (ProgressInfo)c).ToList();

            List<ProgressInfo> tmpProgressList = new List<ProgressInfo>();
            tmpProgressList.AddRange(progressList);

            if (progressList.Count > 0)
            {
                foreach (ProgressInfo info in progressList)
                {
                    if (info.Type.Equals("Download"))
                    {
                        // 如果本地端未完成檔案存在才顯示於進度清單中，否則就將該筆進度刪除
                        if (File.Exists(info.LocalFilePath))
                        {
                            FileInfo localFileInfo = new FileInfo(info.LocalFilePath);
                            long localFileSize = localFileInfo.Length;
                            int percentage = Convert.ToInt32(((double)localFileSize / (double)info.FileSize) * 100);
                            _dataDownloadFiles.Add(new FileProgressItem
                            {
                                Name = System.IO.Path.GetFileName(info.LocalFilePath),
                                Progress = percentage,
                                FileId = info.FileId
                            });
                        }
                        else
                        {
                            int indexOfFile = tmpProgressList.IndexOf(info);
                            tmpProgressList.RemoveAt(indexOfFile);
                        }
                    }
                    else
                    {

                    }
                }

                // Serialize progress list to json format
                string jsonList = JsonConvert.SerializeObject(tmpProgressList, Formatting.Indented);

                // Overwrite progress list
                File.WriteAllText(GlobalHelper.ProgressList, jsonList, Encoding.UTF8);
            }
        }

        public void UpdateProgressList(string type, string remoteFilePath, string localFilePath)
        {
            // Create fileId
            string fileId = "Download_" + Guid.NewGuid().ToString().Replace('-', '_');

            // Get remote file size
            ApiHelper ah = new ApiHelper();
            long fileSize = ah.GetFileSize(remoteFilePath);

            // Read progress list
            List<ProgressInfo> progressList = JsonConvert.DeserializeObject<List<ProgressInfo>>(
                File.ReadAllText(GlobalHelper.ProgressList)).Select(c => (ProgressInfo)c).ToList();

            //var existFile = progressList.Where(o =>
            //    o.Type == "Download"
            //    && o.RemoteFilePath == remoteFilePath
            //    && o.LocalFilePath == localFilePath).FirstOrDefault();
            //int indexOfFile = progressList.IndexOf(existFile);

            //if (indexOfFile != -1)
            //{
            //    progressList.RemoveAt(indexOfFile);
            //}

            // Check file is duplicated
            
            if (File.Exists(localFilePath) || File.Exists(localFilePath + ".wftp"))
            {
                int i = 1;
                string filePathWithoutExt = String.Format(@"{0}\{1}",
                    System.IO.Path.GetDirectoryName(localFilePath),
                    System.IO.Path.GetFileNameWithoutExtension(localFilePath));
                string filePathExt = System.IO.Path.GetExtension(localFilePath);

                while (true)
                {
                    localFilePath = String.Format("{0} ({1}){2}",
                        filePathWithoutExt,
                        i,
                        filePathExt);
                    if (File.Exists(localFilePath) || File.Exists(localFilePath + ".wftp"))
                    {
                        i++;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
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
            File.WriteAllText(GlobalHelper.ProgressList, jsonList, Encoding.UTF8);

            if (type.Equals("Download"))
            {
                // Add file to download list
                Switcher.progress._dataDownloadFiles.Add(new FileProgressItem
                {
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

                StartDownload(fileInfo);
            }
            else
            {
                // Upload file to FTP server

            }
        }

        public void StartDownload(Dictionary<string, string> fileInfo)
        {
            lvwDownloadList.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (ThreadStart)delegate
            {
                BackgroundWorker bgworkerStartDownload = new BackgroundWorker();
                bgworkerStartDownload.DoWork += bgworkerStartDownload_DoWorkHandler;
                bgworkerStartDownload.RunWorkerCompleted += bgworkerStartDownload_RunWorkerCompleted;
                bgworkerStartDownload.WorkerReportsProgress = true;
                bgworkerStartDownload.ProgressChanged += (s, x) =>
                {
                    FileProgressItem item = _dataDownloadFiles.Where(file => file.FileId == fileInfo["FileId"]).First();
                    item.Progress = x.ProgressPercentage;
                };

                bgworkerStartDownload.RunWorkerAsync(fileInfo);
            });
        }

        public void bgworkerStartDownload_DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgworkerUpdateProgress = sender as BackgroundWorker;
            Dictionary<string, string> fileInfo = (Dictionary<string, string>)e.Argument;
            Dictionary<string, string> asyncResult = new Dictionary<string, string>();
            asyncResult.Add("IsCompleted", "false");
            asyncResult.Add("FileId", fileInfo["FileId"]);
            asyncResult.Add("LocalFilePath", "");

            // Asynchronous FTP Download
            Chilkat.Ftp2 ftp = new Chilkat.Ftp2();

            bool success;

            success = ftp.UnlockComponent(GlobalHelper.ComponentCode);
            if (success != true)
            {
                MessageBox.Show(ftp.LastErrorText);
                return;
            }
            
            ftp.Hostname = GlobalHelper.FtpHost;
            ftp.Username = GlobalHelper.FtpUsername;
            ftp.Password = GlobalHelper.FtpPasswrod;
            // Resume download
            ftp.RestartNext = true;

            // Connect and login to the FTP server.
            success = ftp.Connect();
            if (success != true)
            {
                MessageBox.Show(ftp.LastErrorText);
                return;
            }
            
            string localFilename = String.Format(@"{0}\{1}",fileInfo["LocalFilePath"], fileInfo["LocalFileName"]);
            localFilename += ".wftp";
            asyncResult["LocalFilePath"] = localFilename;
            string remoteFilename = fileInfo["RemoteFilePath"];
            long remoteFilesize = Convert.ToInt64(fileInfo["RemoteFileSize"]);

            success = ftp.AsyncGetFileStart(remoteFilename, localFilename);
            if (success != true)
            {
                MessageBox.Show(ftp.LastErrorText);
                return;
            }

            while (ftp.AsyncFinished != true)
            {
                if (_cancelList.Contains(fileInfo["FileId"]))
                {
                    ftp.AsyncAbort();
                    break;
                }

                if (File.Exists(localFilename))
                {
                    FileInfo localFile = new FileInfo(localFilename);
                    double percentage = ((double)localFile.Length / (double)remoteFilesize) * 100;
                    bgworkerUpdateProgress.ReportProgress((int)percentage);
                }

                // Sleep 0.5 second.
                ftp.SleepMs(500);
            }

            bool downloadSuccess = false;
            // Did the download succeed?
            if (ftp.AsyncSuccess == true)
            {
                downloadSuccess = true;
                bgworkerUpdateProgress.ReportProgress(100);
                asyncResult["IsCompleted"] = "true";
            }
            else
            {

            }
            
            ftp.Disconnect();
            ftp.Dispose();

            if (downloadSuccess)
            {
                File.Move(localFilename, localFilename.Replace(".wftp", ""));
            }

            e.Result = asyncResult;
        }

        private void bgworkerStartDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dictionary<string,string> asyncResult = (Dictionary<string,string>)e.Result;
            bool isCompleted = Convert.ToBoolean(asyncResult["IsCompleted"]);
            string fileId = asyncResult["FileId"];

            if (isCompleted || _cancelList.Contains(fileId))
            {
                List<ProgressInfo> progressList = JsonConvert.DeserializeObject<List<ProgressInfo>>(
                    File.ReadAllText(GlobalHelper.ProgressList)).Select(c => (ProgressInfo)c).ToList();

                var fileList = progressList.Where(o => o.FileId == fileId);
                List<int> fileIndex = new List<int>();
                foreach (var file in fileList)
                {
                    fileIndex.Add(progressList.IndexOf(file));
                }
                foreach (var index in fileIndex)
                {
                    progressList.RemoveAt(index);
                }

                string jsonList = JsonConvert.SerializeObject(progressList, Formatting.Indented);
                File.WriteAllText(GlobalHelper.ProgressList, jsonList, Encoding.UTF8);

                if (_cancelList.Contains(fileId))
                {
                    File.Delete(asyncResult["LocalFilePath"]);
                    _cancelList.Remove(fileId);
                }
            }
        }

        #endregion
    }

    #region Models

    public class ProgressInfo
    {
        public string Type;
        public string RemoteFilePath;
        public string LocalFilePath;
        public long FileSize;
        public string FileId;
    }

    public class FileProgressInfo
    {
        public string Name { set; get; }
        public int Progress { set; get; }
    }

    public class FileProgressItem : INotifyPropertyChanged
    {
        private string _name;
        private int _progress;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        public int Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                RaisePropertyChanged("Progress");
            }
        }
        public string FileId { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(String propertyName)
        {
            if ((PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    #endregion
}
