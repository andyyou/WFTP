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
            var data = new ObservableCollection<DownItem>();
            data.Add(new DownItem() { Name = "File-1", DownloadProcess = 60 });
            data.Add(new DownItem() { Name = "File-2", DownloadProcess = 100 });
            data.Add(new DownItem() { Name = "File-3", DownloadProcess = 10 });
            data.Add(new DownItem() { Name = "File-4", DownloadProcess = 5 });
            lvwDownloadList.DataContext = data;
        }
    }
    #region Sapple Data For Test
    public class DownItem
    {
        public string Name { set; get; }
        public int DownloadProcess { set; get; }

    }
    #endregion
}
