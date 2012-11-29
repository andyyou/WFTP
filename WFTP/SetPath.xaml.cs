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
using System.Xml;
using DataProvider;
using System.Xml.XPath;
using WFTP.Lib;
using System.IO;

namespace WFTP
{
    /// <summary>
    /// SetPath.xaml 的互動邏輯
    /// </summary>
    public partial class SetPath : Window
    {
        #region Data Members
        private XmlDocument _xdoc;
        private string _ftpPath = "/";
        private Dictionary<int, int> _catalogLevelId = new Dictionary<int, int>();
        private Dictionary<int, string> _catalogLevelName = new Dictionary<int, string>();
        #endregion

        #region Properties
        public string Path { set; get; }
        #endregion

        public SetPath()
        {
            InitializeComponent();
            GetBreadcrumbBarPath();

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
        public SetPath(int x, int y)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = x;
            this.Top = y;
            
            GetBreadcrumbBarPath();

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

        #region Action Events
        private void btnGetPath_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Path = null;
            this.Close();
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
            

            // Lazy loading for BreadcrumbBar
            if (level > 1)
            {
                GetBreadcrumbBarPath(level);
                this.Path = navBar.Path;
            }
        }
        #endregion

        #region R Methods
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

        #endregion

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
                    if (lv1.Count() > 0)
                    {
                        id = lv1.First().ClassifyId;
                        name = lv1.First().ClassName;
                    }
                    else
                    {
                        return;
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
            _catalogLevelName[level + 1] = name;
        }

        /// <summary>
        /// 取得目錄內容
        /// </summary>
        /// <param name="level">目錄階層</param>
        /// <param name="id">關聯 ID</param>
        private void GetCatalog(int level)
        {

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
            }

            List<string> remoteFolderFullPathList = GetFtpCatalog().ToList();
            Dictionary<string, string> remoteFileList = new Dictionary<string, string>();
            foreach (var item in remoteFolderFullPathList)
            {
                remoteFileList.Add(item.Substring(item.LastIndexOf('/') + 1), item);
            }

            
          
            
        }
    }
}
