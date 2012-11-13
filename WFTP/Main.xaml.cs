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
using DataProvider;
using MahApps.Metro.Controls;

namespace WFTP
{
    /// <summary>
    /// Main.xaml 的互動邏輯
    /// </summary>
    public partial class Main : MetroWindow
    {
        public Main()
        {
            InitializeComponent();
        }

        private void CloseButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void MaximizeButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void ChangeViewButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void MinimizeButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void DragableGridMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            lvwClassify.Items.Clear();

            WFTPDbContext db = new WFTPDbContext();

            var lv1Classify = db.Lv1Classify;

            foreach (var classifyItem in lv1Classify)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Content = classifyItem.NickName;

                Tile tile = new Tile();
                tile.Title = classifyItem.NickName;
                tile.Width = 115;
                tile.Height = 115;
                tile.Count = "5";

                Image img = new Image();
                img.Width = 50;
                img.Height = 50;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(@"pack://application:,,,/WFTP;component/Images/logo.jpg");
                bitmap.EndInit();

                img.Source = bitmap;

                tile.Content = img;

                lvwClassify.Items.Add(tile);
            }
        }

        private void btnManage_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            Slide s = new Slide();
            
        }

        private void btnAdvanceQuery_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
