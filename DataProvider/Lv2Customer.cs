using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;
using System.Data.Linq;

namespace DataProvider
{
   [Table(Name = "Lv2Customers")]
   public class CLv2Customer
   {
       private Int32 _CompanyId;
       private String _CompanyName;
       private String _CompanyNickName;
       private Int32 _ClassifyId;
       private DateTime _CreateDate;
       private EntityRef<CLv1Classify> _CLv1Classify;
       private EntitySet<CLv3CustomerBranch> _CLv3CustomerBranch;

       #region Properties
       [Column(IsDbGenerated = true, IsPrimaryKey = true, Storage = "_CompanyId", DbType = "INT NOT NULL IDENTITY", AutoSync = AutoSync.OnInsert)]
       public Int32 CompanyId
       {
           get
           {
               return _CompanyId;
           }
       }

       [Column(Storage = "_CompanyName")]
       public String CompanyName
       {
           get
           {
               return _CompanyName;
           }
           set
           {
               _CompanyName = value;
           }
       }

       [Column(Storage = "_CompanyNickName")]
       public String CompanyNickName
       {
           get
           {
               return _CompanyNickName;
           }
           set
           {
               _CompanyNickName = value;
           }
       }

       [Column(Storage = "_ClassifyId")]
       public int ClassifyId
       {
           get
           {
               return _ClassifyId;
           }
           set
           {
               _ClassifyId = value;
           }
       }

       // EntityRef 單數
       [Association(Storage = "_CLv1Classify", ThisKey = "ClassifyId", DeleteRule = "CASCADE")]
       public CLv1Classify Classify
       {
           get {
               return this._CLv1Classify.Entity;
           }
           set {
               this._CLv1Classify.Entity = value;
           }
       }
       // EntitySet 偶數
       [Association(Storage = "_CLv3CustomerBranch", OtherKey = "CompanyId", DeleteRule = "CASCADE")]
       public EntitySet<CLv3CustomerBranch> Branches
       {
           get { return this._CLv3CustomerBranch; }
           set { this._CLv3CustomerBranch.Assign(value); }
       }

       [Column(Storage = "_CreateDate")]
       public DateTime CreateDate
       {
           get
           {
               return _CreateDate;
           }
           set
           {
               _CreateDate = value;
           }
       }
       #endregion

       public CLv2Customer()
       {
           this._CLv1Classify = new EntityRef<CLv1Classify>();
           this._CLv3CustomerBranch = new EntitySet<CLv3CustomerBranch>();
       }

       #region Methodss
       public static void InsertOrUpdate(int? companyId, string companyName, string companyNickName, int classifyId)
       {
           WFTPDbContext db = new WFTPDbContext();
           if (companyId == null) //Insert
           {
               try
               {
                   CLv2Customer newCustomer = new CLv2Customer();
                   newCustomer.CompanyName = companyName;
                   newCustomer.CompanyNickName = companyNickName;
                   newCustomer.CreateDate = DateTime.Now;
                   newCustomer.ClassifyId = classifyId;
                   db.Lv2Customers.InsertOnSubmit(newCustomer);
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
                   var customer = (from customers in db.GetTable<CLv2Customer>()
                                   where customers.CompanyId == companyId
                                   select customers).SingleOrDefault();
                   if (classifyId > 0)
                    customer.ClassifyId = classifyId;

                   if (!String.IsNullOrEmpty(companyName))
                    customer.CompanyName = companyName;

                   if (!String.IsNullOrEmpty(companyNickName))
                   customer.CompanyNickName = companyNickName;

                   db.SubmitChanges();
               }
               catch (Exception ex)
               {
                   throw ex;
               }
           }
       }
       public static void Delete(int companyId, string loginUserId)
       {
           WFTPDbContext db = new WFTPDbContext();
           try
           {
               var customer = (from customers in db.GetTable<CLv2Customer>()
                               where customers.CompanyId == companyId
                               select customers).SingleOrDefault();
               string record = String.Format("ClassifyId:{0}, CompanyId:{1}, CompanyName:{2}, CompanyNickName:{3}", customer.ClassifyId, customer.CompanyId, customer.CompanyName, customer.CompanyNickName);
               DeleteLog.Insert("dbo.Lv2Customers", record, loginUserId);
               db.Lv2Customers.DeleteOnSubmit(customer);
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