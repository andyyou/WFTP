using System.Windows.Controls;
using WFTP.Pages;

namespace WFTP
{
    /// <summary>
    /// 載入 UserControl into Main.xml Controller
    /// </summary>
    public static class Switcher
    {
        public static Main main;
        public static Query query;
        public static Progress progress;
        public static Upload upload;
        public static Manage manage;
        public static Login login;

        public static void Switch(UserControl newPage)
        {
            main.Navigate(newPage);
        }
    }
}
