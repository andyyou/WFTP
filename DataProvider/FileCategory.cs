using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    [Table(Name = "FileCategorys")]
    public class CFileCategory
    {
        [Column(IsDbGenerated = false, IsPrimaryKey = true)]
        public Int32 FileCategoryId;
        [Column]
        public String ClassName;
        [Column]
        public String ClassNickName;
        [Column]
        public DateTime CreateDate;
    }
}
