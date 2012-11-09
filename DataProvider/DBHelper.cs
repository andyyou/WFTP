using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace DataProvider
{
    public static class DBHelper
    {
        public static SqlConnection GetConnection()
        {
            string str = "Data Source=WIN-APPUTU; Initial Catalog=Andy; Persist Security Info=True;User ID=sa;Password=apputu.SQL;";
            SqlConnection conn = new SqlConnection(str);

            return conn;
        }

        public static string GetConnctionString()
        {
            string str = "Data Source=WIN-APPUTU; Initial Catalog=Andy; Persist Security Info=True;User ID=sa;Password=apputu.SQL;";
            return str;
        }

    }
}
