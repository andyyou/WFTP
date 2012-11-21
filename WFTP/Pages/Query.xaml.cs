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

namespace WFTP.Pages
{
    /// <summary>
    /// Query.xaml 的互動邏輯
    /// </summary>
    public partial class Query : UserControl, ISwitchable
    {
        private List<string> _remoteFolders = new List<string>();

        public Query()
        {
            InitializeComponent();
            GetClassify();
            GetBreadcrumbBarPath();
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

        private void GetClassify()
        {
            lvwClassify.Items.Clear();

            WFTPDbContext db = new WFTPDbContext();

            var lv1Classify = from classify in db.Lv1Classifications
                              let subCount = 
                                  (from customer in db.Lv2Customers
                                  where customer.ClassifyId == classify.ClassifyId
                                  select customer).Count()
                              select new
                              {
                                  NickName = classify.NickName,
                                  Counts = subCount
                              };

            foreach (var classifyItem in lv1Classify)
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
                tile.Title = classifyItem.NickName;
                tile.FontFamily = new FontFamily("Microsoft JhengHei");
                tile.Width = 120;
                tile.Height = 120;
                tile.Count = classifyItem.Counts.ToString();
                tile.Margin = new Thickness(5, 5, 5, 5);
                tile.Content = img;
                if (tile.Count == "0")
                {
                    tile.Background = new SolidColorBrush(Color.FromRgb(255, 93, 93));
                }
                
                lvwClassify.Items.Add(tile);
            }
        }

        #region ISwitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void navBar_PathChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            string dump = String.Format("NewValue: {0}\nOldValue: {1}\nOriginSource: {2}\nSource: {3}\nRoutedEvent: {4}", e.NewValue.ToString(), e.OldValue, e.OriginalSource, e.Source,e.RoutedEvent);
            MessageBox.Show(dump);
        }
    }
}
