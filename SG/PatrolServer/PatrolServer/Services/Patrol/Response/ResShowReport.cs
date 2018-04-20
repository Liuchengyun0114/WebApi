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
    /// 查看特巡报告头部及明细生成报表 接口编号=1005
    /// </summary>
    [DataContract]
    public class ResShowReport
    {
        [DataMember(Name = "function_id")]
        public string function_id { get; set; }

        [DataMember(Name = "return_flag")]
        public string return_flag { get; set; }

        [DataMember(Name = "return_msg")]
        public string return_msg { get; set; }

        [DataMember(Name = "patrol_header")]
        public PatrolHeaderInfo patrol_header { get; set; }

        [DataMember(Name = "facade_list")]
        public List<PatrolDetailInfo> facade_list { get; set; }

        [DataMember(Name = "patrol_detail_list")]
        public List<PatrolDetailInfo> patrol_detail_list { get; set; }

        public ResShowReport()
        {
            this.function_id = ((int)MessageHelper.WebFunctionIDs.ShowReport).ToString();//注意枚举ToString直接得到字符串
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
        public static List<PatrolDetailInfo> getFacadeImageList(List<PatrolDetailInfo> source)
        {
            List<PatrolDetailInfo> ret = new List<PatrolDetailInfo>();
            if (source != null && source.Count > 0)
            {
                List<PatrolDetailInfo> indexList = new List<PatrolDetailInfo>();
                //取得外观一条记录
                for (int i = 0; i < source.Count; i++)
                {
                    PatrolDetailInfo item = source[i];

                    if (item.location_code == "SP0001")
                    {
                        //新增外观图片
                        ret.Add(item);
                        indexList.Add(item);
                        break;
                    }
                }
                ////取得铭牌一条记录
                //for (int i = 0; i < source.Count; i++)
                //{
                //    PatrolDetailInfo item = source[i];

                //    if (item.location_code == "SP0002")
                //    {
                //        //新增铭牌图片
                //        ret.Add(item);
                //        indexList.Add(item);
                //        break;
                //    }
                //}
                ////取得工作小时表一条记录
                //for (int i = 0; i < source.Count; i++)
                //{
                //    PatrolDetailInfo item = source[i];

                //    if (item.location_code == "SP0013")
                //    {
                //        //新增工作小时表图片
                //        ret.Add(item);
                //        indexList.Add(item);
                //        break;
                //    }
                //}
                //原列表删除对象
                foreach (PatrolDetailInfo item in indexList)
                {
                    if (source.Contains(item))
                    {
                        source.Remove(item);
                    } 
                }
            }

            //只取外观两张图片信息
            return ret;
        }

        //将datatable数据转换为Json
        public static List<PatrolDetailInfo> getPatrolDetailList(DataTable source)
        {
            List<PatrolDetailInfo> ret = new List<PatrolDetailInfo>();
            foreach (DataRow item in source.Rows)
            {
                PatrolDetailInfo obj = new PatrolDetailInfo();
                obj.patrol_no = item[PatrolEntity.DetailPropertyFlag.PatrolNo.ToString()].ToString();
                obj.sub_no = item[PatrolEntity.DetailPropertyFlag.SubNO.ToString()].ToString();
                obj.is_important = item[PatrolEntity.DetailPropertyFlag.IsImportant.ToString()].ToString();
                obj.is_selected = item[PatrolEntity.DetailPropertyFlag.IsSelected.ToString()].ToString() == "1";
                obj.pic_url = item[PatrolEntity.DetailPropertyFlag.PicUrl.ToString()].ToString();
                obj.location_code = item[PatrolEntity.DetailPropertyFlag.LocationCode.ToString()].ToString();
                obj.location_code_name = item[PatrolEntity.DetailPropertyFlag.LocationCodeName.ToString()].ToString();
                obj.spot_code = item[PatrolEntity.DetailPropertyFlag.SpotCode.ToString()].ToString();
                obj.spot_code_name = item[PatrolEntity.DetailPropertyFlag.SpotCodeName.ToString()].ToString();
                obj.remarks = item[PatrolEntity.DetailPropertyFlag.Remarks.ToString()].ToString();
                obj.status = item[PatrolEntity.DetailPropertyFlag.Status.ToString()].ToString();
                obj.question_level = item[PatrolEntity.DetailPropertyFlag.QuestionLevel.ToString()].ToString();
                obj.status_name = item[PatrolEntity.DetailPropertyFlag.StatusName.ToString()].ToString();
                obj.question_level_name = item[PatrolEntity.DetailPropertyFlag.QuestionLevelName.ToString()].ToString();
                ret.Add(obj);
            }
            return ret;
        }

        /// <summary>
        /// 取得头部数据
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static PatrolHeaderInfo getPatrolHeader(DataTable source)
        {
            PatrolHeaderInfo obj = new PatrolHeaderInfo();
            if (source != null && source.Rows.Count > 0)
            {
                DataRow item = source.Rows[0];
                obj.contactor_name = item[PatrolEntity.HeaderPropertyFlag.ContactorName1.ToString()].ToString();
                obj.contactor_phone = item[PatrolEntity.HeaderPropertyFlag.Contaction1.ToString()].ToString();
                obj.customer = item[PatrolEntity.HeaderPropertyFlag.Customer.ToString()].ToString();
                obj.is_emergency = item[PatrolEntity.HeaderPropertyFlag.IsEmergency.ToString()].ToString();
                obj.machine_no = item[PatrolEntity.HeaderPropertyFlag.MachineNO.ToString()].ToString();
                obj.machine_name = item[PatrolEntity.HeaderPropertyFlag.MachineName.ToString()].ToString();
                obj.machine_status = item[PatrolEntity.HeaderPropertyFlag.MachineStatus.ToString()].ToString();
                obj.machine_status_name = item[PatrolEntity.HeaderPropertyFlag.MachineStatusName.ToString()].ToString();
                obj.machine_type = item[PatrolEntity.HeaderPropertyFlag.MachineType.ToString()].ToString();
                obj.maker = item[PatrolEntity.HeaderPropertyFlag.MakerCD.ToString()].ToString();
                obj.operator_name = item[PatrolEntity.HeaderPropertyFlag.ContactorName2.ToString()].ToString();
                obj.operator_phone = item[PatrolEntity.HeaderPropertyFlag.Contaction2.ToString()].ToString();
                obj.patrol_no = item[PatrolEntity.HeaderPropertyFlag.PatrolNO.ToString()].ToString();
                obj.remarks = item[PatrolEntity.HeaderPropertyFlag.Remarks.ToString()].ToString();
                obj.report_date = item[PatrolEntity.HeaderPropertyFlag.ReportDate.ToString()].ToString();
                obj.report_status = item[PatrolEntity.HeaderPropertyFlag.ReportStatus.ToString()].ToString();
                obj.report_uri = item[PatrolEntity.HeaderPropertyFlag.ReportUri.ToString()].ToString();
                obj.reporter = item[PatrolEntity.HeaderPropertyFlag.Reporter.ToString()].ToString();
                obj.reporter_phone = item[PatrolEntity.HeaderPropertyFlag.ReporterPhone.ToString()].ToString();
                obj.reporter_name = item[PatrolEntity.HeaderPropertyFlag.ReporterName.ToString()].ToString();
                obj.work_no = item[PatrolEntity.HeaderPropertyFlag.WorkNO.ToString()].ToString();
                obj.worked_times = item[PatrolEntity.HeaderPropertyFlag.WorkedTimes.ToString()].ToString();
                obj.province = item[PatrolEntity.HeaderPropertyFlag.Province.ToString()].ToString();
                obj.city = item[PatrolEntity.HeaderPropertyFlag.City.ToString()].ToString();
                obj.address = item[PatrolEntity.HeaderPropertyFlag.Address.ToString()].ToString();

                obj.company_name = item[PatrolEntity.HeaderPropertyFlag.CompanyName.ToString()].ToString();
                obj.company_tel = item[PatrolEntity.HeaderPropertyFlag.CompanyTel.ToString()].ToString();
                obj.company_address = item[PatrolEntity.HeaderPropertyFlag.CompanyAddress.ToString()].ToString();
                obj.customer_name = item[PatrolEntity.HeaderPropertyFlag.CustomerName.ToString()].ToString();
                obj.customer_tel = item[PatrolEntity.HeaderPropertyFlag.CustomerTel.ToString()].ToString();
                obj.customer_address = item[PatrolEntity.HeaderPropertyFlag.CustomerAddress.ToString()].ToString();
                //现场联系人类型
                obj.operator_type_name = item[PatrolEntity.HeaderPropertyFlag.OperatorTypeName.ToString()].ToString();
                //发布日期(生成报告书后此记录不会再改变)
                obj.publish_date = DateTime.Parse(item[PatrolEntity.HeaderPropertyFlag.UpdatedAt.ToString()].ToString()).ToString("yyyy年MM月dd日"); 
            }

            return obj;
        }

    }
}
