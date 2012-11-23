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
using System.Collections.ObjectModel;

namespace WFTP.Pages
{
    /// <summary>
    /// Upload.xaml 的互動邏輯
    /// </summary>
    public partial class Upload : UserControl, ISwitchable
    {
        public Upload()
        {
            InitializeComponent();
            // For test
            var data = new ObservableCollection<FileItem>();
            data.Add(new FileItem() { Name = "File-1",  TargetPath = "/PP/TUC/XX"});
            data.Add(new FileItem() { Name = "File-2",  TargetPath = "/PP/TUC/XX" });
            data.Add(new FileItem() { Name = "File-3",  TargetPath = "/PP/TUC/XX" });
            data.Add(new FileItem() { Name = "File-4",  TargetPath = "/PP/TUC/XX" });
            lvwToUplpad.DataContext = data;
            lvwTempList.DataContext = data;
        }


        #region ISwitchable Members

        public void UtilizeState(object state)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    #region Sapple Data For Test
    public class FileItem
    {
        public string Name { set; get; }
        public string TargetPath { set; get; }

    }
    #endregion
}
