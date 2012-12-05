using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace DataProvider
{
    public static class DBHelper
    {
        public static SqlConnection GetConnection()
        {
            string str = "Data Source=192.168.100.248; Initial Catalog=WFTP; Persist Security Info=True;User ID=sa;Password=apputu.SQL;";
            SqlConnection conn = new SqlConnection(str);

            return conn;
        }

        public static string GetConnctionString()
        {
            string str = "Data Source=192.168.100.248; Initial Catalog=WFTP; Persist Security Info=True;User ID=sa;Password=apputu.SQL;";
            return str;
        }

        public static string GenerateFileFullPath(int fileId)
        {
            SqlConnection conn = new SqlConnection(GetConnctionString());
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "GeneratePath";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = conn;
            cmd.Parameters.Add(new SqlParameter("@FileId", fileId));
            conn.Open();

            reader = cmd.ExecuteReader();
            // Data is accessible through the DataReader object here.
            reader.Read();
            string path = reader["Path"].ToString();
            conn.Close();
            return path;
        }

    }
}
