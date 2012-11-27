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

namespace WFTP.Pages
{
    /// <summary>
    /// Query.xaml 的互動邏輯
    /// </summary>
    public partial class Query : UserControl, ISwitchable
    {
        private List<string> _remoteFolders = new List<string>();
        private Dictionary<int, int> _catalogLevelId = new Dictionary<int, int>();
        private Dictionary<int, string> _catalogLevelName = new Dictionary<int, string>();
        private bool _isTileView = true;
        private string _ftpPath = "/";

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
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.CreateElement("bc");
            XmlAttribute xmlns = doc.CreateAttribute("xmlns");
            xmlns.Value = "";
            XmlAttribute t = doc.CreateAttribute("title");
            t.Value = "分類";
            root.Attributes.Append(xmlns);
            root.Attributes.Append(t);
            doc.AppendChild(root);

            // Append child node
            WFTPDbContext db = new WFTPDbContext();
            // Lv1
            var lv1 = from classify in db.Lv1Classifications
                      select classify;
            foreach (var cls in lv1)
            {
                XmlElement xelClassify = doc.CreateElement("bc");
                xelClassify.SetAttribute("title", cls.NickName);
                 // Lv2
                var lv2 = from company in db.Lv2Customers
                          where company.ClassifyId == cls.ClassifyId
                          select company;
               
                foreach (var company in lv2)
                {
                    XmlElement xelCompany = doc.CreateElement("bc");
                    xelCompany.SetAttribute("title", company.CompanyNickName);
                    // Lv3
                    var lv3 = from branch in db.Lv3CustomerBranches
                              where branch.CompanyId == company.CompanyId
                              select branch;
                    foreach (var branch in lv3)
                    {
                        XmlElement xelBranch = doc.CreateElement("bc");
                        xelBranch.SetAttribute("title", branch.BranchNickName);
                        // Lv4
                        var lv4 = from line in db.Lv4Lines
                                  where line.BranchId == branch.BranchId
                                  select line;
                        foreach (var line in lv4)
                        {
                            XmlElement xelLine = doc.CreateElement("bc");
                            xelLine.SetAttribute("title", line.LineNickName);
                            // Lv5
                            var lv5 = from category in db.Lv5FileCategorys
                                      select category;
                            foreach (var category in lv5)
                            {
                                XmlElement xelFileCategory = doc.CreateElement("bc");
                                xelFileCategory.SetAttribute("title", category.ClassNickName);
                                xelLine.AppendChild(xelFileCategory);
                            }

                            xelBranch.AppendChild(xelLine);
                        }
                        xelCompany.AppendChild(xelBranch);
                    }
                    xelClassify.AppendChild(xelCompany);
                }
                root.AppendChild(xelClassify);

            }

            // edit static provider
            XmlDataProvider dataFolders = this.FindResource("dataProvider") as XmlDataProvider;
            dataFolders.Document = doc;
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

            foreach (var classifyItem in classify)
            {
                if (remoteFileList.ContainsKey(classifyItem.Name))
                {
                    Dictionary<string, string> dicInfo = new Dictionary<string, string>();
                    dicInfo.Add("Id", classifyItem.Id.ToString());
                    dicInfo.Add("Name", classifyItem.Name);

                    ListViewItem lvi = new ListViewItem();

                    if (_isTileView || level < 6)
                    {
                        lvwClassify.View = lvwClassify.FindResource("TileView") as ViewBase;

                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        if (level < 6)
                        {
                            bitmap.UriSource = new Uri(@"pack://application:,,,/WFTP;component/Icons/folder.ico");
                        }
                        else
                        {
                            ExtensionHelper helper = new ExtensionHelper();

                            string[] filename = classifyItem.NickName.Split('.');
                            string ext = filename.Last();

                            bitmap.UriSource = new Uri(helper.GetIconPath(ext));
                        }
                        bitmap.EndInit();

                        Image img = new Image();
                        img.Width = 60;
                        img.Height = 60;
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

                        lvwClassify.Items.Add(tile);
                    }
                    else
                    {
                        lvwClassify.View = lvwClassify.FindResource("ListView") as ViewBase;

                        System.Collections.ObjectModel.ObservableCollection<FileInfo> fileCollection = 
                            new System.Collections.ObjectModel.ObservableCollection<FileInfo>();

                        fileCollection.Add(new FileInfo{
                            FileName = "1.png",
                            FilePath="/test/1.png"
                        });
                        fileCollection.Add(new FileInfo{
                            FileName = "2.png",
                            FilePath="/test/2.png"
                        });
                        fileCollection.Add(new FileInfo{
                            FileName = "3.png",
                            FilePath="/test/3.png"
                        });

                        lvwClassify.ItemsSource = fileCollection;
                    }
                }
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
        // For list mode
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


        #region Test

        
        
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
