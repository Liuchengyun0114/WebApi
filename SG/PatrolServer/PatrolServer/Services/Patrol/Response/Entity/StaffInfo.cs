using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Serialization;
using System.IO;

namespace PatrolServer.Services.Patrol.Response.Entity
{
    /// <summary>
    /// 特巡代理店分公司员工三者关联信息映射类,用于序列化Json或者反序列化对象
    /// </summary>
    [DataContract]
    public class StaffInfo
    {
        //此处属性用于显示列表时候

        [DataMember(Name = "code")]
        public string code { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "companycd")]
        public string companycd { get; set; }

        [DataMember(Name = "subcompanycd")]
        public string subcompanycd { get; set; }
    }
}
