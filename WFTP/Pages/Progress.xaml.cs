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

        public Progress()
        {
            InitializeComponent();
            
            // Initialize listview databinding
            lvwDownloadList.ItemsSource = _dataDownloadFiles;
            lvwUploadList.ItemsSource = _dataUploadFiles;
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
            Dictionary<string, string> fileInfo = (Dictionary<string, string>)e.Argument;

            BackgroundWorker worker = sender as BackgroundWorker;

            int i = 0;
            while (i <= 100)
            {
                worker.ReportProgress(i);
                i++;
                Thread.Sleep(50);
            }
            e.Result = fileInfo["FileId"];
        }

        private void bgworkerUpdateProgress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string fileId = e.Result.ToString();

            List<WFTP.Main.ProgressInfo> progressList = JsonConvert.DeserializeObject<List<WFTP.Main.ProgressInfo>>(
                File.ReadAllText(WFTP.Main._PROGRESSLIST)).Select(c => (WFTP.Main.ProgressInfo)c).ToList();

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
            File.WriteAllText(WFTP.Main._PROGRESSLIST, jsonList, Encoding.UTF8);
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
