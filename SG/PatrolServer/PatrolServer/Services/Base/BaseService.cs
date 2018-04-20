using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Json;
using System.Collections;
using System.Data;
using Model;
using Model.Controller;
using Model.BusinessRule;
using Model.EntityManager;
using PatrolServer.Services.Base;
using PatrolServer.Services.Base.Response;
using PatrolServer.Services.Base.Response.Entity;

namespace PatrolServer.Services.Base
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“BaseService”。
    public class BaseService : IBaseService
    {
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public Stream GetUserList()
        {
            ResGetUserList response = new ResGetUserList();
            try
            {
                //获取数据调用数据库接口
                DataTable dt = UserEntity.getUserList4App();

                UserListInfo userlist = new UserListInfo();

                List<UserInfo> userinfoList = ResGetUserList.Transfer(dt);
                userlist.count = UserEntity.getUserCount4App();
                userlist.list = userinfoList;

                //组织数据
                response.userlist = userlist;

                //输出成功状态
                response.SetSuccess();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("手机端获取用户列表错误" + ex.Message);
                #endregion
            }

            //将消息序列化为Json格式数据
            DataContractJsonSerializer obj2Json = new DataContractJsonSerializer(typeof(ResGetUserList));
            MemoryStream ms = new MemoryStream();
            obj2Json.WriteObject(ms, response);
            
            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            return ms;
        }
        
        public Stream GetBaseInfo()
        {

            ResGetBaseInfo response = new ResGetBaseInfo();
            try
            {
                //*.取得机型列表
                List<NodeInfo> modelList = List2Tree(MACHINETYPEMSTRule.GetList());

                //*.取得点检部位树形菜单
                List<PatrolSpotParts> spotPartsListBase = PatrolSpotPartsRule.GetList();
                List<NodeInfo> checkList = SpotPartsList2Tree(spotPartsListBase);

                //*.取得机器状态列表
                List<NodeInfo> machineStatusList =  List2Tree(PatrolCodeMstRule.GetListOfMachineStatus());

                //*.取得问题程度列表
                List<NodeInfo> questionLevelList = List2Tree(PatrolCodeMstRule.GetListOfQuestionLevel());

                //*.取得点检状态列表
                List<NodeInfo> spotStatusList = List2Tree(PatrolCodeMstRule.GetListOfSpotStatus());

                //*.取得联系方式列表
                List<NodeInfo> contactTypeList = List2Tree(PatrolCodeMstRule.GetListOfContactType());

                //*.取得联系人类型列表
                List<NodeInfo> contactorTypeList = List2Tree(PatrolCodeMstRule.GetListOfContactorType());

                //组织数据
                response.model_list = modelList;
                response.check_list = checkList;
                response.machine_status_list = machineStatusList;
                response.level_list = questionLevelList;
                response.spot_status_list = spotStatusList;
                response.phone_type_list = contactTypeList;
                response.contact_type_list = contactorTypeList;
                //输出成功状态
                response.SetSuccess();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("手机端获取基础数据错误" + ex.Message);
                #endregion
            }

            //将消息序列化为Json格式数据
            DataContractJsonSerializer obj2Json = new DataContractJsonSerializer(typeof(ResGetBaseInfo));
            MemoryStream ms = new MemoryStream();
            obj2Json.WriteObject(ms, response);


            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            return ms;

        }

        /// <summary>
        /// 获取服务器列表
        /// </summary>
        /// <returns></returns>
        public Stream GetServerList()
        {
            ResGetServerList response = new ResGetServerList();
            try
            {
                //*.取得机型列表
                List<ServerInfo> server_list = List2Tree(PatrolServerListRule.GetList());

                //组织数据
                response.server_list = server_list;

                //输出成功状态
                response.SetSuccess();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("手机端获取服务器列表错误" + ex.Message);
                #endregion
            }

            //将消息序列化为Json格式数据
            DataContractJsonSerializer obj2Json = new DataContractJsonSerializer(typeof(ResGetServerList));
            MemoryStream ms = new MemoryStream();
            obj2Json.WriteObject(ms, response);
            
            //注意一定要设置流的位置到开始位置,否则没有消息输出
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// 测试服务是否成功开启
        /// </summary>
        /// <returns></returns>
        public Stream ShowInfo()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes("Base Test"));
        }

        #region 辅助方法

        /// <summary>
        /// 将点检部位列表转换为树形列表
        /// </summary>
        /// <param name="source">点检部位列表</param>
        /// <returns></returns>
        public static List<NodeInfo> SpotPartsList2Tree(List<PatrolSpotParts> source){
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
                CommonInfo.Error("手机端点检部位转换列表发生错误" + ex.Message);
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
                CommonInfo.Error("手机端特巡数据字典转换列表发生错误" + ex.Message);
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
                CommonInfo.Error("手机端机型转换列表发生错误" + ex.Message);
                #endregion
            }

            return list;

        }


        /// <summary>
        /// 列表类型转换
        /// </summary>
        /// <param name="source">数据字典列表</param>
        /// <returns></returns>
        public static List<ServerInfo> List2Tree(List<PatrolServerList> source)
        {
            List<ServerInfo> list = new List<ServerInfo>();
            try
            {
                if (source != null && source.Count > 0)
                {
                    foreach (PatrolServerList item in source)
                    {
                        ServerInfo temp = new ServerInfo();
                        temp.address = item.Address;
                        temp.name = item.Name;
                        temp.isMainServer = item.IsMainServer;
                        temp.website_url = item.WebsiteUrl;
                        list.Add(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                #region 日志输出
                CommonInfo.Error("手机端特巡服务器转换列表发生错误" + ex.Message);
                #endregion
            }

            return list;

        }

        #endregion


        public Stream GetUserListOptions()
        {
            return null;
        }

        public Stream GetBaseInfoOptions()
        {
            return null;
        }

        public Stream GetServerListOptions()
        {
            return null;
        }
    }
}
