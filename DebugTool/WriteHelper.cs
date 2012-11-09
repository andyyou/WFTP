using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DebugTool
{
    public class WriteHelper
    {
        const string PATH = @"c:\temp\WFTP.txt";
        const string ERROR_PATH = @"c:\temp\WFTP_ErrorLog.txt";
        static object lockMe = new object();
        public static int i = 1;
        public WriteHelper()
        {

        }
        public static void Log(string msg )
        {
           
            lock (lockMe)
            {
                using (StreamWriter sw = new StreamWriter(PATH, true))
                {
                    sw.WriteLine(i + ". : " + msg + "\n");
                    sw.Close(); 
                } 
                i++;
            }
        }

        public static void ErrorLog(string msg)
        {

            lock (lockMe)
            {
                using (StreamWriter sw = new StreamWriter(ERROR_PATH, true))
                {
                    sw.WriteLine(DateTime.Now.ToString() + ";  " +  i +  ". : " + msg + "\n");
                    sw.Close();
                }
                i++;
            }
        }
    }
}
