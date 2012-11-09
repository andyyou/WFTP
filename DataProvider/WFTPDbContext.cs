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

    }
}
