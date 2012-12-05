using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    [Table(Name="Files")]
    public class CFile
    {
        [Column(IsDbGenerated = false, IsPrimaryKey = true)]
        public Int32 FileId;

        [Column]
        public int FileCategoryId;

        [Column]
        public int LineId;

        [Column]
        public String OriginFileName;

        [Column]
        public DateTime CreateDate;

        [Column]
        public DateTime LastUploadDate;

        [Column]
        public String CreateUser;

        [Column]
        public String LastEditUser;

        [Column]
        public String FileName;

        [Column]
        public Boolean IsDeleted;

        [Column]
        public string Path;
    }
}
