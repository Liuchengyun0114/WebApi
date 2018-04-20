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
using Model;
using Model.EntityManager;
using PatrolServer.Services.Patrol.Response.Entity;

namespace PatrolServer.Services.Patrol.Response
{
    /// <summary>
    /// 获取特巡报告查询条件基础数据 接口编号=1000
    /// </summary>
    [DataContract]
    public class ResGetPatrolBase
    {
        [DataMember(Name = "function_id")]
        public string function_id { get; set; }

        [DataMember(Name = "return_flag")]
        public string return_flag { get; set; }

        [DataMember(Name = "return_msg")]
        public string return_msg { get; set; }

        [DataMember(Name = "patrolbase")]
        public PatrolBaseInfo patrolbase { get; set; }

        public ResGetPatrolBase()
        {
            this.function_id = ((int)MessageHelper.WebFunctionIDs.GetPatrolBase).ToString();//注意枚举ToString直接得到字符串
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
        public static List<StaffInfo> Transfer(DataTable source)
        {
            List<StaffInfo> ret = new List<StaffInfo>();
            foreach (DataRow item in source.Rows)
            {
                StaffInfo obj = new StaffInfo();
                obj.code = item[BaseEntity.HeaderPropertyFlag.StaffCD.ToString()].ToString();
                obj.name = item[BaseEntity.HeaderPropertyFlag.StaffName.ToString()].ToString();
                obj.subcompanycd = item[BaseEntity.HeaderPropertyFlag.SubCompanyCD.ToString()].ToString();
                obj.companycd = item[BaseEntity.HeaderPropertyFlag.CompanyCD.ToString()].ToString();

                ret.Add(obj);
            }
            return ret;
        }
        //将代理店转换成列表
        public static List<CompanyInfo> Transfer(List<COMPANYMST> source)
        {
            List<CompanyInfo> ret = new List<CompanyInfo>();
            foreach (COMPANYMST item in source)
            {
                CompanyInfo obj = new CompanyInfo();
                obj.code = item.COMPANYCD;
                obj.name = item.COMPANYNM;

                ret.Add(obj);
            }
            return ret;
        }
        //将分公司转换成列表
        public static List<SubCompanyInfo> Transfer(List<SUBCOMPANYMST> source)
        {
            List<SubCompanyInfo> ret = new List<SubCompanyInfo>();
            foreach (SUBCOMPANYMST item in source)
            {
                SubCompanyInfo obj = new SubCompanyInfo();
                obj.code = item.SUBCOMPANYCD;
                obj.name = item.SUBCOMPANYNM;
                obj.parent = item.COMPANYCD;

                ret.Add(obj);
            }
            return ret;
        }

    }
}
