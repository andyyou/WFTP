using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace WFTP.Helper
{
    public static class FocusHelper
    {
        public static void Focus(UIElement element)
        {
            if (!element.Focus())
            {
                element.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(delegate()
                {
                    element.Focus();
                }));
            }
        }
    }

}
