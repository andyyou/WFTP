using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq;
using System.Data.Linq;
using System.Data.Linq.Mapping;


namespace DataProvider
{
    
    [Table(Name = "Employee")]
    public class CEmployee
    {
        [Column(IsDbGenerated=false, IsPrimaryKey=true)]
        public Int32 EId;

        [Column]
        public String Account;

        [Column]
        public String Password;

        [Column(IsDbGenerated=true)]
        public DateTime LastLoginDate;

        [Column]
        public String Rank;

        [Column]
        public String Name;

        [Column]
        public String Email;

        [Column]
        public Boolean Activity;

    }
}
