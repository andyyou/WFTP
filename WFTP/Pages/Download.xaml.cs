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

namespace WFTP.Pages
{
    /// <summary>
    /// Download.xaml 的互動邏輯
    /// </summary>
    public partial class Download : UserControl
    {
        public Download()
        {
            InitializeComponent();
            // For test
            var downloadFile = new ObservableCollection<FileProcessInfo>();
            downloadFile.Add(new FileProcessInfo() { Name = "Download-File-1", Process = 60 });
            downloadFile.Add(new FileProcessInfo() { Name = "Download-File-2", Process = 100 });
            downloadFile.Add(new FileProcessInfo() { Name = "Download-File-3", Process = 10 });
            downloadFile.Add(new FileProcessInfo() { Name = "Download-File-4", Process = 5 });
            lvwDownloadList.DataContext = downloadFile;

            var uploadFile = new ObservableCollection<FileProcessInfo>();
            uploadFile.Add(new FileProcessInfo() { Name = "Upload-File-1", Process = 10 });
            uploadFile.Add(new FileProcessInfo() { Name = "Upload-File-2", Process = 74 });
            uploadFile.Add(new FileProcessInfo() { Name = "Upload-File-3", Process = 57 });
            uploadFile.Add(new FileProcessInfo() { Name = "Upload-File-4", Process = 100 });
            lvwUploadList.DataContext = uploadFile;
        }
    }
    #region Sample Data For Test

    public class FileProcessInfo
    {
        public string Name { set; get; }
        public int Process { set; get; }

    }

    #endregion
}
