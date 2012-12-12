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
using Newtonsoft.Json;
using System.IO;

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

        public void ReadProgressList()
        {
            // Read progress list
            List<Main.ProgressInfo> progressList = JsonConvert.DeserializeObject<List<Main.ProgressInfo>>(
                File.ReadAllText(Main._LISTPATH)).Select(c => (Main.ProgressInfo)c).ToList();

            List<Main.ProgressInfo> tmpProgressList = new List<Main.ProgressInfo>();
            tmpProgressList.AddRange(progressList);

            if (progressList.Count > 0)
            {
                foreach (Main.ProgressInfo info in progressList)
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
                File.WriteAllText(Main._LISTPATH, jsonList, Encoding.UTF8);
            }
        }

        private void btnResume_Click(object sender, RoutedEventArgs e)
        {
            // Read progress list
            List<Main.ProgressInfo> progressList = JsonConvert.DeserializeObject<List<Main.ProgressInfo>>(
                File.ReadAllText(Main._LISTPATH)).Select(c => (Main.ProgressInfo)c).ToList();

            if (progressList.Count > 0)
            {
                foreach (Main.ProgressInfo info in progressList)
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

                        Switcher.main.DownloadFile(fileInfo);
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
        }

        public void UpdateProgress(Dictionary<string, string> fileInfo)
        {
            lvwDownloadList.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (ThreadStart)delegate
            {
                BackgroundWorker bgworkerUpdateProgress = new BackgroundWorker();
                bgworkerUpdateProgress.DoWork += bgworkerUpdateProgress_DoWorkHandler;
                bgworkerUpdateProgress.RunWorkerCompleted += bgworkerUpdateProgress_RunWorkerCompleted;
                bgworkerUpdateProgress.WorkerReportsProgress = true;
                bgworkerUpdateProgress.ProgressChanged += (s, x) =>
                {
                    FileProgressItem item = _dataDownloadFiles.Where(file => file.FileId == fileInfo["FileId"]).First();
                    item.Progress = x.ProgressPercentage;
                };

                bgworkerUpdateProgress.RunWorkerAsync(fileInfo);
            });
        }

        public void bgworkerUpdateProgress_DoWorkHandler(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(500);

            Dictionary<string, string> fileInfo = (Dictionary<string, string>)e.Argument;
            long remoteFileSize = Convert.ToInt64(fileInfo["RemoteFileSize"]);

            BackgroundWorker bgworkerUpdateProgress = sender as BackgroundWorker;

            FileInfo localFileInfo = new FileInfo(String.Format(@"{0}\{1}", fileInfo["LocalFilePath"], fileInfo["LocalFileName"]));
            long localFileSize = localFileInfo.Length;

            while (localFileSize <= remoteFileSize)
            {
                ////////////////////////////////////////////////////////////////////////
                if (_cancelList.Contains(fileInfo["FileId"]))
                {
                    e.Cancel = true;
                    break;
                }
                ////////////////////////////////////////////////////////////////////////

                localFileInfo = new FileInfo(String.Format(@"{0}\{1}", fileInfo["LocalFilePath"], fileInfo["LocalFileName"]));
                localFileSize = localFileInfo.Length;
                double size = ((double)localFileSize / (double)remoteFileSize) * 100;
                bgworkerUpdateProgress.ReportProgress((int)size);

                if (localFileSize == remoteFileSize)
                {
                    break;
                }
                Thread.Sleep(500);
            }

            e.Result = fileInfo["FileId"];
        }

        private void bgworkerUpdateProgress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                string fileId = e.Result.ToString();

                List<Main.ProgressInfo> progressList = JsonConvert.DeserializeObject<List<Main.ProgressInfo>>(
                    File.ReadAllText(Main._LISTPATH)).Select(c => (Main.ProgressInfo)c).ToList();

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
                File.WriteAllText(Main._LISTPATH, jsonList, Encoding.UTF8);
            }
        }
    }

    #region Models

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
