using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    [Table(Name = "Lv1Classifications")]
    public class CLv1Classify
    {
        private Int32 _ClassifyId;
        private String _ClassName;
        private String _NickName;
        private EntitySet<CLv2Customer> _CLv2Customer;
      
        #region Properties
        [Column(IsDbGenerated = true, IsPrimaryKey = true, Storage = "_ClassifyId", DbType = "INT NOT NULL IDENTITY", AutoSync = AutoSync.OnInsert)]
        public Int32 ClassifyId{
            get {
                return _ClassifyId;
            }
        }

        [Column(Storage = "_ClassName")]
        public String ClassName
        {
            get
            {
                return _ClassName;
            }
            set
            {
                _ClassName = value;
            }
        }

        [Column(Storage = "_NickName")]
        public String NickName
        {
            get
            {
                return _NickName;
            }
            set
            {
                _NickName = value;
            }
        }

        // EntitySet 偶數 EntityRef 單數
        [Association(Storage = "_CLv2Customer", OtherKey = "ClassifyId", DeleteRule = "CASCADE")]
        public EntitySet<CLv2Customer> Customers
        {
            get { return this._CLv2Customer; }
            set { this._CLv2Customer.Assign(value); }
        }
        #endregion

        public CLv1Classify()
        {
            this._CLv2Customer = new EntitySet<CLv2Customer>();
        }

        #region Methods
        public static void InsertOrUpdate(int? id, string className, string nickName)
        {
            WFTPDbContext db = new WFTPDbContext();
            if (id == null) //Insert
            {
                try
                {
                    CLv1Classify newClassify = new CLv1Classify();
                    newClassify.ClassName = className;
                    newClassify.NickName = nickName;
                    db.Lv1Classifications.InsertOnSubmit(newClassify);
                    db.SubmitChanges();
                }
                catch(Exception ex)
                {
                    throw ex;
                }

            }
            else //Update
            {
                try
                {
                    var classify = (from classifies in db.GetTable<CLv1Classify>()
                                    where classifies.ClassifyId == id
                                    select classifies).SingleOrDefault();
                    if (!String.IsNullOrEmpty(className))
                        classify.ClassName = className;

                    if (!String.IsNullOrEmpty(nickName))
                    classify.NickName = nickName;
                    db.SubmitChanges();
                }
                catch(Exception ex)
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
                var classify = (from classifies in db.GetTable<CLv1Classify>()
                                where classifies.ClassifyId == id
                                select classifies).SingleOrDefault();
                string record = String.Format("ClassifyId:{0}, ClassName:{1}, NickName:{2}",classify.ClassifyId,classify.ClassName,classify.NickName);
                DeleteLog.Insert("dbo.Lv1Classifications",record,loginUserId);
                db.Lv1Classifications.DeleteOnSubmit(classify);
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static int GetClassifyIdByName(string name)
        {
            WFTPDbContext db = new WFTPDbContext();
            try
            {
                var classify = (from classifies in db.GetTable<CLv1Classify>()
                                where classifies.ClassName == name
                                select classifies).SingleOrDefault();
                return classify.ClassifyId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

    }
}
