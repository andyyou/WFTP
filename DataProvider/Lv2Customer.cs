using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace DataProvider
{
   [Table(Name = "Lv2Customers")]
   public class CLv2Customer
   {
       [Column(IsDbGenerated = true, IsPrimaryKey = true)]
       public Int32 CompanyId;

       [Column]
       public String CompanyName;

       [Column]
       public String CompanyNickName;

       [Column]
       public int ClassifyId;

       [Column]
       public DateTime CreateDate;

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

       
   }
}