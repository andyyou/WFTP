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

namespace WFTP
{
    /// <summary>
    /// Update.xaml 的互動邏輯: 編輯資料視窗
    /// </summary>
    public partial class Update : Window
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
        /// <summary>
        /// Combobox Datasource
        /// </summary>
        private BindingList<ClassifyItem> _dataClassifyItem = new BindingList<ClassifyItem>();

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

            // 如果是 Lv2 提供可編輯分類
            string[] level = path.Split(new char[]{'/'},StringSplitOptions.RemoveEmptyEntries);
            if (level.Length == 2)
            {
                cmbClassify.Visibility = System.Windows.Visibility.Visible;
                lbClassify.Visibility = System.Windows.Visibility.Visible;

                // 取得資料
                WFTPDbContext db = new WFTPDbContext();
                var classifies = from classes in db.Lv1Classifications
                                 select classes;
                int classifyId = 0 ;
                foreach (var cls in classifies)
                {
                    _dataClassifyItem.Add(new ClassifyItem() { Id = cls.ClassifyId, Name = cls.ClassName, NickName = cls.NickName });
                    if (cls.ClassName == level[0])
                        classifyId = cls.ClassifyId;
                }
                cmbClassify.ItemsSource = _dataClassifyItem;
                cmbClassify.SelectedValue = classifyId;
            }
            else
            {
                cmbClassify.Visibility = System.Windows.Visibility.Collapsed;
                lbClassify.Visibility = System.Windows.Visibility.Collapsed;
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
            
            ClassifyItem item = cmbClassify.SelectedItem as ClassifyItem;
            if (item == null)
            {
                this.ClassifyId = 0;
                this.NewPath = this.NewPath.Substring(0, this.NewPath.LastIndexOf('/') + 1) + this.SystemName;
            }
            else
            {
                this.ClassifyId = item.Id;
                this.NewPath = String.Format("/{0}/{1}", item.Name, this.SystemName);
            }
         
            // 判斷如果有這個名稱則提醒
            ApiHelper api = new ApiHelper();
            // True: 可以編輯, False: 不能編輯
            if (api.CheckRenamePath(this.OldPath,this.NewPath))
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

    #region Model
    /// <summary>
    /// Combobox 分類使用 Model
    /// </summary>
    public class ClassifyItem : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _nickName;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        public string NickName
        {
            get
            {
                return _nickName;
            }
            set
            {
                _nickName = value;
                RaisePropertyChanged("Name");
            }
        }
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                RaisePropertyChanged("ClassifyId");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(String propertyName)
        {
            if ((PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    #endregion
}
