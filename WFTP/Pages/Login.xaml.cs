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
using DataProvider;
using WFTP.Helper;

namespace WFTP.Pages
{
    /// <summary>
    /// Login.xaml 的互動邏輯: 登入
    /// </summary>
    public partial class Login : UserControl
    {
        /// <summary>
        /// 建構子
        /// </summary>
        public Login()
        {
            InitializeComponent();
            // 檢查是否有記憶 帳號 密碼
            if (Properties.Settings.Default.RememberId)
            {
                txtID.Text = Properties.Settings.Default.Id;
                togRememberId.IsChecked = true;
            }
            if (Properties.Settings.Default.RememberPwd)
            {
                txtPassword.Password = Properties.Settings.Default.Pwd;
                togRememberPwd.IsChecked = true;
            }
        }
        /// <summary>
        /// 執行登入
        /// </summary>
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtID.Text.Trim()) || String.IsNullOrEmpty(txtPassword.Password))
            {
                lblMessage.Content = "帳號或密碼不得為空";
            }
            else
            {
                WFTPDbContext db = new WFTPDbContext();
                string account = txtID.Text.Trim();
                string pwd = txtPassword.Password;

                CEmployee user = (from employe in db.Employees
                           where employe.Account == account && employe.Password == pwd
                           select employe).FirstOrDefault();

                if (user != null && user.Activity)
                {
                    // 登入成功時儲存登入頁面相關欄位資料
                    Properties.Settings.Default.Id = account;
                    Properties.Settings.Default.RememberId = Convert.ToBoolean(togRememberId.IsChecked);
                    Properties.Settings.Default.Pwd = pwd;
                    Properties.Settings.Default.RememberPwd = Convert.ToBoolean(togRememberPwd.IsChecked);
                    Properties.Settings.Default.Save();
                    int rank = Convert.ToInt32(user.Rank);

                    // 儲存全域需要的帳號資訊
                    GlobalHelper.AdminItem = new AdminItem();
                    GlobalHelper.AdminItem.IsAdmin = (rank <= 2) ? true : false; // 管理權限定義
                    GlobalHelper.AdminItem.Rank = rank;
                    GlobalHelper.LoginUserID = account;

                    // 顯示可執行的頁面按鈕
                    Switcher.Switch(Switcher.query);
                    Switcher.main.btnQuery.Visibility = Visibility.Visible;
                    Switcher.main.btnManage.Visibility = (GlobalHelper.AdminItem.IsAdmin) ? Visibility.Visible : Visibility.Collapsed;
                    Switcher.main.btnUpload.Visibility = Visibility.Visible;
                    Switcher.main.btnProgress.Visibility = Visibility.Visible;
                }
                else
                {
                    lblMessage.Content = "帳號或密碼有誤";
                }
            }
        }
        /// <summary>
        /// 記憶帳號
        /// </summary>
        private void togRememberId_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RememberId = Convert.ToBoolean(togRememberId.IsChecked);
            if (!Convert.ToBoolean(togRememberId.IsChecked))
            {
                Properties.Settings.Default.Id = "";
            }
            Properties.Settings.Default.Save();
        }
        /// <summary>
        /// 記憶密碼
        /// </summary>
        private void togRememberPwd_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RememberPwd = Convert.ToBoolean(togRememberPwd.IsChecked);
            if (!Convert.ToBoolean(togRememberPwd.IsChecked))
            {
                Properties.Settings.Default.Pwd = "";
            }
            Properties.Settings.Default.Save();
        }
    }
}
