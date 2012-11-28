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
using WFTP.Lib;
using System.IO;

namespace WFTP.Pages
{
    /// <summary>
    /// Upload.xaml 的互動邏輯
    /// </summary>
    public partial class Upload : UserControl, ISwitchable
    {
        private dynamic _dataTmp = new ObservableCollection<FileInfo>();
        private dynamic _dataTo = new ObservableCollection<FileItem>();
        public Upload()
        {
            InitializeComponent();
        }
       
        #region ISwitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            lvwToUplpad.DataContext = _dataTo;
            lvwTempList.DataContext = _dataTmp;
        }

        private void btnSettingFolder_Click(object sender, RoutedEventArgs e)
        {
            lbPath.Width = 500;
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
            {
                lbPath.Content = dialog.SelectedPath.ToString();
                lbPath.ToolTip = dialog.SelectedPath.ToString();
            }

            DirectoryInfo dirInfo = new DirectoryInfo(lbPath.Content.ToString());
            FileInfo[] files = dirInfo.GetFiles("*");
            
            foreach (FileInfo file in files)
            {
                _dataTmp.Add(file);
            }
            

        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            List<FileInfo> removeItems = new List<FileInfo>();
            foreach (FileInfo i in lvwTempList.SelectedItems)
            {
                _dataTo.Add(new FileItem() { File = i, TargetPath = "/PP/TUC/XX" });
                removeItems.Add(i);
            }

            foreach (FileInfo f in removeItems)
            {
                _dataTmp.Remove(f);
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            List<FileItem> removeItems = new List<FileItem>();
            foreach (FileItem i in lvwToUplpad.SelectedItems)
            {
                _dataTmp.Add(i.File);
                removeItems.Add(i);
            }

            foreach (FileItem f in removeItems)
            {
                _dataTo.Remove(f);
            }
        }

        
    }

    #region Sapple Data For Test
    public class FileItem
    {
        public FileInfo File { set; get; }
        public string TargetPath { set; get; }

    }
    #endregion
}
