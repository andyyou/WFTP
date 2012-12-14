using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    public class WFTPDbContext : DataContext
    {
        // Constructor
        public WFTPDbContext():base(DBHelper.GetConnctionString().ToString())
        {
           
        }

        //Tables
        public Table<CEmployee> Employees;
        public Table<CLv1Classify> Lv1Classifications;
        public Table<CLv2Customer> Lv2Customers;
        public Table<CLv3CustomerBranch> Lv3CustomerBranches;
        public Table<CLv4Line> Lv4Lines;
        public Table<CFileCategory> Lv5FileCategorys;
        public Table<CFile> Lv6Files;
        public Table<DeleteLog> DeleteLogs;
        
    }
}
