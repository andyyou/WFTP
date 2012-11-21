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

namespace WFTP.Pages
{
    /// <summary>
    /// Query.xaml 的互動邏輯
    /// </summary>
    public partial class Query : UserControl, ISwitchable
    {
        private List<string> _remoteFolders = new List<string>();
        private Dictionary<int, int> _catalogTree = new Dictionary<int, int>();

        public Query()
        {
            InitializeComponent();
            GetCatalog(1);
            GetBreadcrumbBarPath();
            lvwClassify.Tag = 1;

            //_catalogTree.Add("Lv1Classifications", 0);
            //_catalogTree.Add("Lv2Customers", 0);
            //_catalogTree.Add("Lv3CustomerBranches", 0);
            //_catalogTree.Add("Lv4Lines", 0);
            //_catalogTree.Add("Lv5FileCategorys", 0);
            _catalogTree.Add(1, 0);
            _catalogTree.Add(2, 0);
            _catalogTree.Add(3, 0);
            _catalogTree.Add(4, 0);
            _catalogTree.Add(5, 0);
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
        private void GetCatalog(int level, int id = 0)
        {
            lvwClassify.Items.Clear();

            dynamic classify = null;

            switch (level)
            {
                case 1:
                    classify = GetLv1Catalog();
                    break;
                case 2:
                    classify = GetLv2Catalog(id);
                    break;
                case 3:
                    classify = GetLv3Catalog(id);
                    break;
                case 4:
                    classify = GetLv4Catalog(id);
                    break;
                case 5:
                    classify = GetFileCatalog(id);
                    break;
                case 6:
                    classify = GetFileList(id);
                    break;
            }

            foreach (var classifyItem in classify)
            {
                ListViewItem lvi = new ListViewItem();

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(@"pack://application:,,,/WFTP;component/Icons/folder.ico");
                bitmap.EndInit();

                Image img = new Image();
                img.Width = 60;
                img.Height = 60;
                img.Source = bitmap;

                Tile tile = new Tile();
                string title = Convert.ToString(classifyItem.NickName);
                tile.Title = title.Length > 12 ? String.Format("{0}…", title.Substring(0,11)) : title;

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
                tile.Tag = classifyItem.Id;
                if (level < 6)
                {
                    tile.Click += new RoutedEventHandler(tile_Click);
                }
                if (tile.Count == "0")
                {
                    tile.Background = new SolidColorBrush(Color.FromRgb(255, 93, 93));
                    tile.Click -= new RoutedEventHandler(tile_Click);
                }

                lvwClassify.Items.Add(tile);
            }
        }

        #region 取得分類名稱及其子項目數量

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

        private dynamic GetLv2Catalog(int id)
        {
            WFTPDbContext db = new WFTPDbContext();

            var lv2Catalog = from customer in db.Lv2Customers
                              where customer.ClassifyId == id
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

        private dynamic GetLv3Catalog(int id)
        {
            WFTPDbContext db = new WFTPDbContext();

            var lv3Catalog = from branch in db.Lv3CustomerBranches
                              where branch.CompanyId == id
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

        private dynamic GetLv4Catalog(int id)
        {
            WFTPDbContext db = new WFTPDbContext();

            var lv4Catalog = from line in db.Lv4Lines
                             where line.BranchId == id
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

        private dynamic GetFileCatalog(int id)
        {
            WFTPDbContext db = new WFTPDbContext();

            var fileCatalogList = from fileCatalog in db.Lv5FileCategorys
                             let subCount =
                                 (from file in db.Lv6Files
                                  where file.LineId == id && file.FileCategoryId ==fileCatalog.FileCategoryId
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

        private dynamic GetFileList(int id)
        {
            WFTPDbContext db = new WFTPDbContext();

            var fileList = from file in db.Lv6Files
                           where file.LineId == _catalogTree[4] && file.FileCategoryId == id && file.IsDeleted == false
                           select new
                           {
                               Id = file.FileId,
                               NickName = file.FileName
                           };

            return fileList;
        }
        
        #endregion

        private void tile_Click(object sender, RoutedEventArgs e)
        {
            Tile tile = (Tile)sender;
            int tileTag = (int)tile.Tag;
            _catalogTree[Convert.ToInt32(lvwClassify.Tag)] = tileTag;
            int level = Convert.ToInt32(lvwClassify.Tag) + 1;
            GetCatalog(level, tileTag);
            lvwClassify.Tag = level;
        }

        private void navBar_PathChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            //string dump = String.Format("NewValue: {0}\nOldValue: {1}\nOriginSource: {2}\nSource: {3}\nRoutedEvent: {4}", e.NewValue.ToString(), e.OldValue, e.OriginalSource, e.Source, e.RoutedEvent);
            //MessageBox.Show(dump);
            string[] displayPath = navBar.GetDisplayPath().Split('\\');
            MessageBox.Show(displayPath.Count().ToString());
        }

        #region ISwitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
