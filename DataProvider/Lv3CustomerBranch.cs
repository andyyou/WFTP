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
        [Column(IsDbGenerated = true, IsPrimaryKey = true)]
        public Int32 BranchId;

        [Column]
        public String BranchName;

        [Column]
        public String BranchNickName;

        [Column]
        public Int32 CompanyId;

        [Column]
        public DateTime CreateDate;

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
                    if (companyId> 0)
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
    }
}
