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
using WFTP.Helper;

namespace WFTP.Pages
{
    /// <summary>
    /// Manage.xaml 的互動邏輯
    /// </summary>
    public partial class Manage : UserControl, ISwitchable
    {
        public Manage()
        {
            InitializeComponent();
        }

        #region ISwitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion

        // 如果該欄位不須更新就留 "" 或 -1  ID為null是新增 有ID就是update
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            CLv1Classify.InsertOrUpdate(null, "PPP", "PPP");
        }

        private void btnLv2_Click(object sender, RoutedEventArgs e)
        {
            CLv2Customer.InsertOrUpdate(null, "TESTCompany", "TSTC", 1);
            CLv2Customer.InsertOrUpdate(null, "TTTTT", "TTTTT", CLv1Classify.GetClassifyIdByName("PP"));
        }

        private void btnLv3_Click(object sender, RoutedEventArgs e)
        {
            CLv3CustomerBranch.InsertOrUpdate(null, "Taiwan", "台灣TEST", 6);
        }

        private void btnLv4_Click(object sender, RoutedEventArgs e)
        {
            CLv4Line.InsertOrUpdate(null, "3", "三號TEST", 191);
        }

        private void btnCategory_Click(object sender, RoutedEventArgs e)
        {
            CFileCategory.InsertOrUpdate(null, "TESTFileCategory", "檔案分類");
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            CFile.InsertOrUpdate(null,2,2,"OriginFileName","RenameFileName",false,GlobalHelper.LoginUserID);

        }


    }
}
