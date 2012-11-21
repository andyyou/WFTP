using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    [Table(Name = "Lv4Lines")]
    public class CLv4Line
    {
        [Column(IsDbGenerated = false, IsPrimaryKey = true)]
        public Int32 LineId;

        [Column]
        public String LineName;

        [Column]
        public String LineNickName;

        [Column]
        public int BranchId;

        [Column]
        public DateTime CreateDate;

    }
}
