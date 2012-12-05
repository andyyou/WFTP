using System.Windows.Controls;
using WFTP.Pages;

namespace WFTP
{
    public static class Switcher
    {
        public static Main main;
        public static Query query;
        public static Progress download;
        public static Upload upload;
        public static Manage manage;

        public static void Switch(UserControl newPage)
        {
            main.Navigate(newPage);
        }

        public static void Switch(UserControl newPage, object state)
        {
            main.Navigate(newPage, state);
        }
    }
}
