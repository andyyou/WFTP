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
        [Column(IsDbGenerated = true, IsPrimaryKey = true)]
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
    }
}
