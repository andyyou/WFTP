using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace DataProvider
{
    [Table(Name = "Lv4Lines")]
    public class CLv4Line
    {
        private Int32 _LineId;
        private String _LineName;
        private String _LineNickName;
        private Int32 _BranchId;
        private DateTime _CreateDate;
        private EntityRef<CLv3CustomerBranch> _CLv3CustomerBranch;
        private EntitySet<CFile> _CFile;

        #region Properties
        [Column(Storage = "_LineId", IsDbGenerated = true, IsPrimaryKey = true, DbType = "INT NOT NULL IDENTITY")]
        public Int32 LineId
        {
            get { return _LineId; }
        }

        [Column(Storage = "_LineName")]
        public String LineName {
            get { return _LineName; }
            set { _LineName = value; }
        }

        [Column(Storage = "_LineNickName")]
        public String LineNickName 
        {
            get { return _LineNickName; }
            set { _LineNickName = value; }
        }

        [Column(Storage = "_BranchId")]
        public Int32 BranchId 
        {
            get { return _BranchId; }
            set { _BranchId = value; }
        }

        [Column(Storage = "_CreateDate")]
        public DateTime CreateDate
        {
            get { return _CreateDate; }
            set { _CreateDate = value; }
        }
        // EntityRef 單數
        [Association(Storage = "_CLv3CustomerBranch", ThisKey = "BranchId", DeleteRule = "CASCADE")]
        public CLv3CustomerBranch Branch
        {
            get { return this._CLv3CustomerBranch.Entity; }
            set { this._CLv3CustomerBranch.Entity = value; }
        }
        // EntitySet 偶數
        [Association(Storage = "_CFile", OtherKey = "LineId", DeleteRule = "CASCADE")]
        public EntitySet<CFile> Files
        {
            get { return this._CFile; }
            set { this._CFile.Assign(value); }
        }
        #endregion

        public CLv4Line()
        {
            this._CLv3CustomerBranch = new EntityRef<CLv3CustomerBranch>();
            this._CFile = new EntitySet<CFile>();
        }

        #region Methods
        public static void InsertOrUpdate(int? lineId, string lineName, string lineNickName, int branchId)
        {
            WFTPDbContext db = new WFTPDbContext();
            if (lineId == null) //Insert
            {
                try
                {
                    CLv4Line line = new CLv4Line();
                    line.LineName = lineName;
                    line.LineNickName = lineNickName;
                    line.CreateDate = DateTime.Now;
                    line.BranchId = branchId;
                    db.Lv4Lines.InsertOnSubmit(line);
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
                    var line = (from lines in db.GetTable<CLv4Line>()
                                where lines.LineId == lineId
                                select lines).SingleOrDefault();
                    if (branchId>0)
                        line.BranchId = branchId;

                    if (!String.IsNullOrEmpty(lineName))
                        line.LineName = lineName;

                    if (!String.IsNullOrEmpty(lineNickName))
                    line.LineNickName = lineNickName;

                    db.SubmitChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public static void Delete(int lineId, string loginUserId)
        {
            WFTPDbContext db = new WFTPDbContext();
            try
            {
                var line = (from lines in db.GetTable<CLv4Line>()
                            where lines.LineId == lineId
                            select lines).SingleOrDefault();
                string record = String.Format("LineId:{0}, BranchId:{1}, LineName:{2}, LineNickName:{3}, CreateDate:{4}", line.LineId, line.BranchId, line.LineName, line.LineNickName, line.CreateDate);
                DeleteLog.Insert("dbo.Lv4Lines", record, loginUserId);
                db.Lv4Lines.DeleteOnSubmit(line);
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
