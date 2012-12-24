using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    [Table(Name = "Employees")]
    public class CEmployee
    {
        private Int32 _EId;
        private String _Account;
        private String _Password;
        private DateTime _LastLoginDate;
        private DateTime _CreateDate;
        private String _Rank;
        private String _Name;
        private String _Email;
        private Boolean _RecvNotify;
        private Boolean _Activity;

        #region Properties
        [Column(Storage = "_EId", IsDbGenerated = true, IsPrimaryKey = true, DbType = "INT NOT NULL IDENTITY", AutoSync = AutoSync.OnInsert)]
        public Int32 EId
        {
            get { return _EId; }
        }

        [Column(Storage = "_Account")]
        public String Account
        {
            get { return _Account; }
            set { _Account = value; }
        }

        [Column(Storage = "_Password")]
        public String Password
        {
            get { return _Password; }
            set { _Password = value; }
        }

        [Column(Storage = "_CreateDate")]
        public DateTime CreateDate
        {
            get { return _CreateDate; }
            set { _CreateDate = value; }
        }

        [Column(Storage = "_LastLoginDate")]
        public DateTime LastLoginDate
        {
            get { return _LastLoginDate; }
            set { _LastLoginDate = value; }
        }

        [Column(Storage = "_Rank")]
        public String Rank
        {
            get { return _Rank; }
            set { _Rank = value; }
        }

        [Column(Storage = "_Name")]
        public String Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        [Column(Storage = "_Email")]
        public String Email
        {
            get { return _Email; }
            set { _Email = value; }
        }

        [Column(Storage = "_RecvNotify")]
        public Boolean RecvNotify
        {
            get { return _RecvNotify; }
            set { _RecvNotify = value; }
        }

        [Column(Storage = "_Activity")]
        public Boolean Activity
        {
            get { return _Activity; }
            set { _Activity = value; }
        }
        #endregion

        #region Methods
        public static void InsertOrUpdate(int? eId, string account, string pwd, DateTime createDate, DateTime LastLoginDate, string rank, string name, string email, bool? recvNotify, bool? activity)
        {
            WFTPDbContext db = new WFTPDbContext();
            if (eId == null) //Insert
            {
                try
                {
                    CEmployee e = new CEmployee();
                    e.Account = account;
                    e.Activity = (bool)activity;
                    e.Email = email;
                    e.CreateDate = createDate;
                    e.Name = name;
                    e.Password = pwd;
                    e.Rank = rank;
                    e.RecvNotify = (bool)recvNotify;
                    
                    db.Employees.InsertOnSubmit(e);
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
                    var empolyee = (from e in db.GetTable<CEmployee>()
                                where e.EId == eId
                                select e).SingleOrDefault();
                    if (!String.IsNullOrEmpty(account))
                        empolyee.Account = account;

                    if (!String.IsNullOrEmpty(pwd))
                        empolyee.Password = pwd;

                    if (!String.IsNullOrEmpty(rank))
                        empolyee.Rank = rank;

                    if (!String.IsNullOrEmpty(name))
                        empolyee.Name = name;

                    if (!String.IsNullOrEmpty(email))
                        empolyee.Email = email;

                    if (!String.IsNullOrEmpty(rank))
                        empolyee.Rank = rank;

                    if (!String.IsNullOrEmpty(rank))
                        empolyee.Rank = rank;

                    empolyee.LastLoginDate = DateTime.Now;

                    if (recvNotify.HasValue)
                        empolyee.RecvNotify = (bool)recvNotify;

                    if (activity.HasValue)
                        empolyee.Activity = (bool)activity;

                    db.SubmitChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public static void Delete(int eid, string loginUserId)
        {
            WFTPDbContext db = new WFTPDbContext();
            try
            {
                var empolyee = (from e in db.GetTable<CEmployee>()
                                where e.EId == eid
                                select e).SingleOrDefault();

                string record = String.Format("EId:{0}, Account:{1}, Email:{2}, LastLoginDate:{3}, Name:{4}, Password:{5}, Rank:{6}, RecvNotify:{7}", empolyee.EId, empolyee.Account, empolyee.Activity, empolyee.Email, empolyee.LastLoginDate, empolyee.Name, empolyee.Password, empolyee.Rank, empolyee.RecvNotify);
                DeleteLog.Insert("dbo.Files", record, loginUserId);
                db.Employees.DeleteOnSubmit(empolyee);
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
