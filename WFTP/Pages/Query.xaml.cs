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
using MahApps.Metro.Controls;
using System.Xml;
using Odyssey.Controls;
using WFTP.Helper;
using System.Xml.XPath;
using System.ComponentModel;
using System.Data.Linq.SqlClient;
using System.Threading;
using System.Windows.Threading;


namespace WFTP.Pages
{
    /// <summary>
    /// Query.xaml 的互動邏輯: 查詢頁面
    /// </summary>
    public partial class Query : UserControl
    {
        #region Data Members
        /// <summary>
        /// 判斷 一般查詢 為 Tile Mode 或 List Mode
        /// </summary>
        private bool _isTileView = true;
        /// <summary>
        /// 判斷 進階查詢 為 Tile Mode 或 List Mode
        /// </summary>
        private bool _isAdvanceTileView = true;
        /// <summary>
        /// 累積組合當下的實體路徑 /PP/TUC/Taiwan/Line1
        /// </summary>
        private string _ftpPath = "/";
        /// <summary>
        /// 累積組合當下的實體路徑對應的ID  /2/1/5/6
        /// </summary>
        private string _idPath = "/";
        /// <summary>
        /// 儲存當下每一階層的ID
        /// </summary>
        private Dictionary<int, int> _catalogLevelId = new Dictionary<int, int>();
        /// <summary>
        /// 儲存當下每一階層Path Name
        /// </summary>
        private Dictionary<int, string> _catalogLevelName = new Dictionary<int, string>();
        /// <summary>
        /// 儲存搜尋條件 Key:db欄位, Value:搜尋值
        /// </summary>
        private Dictionary<string, string> _searchConditions = new Dictionary<string,string>();
        /// <summary>
        /// 提供麵包屑Bar資料來源
        /// </summary>
        private XmlDocument _xdoc;
        /// <summary>
        /// 進階搜尋公司列表 Combobox 的資料來源
        /// </summary>
        private BindingList<CompanyItem> _dataCompanys = new BindingList<CompanyItem>();
        /// <summary>
        /// 進階搜尋換頁 Combobox 的資料來源
        /// </summary>
        private BindingList<int> _dataPager = new BindingList<int>();
        /// <summary>
        /// 進階搜尋 總頁數
        /// </summary>
        private int _advTotalPage = 1;
        /// <summary>
        /// 進階搜尋 目前頁 Index
        /// </summary>
        private int _advCurrentPage = 1;
        /// <summary>
        /// 進階搜尋 一頁筆數 Size
        /// </summary>
        private const int _advPageSize = 12;
        #endregion

        /// <summary>
        /// 建構子
        /// </summary>
        public Query()
        {
            InitializeComponent();
            GetCatalog(1);
            GetBreadcrumbBarPath();
            InitAdvanceCatalog();

            lvwClassify.Tag = 1;
            lvwAdvanceClassify.Tag = 0;

            // Initialize catalog level id
            _catalogLevelId.Add(1, 0);
            _catalogLevelId.Add(2, 0);
            _catalogLevelId.Add(3, 0);
            _catalogLevelId.Add(4, 0);
            _catalogLevelId.Add(5, 0);

            // Initialize catalog level name
            _catalogLevelName.Add(2, "");
            _catalogLevelName.Add(3, "");
            _catalogLevelName.Add(4, "");
            _catalogLevelName.Add(5, "");
            _catalogLevelName.Add(6, "");

            // Initialize cmb databinding for Advance Query
            cmbSearchCompany.ItemsSource = _dataCompanys;
            cmbPager.ItemsSource = _dataPager;

            // Initialize search dictionary
             _searchConditions.Add("FileCategoryId","");
             _searchConditions.Add("LastUploadDateStart","");
             _searchConditions.Add("LastUploadDateEnd", "");
             _searchConditions.Add("FileName","");
             _searchConditions.Add("LineId","");
             _searchConditions.Add("CompanyId", "");

            // Example
            // Sample for use store procedure generate full path by FileId
            // string answer = DBHelper.GenerateFileFullPath(1);

        }

