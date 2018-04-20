using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Serialization;
using System.IO;

namespace PatrolServer.Services.Patrol.Request
{
    /// <summary>
    /// 导出特巡报告书到Excel 客户端请求数据包装对象
    /// </summary>
    [DataContract]
    public class ReqExportExcel
    {
        [DataMember(Name = "patrol_no")]
        public string patrol_no { get; set; }
    }
}
