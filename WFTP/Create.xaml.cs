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
using System.Windows.Shapes;
using WFTP.Helper;

namespace WFTP
{
    /// <summary>
    /// GetInput.xaml 的互動邏輯
    /// </summary>
    public partial class Create : Window
    {
        #region Properties
        public string SystemName { set; get; }
        public string NickName { set; get; }
        public string PrePath { set; get; }
        #endregion
        public Create(int x, int y, string path)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = x;
            this.Top = y;
            this.PrePath = path;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.SystemName = null;
            this.NickName = null;
            this.Close();
        }

        private void btnGetPath_Click(object sender, RoutedEventArgs e)
        {
            this.SystemName = txtName.Text.Trim();
            this.NickName = txtNickName.Text.Trim();
            string path = this.PrePath + "/" + txtName.Text.Trim();
            // 判斷如果有這個名稱則提醒
            ApiHelper api = new ApiHelper();
            if (!api.CheckPath(path))
            {
                txtName.Foreground = Brushes.Black;
                lbMessage.Content = "";
                this.Close();
            }
            else
            {
                txtName.Foreground = Brushes.Red;
                lbMessage.Content = "此系統名稱已存在,請換別的名稱";
            }

            
        }
    }
}
