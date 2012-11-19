using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace DataProvider
{
   [Table(Name = "Lv2Customers")]
   public class CLv2Customer
   {
       [Column(IsDbGenerated = false, IsPrimaryKey = true)]
       public Int32 CompanyId;

       [Column]
       public String CompanyName;

       [Column]
       public int ClassifyId;

       [Column]
       public DateTime CreateDate;
   }
}