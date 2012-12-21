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

            // 初始化各頁面
            Switcher.query = new Query();
            Switcher.progress = new Progress();
            Switcher.upload = new Upload();
            Switcher.manage = new Manage();

            // 登入前隱藏功能
            btnQuery.Visibility = Visibility.Hidden;
            btnManage.Visibility = Visibility.Hidden;
            btnUpload.Visibility = Visibility.Hidden;
            btnDownload.Visibility = Visibility.Hidden;

            // 初始化Switcher
            Switcher.main = this;
            Switcher.Switch(new Login()); //載入 Login
        }

        #region User Control Event

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
            // Refresh temp list
            Switcher.upload.RefreshTempList();
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            //Switcher.Switch(new Download());
            Switcher.Switch(Switcher.progress);
        }

        #endregion

        #region Switcher

        public void Navigate(UserControl nextPage)
        {
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
