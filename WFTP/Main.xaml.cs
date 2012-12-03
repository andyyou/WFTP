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

namespace WFTP
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class Main : MetroWindow
    {
        public DataTable _progressList;

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

            // 初始化Switcher
            Switcher.main = this;
            Switcher.Switch(new Login()); //載入 Login
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

        public void UpdateProgressList(string type, string remoteFilePath, string localFilePath)
        {
            FTPClient client = new FTPClient();
            long fileSize = client.GetFileSize(remoteFilePath);



            //JObject o = JObject.Parse(File.ReadAllText(@"c:\test.json"));

            //string fileList = "";
            //foreach (var item in o["Progress"]["Items"])
            //{
            //    fileList += string.Format("{0}\t{1}\t{2}\t{3}\t{4}\n", item["Type"], item["RemotePath"], item["LocalPath"], item["FileSize"], item["Percent"]);
            //}


            //MessageBox.Show(fileList);







        }

        #endregion

        #region Data Model

        public class PogressInfo
        {
            public string Type;
            public string RemotePath;
            public string LocalPath;
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
