using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    [Table(Name = "DeleteLog")]
    public class DeleteLog
    {
        [Column(IsDbGenerated = true, IsPrimaryKey = true)]
        public Int32 Id;

        [Column]
        public String TableName;

        [Column]
        public String Record;

        [Column]
        public String EditUser;

        [Column]
        public DateTime Date;

        // Just provide insert for log
        public static void Insert(string tableName, string record, string loginUserId)
        {
            WFTPDbContext db = new WFTPDbContext();

            try
            {
                DeleteLog log = new DeleteLog();
                log.TableName = tableName;
                log.Record = record;
                log.Date = DateTime.Now;
                log.EditUser = loginUserId;
                db.DeleteLogs.InsertOnSubmit(log);
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
