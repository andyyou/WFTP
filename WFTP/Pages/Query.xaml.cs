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
using WFTP.Lib;
using System.Xml.XPath;
using System.ComponentModel;

namespace WFTP.Pages
{
    /// <summary>
    /// Query.xaml 的互動邏輯
    /// </summary>
    public partial class Query : UserControl, ISwitchable
    {
        #region DataMembers

        private List<string> _remoteFolders = new List<string>();
        private Dictionary<int, int> _catalogLevelId = new Dictionary<int, int>();
        private Dictionary<int, string> _catalogLevelName = new Dictionary<int, string>();
        private bool _isTileView = true;
        private string _ftpPath = "/";
        private XmlDocument _xdoc;
        // For Advance Query
        private BindingList<CompanyItem> _dataCompanys = new BindingList<CompanyItem>();
        #endregion

        public Query()
        {
            InitializeComponent();
            GetCatalog(1);
            GetBreadcrumbBarPath();
            GetAdvanceCatalog();

            lvwClassify.Tag = 1;
            lvwAdvanceClassify.Tag = 1;

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

        }

        #region Actions Events

        // For Tile Mode
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
                DownloadFile(tile.Tag.ToString());
            }
        }

        private void tileAdvance_Click(object sender, RoutedEventArgs e)
        {

            grdSearch.Visibility = System.Windows.Visibility.Visible;
            Tile tile = (Tile)sender;

            // download chosen file here
            // DownloadFile(tile.Tag.ToString());
            
        }

        // For List Mode
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
                DownloadFile(btn.Tag.ToString());
            }
        }

        private void navBar_PathChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            string displayPath = navBar.GetDisplayPath();
            string[] pathList = navBar.GetDisplayPath().Split('\\');
            int level = pathList.Count();

            _ftpPath = "/";
            if (!displayPath.Equals("分類"))
            {
                GetCatalogInfo(level, pathList.Last());
                level++;

                for (int i = 2; i <= level; i++)
                {
                    _ftpPath = String.Format("{0}{1}/", _ftpPath, _catalogLevelName[i]);
                }
            }

            GetCatalog(level);
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
            GetCatalog(Convert.ToInt32(lvwClassify.Tag));
        }

        private void btnListView_Click(object sender, RoutedEventArgs e)
        {
            _isTileView = false;
            GetCatalog(Convert.ToInt32(lvwClassify.Tag));
        }

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
                        _dataCompanys.Add(new CompanyItem { Name = c.CompanyNickName, ClassifyId = c.ClassifyId, CompanyId = c.CompanyId});
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
        // Advance Tab back to index
        private void btnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            grdSearch.Visibility = System.Windows.Visibility.Hidden;
        }
        #endregion

        #region R Method

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
        /// 效能改善: 延遲載入
        /// </summary>
        /// <param name="level">選擇到的層級才載入</param>
        public void GetBreadcrumbBarPath(int level)
        {
            WFTPDbContext db = new WFTPDbContext();
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
        /// 取得目錄內容
        /// </summary>
        /// <param name="level">目錄階層</param>
        private void GetCatalog(int level)
        {
            lvwClassify.ItemsSource = null;
            lvwClassify.Items.Clear();

            // display mode switch btn 
            if (level == 6)
            {
                // display mode switch btn 
                btnListView.Visibility = Visibility.Visible;
                btnTileView.Visibility = Visibility.Visible;
            }
            else
            {
                btnListView.Visibility = Visibility.Hidden;
                btnTileView.Visibility = Visibility.Hidden;
            }

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

            List<string> remoteFolderFullPathList = GetFtpCatalog().ToList();
            Dictionary<string, string> remoteFileList = new Dictionary<string, string>();
            foreach (var item in remoteFolderFullPathList)
            {
                remoteFileList.Add(item.Substring(item.LastIndexOf('/') + 1), item);
            }

            System.Collections.ObjectModel.ObservableCollection<FileInfo> fileCollection =
                new System.Collections.ObjectModel.ObservableCollection<FileInfo>();

            // 刪除舊有暫存檔
            if (level == 6)
            {
                string[] oldFiles = null;
                oldFiles = System.IO.Directory.GetFiles(System.IO.Path.GetTempPath(),"WFTP*");

                foreach (string file in oldFiles)
                {
                    System.IO.File.Delete(file);
                }
            }

            foreach (var classifyItem in classify)
            {
                if (remoteFileList.ContainsKey(classifyItem.Name))
                //if (true)
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
                                string tmpFolder = System.IO.Path.GetTempPath();
                                string localFileName = string.Format("WFTP-{0}", classifyItem.Name);

                                FTPClient client = new FTPClient();
                                client.Get(remoteFileList[classifyItem.Name], tmpFolder, localFileName, false);
                                bitmap.UriSource = new Uri(String.Format(@"{0}\{1}", tmpFolder, localFileName));
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
                            tile.Count = classifyItem.Counts.ToString();
                        }
                        else
                        {
                            tile.Count = "";
                        }

                        if (level == 6)
                        {
                            tile.Tag = remoteFileList[classifyItem.Name];

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
                            FilePath = remoteFileList[classifyItem.Name]
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
        /// 取得進階搜尋內容
        /// </summary>
        /// <param name="level">階層 進階搜尋只有兩層</param>
        private void GetAdvanceCatalog()
        {
            // UNDONE: 1
            lvwAdvanceClassify.ItemsSource = null;
            lvwAdvanceClassify.Items.Clear();

            dynamic fileCatalogs = GetAdvanceFileCatalog();
            foreach (var catalog in fileCatalogs)
            {
                if (true)
                {
                    lvwAdvanceClassify.View = lvwAdvanceClassify.FindResource("TileView") as ViewBase;
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
                    tile.Title = title.Length > 12 ? String.Format("{0}…", title.Substring(0, 11)) : title;
                    tile.Click +=new RoutedEventHandler(tileAdvance_Click);
                    lvwAdvanceClassify.Items.Add(tile);
                }
            }
            
        }


        // 從資料庫取得分類名稱及其子項目數量(階層 1)
        private dynamic GetLv1Catalog()
        {
            WFTPDbContext db = new WFTPDbContext();

            var lv1Catalog = from classify in db.Lv1Classifications
                              let subCount =
                                  (from customer in db.Lv2Customers
                                   where customer.ClassifyId == classify.ClassifyId
                                   select customer).Count()
                              select new
                              {
                                  Id = classify.ClassifyId,
                                  Name = classify.ClassName,
                                  NickName = classify.NickName,
                                  Counts = subCount
                              };

            return lv1Catalog;
        }

        // 從資料庫取得分類名稱及其子項目數量(階層 2)
        private dynamic GetLv2Catalog()
        {
            WFTPDbContext db = new WFTPDbContext();

            var lv2Catalog = from customer in db.Lv2Customers
                              where customer.ClassifyId == _catalogLevelId[1]
                              let subCount =
                                  (from branch in db.Lv3CustomerBranches
                                   where branch.CompanyId == customer.CompanyId
                                   select branch).Count()
                              select new
                              {
                                  Id = customer.CompanyId,
                                  Name = customer.CompanyName,
                                  NickName = customer.CompanyNickName,
                                  Counts = subCount
                              };

            return lv2Catalog;
        }

        // 從資料庫取得分類名稱及其子項目數量(階層 3)
        private dynamic GetLv3Catalog()
        {
            WFTPDbContext db = new WFTPDbContext();

            var lv3Catalog = from branch in db.Lv3CustomerBranches
                             where branch.CompanyId == _catalogLevelId[2]
                              let subCount =
                                  (from line in db.Lv4Lines
                                   where line.BranchId == branch.BranchId
                                   select line).Count()
                              select new
                              {
                                  Id = branch.BranchId,
                                  Name = branch.BranchName,
                                  NickName = branch.BranchNickName,
                                  Counts = subCount
                              };

            return lv3Catalog;
        }

        // 從資料庫取得分類名稱及其子項目數量(階層 4)
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

        // 從資料庫取得分類名稱及其子項目數量(階層 5)
        private dynamic GetFileCatalog()
        {
            WFTPDbContext db = new WFTPDbContext();

            var fileCatalogList = from fileCatalog in db.Lv5FileCategorys
                             let subCount =
                                 (from file in db.Lv6Files
                                  where file.LineId == _catalogLevelId[4] && file.FileCategoryId == fileCatalog.FileCategoryId && file.IsDeleted == false
                                  select file).Count()
                             select new
                             {
                                 Id = fileCatalog.FileCategoryId,
                                 Name = fileCatalog.ClassName,
                                 NickName = fileCatalog.ClassNickName,
                                 Counts = subCount
                             };

            return fileCatalogList;
        }

        // 從資料庫取得分類名稱及其子項目數量(階層 6)
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

        // 從資料庫取得分類名稱及其子項目數量(階層 5) Advance
        private dynamic GetAdvanceFileCatalog()
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

        // 從資料庫取得分類名稱及其子項目數量(階層 6) Advance
        private dynamic GetAdvanceFileList()
        {
            WFTPDbContext db = new WFTPDbContext();

            var fileList = from file in db.Lv6Files
                           where file.FileCategoryId == 2 && file.IsDeleted == false
                           select new
                           {
                               Id = file.FileId,
                               Name = file.FileName,
                               NickName = file.FileName
                           };

            return fileList;
        }

        // 從資料庫取得所有公司分類
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

        // 取得 FTP 資料夾清單
        private string[] GetFtpCatalog()
        {
            FTPClient client = new FTPClient();

            return client.Dir(_ftpPath);
        }
    
        // 取得階層名稱及 Id
        private void GetCatalogInfo(int level, string condition)
        {
            WFTPDbContext db = new WFTPDbContext();
            int id = 0;
            string name = "";

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
                    id = lv1.First().ClassifyId;
                    name = lv1.First().ClassName;
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
                    id = lv2.First().CompanyId;
                    name = lv2.First().CompanyName;
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
                    id = lv3.First().BranchId;
                    name = lv3.First().BranchName;
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
                    id = lv4.First().LineId;
                    name = lv4.First().LineName;
                    break;
                case 5:
                    var lv5 = from catalog in db.Lv5FileCategorys
                              where catalog.ClassNickName == condition
                              select new
                              {
                                  catalog.FileCategoryId,
                                  catalog.ClassName
                              };
                    id = lv5.First().FileCategoryId;
                    name = lv5.First().ClassName;
                    break;
            }
            _catalogLevelId[level] = id;
            _catalogLevelName[level+1] = name;
        }

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
                MessageBox.Show(filePath + "\n" + fileName + "\n" + fileExt);
            }
        }

        #endregion

        #region FileModel

        public class FileInfo
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }
        }

        #endregion

        #region ISwitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Models
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
