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
    /// 特巡用户列表信息映射类,用于序列化Json或者反序列化对象
    /// </summary>
    [DataContract]
    public class StaffUserInfo
    {
        //此处属性用于显示列表时候

        [DataMember(Name = "staffcd")]
        public string staffcd { get; set; }

        [DataMember(Name = "staffname")]
        public string staffname { get; set; }

        [DataMember(Name = "subcompanycd")]
        public string subcompanycd { get; set; }

        [DataMember(Name = "companycd")]
        public string companycd { get; set; }
    }
}
