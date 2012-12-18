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

namespace WFTP
{
    /// <summary>
    /// Update.xaml 的互動邏輯
    /// </summary>
    public partial class Update : Window
    {
        #region Properties
        public string SystemName { set; get; }
        public string NickName { set; get; }
        public string OldPath { set; get; }
        public string NewPath { set; get; }
        public int ClassifyId { set; get; }
        public bool IsDone { set; get; }
        private BindingList<PrevIdItem> _dataPrevItem = new BindingList<PrevIdItem>();
        #endregion
        public Update(int x, int y, string path, string oldNickName)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = x;
            this.Top = y;
            this.OldPath = path;
            this.NewPath = path.Substring(0, path.LastIndexOf('/') + 1);
            txtNickName.Text = oldNickName;
            txtName.Text = path.Substring(path.LastIndexOf('/')+1);
            string[] level = path.Split(new char[]{'/'},StringSplitOptions.RemoveEmptyEntries);
            if (level.Length == 2)
            {
                cmbPrevId.Visibility = System.Windows.Visibility.Visible;
                lbPrevId.Visibility = System.Windows.Visibility.Visible;
                WFTPDbContext db = new WFTPDbContext();
                var classifies = from classes in db.Lv1Classifications
                                 select classes;
                int selected = 0 ;
                foreach (var cls in classifies)
                {
                    _dataPrevItem.Add(new PrevIdItem() { Id = cls.ClassifyId, Name = cls.ClassName, NickName = cls.NickName });
                    if (cls.ClassName == level[0])
                        selected = cls.ClassifyId;
                }
                cmbPrevId.ItemsSource = _dataPrevItem;
                cmbPrevId.SelectedValue = selected;
            }
            else
            {
                cmbPrevId.Visibility = System.Windows.Visibility.Collapsed;
                lbPrevId.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.SystemName = null;
            this.NickName = null;
            this.ClassifyId = 0;
            this.IsDone = false;
            this.Close();
        }

        private void btnGetPath_Click(object sender, RoutedEventArgs e)
        {
            this.SystemName = txtName.Text.Trim();
            this.NickName = txtNickName.Text.Trim();
            this.IsDone = true;
            PrevIdItem item = cmbPrevId.SelectedItem as PrevIdItem;
            if (item == null)
            {
                this.NewPath = this.NewPath.Substring(0, this.NewPath.LastIndexOf('/') + 1) + this.SystemName;
            }
            else
            {
                this.ClassifyId = item.Id;
                this.NewPath = String.Format("/{0}/{1}", item.Name, this.SystemName);
            }
            // 判斷如果有這個名稱則提醒
            ApiHelper api = new ApiHelper();
            if (api.CheckRenamePath(this.OldPath,this.NewPath))
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
    // For ComboboxItem of advance query
    public class PrevIdItem : INotifyPropertyChanged
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
}
