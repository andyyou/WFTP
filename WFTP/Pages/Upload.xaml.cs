﻿using System;
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

namespace WFTP.Pages
{
    /// <summary>
    /// Upload.xaml 的互動邏輯
    /// </summary>
    public partial class Upload : UserControl, ISwitchable
    {
        private dynamic _dataTmp = new BindingList<FileInfo>();
        private dynamic _dataTo = new BindingList<FileItem>();
       
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
            SetPath sp = new SetPath(400,500);
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

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            //For Debug use -- start
            FileInfo info = new FileInfo("C:\\ElectronicGraph1.gif");
            _dataTo.Add(new FileItem() { File = info, TargetPath = @"PP\台光\台灣廠\台光一號線\電氣圖", TargetRealPath = "/PP/EMC/Taiwan/1/Electric Layout/", IsReplace = true });
            //For Debug use -- end

            foreach (FileItem item in _dataTo)
            {
                //MessageBox.Show("本地路徑：" + item.File.FullName + "\n遠端路徑：" + item.TargetPath + "\n遠端真實路徑：" + item.TargetRealPath);
                string filename = System.IO.Path.GetFileName(item.File.FullName);
                Switcher.progress.UpdateProgressList("Upload", item.TargetRealPath + filename, item.File.FullName);
            }
            _dataTo.Clear();
        }

        private void lvwToUplpad_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void lvwToUplpad_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            
            SetPath sp = new SetPath(400,500);
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
      
        
    }
}
