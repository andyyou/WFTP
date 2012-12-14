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
        [Column(IsDbGenerated=true, IsPrimaryKey=true)]
        public Int32 ClassifyId;

        [Column]
        public String ClassName;

        [Column]
        public String NickName;

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

    }
}
