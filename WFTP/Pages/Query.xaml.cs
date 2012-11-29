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

        #endregion

        public Query()
        {
            InitializeComponent();
            GetCatalog(1);
            GetBreadcrumbBarPath();
           
            lvwClassify.Tag = 1;

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
        }

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
        /// <param name="id">關聯 ID</param>
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

                    ListViewItem lvi = new ListViewItem();

                    if (_isTileView || level < 6)
                    {
                        bool isImageFile = false;
                        string iconPath = "";

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

                            string[] filename = classifyItem.NickName.Split('.');
                            string ext = filename.Last();
                            iconPath = helper.GetIconPath(ext);

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
                                client.Get(remoteFileList[classifyItem.Name], tmpFolder, localFileName);
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

                        Tile tile = new Tile();
                        string title = Convert.ToString(classifyItem.NickName);
                        tile.Title = title.Length > 12 ? String.Format("{0}…", title.Substring(0, 11)) : title;

                        tile.FontFamily = new FontFamily("Microsoft JhengHei");
                        tile.Width = 120;
                        tile.Height = 120;
                        if (level < 6)
                        {
                            tile.Count = classifyItem.Counts.ToString();
                        }
                        else
                        {
                            tile.Count = "";
                        }
                        tile.Margin = new Thickness(5, 5, 5, 5);
                        tile.Content = img;
                        if (level == 6)
                        {
                            tile.Tag = remoteFileList[classifyItem.Name];
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
                            tile.BorderThickness = new Thickness(100.0);
                            tile.BorderBrush = new SolidColorBrush(Color.FromRgb(196, 196, 196));
                        }

                        lvwClassify.Items.Add(tile);
                    }
                    else
                    {
                        lvwClassify.View = lvwClassify.FindResource("ListView") as ViewBase;

                        //System.Collections.ObjectModel.ObservableCollection<FileInfo> fileCollection = 
                        //    new System.Collections.ObjectModel.ObservableCollection<FileInfo>();

                        fileCollection.Add(new FileInfo{
                            FileName = classifyItem.Name,
                            FilePath = remoteFileList[classifyItem.Name]
                        });
                        //fileCollection.Add(new FileInfo{
                        //    FileName = "2.png",
                        //    FilePath="/test/2.png"
                        //});
                        //fileCollection.Add(new FileInfo{
                        //    FileName = "3.png",
                        //    FilePath="/test/3.png"
                        //});

                        //lvwClassify.ItemsSource = fileCollection;
                    }
                }
            }
            if (!_isTileView && level == 6)
            {
                lvwClassify.ItemsSource = fileCollection;
            }
        }

        #region 從資料庫取得分類名稱及其子項目數量

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

        private dynamic GetFileCatalog()
        {
            WFTPDbContext db = new WFTPDbContext();

            var fileCatalogList = from fileCatalog in db.Lv5FileCategorys
                             let subCount =
                                 (from file in db.Lv6Files
                                  where file.LineId == _catalogLevelId[4] && file.FileCategoryId == fileCatalog.FileCategoryId
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
        
        #endregion

        #region 取得 FTP 資料夾清單

        private string[] GetFtpCatalog()
        {
            FTPClient client = new FTPClient();

            return client.Dir(_ftpPath);
        }

        #endregion

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
                MessageBox.Show("Download Start!!");
            }
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
                MessageBox.Show("Download Start!!");
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
            if(level > 1)
                GetBreadcrumbBarPath(level);
            
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


        #endregion
    

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

        public class FileInfo
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }
        }

        #region ISwitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
