using System.Windows.Controls;

namespace WFTP
{
    public static class Switcher
    {
        public static Main main;

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
