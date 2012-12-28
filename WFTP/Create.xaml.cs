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
using System.Text.RegularExpressions;
using MahApps.Metro.Controls;

namespace WFTP
{
    /// <summary>
    /// Create.xaml 的互動邏輯 : 新增目錄取得設定的參數
    /// </summary>
    public partial class Create : MetroWindow
    {
        private const string PATTERN_SYSTEMNAME = @"^[\w\-]*$";
        private const string PATTERN_NICKNAME = @"^[\w\- ]*$";

        #region Properties

        /// <summary>
        /// FileName, LineName 使用於Server實際檔案或目錄名稱
        /// </summary>
        public string SystemName { set; get; }
        /// <summary>
        /// GUI顯示的代稱
        /// </summary>
        public string NickName { set; get; }
        /// <summary>
        /// 新增資料所需的前段路徑
        /// </summary>
        public string PrePath { set; get; }
        /// <summary>
        /// 設定完成指標
        /// </summary>
        public bool IsDone { set; get; }

        #endregion
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="x">彈出視窗的 X 座標</param>
        /// <param name="y">彈出視窗的 Y 座標</param>
        /// <param name="path">前段路徑表示新增的目錄會在該路徑底下</param>
        public Create(int x, int y, string path)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = x;
            this.Top = y;
            this.PrePath = path;
            this.IsDone = false;
            TextBox[] txtArray = { txtNickName,txtName };
            foreach (TextBox txt in txtArray)
            { 
               if (String.IsNullOrEmpty(txt.Text))
                   FocusHelper.Focus(txt);
               else
                  FocusHelper.Focus(txtNickName);
            }
        }
        /// <summary>
        /// 取消關閉新增視窗
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.SystemName = null;
            this.NickName = null;
            this.IsDone = false;
            this.Close();
        }
        /// <summary>
        /// 參數設定完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetPath_Click(object sender, RoutedEventArgs e)
        {
            GetPath();
        }
        /// <summary>
        /// 當欄位資料時 Enter 直接可以確認
        /// </summary>
        private void txtNickName_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                GetPath();
        }
        /// <summary>
        /// 處理取得路徑
        /// </summary>
        private void GetPath()
        {
            if (String.IsNullOrEmpty(txtName.Text)|| String.IsNullOrEmpty(txtNickName.Text))
            {
                lbMessage.Content = "欄位不得為空白";
                return;
            }

            if (!Regex.IsMatch(txtName.Text.Trim(), PATTERN_SYSTEMNAME))
            {
                lbMessage.Content = "系統名稱格式錯誤";
                return;
            }
            else
            {
                lbMessage.Content = "";
            }

            if (!Regex.IsMatch(txtNickName.Text.Trim(), PATTERN_NICKNAME))
            {
                lbMessage.Content = "瀏覽名稱格式錯誤";
                return;
            }
            else
            {
                lbMessage.Content = "";
            }
            this.SystemName = txtName.Text.Trim();
            this.NickName = txtNickName.Text.Trim();
            string path = this.PrePath + "/" + txtName.Text.Trim();
            // 判斷如果有這個名稱則提醒
            ApiHelper api = new ApiHelper();
            if (!api.CheckPath(path))
            {
                txtName.Foreground = Brushes.Black;
                lbMessage.Content = "";
                this.IsDone = true;
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
