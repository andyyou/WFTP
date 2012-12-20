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
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Globalization;
using WFTP.Helper;
using System.Security.Cryptography;
using DataProvider;

namespace WFTP.Pages
{
    /// <summary>
    /// Upload.xaml 的互動邏輯
    /// </summary>
    public partial class Upload : UserControl, ISwitchable
    {
        private BindingList<FileInfo> _dataTmp = new BindingList<FileInfo>();
        private BindingList<FileItem> _dataTo = new BindingList<FileItem>();
       
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
            lvwToUplpad.ItemsSource = _dataTo;
            lvwTempList.ItemsSource = _dataTmp;
            // Get Admin Rank
            grdMain.DataContext = GlobalHelper.AdminItem;
        }

        private void btnSettingFolder_Click(object sender, RoutedEventArgs e)
        {
            _dataTmp.Clear();

            lbPath.Width = 500;
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (lbPath.Content.ToString() != "")
            {
                dialog.SelectedPath = lbPath.Content.ToString();
            }
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
            {
                lbPath.Content = dialog.SelectedPath.ToString();
                lbPath.ToolTip = dialog.SelectedPath.ToString();
            }

            DirectoryInfo dirInfo = new DirectoryInfo(lbPath.Content.ToString());
            IEnumerable<FileInfo> files = dirInfo.GetFiles("*").Where(
                p => System.IO.Path.GetExtension(p.Extension) != GlobalHelper.TempUploadFileExt);

            foreach (FileInfo file in files)
            {
                _dataTmp.Add(file);
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            if (lvwTempList.SelectedItems.Count > 0)
            {
                SetPath sp = new SetPath(400, 500);
                string target_path = "";
                string target_real_path = "";
                sp.ShowDialog();
                target_path = sp.Path;
                target_real_path = sp.RealPath;

                if (!String.IsNullOrEmpty(target_path))
                {
                    int pathLevel = target_path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Count();
                    if (pathLevel == 5)
                    {
                        List<FileInfo> removeItems = new List<FileInfo>();
                        foreach (FileInfo i in lvwTempList.SelectedItems)
                        {
                            _dataTo.Add(new FileItem() { File = i, TargetPath = target_path, TargetRealPath = target_real_path, IsReplace = true });
                            removeItems.Add(i);
                        }

                        foreach (FileInfo f in removeItems)
                        {
                            _dataTmp.Remove(f);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (lvwToUplpad.SelectedItems.Count > 0)
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

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            // HACK: 測試資料
            //For Debug use -- start
            //FileInfo info = new FileInfo("C:\\ElectronicGraph1.gif");
            //_dataTo.Add(new FileItem() { File = info, TargetPath = @"PP\台光\台灣廠\台光一號線\電氣圖", TargetRealPath = "/PP/EMC/Taiwan/1/Electric Layout/", IsReplace = true });
            //_dataTo.Add(new FileItem() { File = info, TargetPath = @"PP\台光\台灣廠\台光一號線\安裝照片", TargetRealPath = "/PP/EMC/Taiwan/1/Installation Gallery/", IsReplace = true });
            //測試資料結尾
            if (_dataTo.Count() > 0)
            {
                foreach (FileItem item in _dataTo)
                {
                    string remoteFilePath = item.TargetRealPath + System.IO.Path.GetFileName(item.File.FullName);
                    string localFilePath = item.File.FullName;

                    // Check if the file already exists
                    string[] splitPath = remoteFilePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    string checksum = GetChecksum(localFilePath);

                    WFTPDbContext db = new WFTPDbContext();
                    var fileHash =
                        from classify in db.Lv1Classifications
                        from customer in db.Lv2Customers
                        from branch in db.Lv3CustomerBranches
                        from line in db.Lv4Lines
                        from category in db.Lv5FileCategorys
                        from file in db.Lv6Files
                        where classify.ClassName == splitPath[0] &&
                              customer.CompanyName == splitPath[1] && customer.ClassifyId == classify.ClassifyId &&
                              branch.BranchName == splitPath[2] && branch.CompanyId == customer.CompanyId &&
                              line.LineName == splitPath[3] && line.BranchId == branch.BranchId &&
                              category.ClassName == splitPath[4] &&
                              file.FileCategoryId == category.FileCategoryId && file.LineId == line.LineId
                        select new
                        {
                            checksum = file.FileHash
                        };
                    int existFileCount = fileHash.Where(file => file.checksum == checksum).Count();
                    if (existFileCount > 0)
                    {
                        MessageBox.Show(String.Format("檔案 {0} 已存在!!", System.IO.Path.GetFileName(localFilePath)));
                        continue;
                    }
                    else
                    {
                        string tmpFilePath = localFilePath + GlobalHelper.TempUploadFileExt;
                        File.Move(localFilePath, tmpFilePath);
                        Switcher.progress.UpdateProgressList("Upload", remoteFilePath, tmpFilePath,checksum);
                    }
                }
                _dataTo.Clear();

                // Reload temp folder
                DirectoryInfo dirInfo = new DirectoryInfo(lbPath.Content.ToString());
                IEnumerable<FileInfo> files = dirInfo.GetFiles("*").Where(
                    p => System.IO.Path.GetExtension(p.Extension) != GlobalHelper.TempUploadFileExt);

                _dataTmp.Clear();
                foreach (FileInfo file in files)
                {
                    _dataTmp.Add(file);
                }
            }
        }

        private void lvwToUplpad_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void lvwToUplpad_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            
            SetPath sp = new SetPath(400,200);
            string target_path = "";
            string target_real_path = "";
            if (lvwToUplpad.SelectedItems.Count > 0)
            {
                sp.ShowDialog();
                target_path = sp.Path;
                target_real_path = sp.RealPath;

                if (!String.IsNullOrEmpty(target_path))
                {
                    foreach (FileItem i in lvwToUplpad.SelectedItems)
                    {
                        i.TargetPath = target_path;
                        i.TargetRealPath = target_real_path;
                    }
                }
            }
        }

        private static string GetChecksum(string file)
        {
            using (var stream = new BufferedStream(File.OpenRead(file), 1200000))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty).ToLower();
            }
        }

        private void lvwToUplpad_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvwToUplpad.SelectedItems.Count > 0)
            {
                SetPath sp = new SetPath(400, 500);
                string target_path = "";
                string target_real_path = "";
                sp.ShowDialog();
                target_path = sp.Path;
                target_real_path = sp.RealPath;
                if (!String.IsNullOrEmpty(target_path))
                {
                    int pathLevel = target_path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Count();
                    if (pathLevel == 5)
                    {
                        foreach (FileItem item in lvwToUplpad.SelectedItems)
                        {
                            item.TargetPath = target_path;
                            item.TargetRealPath = target_real_path;
                        }
                        
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
      
        
    }
}
