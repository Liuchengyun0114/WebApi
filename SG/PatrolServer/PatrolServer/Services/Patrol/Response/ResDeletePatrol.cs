﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Serialization;
using System.IO;
using Model;
using PatrolServer.Services.Patrol.Response.Entity;

namespace PatrolServer.Services.Patrol.Response
{
    /// <summary>
    /// 删除特巡报告头部及明细 接口编号=1004
    /// </summary>
    [DataContract]
    public class ResDeletePatrol
    {
        [DataMember(Name = "function_id")]
        public string function_id { get; set; }

        [DataMember(Name = "return_flag")]
        public string return_flag { get; set; }

        [DataMember(Name = "return_msg")]
        public string return_msg { get; set; }

        public ResDeletePatrol()
        {
            this.function_id = ((int)MessageHelper.WebFunctionIDs.DeletePatrol).ToString();//注意枚举ToString直接得到字符串
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

    }
}
