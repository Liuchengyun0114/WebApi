using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using Model;
using Model.BusinessRule;
using Model.EntityManager;
using PatrolServer.Services.Patrol.Request;
using PatrolServer.Services.Patrol.Response;
using PatrolServer.Services.Patrol.Response.Entity;
using PatrolServer.ExcelPrint;

namespace PatrolServer.Services.Patrol
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“PatrolService”。
    public class PatrolService : IPatrolService
    {
        public Stream ShowInfo()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes("Patrol Test"));
        }

        /// <summary>
        /// 取得基础数据 用于查询特巡报告条件
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Stream GetPatrolBase(Stream data)
        {
            #region 查找数据
            ResGetPatrolBase response = new ResGetPatrolBase();
            bool success = false;
            try
            {
                PatrolBaseInfo patrolbase = new PatrolBaseInfo();

                #region 单独处理 未启用
                    ////代理店信息列表
                    //List<NodeInfo> agency_shop_list = List2Tree(COMPANYMSTRule.GetList());

                    ////分公司信息列表
                    //List<SUBCOMPANYMST> filiale_list = List2Tree(SUBCOMPANYMSTRule.GetList());

                    ////报告人信息列表(建友用户系统)
                    //List<NodeInfo> reporter_list = List2Tree(STAFFMSTRule.GetList());
                #endregion

                //代理店信息列表
                List<CompanyInfo> agency_shop_list = ResGetPatrolBase.Transfer(COMPANYMSTRule.GetList());

                //分公司信息列表
                List<SubCompanyInfo> filiale_list = ResGetPatrolBase.Transfer(SUBCOMPANYMSTRule.GetList());

                ////报告人信息列表(建友用户系统)
                List<StaffInfo> reporter_list = ResGetPatrolBase.Transfer(BaseEntity.getStaffListInPatrol());

                ////代理店分公司树形结构数据
                //List<NodeInfo> company_list = GetCompanyTree(agency_shop_list, filiale_list, reporter_list);

                //机型信息列表
                List<NodeInfo> machine_type_list = List2Tree(MACHINETYPEMSTRule.GetList()); ;

                //机器状态信息列表
                List<NodeInfo> machine_status_list = List2Tree(PatrolCodeMstRule.GetListOfMachineStatus()); 
                
                //*.取得点检部位树形菜单
                List<PatrolSpotParts> spotPartsListBase = PatrolSpotPartsRule.GetList();
                List<NodeInfo> checkList = SpotPartsList2Tree(spotPartsListBase);

                //组织数据
                patrolbase.agency_shop_list = agency_shop_list;
                patrolbase.filiale_list = filiale_list;
                patrolbase.reporter_list = reporter_list;
                patrolbase.machine_type_list = machine_type_list;
                patrolbase.machine_status_list = machine_status_list;
                patrolbase.check_list = checkList;
                response.patrolbase = patrolbase;
                //输出成功状态
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("获取特巡基础数据错误" + ex.Message);
                #endregion
            }

            //返回消息体
            if (success)
            {
                response.SetSuccess();
            }
            else
            {
                ////默认是失败
                //response.SetFailed();
            }
            //将消息序列化为Json格式数据
            DataContractJsonSerializer obj2Json = new DataContractJsonSerializer(typeof(ResGetPatrolBase));
            MemoryStream ms = new MemoryStream();
            obj2Json.WriteObject(ms, response);

            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            return ms;

            #endregion
        }

        /// <summary>
        /// 取得特巡报告列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Stream GetPatrolList(Stream data)
        {
            int starttime = Environment.TickCount;
            Console.WriteLine("特巡开始时间：" + starttime);
            #region 查找数据
            ResGetPatrolList response = new ResGetPatrolList();
            bool success = true;
            try
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(ReqGetPatrolList));
                //读取器
                StreamReader sr = new StreamReader(data);
                string dataString = String.Empty;
                while (!sr.EndOfStream)
                {
                    dataString = sr.ReadToEnd();
                    Console.WriteLine(dataString);

                    #region 日志输出
                    CommonInfo.Log(dataString);
                    #endregion

                    //反序列化json为对象注意顺序很重要
                    ReqGetPatrolList request = new ReqGetPatrolList();
                    MemoryStream temp = new MemoryStream(Encoding.UTF8.GetBytes(dataString));
                    request = json.ReadObject(temp) as ReqGetPatrolList;

                    //关闭临时流
                    temp.Close();

                    //调用用户更新密码接口
                    if (request != null)
                    {
                        #region **********（此处加入代码） 根据查询条件 加入业务逻辑代码*************
                        // 身份验证
                        if (request.account != null && request.account != String.Empty && request.token != null && request.token != String.Empty)
                        { 
                            // sql查询字符串组织
                            string queryString = String.Empty;
                            string rangeString = String.Empty;
                            // 按条件查询
                            if (request.search_info != null)
                            {
                                #region 属性查询对应
                                if (request.search_info.AgencyShop != null && request.search_info.AgencyShop != String.Empty)
                                {
                                    queryString += " and sta.CompanyCD = '" + request.search_info.AgencyShop + "' ";
                                }
                                if (request.search_info.Customer != null && request.search_info.Customer != String.Empty)
                                {
                                    queryString += " and t.ContactorName1 like '%" + request.search_info.Customer + "%' ";
                                }
                                if (request.search_info.MachineStatus != null && request.search_info.MachineStatus != String.Empty)
                                {
                                    queryString += " and t.MachineStatus = '" + request.search_info.MachineStatus + "' ";
                                }
                                if (request.search_info.Filiale != null && request.search_info.Filiale != String.Empty)
                                {
                                    queryString += " and sta.SubCompanyCD = '" + request.search_info.Filiale + "' ";
                                }
                                if (request.search_info.HasErrorImage != null && request.search_info.HasErrorImage != String.Empty)
                                {
                                    string haserror = request.search_info.HasErrorImage.Trim().ToUpper();
                                    if (haserror == "0")
                                    {
                                        //无不正常图片
                                        queryString += " and dc.ErrImageCount is null ";
                                    }
                                    else
                                    {
                                        //所有不正常图片
                                        queryString += " and dc.ErrImageCount > 0 ";                                        
                                    }
                                }
                                if (request.search_info.IsEmergency != null && request.search_info.IsEmergency != String.Empty)
                                {
                                    //前台过来的是true和false 需要转换
                                    string istrue = (request.search_info.IsEmergency.Trim().ToLower() == "true") ? "1" : "0";
                                    queryString += " and t.IsEmergency = '" + istrue + "' ";
                                }
                                if (request.search_info.MachineNO != null && request.search_info.MachineNO != String.Empty)
                                {
                                    queryString += " and t.MachineNO like '%" + request.search_info.MachineNO + "%' ";
                                }
                                if (request.search_info.MachineType != null && request.search_info.MachineType != String.Empty)
                                {
                                    queryString += " and t.MachineType = '" + request.search_info.MachineType + "' ";
                                }
                                if (request.search_info.LocationCode != null && request.search_info.LocationCode != String.Empty)
                                {
                                    queryString += " and d.LocationCode = '" + request.search_info.LocationCode + "' ";
                                }
                                if (request.search_info.SpotCode != null && request.search_info.SpotCode != String.Empty)
                                {
                                    queryString += " and d.SpotCode = '" + request.search_info.SpotCode + "' ";
                                }
                                if (request.search_info.Remarks != null && request.search_info.Remarks != String.Empty)
                                {
                                    queryString += " and d.Remarks like '%" + request.search_info.Remarks + "%' ";
                                }
                                if (request.search_info.ReportDateStart != null && request.search_info.ReportDateStart != String.Empty)
                                {

                                    queryString += " and t.ReportDate >= '" + request.search_info.ReportDateStart + "' ";
                                }
                                if (request.search_info.ReportDateClosed != null && request.search_info.ReportDateClosed != String.Empty)
                                {
                                    queryString += " and t.ReportDate <= '" + request.search_info.ReportDateClosed + "' ";
                                }
                                if (request.search_info.Reporter != null && request.search_info.Reporter != String.Empty)
                                {
                                    queryString += " and t.Reporter = '" + request.search_info.Reporter + "' ";
                                }
                                if (request.search_info.ReportStatus != null && request.search_info.ReportStatus != String.Empty)
                                {
                                    queryString += " and t.ReportStatus = '" + request.search_info.ReportStatus + "' ";
                                }

                                //分页控制对象范围
                                int pageIndex = 1;
                                int pageSize = 10;
                                if (request.search_info.Paginator != null)
                                {
                                    pageIndex = request.search_info.Paginator.PageIndex;
                                    pageIndex = pageIndex > 0 ? pageIndex : 1;
                                    pageSize = request.search_info.Paginator.PageSize;
                                    pageSize = pageSize > 0 ? pageSize : 10;

                                    int rangeStart = (pageIndex-1) * pageSize + 1;
                                    int rangeEnd = pageIndex * pageSize;

                                    rangeString += " and orderno between " + rangeStart + " and " + rangeEnd + " ";
                                }
                                else {
                                    rangeString += " and orderno between " + pageIndex + " and " + pageSize + " "; 
                                }

                                //获取数据调用数据库接口
                                DataTable dt = PatrolEntity.getPatrolList(queryString, rangeString);
                                //转换数据
                                response.patrol_list = ResGetPatrolList.Transfer(dt);
                                response.count = PatrolEntity.getPatrolCount(queryString);

                                success = true;
                                #endregion
                            }                            
                        }
                        #endregion
                    }
                }
                sr.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("获取特巡列表数据错误" + ex.Message);
                #endregion
            }

            //返回消息体
            if (success)
            {
                response.SetSuccess();

                #region 日志输出
                CommonInfo.Log("获取特巡列表成功 总数量" + response.count.ToString());
                #endregion
            }
            else
            {
                ////默认是失败
                //response.SetFailed();
            }
            //将消息序列化为Json格式数据
            DataContractJsonSerializer obj2Json = new DataContractJsonSerializer(typeof(ResGetPatrolList));
            MemoryStream ms = new MemoryStream();
            obj2Json.WriteObject(ms, response);

            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            Console.WriteLine("结束时间：" + (Environment.TickCount - starttime));
            return ms;

            #endregion
        }

        /// <summary>
        /// 取得指定编号特巡报告
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Stream GetPatrol(Stream data)
        {
            #region 查找数据
            ResGetPatrol response = new ResGetPatrol();
            bool success = false;
            try
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(ReqGetPatrol));
                //读取器
                StreamReader sr = new StreamReader(data);
                string dataString = String.Empty;
                while (!sr.EndOfStream)
                {
                    dataString = sr.ReadToEnd();
                    Console.WriteLine(dataString);

                    #region 日志输出
                    CommonInfo.Log(dataString);
                    #endregion

                    //反序列化json为对象注意顺序很重要
                    ReqGetPatrol request = new ReqGetPatrol();
                    MemoryStream temp = new MemoryStream(Encoding.UTF8.GetBytes(dataString));
                    request = json.ReadObject(temp) as ReqGetPatrol;

                    //关闭临时流
                    temp.Close();

                    //调用用户更新密码接口
                    if (request != null)
                    {
                        //**********（此处加入代码） 根据查询条件 加入业务逻辑代码*************

                        if (request.account != null && request.account != String.Empty && request.token != null && request.token != String.Empty)
                        {
                            if (request.patrol_no != null && request.patrol_no != String.Empty)
                            {
                                DataTable header = PatrolEntity.getPatrolHeader(request.patrol_no);
                                DataTable detail = PatrolEntity.getPatrolDetail(request.patrol_no);
                                response.patrol_header = ResGetPatrol.getPatrolHeader(header);
                                response.patrol_detail_list = ResGetPatrol.getPatrolDetailList(detail);

                                //设置成功状态
                                success = true;
                            }
                        }
                    }
                }
                sr.Close();
                Console.WriteLine(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("获取特巡数据" + ex.Message);
                #endregion
            }

            //返回消息体
            if (success)
            {
                response.SetSuccess();
            }
            else
            {
                ////默认是失败
                //response.SetFailed();
            }
            //将消息序列化为Json格式数据
            DataContractJsonSerializer obj2Json = new DataContractJsonSerializer(typeof(ResGetPatrol));
            MemoryStream ms = new MemoryStream();
            obj2Json.WriteObject(ms, response);

            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            return ms;

            #endregion
        }

        /// <summary>
        /// 更新特巡报告头表和明细表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Stream UpdatePatrol(Stream data)
        {
            #region 查找数据
            ResUpdatePatrol response = new ResUpdatePatrol();
            bool success = false;
            try
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(ReqUpdatePatrol));
                //读取器
                StreamReader sr = new StreamReader(data);
                string dataString = String.Empty;
                while (!sr.EndOfStream)
                {
                    dataString = sr.ReadToEnd();
                    Console.WriteLine(dataString);

                    #region 日志输出
                    CommonInfo.Log(dataString);
                    #endregion

                    //反序列化json为对象注意顺序很重要
                    ReqUpdatePatrol request = new ReqUpdatePatrol();
                    MemoryStream temp = new MemoryStream(Encoding.UTF8.GetBytes(dataString));
                    request = json.ReadObject(temp) as ReqUpdatePatrol;

                    //关闭临时流
                    temp.Close();

                    //调用用户更新密码接口
                    if (request != null)
                    {
                        //**********（此处加入代码） 根据查询条件 加入业务逻辑代码*************
                        if (request.account != null && request.account != String.Empty && request.token != null && request.token != String.Empty)
                        {
                            string header = getUpdatePatrolHeaderSqlString(request);
                            string detail = getUpdatePatrolDetailSqlString(request);
                            success = PatrolEntity.updatePatrol(header,detail);
                        }
                    }
                }
                sr.Close();
                Console.WriteLine(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("更新特巡数据错误" + ex.Message);
                #endregion
            }

            //返回消息体
            if (success)
            {
                response.SetSuccess();
            }
            else
            {
                ////默认是失败
                //response.SetFailed();
            }
            //将消息序列化为Json格式数据
            DataContractJsonSerializer obj2Json = new DataContractJsonSerializer(typeof(ResUpdatePatrol));
            MemoryStream ms = new MemoryStream();
            obj2Json.WriteObject(ms, response);

            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            return ms;

            #endregion
        }

        /// <summary>
        /// 删除特巡报告头表以及所有明细
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Stream DeletePatrol(Stream data)
        {
            #region 查找数据
            ResDeletePatrol response = new ResDeletePatrol();
            bool success = false;
            try
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(ReqDeletePatrol));
                //读取器
                StreamReader sr = new StreamReader(data);
                string dataString = String.Empty;
                while (!sr.EndOfStream)
                {
                    dataString = sr.ReadToEnd();
                    Console.WriteLine(dataString);

                    #region 日志输出
                    CommonInfo.Log(dataString);
                    #endregion

                    //反序列化json为对象注意顺序很重要
                    ReqDeletePatrol request = new ReqDeletePatrol();
                    MemoryStream temp = new MemoryStream(Encoding.UTF8.GetBytes(dataString));
                    request = json.ReadObject(temp) as ReqDeletePatrol;

                    //关闭临时流
                    temp.Close();

                    //调用用户更新密码接口
                    if (request != null && request.patrol_no.Length > 0 & request.account.Length > 0)
                    {
                        //**********（此处加入代码） 根据查询条件 加入业务逻辑代码*************
                        Console.WriteLine("{0}正在删除特巡：{1}", request.account, request.patrol_no);
                        #region 日志输出
                        CommonInfo.Log(request.account + "正在删除特巡：" + request.patrol_no);
                        #endregion
                        success = PatrolEntity.DeleteReport(request.patrol_no);
                    }
                }
                sr.Close();
                Console.WriteLine(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("删除特巡数据错误" + ex.Message);
                #endregion
            }

            //返回消息体
            if (success)
            {
                response.SetSuccess();
            }
            else
            {
                ////默认是失败
                //response.SetFailed();
            }
            //将消息序列化为Json格式数据
            DataContractJsonSerializer obj2Json = new DataContractJsonSerializer(typeof(ResDeletePatrol));
            MemoryStream ms = new MemoryStream();
            obj2Json.WriteObject(ms, response);

            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            return ms;

            #endregion
        }

        /// <summary>
        /// 展示某个特巡报告,用于生成报表数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Stream ShowReport(string reportid)
        {
            #region 查找数据
            ResShowReport response = new ResShowReport();
            bool success = false;
            try
            {
                if (reportid != null && reportid != String.Empty)
                {
                    string patrolno = PatrolEntity.getPatrolNobyReportId(reportid);
                    if (patrolno != null && patrolno != String.Empty)
                    {
                        DataTable header = PatrolEntity.getPatrolHeader4Report(patrolno);
                        DataTable detail = PatrolEntity.getPatrolDetail4Report(patrolno);
                        //DataTable facadeList = PatrolEntity.getFacadeList4Report(patrolno);
                        List<PatrolDetailInfo> detailList = ResShowReport.getPatrolDetailList(detail);
                        response.patrol_header = ResShowReport.getPatrolHeader(header);
                        
                        response.facade_list = ResShowReport.getFacadeImageList(detailList);
                        response.patrol_detail_list = detailList;

                        //设置成功状态
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("显示特巡报告书数据错误" + ex.Message);
                #endregion
            }

            //返回消息体
            if (success)
            {
                response.SetSuccess();
            }
            else
            {
                ////默认是失败
                //response.SetFailed();
            }
            //将消息序列化为Json格式数据
            DataContractJsonSerializer obj2Json = new DataContractJsonSerializer(typeof(ResShowReport));
            MemoryStream ms = new MemoryStream();
            obj2Json.WriteObject(ms, response);

            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            return ms;

            #endregion
        }

        public Stream CreateReport(string patrolno)
        {
            string reportid = String.Empty;
            bool success = false;
            Console.WriteLine("特巡编号" + patrolno);
            try
            {
                //将特巡报告ID进行加密
                reportid = CreateReportId();
                Console.WriteLine(reportid);
                success = PatrolEntity.CreateReportUri(reportid, patrolno);
            }
            catch (Exception ex)
            {
                Console.WriteLine("报告书失败" + ex.Message);
                #region 日志输出
                CommonInfo.Error("创建特巡报告书错误" + ex.Message);
                #endregion
            }
            if (!success)
            {
                reportid = String.Empty;
            } 

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(reportid));

            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            return ms;
        }
        
        /// <summary>
        ///  导出Excel文件
        /// </summary>
        /// <param name="patrolno"></param>
        /// <returns></returns>
        public Stream ExportExcel(string patrolno)
        {

            #region 查找数据
            ResExportExcel response = new ResExportExcel();
            bool success = false;
            try
            {


                if (patrolno != null && patrolno != String.Empty && patrolno != "undefined")
                {
                    ExcelEntity en = new ExcelEntity();
                    ResShowReport data = en.GetData(patrolno);//"PRN2017071800003");
                    //随机文件夹防止同时访问一个文件
                    string dirName = DateTime.Now.ToString("yyyyMMdd") + "\\" + data.patrol_header.report_uri;
                    string excelFileName = dirName + "\\" + CreateReportId();
                    response.excel_filename = excelFileName + CommonInfo.PDFExtend;
                    //设置成功状态
                    success = en.ExportExcel(data, dirName, excelFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("显示特巡报告书数据错误" + ex.Message);
                #endregion
            }

            //返回消息体
            if (success)
            {
                response.SetSuccess();
            }
            else
            {
                ////默认是失败
                //response.SetFailed();
            }
            //将消息序列化为Json格式数据
            DataContractJsonSerializer obj2Json = new DataContractJsonSerializer(typeof(ResExportExcel));
            MemoryStream ms = new MemoryStream();
            obj2Json.WriteObject(ms, response);

            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            return ms;

            #endregion            

        }

        #region 辅助方法
        /// <summary>
        /// 哈希算法加密特巡报告书Id
        /// </summary>
        /// <returns></returns>
        public static string CreateReportId()
        {
            string data = "PatrolReport";
            int a = 46859;
            int b = 13275;
            long hash = 0;
            Random r = new Random(DateTime.Now.Millisecond);
            char[] d = data.ToCharArray();

            for (int i = 0; i < d.Length; i++)
            {
                hash = hash * a + d[i];
                a = a * b + r.Next(10000, 99999);
            }
            return Math.Abs(hash).ToString();
        }

        /// <summary>
        /// 将点检部位列表转换为树形列表
        /// </summary>
        /// <param name="source">点检部位列表</param>
        /// <returns></returns>
        public static List<NodeInfo> SpotPartsList2Tree(List<PatrolSpotParts> source)
        {
            List<NodeInfo> tree = new List<NodeInfo>();
            try
            {
                if (source != null && source.Count > 0)
                {
                    List<PatrolSpotParts> partsList = source.Where(p => p.ParentID == "root").ToList<PatrolSpotParts>();
                    foreach (PatrolSpotParts item in partsList)
                    {
                        NodeInfo subTree = new NodeInfo();
                        subTree.code = item.ID;
                        subTree.name = item.Name;

                        List<PatrolSpotParts> subList = source.Where(p => p.ParentID == item.ID).ToList<PatrolSpotParts>();
                        List<NodeInfo> subNode = new List<NodeInfo>();
                        foreach (PatrolSpotParts instance in subList)
                        {
                            NodeInfo temp = new NodeInfo();
                            temp.code = instance.ID;
                            temp.name = instance.Name;
                            subNode.Add(temp);
                        }
                        subTree.sub_list = subNode;
                        tree.Add(subTree);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("点检数据转换树形结构错误" + ex.Message);
                #endregion
            }

            return tree;
        }

        /// <summary>
        /// 将代理店分公司员工转换为树形列表
        /// </summary>
        /// <param name="source">点检部位列表</param>
        /// <returns></returns>
        public static List<NodeInfo> GetCompanyTree(List<COMPANYMST> company, List<SUBCOMPANYMST> subcompany, List<StaffUserInfo> staff)
        {
            List<NodeInfo> tree = new List<NodeInfo>();
            try
            {
                if (company != null && company.Count > 0)
                {
                    foreach (COMPANYMST com in company)
                    {

                        NodeInfo companyTree = new NodeInfo();
                        companyTree.code = com.COMPANYCD;
                        companyTree.name = com.COMPANYNM;
                        companyTree.sub_list = new List<NodeInfo>();
                        List<SUBCOMPANYMST> subList = subcompany.Where(p => p.COMPANYCD == com.COMPANYCD).ToList<SUBCOMPANYMST>();
                        foreach (SUBCOMPANYMST sub in subList)
                        {
                            NodeInfo subTree = new NodeInfo();
                            subTree.code = sub.SUBCOMPANYCD;
                            subTree.name = sub.SUBCOMPANYNM;
                            subTree.sub_list = new List<NodeInfo>();
                            List<StaffUserInfo> staffList = staff.Where(p => p.subcompanycd == sub.SUBCOMPANYCD).ToList<StaffUserInfo>();

                            foreach (StaffUserInfo staffuser in staffList)
                            {
                                NodeInfo temp = new NodeInfo();
                                temp.code = staffuser.staffcd;
                                temp.name = staffuser.staffname;
                                //分公司下所有员工

                                subTree.sub_list.Add(temp);
                            }
                            //代理店下分公司
                            companyTree.sub_list.Add(subTree);
                        }
                        //所有代理店
                        tree.Add(companyTree);                        
                    }                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("代理店分公司员工转换树形数据错误" + ex.Message);
                #endregion
            }

            return tree;
        }

        /// <summary>
        /// 列表类型转换
        /// </summary>
        /// <param name="source">数据字典列表</param>
        /// <returns></returns>
        public static List<NodeInfo> List2Tree(List<PatrolCodeMst> source)
        {
            List<NodeInfo> list = new List<NodeInfo>();
            try
            {
                if (source != null && source.Count > 0)
                {
                    foreach (PatrolCodeMst item in source)
                    {
                        NodeInfo temp = new NodeInfo();
                        temp.code = item.CodeCD;
                        temp.name = item.CodeName;
                        list.Add(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("特巡数据字典转换错误" + ex.Message);
                #endregion
            }

            return list;

        }

        /// <summary>
        /// 机型列表类型转换
        /// </summary>
        /// <param name="source">数据字典列表</param>
        /// <returns></returns>
        public static List<NodeInfo> List2Tree(List<MACHINETYPEMST> source)
        {
            List<NodeInfo> list = new List<NodeInfo>();
            try
            {
                if (source != null && source.Count > 0)
                {
                    foreach (MACHINETYPEMST item in source)
                    {
                        NodeInfo temp = new NodeInfo();
                        temp.code = item.MACHINECD;
                        temp.name = item.MACHINENM;
                        list.Add(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("机型转换列表错误" + ex.Message);
                #endregion
            }

            return list;

        }

        /// <summary>
        /// 代理店列表类型转换
        /// </summary>
        /// <param name="source">数据字典列表</param>
        /// <returns></returns>
        public static List<NodeInfo> List2Tree(List<COMPANYMST> source)
        {
            List<NodeInfo> list = new List<NodeInfo>();
            try
            {
                if (source != null && source.Count > 0)
                {
                    foreach (COMPANYMST item in source)
                    {
                        NodeInfo temp = new NodeInfo();
                        temp.code = item.COMPANYCD;
                        temp.name = item.COMPANYNM;
                        list.Add(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("代理店转换列表错误" + ex.Message);
                #endregion
            }

            return list;

        }

        /// <summary>
        /// 分公司列表类型转换
        /// </summary>
        /// <param name="source">数据字典列表</param>
        /// <returns></returns>
        public static List<NodeInfo> List2Tree(List<SUBCOMPANYMST> source)
        {
            List<NodeInfo> list = new List<NodeInfo>();
            try
            {
                if (source != null && source.Count > 0)
                {
                    foreach (SUBCOMPANYMST item in source)
                    {
                        NodeInfo temp = new NodeInfo();
                        temp.code = item.SUBCOMPANYCD;
                        temp.name = item.SUBCOMPANYNM;
                        list.Add(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("分公司转换列表错误" + ex.Message);
                #endregion
            }

            return list;

        }

        /// <summary>
        /// 所有员工列表类型转换
        /// </summary>
        /// <param name="source">数据字典列表</param>
        /// <returns></returns>
        public static List<NodeInfo> List2Tree(List<STAFFMST> source)
        {
            List<NodeInfo> list = new List<NodeInfo>();
            try
            {
                if (source != null && source.Count > 0)
                {
                    foreach (STAFFMST item in source)
                    {
                        NodeInfo temp = new NodeInfo();
                        temp.code = item.STAFFCD;
                        temp.name = item.STAFFNM;
                        list.Add(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("员工转换列表错误" + ex.Message);
                #endregion
            }

            return list;

        }

        private static string getUpdatePatrolHeaderSqlString(ReqUpdatePatrol request)
        {
            Request.Entity.PatrolHeaderInfo item = request.patrol_header;
            string temp = String.Empty;
            if (item != null)
            {
                temp = "  update PatrolReportHeader set ";
                temp += " Remarks = '" + item.remarks + "',";
                temp += " WorkNo = '" + item.work_no + "',";
                temp += " UpdatedAt = '" + DateTime.Now.ToString() + "',";
                temp += " Updator = '" + request.account + "' ";
                temp += " where patrolno = '" + item.patrol_no + "'";
                temp += " ; ";
            }

            return temp;
        }

        private static string getUpdatePatrolDetailSqlString(ReqUpdatePatrol request)
        {
            List<Request.Entity.PatrolDetailInfo> source = request.patrol_detail_list;
            string sql = String.Empty;
            if (source != null && source.Count > 0)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    string temp = String.Empty;
                    Request.Entity.PatrolDetailInfo item = source[i];

                    temp = "  update PatrolReportDetail set ";
                    //temp += " Remarks = '" + item.remarks + "',";
                    string isselected = (item.is_selected.Trim().ToLower() == "false") ? "0" : "1";
                    temp += " IsSelected = '" + isselected + "'";
                    temp += " ,Remarks = '" + item.remarks + "'";
                    temp += " where patrolno = '" + item.patrol_no + "'";
                    temp += " and subno = '" + item.sub_no + "'";
                    temp += " ; ";

                    sql += temp;
                }
            }
            return sql;
        }

        #endregion




        public Stream GetPatrolBaseOptions(Stream data)
        {
            return null;
        }

        public Stream GetPatrolListOptions(Stream data)
        {
            return null;
        }

        public Stream GetPatrolOptions(Stream data)
        {
            return null;
        }

        public Stream UpdatePatrolOptions(Stream data)
        {
            return null;
        }

        public Stream DeletePatrolOptions(Stream data)
        {
            return null;
        }

        public Stream ShowReportOptions(string reportid)
        {
            return null;
        }

        public Stream CreateReportOptions(string patrolno)
        {
            return null;
        }
        
        public Stream ExportExcelOptions(string patrolno)
        {
            return null;
        }
    }
}