        #region User Control Event
        /// <summary>
        /// 載入時把管理資料 AdminItem Binding 至 xaml 以方便 Control 可以直接 Binding
        /// </summary>
        private void query_Loaded(object sender, RoutedEventArgs e)
        {
            query.DataContext = GlobalHelper.AdminItem;
        }
        /// <summary>
        /// Context Menu 右鍵->新增
        /// </summary>
        private void rmenuAdd_Click(object sender, RoutedEventArgs e)
        {
            // 在本層新增
            if (lvwClassify.SelectedItems.Count == 0 || Convert.ToInt32(lvwClassify.Tag) == 5) 
            {
                StringBuilder pathServer = new StringBuilder();
                StringBuilder pathId = new StringBuilder();
                pathServer.Append(_ftpPath);
                pathId.Append(_idPath);
                // ShowDialog
                Create getinput = new Create(400, 200, pathServer.ToString());
                getinput.ShowDialog();
                if (getinput.IsDone)
                {
                    string newFileName = getinput.SystemName;
                    string newNickName = getinput.NickName;
                    pathServer.Append(newFileName);
                    // 產生目錄寫入db
                    CreateFolder(pathServer.ToString(), pathId.ToString(), newNickName);
                }
            }
            // 在下一層新增
            else if (_isTileView) 
            {
                StringBuilder pathServer = new StringBuilder();
                StringBuilder pathId = new StringBuilder();
                pathServer.Append(_ftpPath);
                pathId.Append(_idPath);
                Tile item = lvwClassify.SelectedItem as Tile;
                Dictionary<string, string> tag = item.Tag as Dictionary<string, string>;
                pathServer.Append(tag["Name"]);
                pathId.Append(tag["Id"]);
                Create getInput = new Create(400, 200, pathServer.ToString());
                getInput.ShowDialog();
                string newFileName = "/" + getInput.SystemName;
                string newNickName = getInput.NickName;
                pathServer.Append(newFileName);
                // 產生目錄寫入db
                CreateFolder(pathServer.ToString(), pathId.ToString(), newNickName);
            }
        }
        /// <summary>
        /// Context Menu 右鍵->刪除
        /// </summary>
        private void rmenuDelete_Click(object sender, RoutedEventArgs e)
        {
            if (tabMain.SelectedIndex == 0)
            {
                if (lvwClassify.SelectedItems.Count != 1)
                {
                    return;
                }
                else if (_isTileView) // Delete Folder or File on Tile Mode
                {
                    StringBuilder pathServer = new StringBuilder();
                    StringBuilder pathId = new StringBuilder();
                    pathServer.Append(_ftpPath);
                    pathId.Append(_idPath);
                    Tile item = lvwClassify.SelectedItem as Tile;
                    Dictionary<string, string> tag = item.Tag as Dictionary<string, string>;
                    pathServer.Append(tag["Name"]);
                    pathId.Append(tag["Id"]);
                    // 刪除
                    DeleteFolderOrFile(pathServer.ToString(), pathId.ToString());
                }
                else
                {
                    StringBuilder pathId = new StringBuilder();
                    pathId.Append(_idPath);
                    FileInfo item = lvwClassify.SelectedItem as FileInfo;
                    pathId.Append(item.FileId);
                    // 刪除
                    DeleteFolderOrFile(item.FilePath, pathId.ToString());
                }
            }
            else if (tabMain.SelectedIndex == 1)
            {
                if (lvwAdvanceClassify.SelectedItems.Count != 1)
                {
                    return;
                }
                else if (_isTileView) // Delete Folder or File on Tile Mode
                {
                    StringBuilder pathServer = new StringBuilder();
                    StringBuilder pathId = new StringBuilder();
                    pathServer.Append(_ftpPath);
                    pathId.Append(_idPath);
                    Tile item = lvwClassify.SelectedItem as Tile;
                    Dictionary<string, string> tag = item.Tag as Dictionary<string, string>;
                    pathServer.Append(tag["Name"]);
                    pathId.Append(tag["Id"]);
                    // 刪除
                    DeleteFolderOrFile(pathServer.ToString(), pathId.ToString());
                }
                else
                {
                    StringBuilder pathId = new StringBuilder();
                    pathId.Append(_idPath);
                    FileInfo item = lvwAdvanceClassify.SelectedItem as FileInfo;
                    pathId.Append(item.FileId);
                    // 刪除
                    DeleteFolderOrFile(item.FilePath, pathId.ToString());
                }
            }
        }
        /// <summary>
        /// Context Menu 右鍵->取消
        /// </summary>
        private void rmenuCancelSelected_Click(object sender, RoutedEventArgs e)
        {
            lvwClassify.UnselectAll();
            lvwAdvanceClassify.UnselectAll();
        }
        /// <summary>
        /// Context Menu 右鍵->編輯
        /// </summary>
        private void rmenuEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lvwClassify.SelectedItems.Count != 1)
            {
                return;
            }
            // 只能選取才編輯, 因為最後一層 File 不能編輯所以不用處理 List Mode
            else if (_isTileView)
            {
                StringBuilder pathServer = new StringBuilder();
                StringBuilder pathId = new StringBuilder();
                pathServer.Append(_ftpPath);
                pathId.Append(_idPath);
                Tile item = lvwClassify.SelectedItem as Tile;
                Dictionary<string, string> tag = item.Tag as Dictionary<string, string>;
                pathServer.Append(tag["Name"]);
                pathId.Append(tag["Id"]);
                Update getInput = new Update(400, 200, pathServer.ToString(), item.Title);
                getInput.ShowDialog();
                if (getInput.IsDone)
                {
                    string newNickName = getInput.NickName;
                    string newSystemName = getInput.SystemName;
                    string rebuildPath = getInput.NewPath;
                    string rebuildPathId = pathId.ToString();
                    if (getInput.ClassifyId > 0)
                    {
                        rebuildPathId = "/" + getInput.ClassifyId + pathId.ToString().Substring(pathId.ToString().LastIndexOf('/'));
                    }
                    // 編輯更新欄位
                    RenameFolder(pathServer.ToString(), rebuildPathId, rebuildPath, newNickName);
                }
            }
            
        }
        #endregion

        #region Query Events

        private void tile_Click(object sender, RoutedEventArgs e)
        {
            int level = Convert.ToInt32(lvwClassify.Tag) + 1;

            Tile tile = (Tile)sender;

            if (level == 2)
            {
                navBar.Path = tile.Title;
            }
            else if (level <= 6)
            {
                navBar.Path = String.Format(@"{0}\{1}", navBar.Path, tile.Title);
            }
            else
            {
                // download chosen file here
                Dictionary<string, string> info = (Dictionary<string, string>)tile.Tag;
                DownloadFile(DBHelper.GenerateFileFullPath(Convert.ToInt32(info["Id"])));
            }
        }
        private void lstDown_Click(object sender, RoutedEventArgs e)
        {
            int level = Convert.ToInt32(lvwClassify.Tag) + 1;
            Button btn = (Button)sender;

            if (level == 2)
            {
                navBar.Path = btn.Tag.ToString();
            }
            else if (level <= 6)
            {
                navBar.Path = String.Format(@"{0}\{1}", navBar.Path, btn.Tag.ToString());
            }
            else
            {
                // download chosen file here
                Dictionary<string, string> info = (Dictionary<string, string>)btn.Tag;
                DownloadFile(DBHelper.GenerateFileFullPath(Convert.ToInt32(info["Id"])));
            }
        }
        private void lstDelete_Click(object sender, RoutedEventArgs e)
        { 
        }
        private void lstAdvanceDown_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            DownloadFile(btn.Tag.ToString());
        }
       
        private void navBar_PathChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            string displayPath = navBar.GetDisplayPath();
            string[] pathList = navBar.GetDisplayPath().Split('\\');
            int level = pathList.Count();
            _ftpPath = "/";
            _idPath = "/";
            if (!displayPath.Equals("分類"))
            {
                level = GetCatalogInfo(level, pathList.Last());
                level++;
                // UNDONE:處理因為非同步目錄已不存在問題
                for (int i = 2; i <= level; i++)
                {
                    if (String.IsNullOrEmpty(_catalogLevelName[i]))
                        break;
                    _ftpPath = String.Format("{0}{1}/", _ftpPath, _catalogLevelName[i]);
                    _idPath = String.Format("{0}{1}/", _idPath, _catalogLevelId[i-1]);
                }
            
            }
           
           
            new Thread(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                 new Action(() =>
                 {
                     GetCatalog(level);
                 }));
            }).Start();
            //GetCatalog(level);
            lvwClassify.Tag = level;

            // Lazy loading for BreadcrumbBar
            if (level > 1)
            {
                GetBreadcrumbBarPath(level);
            }
        }
        private void btnTileView_Click(object sender, RoutedEventArgs e)
        {
            _isTileView = true;
            new Thread(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                 new Action(() =>
                 {
                     GetCatalog(Convert.ToInt32(lvwClassify.Tag));
                 }));
            }).Start();
            //GetCatalog(Convert.ToInt32(lvwClassify.Tag));
        }
        private void btnListView_Click(object sender, RoutedEventArgs e)
        {
            _isTileView = false;
            new Thread(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                 new Action(() =>
                 {
                     GetCatalog(Convert.ToInt32(lvwClassify.Tag));
                 }));
            }).Start();
            //GetCatalog(Convert.ToInt32(lvwClassify.Tag));
        }
        private void btnPrevLv_Click(object sender, RoutedEventArgs e)
        {
            string[] paths = navBar.Path.Split(new char[]{'\\'},StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length == 1)
            {
                navBar.Path = "分類";
            }
            else
            {
                string path = navBar.Path.Substring(0, navBar.Path.LastIndexOf("\\"));
                navBar.Path = path;
            }
            
        }
        private void btnQueryHome_Click(object sender, RoutedEventArgs e)
        {
            navBar.Path = "分類";
        }
        private void lvwClassify_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            #region 備註
            /*
            * M1: 1 => Add, Cancel
            * M2: 2 => Add, Edit, Delete, Cancel
            * M3: 3 => Eidt, Delete, Cancel
            * M4: 4 => Cancel
            * M5: 5 => Delete, Cancel
            */
            #endregion
            int level = Convert.ToInt32(lvwClassify.Tag);

            if (lvwClassify.SelectedItems.Count > 0)
            {
                // Tile Right Click 
                lvwClassify.ContextMenu = new ContextMenu();

                if (level < 5)
                {
                    lvwClassify.ContextMenu.ItemsSource = GetMenuItems(2);
                }
                else
                {
                    if(level == 6)
                        lvwClassify.ContextMenu.ItemsSource = GetMenuItems(5);
                    else
                        lvwClassify.ContextMenu.ItemsSource = GetMenuItems(3);
                }

                if (lvwClassify.ContextMenu.Items.Count > 0)
                {
                    lvwClassify.ContextMenu.PlacementTarget = this;
                    lvwClassify.ContextMenu.IsOpen = true;
                }
                else
                {
                    lvwClassify.ContextMenu = null;
                }
            }
            else
            {
                // ListView Right Click 
                lvwClassify.ContextMenu = new ContextMenu();
                if (level <= 5)
                {
                    lvwClassify.ContextMenu.ItemsSource = GetMenuItems(1);
                }
                else
                {
                    lvwClassify.ContextMenu.ItemsSource = GetMenuItems(4);
                }

                if (lvwClassify.ContextMenu.Items.Count > 0)
                {
                    lvwClassify.ContextMenu.PlacementTarget = this;
                    lvwClassify.ContextMenu.IsOpen = true;
                }
                else
                {
                    lvwClassify.ContextMenu = null;
                }

            }
        }
        #region 備註
        /*
         * M1: 1 => Add, Cancel
         * M2: 2 => Add, Edit, Delete, Cancel
         * M3: 3 => Edit, Delete, Cancel
         * M4: 4 => Cancel
         * M5: 5 => Delete, Cancel
        */
        #endregion
        private IEnumerable<object> GetMenuItems(int mode)
        {
            if (GlobalHelper.AdminItem.IsAdmin)
            {
                // Add
                MenuItem itemAdd = new MenuItem();
                itemAdd.Header = "新增";
                itemAdd.Click += rmenuAdd_Click;
                Image imgAdd = new Image();
                imgAdd.Source = new BitmapImage(new Uri("/WFTP;component/Images/icon_plus.png", UriKind.Relative));
                itemAdd.Icon = imgAdd;
                // Cancel
                MenuItem itemCancel = new MenuItem();
                itemCancel.Header = "取消選取";
                itemCancel.Click += rmenuCancelSelected_Click;
                Image imgCancel = new Image();
                imgCancel.Source = new BitmapImage(new Uri("/WFTP;component/Images/icon_cancel.png", UriKind.Relative));
                itemCancel.Icon = imgCancel;
                // Edit
                MenuItem itemEdit = new MenuItem();
                itemEdit.Header = "編輯";
                itemEdit.Click += rmenuEdit_Click;
                Image imgEdit = new Image();
                imgEdit.Source = new BitmapImage(new Uri("/WFTP;component/Images/icon_edit.gif", UriKind.Relative));
                itemEdit.Icon = imgEdit;
                // Delete
                MenuItem itemDelete = new MenuItem();
                itemDelete.Header = "刪除";
                itemDelete.Click += rmenuDelete_Click;
                Image imgDelete = new Image();
                imgDelete.Source = new BitmapImage(new Uri("/WFTP;component/Images/icon_remove.png", UriKind.Relative));
                itemDelete.Icon = imgDelete;

                switch (mode)
                {
                    case 1:
                        yield return itemAdd;
                        yield return new Separator();
                        yield return itemCancel;
                        break;
                    case 2:
                        yield return itemAdd;
                        yield return itemEdit;
                        yield return itemDelete;
                        yield return new Separator();
                        yield return itemCancel;
                        break;
                    case 3:
                        yield return itemEdit;
                        yield return itemDelete;
                        yield return new Separator();
                        yield return itemCancel;
                        break;
                    case 4:
                        yield return itemCancel;
                        break;
                    case 5:
                        yield return itemDelete;
                        yield return new Separator();
                        yield return itemCancel;
                        break;
                }
            }
        }
        private bool IsFindListViewItem(MouseEventArgs e)
        {
            var visualHitTest = VisualTreeHelper.HitTest(lvwClassify, e.GetPosition(lvwClassify)).VisualHit;

            while (visualHitTest != null)
            {
                if (visualHitTest is ListViewItem)
                {
                    return true;
                }
                else if (visualHitTest == lvwClassify)
                {
                    return false ;
                }

                visualHitTest = VisualTreeHelper.GetParent(visualHitTest);
            }

            return false;
        }
        private void lvwClassify_MouseDown(object sender, MouseEventArgs e)
        {
            if (!IsFindListViewItem(e))
            {
                lvwClassify.UnselectAll();
            }
        }
        #endregion

        #region Advance Query Event

        private void tileAdvance_Click(object sender, RoutedEventArgs e)
        {
            Tile originTile = (Tile)sender;
            int level = Convert.ToInt32(lvwAdvanceClassify.Tag);
            
            if (level == 0)
            {
                grdSearch.Visibility = System.Windows.Visibility.Visible;
                
                _searchConditions["FileCategoryId"] = originTile.Tag.ToString();
                lvwAdvanceClassify.Items.Clear();
                lvwAdvanceClassify.Tag = 1;
            }
            else
            {
                DownloadFile(originTile.Tag.ToString());
            }
        }
        // 回Advance Query首頁
        private void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            grdSearch.Visibility = System.Windows.Visibility.Hidden;
            lbMessage.Content = "";
            lbMessage.Visibility = System.Windows.Visibility.Hidden;
            cmbPager.Visibility = System.Windows.Visibility.Hidden;
            InitAdvanceCatalog();
            lvwAdvanceClassify.Tag = 0;
        }
        // 進階查詢項目切換
        private void cmbSearchClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            ComboBoxItem item = cmb.SelectedItem as ComboBoxItem;
            // Hidden All Controls
            if (grdSearch.Children.Contains(btnSearch))
            {
                txtSearch.Visibility = System.Windows.Visibility.Hidden;
                wpDate.Visibility = System.Windows.Visibility.Hidden;
                cmbSearchCompany.Visibility = System.Windows.Visibility.Hidden;
            }
            switch (item.Content.ToString().Trim())
            {
                case "公司":
                    var companys = GetCompanyList();
                    _dataCompanys.Clear();
                    foreach (var c in companys)
                    {
                        _dataCompanys.Add(new CompanyItem { Name = c.CompanyNickName, ClassifyId = c.ClassifyId, CompanyId = c.CompanyId });
                    }
                    cmbSearchCompany.Visibility = System.Windows.Visibility.Visible;
                    break;

                case "日期":
                    wpDate.Visibility = System.Windows.Visibility.Visible;
                    break;

                case "檔名":
                    txtSearch.Visibility = System.Windows.Visibility.Visible;
                    break;

                default:
                    break;
            }
        }
        // 執行搜尋
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _searchConditions["LastUploadDateStart"] = "";
            _searchConditions["LastUploadDateEnd"] = "";
            _searchConditions["FileName"] = "";
            _searchConditions["LineId"] = "";
            _searchConditions["CompanyId"] = "";
            lbMessage.Content = "";
            // 1. Filter conditions
            ComboBoxItem item = cmbSearchClass.SelectedItem as ComboBoxItem;
            switch (item.Content.ToString().Trim())
            {
                case "公司":
                    if (cmbSearchCompany.Visibility == System.Windows.Visibility.Visible)
                    {
                        CompanyItem i = cmbSearchCompany.SelectedItem as CompanyItem;
                        _searchConditions["CompanyId"] = i.CompanyId.ToString();
                    }
                    break;

                case "日期":
                    if (wpDate.Visibility == System.Windows.Visibility.Visible)
                    {
                        if (dtpSearchStart.SelectedDate > dtpSearchEnd.SelectedDate)
                        {
                            DateTime? tmp = dtpSearchStart.SelectedDate;
                            dtpSearchStart.SelectedDate = dtpSearchEnd.SelectedDate;
                            dtpSearchEnd.SelectedDate = tmp;
                        }
                        if (dtpSearchStart.SelectedDate != null)
                            _searchConditions["LastUploadDateStart"] = dtpSearchStart.SelectedDate.Value.ToShortDateString();
                        if (dtpSearchEnd.SelectedDate != null)
                            _searchConditions["LastUploadDateEnd"] = dtpSearchEnd.SelectedDate.Value.ToShortDateString();
                    }

                    break;

                case "檔名":
                    if (txtSearch.Visibility == System.Windows.Visibility.Visible)
                    {
                        _searchConditions["FileName"] = txtSearch.Text.Trim();
                    }
                    break;

                default:
                    break;
            }
            // 設定完條件後執行物件建置
            _advCurrentPage = 1;
            new Thread(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                 new Action(() =>
                 {
                     GenerateListviewItem(_advCurrentPage);
                 }));
            }).Start();
            // GenerateListviewItem();
            
        }
        private void btnAdvanceTileView_Click(object sender, RoutedEventArgs e)
        {
            _isAdvanceTileView = true;
            new Thread(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                 new Action(() =>
                 {
                     GenerateListviewItem(_advCurrentPage);
                 }));
            }).Start();
            // GenerateListviewItem();
        }

        private void btnAdvanceListView_Click(object sender, RoutedEventArgs e)
        {
            _isAdvanceTileView = false;
            new Thread(() =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                 new Action(() =>
                 {
                     GenerateListviewItem(_advCurrentPage);
                 }));
            }).Start();
            // GenerateListviewItem();
        }
        private void cmbPager_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           

        }
        private void cmbPager_DropDownClosed(object sender, EventArgs e)
        {
            if (cmbPager.Items.Count > 0)
            {
                _advCurrentPage = Int32.TryParse(cmbPager.SelectedItem.ToString(), out _advCurrentPage) ? _advCurrentPage : 1;
                new Thread(() =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                     new Action(() =>
                     {
                         GenerateListviewItem(_advCurrentPage);
                     }));
                }).Start();
            }
        }
        private void lvwAdvanceClassify_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            int level = Convert.ToInt32(lvwAdvanceClassify.Tag);
            if (lvwAdvanceClassify.SelectedItems.Count > 0)
            {
                // Tile Right Click 
                lvwAdvanceClassify.ContextMenu = new ContextMenu();
                if (level == 1)
                {
                    lvwAdvanceClassify.ContextMenu.ItemsSource = GetMenuItems(5);
                }
                
                if (lvwAdvanceClassify.ContextMenu.Items.Count > 0)
                {
                    lvwAdvanceClassify.ContextMenu.PlacementTarget = this;
                    lvwAdvanceClassify.ContextMenu.IsOpen = true;
                }
                else
                {
                    lvwAdvanceClassify.ContextMenu = null;
                }
            }
            else
            {
                lvwAdvanceClassify.ContextMenu = new ContextMenu();
                if (lvwAdvanceClassify.ContextMenu.Items.Count > 0)
                {
                    lvwAdvanceClassify.ContextMenu.PlacementTarget = this;
                    lvwAdvanceClassify.ContextMenu.IsOpen = true;
                }
                else
                {
                    lvwAdvanceClassify.ContextMenu = null;
                }
            }
        }
        private void lvwAdvanceClassify_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsFindListViewItem(e))
            {
                lvwAdvanceClassify.UnselectAll();
            }
        }
        #endregion

        #region R Method

        /// <summary>
        /// Query:改善效能第一次載入只讀取第一層
        /// </summary>
        private void GetBreadcrumbBarPath()
        {
            // Combination Datasource of Folder secheme
            // Initialize root
            _xdoc = new XmlDocument();
            XmlNode root = _xdoc.CreateElement("bc");
            XmlAttribute xmlns = _xdoc.CreateAttribute("xmlns");
            xmlns.Value = "";
            XmlAttribute t = _xdoc.CreateAttribute("title");
            t.Value = "分類";
            root.Attributes.Append(xmlns);
            root.Attributes.Append(t);
            _xdoc.AppendChild(root);

            // Append child node
            WFTPDbContext db = new WFTPDbContext();
            // Lv1 must load
            var lv1 = from classify in db.Lv1Classifications
                      select classify;

            foreach (var cls in lv1)
            {
                XmlElement xelClassify = _xdoc.CreateElement("bc");
                xelClassify.SetAttribute("title", cls.NickName);
                xelClassify.SetAttribute("id", cls.ClassifyId.ToString());
                root.AppendChild(xelClassify);
            }
            
            // edit static provider
            XmlDataProvider dataFolders = this.FindResource("dataProvider") as XmlDataProvider;
            dataFolders.Document = _xdoc;
        }
        /// <summary>
        /// Query:效能改善: 延遲載入
        /// </summary>
        /// <param name="level">選擇到的層級才載入</param>
        public void GetBreadcrumbBarPath(int level)
        {
            WFTPDbContext db = new WFTPDbContext();

            // Get remote folder list
            ApiHelper api = new ApiHelper();
            List<string> remoteFolderList = api.Dir(_ftpPath, true).ToList();

            switch (level)
            { 
                case 2:
                    var lv2 = from company in db.Lv2Customers
                              where company.ClassifyId == _catalogLevelId[1] 
                              select company;
                    string expr = String.Format("/bc/bc[@id={0}]", _catalogLevelId[1]);
                    XmlNode xndClassify = _xdoc.SelectSingleNode(expr);
                    if (xndClassify.ChildNodes.Count > 0)
                    {
                        XPathNavigator navigator = _xdoc.CreateNavigator();
                        XPathNavigator first = navigator.SelectSingleNode(expr + "/bc[1]");
                        XPathNavigator last = navigator.SelectSingleNode(expr + "/bc[last()]");
                        navigator.MoveTo(first);
                        navigator.DeleteRange(last);
                    }
                    foreach (var company in lv2)
                    {
                        if (!remoteFolderList.Contains(company.CompanyName)) continue;
                        XmlElement xelCompany = _xdoc.CreateElement("bc");
                        xelCompany.SetAttribute("title", company.CompanyNickName);
                        xelCompany.SetAttribute("id", company.CompanyId.ToString());
                        xndClassify.AppendChild(xelCompany);
                    }
                    break;
                case 3:
                    var lv3 = from branch in db.Lv3CustomerBranches
                              where branch.CompanyId == _catalogLevelId[2] 
                              select branch;
                    expr = String.Format("/bc/bc[@id={0}]/bc[@id={1}]", _catalogLevelId[1], _catalogLevelId[2]);
                    XmlNode xndCompany = _xdoc.SelectSingleNode(expr);
                    if (xndCompany.ChildNodes.Count > 0)
                    {
                        XPathNavigator navigator = _xdoc.CreateNavigator();
                        XPathNavigator first = navigator.SelectSingleNode(expr + "/bc[1]");
                        XPathNavigator last = navigator.SelectSingleNode(expr + "/bc[last()]");
                        navigator.MoveTo(first);
                        navigator.DeleteRange(last);
                    }
                    foreach (var branch in lv3)
                    {
                        if (!remoteFolderList.Contains(branch.BranchName)) continue;
                        XmlElement xelBranch = _xdoc.CreateElement("bc");
                        xelBranch.SetAttribute("title", branch.BranchNickName);
                        xelBranch.SetAttribute("id", branch.BranchId.ToString());
                        xndCompany.AppendChild(xelBranch);
                    }
                    break;
                case 4:
                    var lv4 = from line in db.Lv4Lines
                              where line.BranchId == _catalogLevelId[3]
                              select line;
                    expr = String.Format("/bc/bc[@id={0}]/bc[@id={1}]/bc[@id={2}]", _catalogLevelId[1], _catalogLevelId[2], _catalogLevelId[3]);
                    XmlNode xndBranch = _xdoc.SelectSingleNode(expr);
                    if (xndBranch.ChildNodes.Count > 0)
                    {
                        XPathNavigator navigator = _xdoc.CreateNavigator();
                        XPathNavigator first = navigator.SelectSingleNode(expr + "/bc[1]");
                        XPathNavigator last = navigator.SelectSingleNode(expr + "/bc[last()]");
                        navigator.MoveTo(first);
                        navigator.DeleteRange(last);
                    }
                    foreach (var line in lv4)
                    {
                        if (!remoteFolderList.Contains(line.LineName)) continue;
                        XmlElement xelLine = _xdoc.CreateElement("bc");
                        xelLine.SetAttribute("title", line.LineNickName);
                        xelLine.SetAttribute("id", line.LineId.ToString());
                        xndBranch.AppendChild(xelLine);
                    }
                    break;
                case 5:
                    var lv5 = from category in db.Lv5FileCategorys
                              select category;
                    expr = String.Format("/bc/bc[@id={0}]/bc[@id={1}]/bc[@id={2}]/bc[@id={3}]", _catalogLevelId[1], _catalogLevelId[2], _catalogLevelId[3], _catalogLevelId[4]);
                    XmlNode xndLine = _xdoc.SelectSingleNode(expr);
                    if (xndLine.ChildNodes.Count > 0)
                    {
                        // nothing.
                    }
                    else
                    {
                        foreach (var category in lv5)
                        {
                            XmlElement xelCategory = _xdoc.CreateElement("bc");
                            xelCategory.SetAttribute("title", category.ClassNickName);
                            xelCategory.SetAttribute("id", category.FileCategoryId.ToString());
                            xndLine.AppendChild(xelCategory);
                        }
                    }
                    break;
            }
        }
        /// <summary>
        /// Query:取得目錄內容
        /// </summary>
        /// <param name="level">目錄階層</param>
        private void GetCatalog(int level)
        {
            lvwClassify.ItemsSource = null;
            lvwClassify.Items.Clear();

            // display mode switch btn 
            if (level == 6)
            {
                btnListView.Visibility = Visibility.Visible;
                btnTileView.Visibility = Visibility.Visible;
            }
            else
            {
                btnListView.Visibility = Visibility.Hidden;
                btnTileView.Visibility = Visibility.Hidden;
            }

            // 取得各階層資料
            dynamic classify = null;

            switch (level)
            {
                case 1:
                    classify = GetLv1Catalog();
                    break;
                case 2:
                    classify = GetLv2Catalog();
                    break;
                case 3:
                    classify = GetLv3Catalog();
                    break;
                case 4:
                    classify = GetLv4Catalog();
                    break;
                case 5:
                    classify = GetFileCatalog();
                    break;
                case 6:
                    classify = GetFileList();
                    break;
            }

           
            ApiHelper api = new ApiHelper();
            List<string> remoteFolderFullPathList = api.Dir(_ftpPath).ToList();
            Dictionary<string, string> remoteFileList = new Dictionary<string, string>();
            foreach (var item in remoteFolderFullPathList)
            {
                remoteFileList.Add(item.Substring(item.LastIndexOf('/') + 1), item);
            }

            System.Collections.ObjectModel.ObservableCollection<FileInfo> fileCollection =
                new System.Collections.ObjectModel.ObservableCollection<FileInfo>();

            foreach (var classifyItem in classify)
            {
                if (remoteFileList.ContainsKey(classifyItem.Name))
                {
                    Dictionary<string, string> dicInfo = new Dictionary<string, string>();
                    dicInfo.Add("Id", classifyItem.Id.ToString());
                    dicInfo.Add("Name", classifyItem.Name);

                    if (_isTileView || level < 6)
                    {
                        bool isImageFile = false;

                        lvwClassify.View = lvwClassify.FindResource("TileView") as ViewBase;

                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        if (level < 6)
                        {
                            bitmap.UriSource = new Uri(@"pack://application:,,,/WFTP;component/Icons/folder.ico");
                        }
                        else
                        {
                            ExtensionHelper helper = new ExtensionHelper();

                            string iconPath = helper.GetIconPath(
                                System.IO.Path.GetExtension(classifyItem.NickName));

                            if (iconPath != "img.ico")
                            {
                                bitmap.UriSource = new Uri(iconPath);
                            }
                            else
                            {
                                isImageFile = true;
                                bitmap.UriSource = new Uri(String.Format(GlobalHelper.ApiThumb, remoteFileList[classifyItem.Name]));
                            }
                        }
                        bitmap.EndInit();
                        
                        Image img = new Image();
                        if (!isImageFile)
                        {
                            img.Width = 60;
                            img.Height = 60;
                        }
                        else
                        {
                            img.Width = 120;
                            img.Height = 120;
                        }
                        img.Source = bitmap;

                        string title = Convert.ToString(classifyItem.NickName);
                        Tile tile = new Tile();
                        tile.FontFamily = new FontFamily("Microsoft JhengHei");
                        tile.Width = 120;
                        tile.Height = 120;
                        tile.Margin = new Thickness(5);
                        tile.Content = img;

                        if (level < 6)
                        {
                            if (level == 4)
                            {
                                tile.Count = classifyItem.Counts.ToString();
                            }
                            else
                            {
                                tile.Count = Convert.ToString(api.GetCount(_ftpPath + dicInfo["Name"]));
                            }
                        }
                        else
                        {
                            tile.Count = "";
                        }

                        if (level == 6)
                        {
                            // tile.Tag = remoteFileList[classifyItem.Name];
                            tile.Tag = dicInfo;
                            ToolTip tip = new ToolTip();
                            tip.Content = title;
                            tile.ToolTip = tip;
                        }
                        else
                        {
                            tile.Tag = dicInfo;
                        }
                        tile.Click += new RoutedEventHandler(tile_Click);

                        if (tile.Count == "0")
                        {
                            tile.Background = new SolidColorBrush(Color.FromRgb(255, 93, 93));
                        }
                        if (isImageFile)
                        {
                            tile.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                        }
                        else
                        {
                            tile.Title = title.Length > 12 ? String.Format("{0}…", title.Substring(0, 11)) : title;
                        }
                        
                        lvwClassify.Items.Add(tile);
                    }
                    else
                    {
                        lvwClassify.View = lvwClassify.FindResource("ListView") as ViewBase;

                        fileCollection.Add(new FileInfo{
                            FileName = classifyItem.Name,
                            FilePath = remoteFileList[classifyItem.Name],
                            FileId = classifyItem.Id
                        });
                    }
                }
            }
            if (!_isTileView && level == 6)
            {
                lvwClassify.ItemsSource = fileCollection;
            }
        }
        /// <summary>
        /// Advance: 初始化 Advance Query 第一層
        /// </summary>
        private void InitAdvanceCatalog()
        {
            lvwAdvanceClassify.ItemsSource = null;
            lvwAdvanceClassify.Items.Clear();

            dynamic fileCatalogs = GetOnlyFileCatalog();
            lvwAdvanceClassify.View = lvwAdvanceClassify.FindResource("TileView") as ViewBase;
            foreach (var catalog in fileCatalogs)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(@"pack://application:,,,/WFTP;component/Icons/folder.ico");
                bitmap.EndInit();
                Image img = new Image();
                img.Width = 60;
                img.Height = 60;
                img.Source = bitmap;
                string title = Convert.ToString(catalog.NickName);
                Tile tile = new Tile();
                tile.FontFamily = new FontFamily("Microsoft JhengHei");
                tile.Width = 120;
                tile.Height = 120;
                tile.Margin = new Thickness(5);
                tile.Content = img;
                tile.Tag = catalog.Id;
                tile.Title = title.Length > 12 ? String.Format("{0}…", title.Substring(0, 11)) : title;
                tile.Click += new RoutedEventHandler(tileAdvance_Click);
                lvwAdvanceClassify.Items.Add(tile);
            }
        }
        /// <summary>
        /// Query:從資料庫取得分類名稱及其子項目數量(階層 1)
        /// </summary>
        /// <returns>Lv1 資料</returns>
        private dynamic GetLv1Catalog()
        {
            WFTPDbContext db = new WFTPDbContext();
            var lv1Catalog = from classify in db.Lv1Classifications
                              select new
                              {
                                  Id = classify.ClassifyId,
                                  Name = classify.ClassName,
                                  NickName = classify.NickName,
                              };

            return lv1Catalog;
        }
        /// <summary>
        /// Query:從資料庫取得分類名稱及其子項目數量(階層 2)
        /// </summary>
        /// <returns>Lv2 資料</returns>
        private dynamic GetLv2Catalog()
        {
            WFTPDbContext db = new WFTPDbContext();

            var lv2Catalog = from customer in db.Lv2Customers
                              where customer.ClassifyId == _catalogLevelId[1]
                              select new
                              {
                                  Id = customer.CompanyId,
                                  Name = customer.CompanyName,
                                  NickName = customer.CompanyNickName,
                              };

            return lv2Catalog;
        }
        /// <summary>
        /// Query:從資料庫取得分類名稱及其子項目數量(階層 3)
        /// </summary>
        /// <returns>Lv3 資料</returns>
        private dynamic GetLv3Catalog()
        {
            WFTPDbContext db = new WFTPDbContext();
            var lv3Catalog = from branch in db.Lv3CustomerBranches
                             where branch.CompanyId == _catalogLevelId[2]
                              select new
                              {
                                  Id = branch.BranchId,
                                  Name = branch.BranchName,
                                  NickName = branch.BranchNickName,
                              };

            return lv3Catalog;
        }
        // 
        /// <summary>
        /// Query:從資料庫取得分類名稱及其子項目數量(階層 4)
        /// </summary>
        /// <returns>Lv4 資料</returns>
        private dynamic GetLv4Catalog()
        {
            WFTPDbContext db = new WFTPDbContext();

            var lv4Catalog = from line in db.Lv4Lines
                             where line.BranchId == _catalogLevelId[3]
                             let subCount =
                                 (from fileCatalog in db.Lv5FileCategorys
                                  select fileCatalog).Count()
                             select new
                             {
                                 Id = line.LineId,
                                 Name = line.LineName,
                                 NickName = line.LineNickName,
                                 Counts = subCount
                             };

            return lv4Catalog;
        }
        /// <summary>
        /// Query:從資料庫取得分類名稱及其子項目數量(階層 5)
        /// </summary>
        /// <returns>Lv5 資料</returns>
        private dynamic GetFileCatalog()
        {
            WFTPDbContext db = new WFTPDbContext();
            var fileCatalogList = from fileCatalog in db.Lv5FileCategorys
                             select new
                             {
                                 Id = fileCatalog.FileCategoryId,
                                 Name = fileCatalog.ClassName,
                                 NickName = fileCatalog.ClassNickName,
                             };
            return fileCatalogList;
        }
        /// <summary>
        /// Query:從資料庫取得分類名稱及其子項目數量(階層 6)
        /// </summary>
        /// <returns>Lv6 資料</returns>
        private dynamic GetFileList()
        {
            WFTPDbContext db = new WFTPDbContext();
            var fileList = from file in db.Lv6Files
                           where file.LineId == _catalogLevelId[4] && file.FileCategoryId == _catalogLevelId[5] && file.IsDeleted == false
                           select new
                           {
                               Id = file.FileId,
                               Name = file.FileName,
                               NickName = file.FileName
                           };
            return fileList;
        }
        // Query of Manage: 建立目錄
        private bool CreateFolder(string path, string idPath, string folderName)
        {
            string[] paths = path.Split(new char[] {'/'},StringSplitOptions.RemoveEmptyEntries);
            // ID Path 會少一層用來寫入db用
            string[] ids = idPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            int level = paths.Count();
            ApiHelper api = new ApiHelper();
            switch (level)
            { 
                case 1:
                    try
                    {
                        if (api.CreateDirectory(path))
                        {
                            CLv1Classify.InsertOrUpdate(null, paths[0], folderName);
                            GetBreadcrumbBarPath();
                            navBar.Path = path;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    break;
                case 2:
                    int classfyId = Convert.ToInt32(ids[0]);
                    try
                    {
                        if (api.CreateDirectory(path))
                        {
                            CLv2Customer.InsertOrUpdate(null, paths[1], folderName, classfyId);
                            navBar.Path = path;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    break;
                case 3:
                    int companyId = Convert.ToInt32(ids[1]);
                    try
                    {
                        if (api.CreateDirectory(path))
                        {
                            CLv3CustomerBranch.InsertOrUpdate(null, paths[2], folderName, companyId);
                            navBar.Path = path;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    break;
                case 4:
                    int branchId =  Convert.ToInt32(ids[2]);
                    try
                    {
                        if (api.CreateDirectory(path) && api.CreateCategorys(path))
                        {
                            CLv4Line.InsertOrUpdate(null, paths[3], folderName, branchId);
                            navBar.Path = path;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    break;
                case 5:
                    try
                    {
                        if (api.AddCategorys(paths[4]))
                        {
                            CFileCategory.InsertOrUpdate(null, paths[4], folderName);
                            navBar.Path = path;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    break;
            };
            return true;
        }
        // Query of Manage: 刪除目錄或檔案
        private void DeleteFolderOrFile(string path, string idPath)
        {
            string[] paths = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ids = idPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            int id = 0;
            int level = paths.Count();
            ApiHelper api = new ApiHelper();
            switch (level)
            {
                case 1:
                    id = Convert.ToInt32(ids[0]);
                    if (System.Windows.MessageBox.Show("是否刪除?", "警告", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            if(api.RemoveDirectory(path))
                            {
                                CLv1Classify.Delete(id, GlobalHelper.LoginUserID);
                                GetBreadcrumbBarPath();
                                navBar.Path = path;
                            }
                        }
                        catch(Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
                case 2:
                    id = Convert.ToInt32(ids[1]);
                    if (System.Windows.MessageBox.Show("是否刪除?", "警告", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            if( api.RemoveDirectory(path))
                            {
                                CLv2Customer.Delete(id, GlobalHelper.LoginUserID);
                                navBar.Path = path;
                            }
                        }
                        catch(Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
                case 3:
                    id = Convert.ToInt32(ids[2]);
                    if (System.Windows.MessageBox.Show("是否刪除?", "警告", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            if(api.RemoveDirectory(path))
                            {
                                CLv3CustomerBranch.Delete(id, GlobalHelper.LoginUserID);
                                navBar.Path = path;
                            }
                        }
                         catch(Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
                case 4:
                    id = Convert.ToInt32(ids[3]);
                    if (System.Windows.MessageBox.Show("是否刪除?", "警告", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            if(api.RemoveDirectory(path))
                            {
                                CLv4Line.Delete(id, GlobalHelper.LoginUserID);
                                navBar.Path = path;
                            }
                        }
                        catch(Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
                case 5:
                    id = Convert.ToInt32(ids[4]);
                    if (System.Windows.MessageBox.Show("是否刪除?", "警告", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            // CHECK:
                            int result = api.RemoveCategorys(paths[4]);
                            if (result > 0) // 這邊需要移除所有公司的FileCategory ex BOM,Documents
                            {
                                CFileCategory.Delete(id, GlobalHelper.LoginUserID);
                                navBar.Path = path;
                            }
                        }
                        catch(Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
                case 6:
                    id = Convert.ToInt32(ids[5]);
                    if (System.Windows.MessageBox.Show("是否刪除?", "警告", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            path = DBHelper.GenerateFileFullPath(id);
                            if (api.RemoveDirectory(path))
                            {
                                CFile.Delete(id, GlobalHelper.LoginUserID);
                                navBar.Path = path;
                            }
                        }
                        catch(Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
            };
        }
        // Query of Manage: 編輯名稱
        private void RenameFolder(string path, string idPath, string newPath, string newNickName)
        {
            string[] paths = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string[] newPaths = newPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ids = idPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            int level = paths.Count();
            int id = 0;
            ApiHelper api = new ApiHelper();
            switch (level)
            {
                case 1:
                    id = Convert.ToInt32(ids[0]);
                    if (api.Rename(path, newPath))
                    {
                        try
                        {
                            CLv1Classify.InsertOrUpdate(id, newPaths[0], newNickName);
                            GetBreadcrumbBarPath();
                            navBar.Path = path;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
                case 2:
                    id = Convert.ToInt32(ids[1]);
                    int classfyId = Convert.ToInt32(ids[0]);
                    if (api.Rename(path, newPath))
                    {
                        try
                        {
                            CLv2Customer.InsertOrUpdate(id, newPaths[1], newNickName, classfyId);
                            navBar.Path = path;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
                case 3:
                    id = Convert.ToInt32(ids[2]);
                    if (api.Rename(path, newPath))
                    {
                        try
                        {
                            CLv3CustomerBranch.InsertOrUpdate(id, newPaths[2], newNickName, 0);
                            navBar.Path = path;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
                case 4:
                    id = Convert.ToInt32(ids[3]);
                    if (api.Rename(path, newPath))
                    {
                        try
                        {
                            CLv4Line.InsertOrUpdate(id, newPaths[3], newNickName, 0);
                            navBar.Path = path;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
                case 5:
                    id = Convert.ToInt32(ids[4]);
                    int result = api.RenameCategorys(paths[4], newPaths[4]);
                    if (result >= 0)
                    {
                        try
                        {
                            CFileCategory.InsertOrUpdate(id, newPaths[4], newNickName);
                            navBar.Path = path;
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    break;
            };
        }
        // Advance:從資料庫取得檔案分類名稱(階層 1) 
        private dynamic GetOnlyFileCatalog()
        {
            WFTPDbContext db = new WFTPDbContext();
            var fileCatalogList = from fileCatalog in db.Lv5FileCategorys
                                  select new
                                  {
                                      Id = fileCatalog.FileCategoryId,
                                      Name = fileCatalog.ClassName,
                                      NickName = fileCatalog.ClassNickName
                                  };

            return fileCatalogList;
        }
        // Advance:從資料庫取得檔案列表(階層 2) 使用_searchConditions加入搜尋條件
        private dynamic GetAdvanceFileList()
        {
            WFTPDbContext db = new WFTPDbContext();
            var tmp = db.Lv6Files.Select(x => x);
            foreach(KeyValuePair<string,string> condiction in _searchConditions)
            {
                if (!String.IsNullOrEmpty(condiction.Value))
                {
                    switch (condiction.Key)
                    {
                        case "FileCategoryId":
                            int value = Int32.TryParse(condiction.Value, out value)?value:0;
                            tmp = tmp.Where(x => x.FileCategoryId == value);
                            break;
                        case "LastUploadDateStart":
                            DateTime dateStart = DateTime.TryParse(condiction.Value.ToString(), out dateStart)?dateStart:DateTime.Now.AddDays(-1);
                            tmp = tmp.Where(x => x.LastUploadDate >= dateStart);
                            break;
                        case "LastUploadDateEnd":
                            DateTime dateEnd = DateTime.TryParse(condiction.Value.ToString(), out dateEnd)?dateEnd:DateTime.Now;
                            tmp = tmp.Where(x => x.LastUploadDate < dateEnd);
                            break;
                        case "FileName":
                            string fileWord = condiction.Value.ToString();
                            tmp = tmp.Where(x => x.FileName.Contains(fileWord));
                            break;
                        case "LineId":
                            int line = Convert.ToInt32(condiction.Value);
                            tmp = tmp.Where(x => x.LineId == line);
                            break;
                        case "CompanyId":
                            int companyId = Convert.ToInt32(condiction.Value);
                            List<int> branchIds = db.Lv3CustomerBranches.Where(b => b.CompanyId == companyId).Select(br => br.BranchId).ToList();
                            List<int> lineIds = db.Lv4Lines.Where(li => branchIds.Contains(li.BranchId)).Select(y => y.LineId).ToList();
                            tmp = tmp.Where(x => lineIds.Contains(x.LineId));
                            break;
                    }
                }
            }

            return tmp.Where(x => x.IsDeleted == false).Select(n => new { Id = n.FileId, Name = n.FileName, NickName = n.FileName, FullPath = n.Path});
        }
        private dynamic GetAdvanceFileList(int skipNum, int recordNum)
        {
            WFTPDbContext db = new WFTPDbContext();
            var tmp = db.Lv6Files.Select(x => x);
            foreach (KeyValuePair<string, string> condiction in _searchConditions)
            {
                if (!String.IsNullOrEmpty(condiction.Value))
                {
                    switch (condiction.Key)
                    {
                        case "FileCategoryId":
                            int value = Int32.TryParse(condiction.Value, out value) ? value : 0;
                            tmp = tmp.Where(x => x.FileCategoryId == value);
                            break;
                        case "LastUploadDateStart":
                            DateTime dateStart = DateTime.TryParse(condiction.Value.ToString(), out dateStart) ? dateStart : DateTime.Now.AddDays(-1);
                            tmp = tmp.Where(x => x.LastUploadDate >= dateStart);
                            break;
                        case "LastUploadDateEnd":
                            DateTime dateEnd = DateTime.TryParse(condiction.Value.ToString(), out dateEnd) ? dateEnd : DateTime.Now;
                            tmp = tmp.Where(x => x.LastUploadDate < dateEnd);
                            break;
                        case "FileName":
                            string fileWord = condiction.Value.ToString();
                            tmp = tmp.Where(x => x.FileName.Contains(fileWord));
                            break;
                        case "LineId":
                            int line = Convert.ToInt32(condiction.Value);
                            tmp = tmp.Where(x => x.LineId == line);
                            break;
                        case "CompanyId":
                            int companyId = Convert.ToInt32(condiction.Value);
                            List<int> branchIds = db.Lv3CustomerBranches.Where(b => b.CompanyId == companyId).Select(br => br.BranchId).ToList();
                            List<int> lineIds = db.Lv4Lines.Where(li => branchIds.Contains(li.BranchId)).Select(y => y.LineId).ToList();
                            tmp = tmp.Where(x => lineIds.Contains(x.LineId));
                            break;
                    }
                }
            }
            return tmp.Where(x => x.IsDeleted == false).Select(n => new { Id = n.FileId, Name = n.FileName, NickName = n.FileName, FullPath = n.Path }).Skip(skipNum).Take(recordNum);
        }
        // Advance:從資料庫取得所有公司分類
        private dynamic GetCompanyList()
        {
            WFTPDbContext db = new WFTPDbContext();
            var companyList = from customer in db.Lv2Customers
                              select new
                              {
                                  CompanyId = customer.CompanyId,
                                  CompanyNickName = customer.CompanyNickName,
                                  ClassifyId = customer.ClassifyId
                              };

            return companyList;
        }
        // Advance:執行把資料建置到Listview
        private void GenerateListviewItem(int page)
        {
            lvwAdvanceClassify.ItemsSource = null;
            lvwAdvanceClassify.Items.Clear();
            cmbPager.ItemsSource = null;
            _dataPager.Clear();

            dynamic files = GetAdvanceFileList();
            int totalRecordNum = Enumerable.Count(files);
            _advCurrentPage = page;
            
            if (totalRecordNum <= _advPageSize)
            {
                _advTotalPage = 1;
                cmbPager.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                _advTotalPage = totalRecordNum % _advPageSize == 0 ?
                             totalRecordNum / _advPageSize :
                             totalRecordNum / _advPageSize + 1;
                if (_advTotalPage == 0)
                    _advTotalPage = 1;
               
                for (int i = 1; i <= _advTotalPage; i++)
                {
                    _dataPager.Add(i);
                }
                cmbPager.Visibility = System.Windows.Visibility.Visible;
                cmbPager.SelectedItem = page ;
            }
            cmbPager.ItemsSource = _dataPager;
            int skipNum = (_advCurrentPage - 1) * _advPageSize;
            files = GetAdvanceFileList(skipNum, _advPageSize);

             // For list mode datasource
             System.Collections.ObjectModel.ObservableCollection<FileInfo> fileCollection =
                 new System.Collections.ObjectModel.ObservableCollection<FileInfo>();

            ApiHelper api = new ApiHelper();
            foreach (var file in files)
            {
                if (_isAdvanceTileView)
                {
                    bool isImageFile = false;
                    lvwAdvanceClassify.View = lvwAdvanceClassify.FindResource("TileView") as ViewBase;

                    // Using store procedure to get full path.
                    string path = DBHelper.GenerateFileFullPath(file.Id);
                    if (api.CheckPath(path))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;


                        ExtensionHelper helper = new ExtensionHelper();
                        string iconPath = helper.GetIconPath(
                            System.IO.Path.GetExtension(file.NickName));

                        if (iconPath != "img.ico")
                        {
                            bitmap.UriSource = new Uri(iconPath);
                        }
                        else
                        {
                            isImageFile = true;
                            bitmap.UriSource = new Uri(String.Format(GlobalHelper.ApiThumb, path));
                        }

                        bitmap.EndInit();
                        Image img = new Image();
                        if (!isImageFile)
                        {
                            img.Width = 60;
                            img.Height = 60;
                        }
                        else
                        {
                            img.Width = 120;
                            img.Height = 120;
                        }

                        img.Source = bitmap;
                        string title = Convert.ToString(file.NickName);
                        Tile tile = new Tile();
                        tile.FontFamily = new FontFamily("Microsoft JhengHei");
                        tile.Width = 120;
                        tile.Height = 120;
                        tile.Margin = new Thickness(5);
                        tile.Content = img;
                        tile.Tag = path; // Download Path
                        tile.ToolTip = file.NickName;
                        tile.Click += new RoutedEventHandler(tileAdvance_Click);
                        if (isImageFile)
                        {
                            tile.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                        }
                        else
                        {
                            tile.Title = title.Length > 12 ? String.Format("{0}…", title.Substring(0, 11)) : title;
                        }
                        lvwAdvanceClassify.Items.Add(tile);
                    }
                }
                else
                {
                    lvwAdvanceClassify.View = lvwAdvanceClassify.FindResource("AdvanceListView") as ViewBase;
                     string path = DBHelper.GenerateFileFullPath(file.Id);
                     if (api.CheckPath(path))
                     {
                         fileCollection.Add(new FileInfo
                         {
                             FileName = file.Name,
                             FilePath = DBHelper.GenerateFileFullPath(file.Id),
                             FileId = file.Id
                         });
                     }
                }
            }
            // 修改Listview Datasource
            if (!_isAdvanceTileView)
            {
                lvwAdvanceClassify.ItemsSource = fileCollection;
            }
            if (lvwAdvanceClassify.Items.Count == 0)
            {
                lbMessage.Visibility = System.Windows.Visibility.Visible;
                lbMessage.Content = "搜尋完成,無資料紀錄";
            }
            else
            {
                lbMessage.Visibility = System.Windows.Visibility.Hidden;
                lbMessage.Content = "";
            }
           
        }
        // Query:取得階層名稱及 Id
        private int GetCatalogInfo(int level, string condition)
        {
            WFTPDbContext db = new WFTPDbContext();
            int id = 0;
            
            string name = "";
            // UNDONE: condition 問題
            WHEN_DATA_NULL:
            switch (level)
            {
                case 1:
                    var lv1 = from classify in db.Lv1Classifications
                               where classify.NickName == condition
                               select new
                               {
                                   classify.ClassifyId,
                                   classify.ClassName
                               };
                    if (lv1.Count() > 0)
                    {
                        id = lv1.First().ClassifyId;
                        name = lv1.First().ClassName;
                    }
                    else
                    {
                        level--;
                        goto WHEN_DATA_NULL;
                    }
                    break;
                case 2:
                    var lv2 = from customer in db.Lv2Customers
                              where customer.CompanyNickName == condition
                                    && customer.ClassifyId == _catalogLevelId[level - 1]
                               select new
                               {
                                   customer.CompanyId,
                                   customer.CompanyName
                               };
                    if (lv2.Count() > 0)
                    {
                        id = lv2.First().CompanyId;
                        name = lv2.First().CompanyName;
                    }
                    else
                    {
                        level--;
                        goto WHEN_DATA_NULL;
                    }
                    break;
                case 3:
                    var lv3 = from branch in db.Lv3CustomerBranches
                              where branch.BranchNickName == condition
                                    && branch.CompanyId == _catalogLevelId[level - 1]
                              select new
                              {
                                  branch.BranchId,
                                  branch.BranchName
                              };
                    if (lv3.Count() > 0)
                    {
                        id = lv3.First().BranchId;
                        name = lv3.First().BranchName;
                    }
                    else
                    {
                        level--;
                        goto WHEN_DATA_NULL;
                    }
                    break;
                case 4:
                    var lv4 = from line in db.Lv4Lines
                              where line.LineNickName == condition
                                    && line.BranchId == _catalogLevelId[level - 1]
                               select new
                               {
                                   line.LineId,
                                   line.LineName
                               };
                    if (lv4.Count() > 0)
                    {
                        id = lv4.First().LineId;
                        name = lv4.First().LineName;
                    }
                    else
                    {
                        level--;
                        goto WHEN_DATA_NULL;
                    }
                    break;
                case 5:
                    var lv5 = from catalog in db.Lv5FileCategorys
                              where catalog.ClassNickName == condition
                              select new
                              {
                                  catalog.FileCategoryId,
                                  catalog.ClassName
                              };
                    if (lv5.Count() > 0)
                    {
                        id = lv5.First().FileCategoryId;
                        name = lv5.First().ClassName;
                    }
                    else
                    {
                        level--;
                        goto WHEN_DATA_NULL;
                    }
                    break;
            }
            _catalogLevelId[level] = id;
            _catalogLevelName[level+1] = name;
            return level;
        }
        // FTP:下載
        private void DownloadFile(string filePath)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string fileExt = System.IO.Path.GetExtension(filePath);

            // Configure save file dialog box
            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.FileName = fileName; // Default file name
            dlg.DefaultExt = fileExt; // Default file extension
            dlg.Filter = String.Format("WFTP documents (*{0})|", fileExt); // Filter files by extension 
            dlg.OverwritePrompt = true;
            
            // Show save file dialog box
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Save document 
                fileName = dlg.FileName;

                Switcher.progress.UpdateProgressList("Download", filePath, dlg.FileName);
                
                //MessageBox.Show(filePath + "\n" + fileName + "\n" + fileExt);
            }
        }
        // Image Lazy Loading
        public static Lazy<ImageDrawing> LoadImage(string fileName)
        {
            return new Lazy<ImageDrawing>(() =>
            {
                System.Drawing.Bitmap b = new System.Drawing.Bitmap(fileName);
                System.Drawing.Size s = b.Size;
                System.Windows.Media.ImageDrawing im = new System.Windows.Media.ImageDrawing();
                im.Rect = new System.Windows.Rect(0, 0, s.Width, s.Height);
                im.ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(fileName, UriKind.Absolute));
                return im;
            });
        }

        #endregion


        #region Models

        public class FileInfo
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public int FileId { set; get; }
        }
        // For ComboboxItem of advance query
        public class CompanyItem : INotifyPropertyChanged
        {
            private string _name;
            private int _classifyId;
            private int _companyId;

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
            public int ClassifyId
            {
                get {
                    return _classifyId;
                }
                set {
                    _classifyId = value;
                    RaisePropertyChanged("ClassifyId");
                }
            }
            public int CompanyId {
                get { return _companyId; }
                set
                {
                    _companyId = value;
                    RaisePropertyChanged("CompanyId");
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
        // For ComboboxItem of advance query
        public class ClassifyItem : INotifyPropertyChanged
        {
            private string _name;
            private int _classifyId;
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
            public int ClassifyId
            {
                get
                {
                    return _classifyId;
                }
                set
                {
                    _classifyId = value;
                    RaisePropertyChanged("ClassifyId");
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
                    RaisePropertyChanged("NickName");
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
}
