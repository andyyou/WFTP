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

        public static void InsertOrUpdate(int? id, string categoryName, string categoryNickName)
        {
            WFTPDbContext db = new WFTPDbContext();
            if (id == null) //Insert
            {
                try
                {
                    CFileCategory category = new CFileCategory();
                    category.ClassName = categoryName;
                    category.ClassNickName = categoryNickName;
                    category.CreateDate = DateTime.Now;
                    db.Lv5FileCategorys.InsertOnSubmit(category);
                    db.SubmitChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            else //Update
            {
                try
                {
                    var category = (from categories in db.GetTable<CFileCategory>()
                                    where categories.FileCategoryId == id
                                    select categories).SingleOrDefault();
                    if (!String.IsNullOrEmpty(categoryName))
                        category.ClassName = categoryName;

                    if (!String.IsNullOrEmpty(categoryNickName))
                        category.ClassNickName = categoryNickName;

                    db.SubmitChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
