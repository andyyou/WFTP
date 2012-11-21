using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    [Table(Name = "Lv3CustomerBranches")]
    public class CLv3CustomerBranch
    {
        [Column(IsDbGenerated = false, IsPrimaryKey = true)]
        public Int32 BranchId;

        [Column]
        public String BranchName;

        [Column]
        public String BranchNickName;

        [Column]
        public Int32 CompanyId;

        [Column]
        public DateTime CreateDate;
    }
}
