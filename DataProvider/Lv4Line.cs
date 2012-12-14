using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    [Table(Name = "Lv4Lines")]
    public class CLv4Line
    {
        [Column(IsDbGenerated = true, IsPrimaryKey = true)]
        public Int32 LineId;

        [Column]
        public String LineName;

        [Column]
        public String LineNickName;

        [Column]
        public int BranchId;

        [Column]
        public DateTime CreateDate;

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
    }
}
