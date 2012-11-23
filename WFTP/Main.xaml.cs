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
using MahApps.Metro.Controls;

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
            // 登入前隱藏功能
            btnQuery.Visibility = Visibility.Hidden;
            btnManage.Visibility = Visibility.Hidden;
            btnUpload.Visibility = Visibility.Hidden;
            btnDownload.Visibility = Visibility.Hidden;
            // 初始化Switcher
            Switcher.main = this;
            Switcher.Switch(new Pages.Login()); //載入 Login
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
            Switcher.Switch(new WFTP.Pages.Query()); 
        }

        private void btnManage_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new WFTP.Pages.Manage());
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new WFTP.Pages.Upload());
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(new WFTP.Pages.Download());
        }

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
