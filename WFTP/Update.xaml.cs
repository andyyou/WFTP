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
using System.ComponentModel;
using DataProvider;
using System.Text.RegularExpressions;
using MahApps.Metro.Controls;

namespace WFTP
{
    /// <summary>
    /// Update.xaml 的互動邏輯: 編輯資料視窗
    /// </summary>
    public partial class Update : MetroWindow
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
        /// 傳入的原始路徑
        /// </summary>
        public string OldPath { set; get; }
        /// <summary>
        /// 編輯後的路徑
        /// </summary>
        public string NewPath { set; get; }
        /// <summary>
        /// 分類ID
        /// </summary>
        public int ClassifyId { set; get; }
        /// <summary>
        /// 設定完成指標
        /// </summary>
        public bool IsDone { set; get; }
       

        #endregion

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="x">視窗 X 座標</param>
        /// <param name="y">視窗 Y 座標</param>
        /// <param name="path">編輯的目錄或座標完整路徑</param>
        /// <param name="oldNickName">未編輯的NickName</param>
        public Update(int x, int y, string path, string oldNickName)
        {
            InitializeComponent();

            // 設定彈出視窗座標
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = x;
            this.Top = y;

            // 紀錄傳入的參數
            this.IsDone = false;
            this.OldPath = path;
            this.NewPath = path.Substring(0, path.LastIndexOf('/') + 1);
            txtNickName.Text = oldNickName;
            txtName.Text = path.Substring(path.LastIndexOf('/')+1);
            TextBox[] txtArray = { txtNickName, txtName };
            foreach (TextBox txt in txtArray)
            {
                if (String.IsNullOrEmpty(txt.Text))
                    FocusHelper.Focus(txt);
                else
                    FocusHelper.Focus(txtNickName);
            }
        }
        /// <summary>
        /// 取消關閉編輯視窗
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.SystemName = null;
            this.NickName = null;
            this.ClassifyId = 0;
            this.IsDone = false;
            this.Close();
        }
        /// <summary>
        ///  確認設定完成
        /// </summary>
        private void btnGetPath_Click(object sender, RoutedEventArgs e)
        {
            GetPath();
        }
        /// <summary>
        /// 當欄位都有資料時 Enter 直接可以確認
        /// </summary>
        private void txtNickName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                GetPath();
        }
        private void GetPath()
        {
            if (String.IsNullOrEmpty(txtName.Text) || String.IsNullOrEmpty(txtNickName.Text))
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
            // 把資料設置 Properties
            this.SystemName = txtName.Text.Trim();
            this.NickName = txtNickName.Text.Trim();
            this.ClassifyId = 0;
            this.NewPath = this.NewPath.Substring(0, this.NewPath.LastIndexOf('/') + 1) + this.SystemName;

            // 判斷如果有這個名稱則提醒
            ApiHelper api = new ApiHelper();
            // True: 可以編輯, False: 不能編輯
            if (api.CheckRenamePath(this.OldPath, this.NewPath))
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
