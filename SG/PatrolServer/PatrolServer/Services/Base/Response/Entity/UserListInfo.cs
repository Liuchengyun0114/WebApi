using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Serialization;
using System.IO;

namespace PatrolServer.Services.Base.Response.Entity
{
    /// <summary>
    /// 用户列表信息映射类,用于序列化Json或者反序列化对象
    /// </summary>
    [DataContract]
    public class UserListInfo
    {
        [DataMember(Name = "count")]
        public int count { get; set; }

        [DataMember(Name = "list")]
        public List<UserInfo> list { get; set; }
    }
}
