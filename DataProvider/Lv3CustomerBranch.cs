using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace DataProvider
{
    [Table(Name = "Lv3CustomerBranches")]
    public class CLv3CustomerBranch
    {
        private Int32 _BranchId;
        private String _BranchName;
        private String _BranchNickName;
        private Int32 _CompanyId;
        private DateTime _CreateDate;
        private EntityRef<CLv2Customer> _CLv2Customer;
        private EntitySet<CLv4Line> _CLv4Line;

        #region Properties
        [Column(IsDbGenerated = true, IsPrimaryKey = true, Storage = "_BranchId", DbType = "INT NOT NULL IDENTITY")]
        public Int32 BranchId
        {
            get { return _BranchId; }
        }


       [Column(Storage = "_BranchName")]
       public String BranchName
       {
           get { return _BranchName; }
           set { _BranchName = value; }
       }

       [Column(Storage = "_BranchNickName")]
       public String BranchNickName
       {
           get { return _BranchNickName; }
           set { _BranchNickName = value; }
       }

       [Column(Storage = "_CompanyId")]
       public Int32 CompanyId
       {
           get { return _CompanyId; }
           set { _CompanyId = value; }
       }

       [Column(Storage = "_CreateDate")]
       public DateTime CreateDate
       {
           get { return _CreateDate; }
           set { _CreateDate = value; }
       }

       //  EntityRef 單數
       [Association(Storage = "_CLv2Customer", ThisKey = "CompanyId", DeleteRule = "CASCADE")]
       public CLv2Customer Customer
       {
           get
           {
               return this._CLv2Customer.Entity;
           }
           set
           {
               this._CLv2Customer.Entity = value;
           }
       }
       // EntitySet 偶數
       [Association(Storage = "_CLv4Line", OtherKey = "BranchId", DeleteRule = "CASCADE")]
       public EntitySet<CLv4Line> Lines
       {
           get { return this._CLv4Line; }
           set { this._CLv4Line.Assign(value); }
       }
       #endregion

       public CLv3CustomerBranch()
       {
           this._CLv2Customer = new EntityRef<CLv2Customer>();
           this._CLv4Line = new EntitySet<CLv4Line>();
       }

       #region Methods
       public static void InsertOrUpdate(int? branchId, string branchName, string branchNickName, int companyId)
       {
           WFTPDbContext db = new WFTPDbContext();
           if (branchId == null) //Insert
           {
               try
               {
                   CLv3CustomerBranch branch = new CLv3CustomerBranch();
                   branch.BranchName = branchName;
                   branch.BranchNickName = branchNickName;
                   branch.CreateDate = DateTime.Now;
                   branch.CompanyId = companyId;
                   db.Lv3CustomerBranches.InsertOnSubmit(branch);
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
                   var branch = (from branches in db.GetTable<CLv3CustomerBranch>()
                                 where branches.BranchId == branchId
                                 select branches).SingleOrDefault();
                   if (companyId > 0)
                       branch.CompanyId = companyId;

                   if (!String.IsNullOrEmpty(branchName))
                       branch.BranchName = branchName;

                   if (!String.IsNullOrEmpty(branchNickName))
                       branch.BranchNickName = branchNickName;

                   db.SubmitChanges();
               }
               catch (Exception ex)
               {
                   throw ex;
               }
           }
       }
       public static void Delete(int branchId, string loginUserId)
       {
           WFTPDbContext db = new WFTPDbContext();
           try
           {
               var branch = (from branches in db.GetTable<CLv3CustomerBranch>()
                             where branches.BranchId == branchId
                             select branches).SingleOrDefault();
               string record = String.Format("BranchId:{0}, BranchName:{1}, BranchNickName:{2}, CompanyId:{3}, CreateDate:{4}", branch.BranchId, branch.BranchName, branch.BranchNickName, branch.CompanyId, branch.CreateDate);
               DeleteLog.Insert("dbo.Lv3CustomerBranches", record, loginUserId);

               db.GetTable<CLv3CustomerBranch>().DeleteOnSubmit(branch);
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
