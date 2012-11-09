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


namespace WFTP
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtID.Text.Trim() == "" || txtPassword.Password == "")
            {
                lblMessage.Content = "帳號或密碼不得為空";
            }
            else
            {
                WFTPDbContext db = new WFTPDbContext();
                string account = txtID.Text.Trim();
                string pwd = txtPassword.Password;

                var user = from employe in db.Employees
                           where employe.Account == account && employe.Password == pwd
                           select employe;

                if (user.Count() > 0)
                {
                    Main window = new Main();
                    window.Show();
                    this.Close();
                }
            }
        }

        private void DragableGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void CloseButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }      
    }
}
