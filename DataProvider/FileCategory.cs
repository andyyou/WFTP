using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace DataProvider
{
    [Table(Name = "FileCategorys")]
    public class CFileCategory
    {
        private Int32 _FileCategoryId;
        private String _ClassName;
        private String _ClassNickName;
        private DateTime _CreateDate;
        private EntitySet<CFile> _CFile;

        #region Properties
        [Column(Storage = "_FileCategoryId", IsDbGenerated = true, IsPrimaryKey = true, DbType = "INT NOT NULL IDENTITY", AutoSync = AutoSync.OnInsert)]
        public Int32 FileCategoryId
        {
            get { return _FileCategoryId; }
        }

        [Column(Storage = "_ClassName")]
        public String ClassName
        {
            get { return _ClassName; }
            set { _ClassName = value; }
        }

        [Column(Storage = "_ClassNickName")]
        public String ClassNickName
        {
            get { return _ClassNickName; }
            set { _ClassNickName = value; }
        }

        [Column(Storage = "_CreateDate")]
        public DateTime CreateDate
        {
            get { return _CreateDate; }
            set { _CreateDate = value; }
        }
        // EntitySet 偶數
        [Association(Storage = "_CFile", OtherKey = "LineId", DeleteRule = "CASCADE")]
        public EntitySet<CFile> Files
        {
            get { return this._CFile; }
            set { this._CFile.Assign(value); }
        }
        #endregion

        public CFileCategory()
        {
            this._CFile = new EntitySet<CFile>();
        }

        #region Methods
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
        public static void Delete(int id, string loginUserId)
        {
            WFTPDbContext db = new WFTPDbContext();
            try
            {
                var category = (from categories in db.GetTable<CFileCategory>()
                                where categories.FileCategoryId == id
                                select categories).SingleOrDefault();
                string record = String.Format("FileCategoryId:{0}, ClassName:{1}, ClassNickName:{2}, CreateDate:{3}", category.FileCategoryId, category.ClassName, category.ClassNickName, category.CreateDate);
                DeleteLog.Insert("dbo.FileCategorys", record, loginUserId);
                db.Lv5FileCategorys.DeleteOnSubmit(category);
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
