using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq.Mapping;

namespace DataProvider
{
    [Table(Name = "SystemConfig")]
    public class SystemConfig
    {
        [Column(Name="FtpHost")]
        public String FtpHost { set; get; }
        [Column(Name = "FtpPort")]
        public Int32 FtpPort { set; get; }
        [Column(Name = "FtpUsername")]
        public String FtpUsername { set; get; }
        [Column(Name = "FtpPassword")]
        public String FtpPassword { set; get; }
        [Column(Name = "FtpComponentCode")]
        public String FtpComponentCode { set; get; }
        [Column(Name = "ApiHost")]
        public String ApiHost { set; get; }
        [Column(Name = "ApiPort")]
        public Int32 ApiPort { set; get; }
        [Column(Name = "ApiKey")]
        public String ApiKey { set; get; }
        [Column(Name = "LimitationDay")]
        public Int32 LimitationDay { set; get; }
    }
}
