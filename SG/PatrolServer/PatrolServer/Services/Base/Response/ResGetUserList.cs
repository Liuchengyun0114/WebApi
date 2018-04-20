using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Serialization;
using System.IO;
using System.Data;
using Model.EntityManager;
using PatrolServer.Services.Base.Response.Entity;

namespace PatrolServer.Services.Base.Response
{

    /// <summary>
    /// 获取用户列表返回数据对象 接口编号=1002
    /// </summary>
    [DataContract]
    public class ResGetUserList
    {
        [DataMember(Name = "function_id")]
        public string function_id { get; set; }

        [DataMember(Name = "return_flag")]
        public string return_flag { get; set; }

        [DataMember(Name = "return_msg")]
        public string return_msg { get; set; }

        [DataMember(Name = "userlist")]
        public UserListInfo userlist { get; set; }                

        public ResGetUserList()
        {
            this.function_id = ((int)MessageHelper.AppFunctionIDs.GetUserList).ToString();//注意枚举ToString直接得到字符串
            this.SetFailed();
        }
        public void SetSuccess()
        {
            this.return_flag = ((int)MessageHelper.ReturnFlag.Success).ToString();
            this.return_msg = MessageHelper.ReturnMsg.Success;
        }
        public void SetFailed()
        {
            this.return_flag = ((int)MessageHelper.ReturnFlag.Failed).ToString();
            this.return_msg = MessageHelper.ReturnMsg.Failed;
        }
        //将datatable数据转换为Json
        public static List<UserInfo> Transfer(DataTable source)
        {
            List<UserInfo> ret = new List<UserInfo>();
            if (source != null)
            {
                foreach (DataRow item in source.Rows)
                {
                    UserInfo obj = new UserInfo();
                    obj.user_id = item[UserEntity.PropertyFlag.UserCD.ToString()].ToString();
                    obj.password = item[UserEntity.PropertyFlag.UserPassword.ToString()].ToString();
                    obj.user_name = item[UserEntity.PropertyFlag.UserName.ToString()].ToString();
                    ret.Add(obj);
                }
            }
            return ret;
        }
    }
}
