using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Data.Linq;


namespace DataProvider
{
    [Table(Name="Files")]
    public class CFile
    {
        private Int32 _FileId;
        private Int32 _FileCategoryId;
        private Int32 _LineId;
        private String _OriginFileName;
        private DateTime _CreateDate;
        private DateTime _LastUploadDate;
        private String _CreateUser;
        private String _LastEditUser;
        private String _FileName;
        private Boolean _IsDeleted;
        private String _FileHash;
        private String _Path;
        private EntityRef<CLv4Line> _CLv4Line;
        private EntityRef<CFileCategory> _CFileCategory;

        #region Properties
        [Column(Storage = "_FileId", IsDbGenerated = true, IsPrimaryKey = true, DbType = "INT NOT NULL IDENTITY")]
        public Int32 FileId
        {
            get { return _FileId; }
        }

        [Column(Storage = "_FileCategoryId")]
        public Int32 FileCategoryId
        {
            get { return _FileCategoryId; }
            set { _FileCategoryId = value; }
        }

        [Column(Storage = "_LineId")]
        public Int32 LineId
        {
            get { return _LineId; }
            set { _LineId = value; }
        }

        [Column(Storage = "_OriginFileName")]
        public String OriginFileName
        {
            get { return _OriginFileName; }
            set { _OriginFileName = value; }
        }

        [Column(Storage = "_CreateDate")]
        public DateTime CreateDate
        {
            get { return _CreateDate; }
            set { _CreateDate = value; }
        }

        [Column(Storage = "_LastUploadDate")]
        public DateTime LastUploadDate
        {
            get { return _LastUploadDate; }
            set { _LastUploadDate = value; }
        }

        [Column(Storage = "_CreateUser")]
        public String CreateUser
        {
            get { return _CreateUser; }
            set { _CreateUser = value; }
        }

        [Column(Storage = "_LastEditUser")]
        public String LastEditUser
        {
            get { return _LastEditUser; }
            set { _LastEditUser = value; }
        }

        [Column(Storage = "_FileName")]
        public String FileName
        {
            get { return _FileName; }
            set { _FileName = value; }
        }

        [Column(Storage = "_IsDeleted")]
        public Boolean IsDeleted
        {
            get { return _IsDeleted; }
            set { _IsDeleted = value; }
        }

        [Column(Storage = "_FileHash")]
        public String FileHash
        {
            get { return _FileHash; }
            set { _FileHash = value; }
        }

        [Column(Storage = "_Path")]
        public String Path
        {
            get { return _Path; }
            set { _Path = value; }
        }

        // EntitySet 偶數 EntityRef 單數
        [Association(Storage = "_CLv4Line", ThisKey = "LineId", DeleteRule = "CASCADE")]
        public CLv4Line Line
        {
            get { return this._CLv4Line.Entity; }
            set { this._CLv4Line.Entity = value; }
        }

        [Association(Storage = "_CFileCategory", ThisKey = "FileCategoryId", DeleteRule = "CASCADE")]
        public CFileCategory FileCategory
        {
            get { return this._CFileCategory.Entity; }
            set { this._CFileCategory.Entity = value; }
        }

        #endregion

        public CFile()
        {
            this._CLv4Line = new EntityRef<CLv4Line>();
            this._CFileCategory = new EntityRef<CFileCategory>();
        }

        #region Methods
        public static void InsertOrUpdate(int? fileId,int categoryId, int lineId, string originFileName, string fileName, bool? isDelete, string loginUserID)
        {
            WFTPDbContext db = new WFTPDbContext();
            if (fileId == null) //Insert
            {
                try
                {
                    CFile f = new CFile();
                    f.FileCategoryId = categoryId;
                    f.LineId = lineId;
                    f.OriginFileName = originFileName;
                    f.FileName = fileName;
                    f.IsDeleted = false;
                    f.CreateDate = DateTime.Now;
                    f.LastUploadDate = DateTime.Now;
                    f.LastEditUser = loginUserID;
                    f.CreateUser = loginUserID;
                    db.Lv6Files.InsertOnSubmit(f);
                    db.SubmitChanges();
                    var file = (from files in db.GetTable<CFile>()
                                    where files.LineId == f.LineId && files.FileCategoryId == f.FileCategoryId
                                    && files.CreateUser == f.CreateUser && files.OriginFileName == f.OriginFileName
                                    select files).LastOrDefault();
                    file.Path = DBHelper.GenerateFileFullPath(file.FileId);
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
                    var file = (from files in db.GetTable<CFile>()
                                where files.FileId == fileId
                                select files).SingleOrDefault();
                    if (categoryId > 0)
                        file.FileCategoryId = categoryId;

                    if (!String.IsNullOrEmpty(fileName))
                        file.FileName = fileName;

                    if (isDelete.HasValue)
                        file.IsDeleted = (bool)isDelete;
                    
                    file.LastEditUser = loginUserID;
                    file.LastUploadDate = DateTime.Now;

                    if(lineId > 0)
                        file.LineId = lineId;

                    if (!String.IsNullOrEmpty(originFileName))
                        file.OriginFileName = originFileName;

                    file.Path = DBHelper.GenerateFileFullPath(file.FileId);
                    db.SubmitChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public static void Delete(int fileId, string loginUserId)
        {
            WFTPDbContext db = new WFTPDbContext();
            try
            {
                var file = (from files in db.GetTable<CFile>()
                            where files.FileId == fileId
                            select files).SingleOrDefault();
                string record = String.Format("FileId:{0}, LineId:{1}, FileName:{2}, FileHash:{3}, Path:{4}, CreateDate:{5}", file.FileId, file.LineId, file.FileName, file.FileHash, file.Path, file.CreateDate);
                DeleteLog.Insert("dbo.Files", record, loginUserId);
                db.Lv6Files.DeleteOnSubmit(file);
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
