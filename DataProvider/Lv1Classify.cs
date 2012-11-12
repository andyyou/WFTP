using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    [Table(Name = "Lv1Classify")]
    public class CLv1Classify
    {
        [Column(IsDbGenerated=false, IsPrimaryKey=true)]
        public Int32 CId;

        [Column]
        public String ClassName;

        [Column]
        public String NickName;
    }
}
