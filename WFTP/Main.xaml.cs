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
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Collections.ObjectModel;
using WFTP.Helper;
using DataProvider;
using System.Reflection;
using System.Diagnostics;

namespace WFTP
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class Main : MetroWindow
    {
        public Main()
        {
            InitializeComponent();

            // 將程式版號顯示於標題列後方
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            this.Title += " Ver." + fvi.ProductVersion;

            // 由資料庫取得 FTP 及 API 相關參數設定
            GetSystemConfig();

            // 初始化各頁面
            Switcher.query = new Query();
            Switcher.progress = new Progress();
            Switcher.upload = new Upload();
            Switcher.manage = new Manage();
            Switcher.login = new Login();

            // 登入前隱藏功能
            btnQuery.Visibility = Visibility.Hidden;
            btnManage.Visibility = Visibility.Hidden;
            btnUpload.Visibility = Visibility.Hidden;
            btnProgress.Visibility = Visibility.Hidden;

            // 初始化Switcher
            Switcher.main = this;
            Switcher.Switch(Switcher.login); //載入 Login
        }

        #region User Control Event
        /// <summary>
        /// 主視窗Bar -> 關閉程式
        /// </summary>
        private void CloseButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// 主視窗Bar -> 最大化視窗
        /// </summary>
        private void MaximizeButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }
        /// <summary>
        /// 主視窗Bar -> 回復正常大小
        /// </summary>
        private void ChangeViewButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Normal;
        }
        /// <summary>
        /// 主視窗Bar -> 最小化視窗
        /// </summary>
        private void MinimizeButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// 主視窗拖移
        /// </summary>
        private void DragableGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        /// <summary>
        /// 切換至查詢頁面(一般查詢,進階查詢,依據管理權限其他管理功能)
        /// </summary>
        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.query); 
        }
        /// <summary>
        /// 切換至管理頁面(管理會員)
        /// </summary>
        private void btnManage_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.manage);
        }
        /// <summary>
        /// 切換至上傳頁面
        /// </summary>
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.upload);
            // Refresh temp list
            Switcher.upload.RefreshTempList();
        }
        /// <summary>
        /// 切換至進度處理頁面
        /// </summary>
        private void btnProgress_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.progress);
        }

        #endregion

        #region Method

        private void GetSystemConfig()
        {
            WFTPDbContext db = new WFTPDbContext();

            SystemConfig config = (from conf in db.SystemConfigs select conf).FirstOrDefault();
            GlobalHelper.ApiKey = config.ApiKey;
            GlobalHelper.ApiHost = config.ApiHost;
            GlobalHelper.ApiPort = config.ApiPort;
            GlobalHelper.ComponentCode = config.FtpComponentCode;
            GlobalHelper.FtpHost = config.FtpHost;
            GlobalHelper.FtpUsername = config.FtpUsername;
            GlobalHelper.FtpPasswrod = config.FtpPassword;

            GlobalHelper.SetApiPath();
        }

        #endregion

        #region Switcher

        public void Navigate(UserControl nextPage)
        {
            this.transitioning.Content = nextPage;
        }

        #endregion
    }
}
