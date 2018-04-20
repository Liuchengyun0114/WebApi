using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Transactions;
using System.Data;
using System.Data.SqlClient;

namespace Model.EntityManager
{


    /// <summary>
    /// 代理店分公司员工查询控制类
    /// </summary>
    public class BaseEntity
    {
        private static readonly String connectString = System.Configuration.ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        #region 获取建友系统已在特巡中的用户列表

        /// <summary>
        /// 获取特巡报告列表
        /// </summary>
        /// <param name="queryString">查询条件</param>
        /// <param name="rangeSting">分页条件</param>
        /// <returns></returns>
        public static DataTable getStaffListInPatrol()
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (SqlConnection conn = new SqlConnection(connectString))
                    {
                        conn.Open();
                        Console.WriteLine("连接成功...");
                        String sqlString = getListSqlString();
                        Console.WriteLine(sqlString);
                        SqlDataAdapter adapter = new SqlDataAdapter(sqlString, conn);
                        DataSet ds = new DataSet();
                        adapter.Fill(ds);

                        scope.Complete();
                        conn.Close();

                        return ds.Tables[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// 取得列表
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="rangeSting"></param>
        /// <returns></returns>
        private static string getListSqlString()
        {
            string sql = String.Empty;
            //查询特巡报告列表sql     
            sql = "    select t.*,";
            sql += "        s.CompanyCD,";
            sql += "        s.SubCompanyCD,";
            sql += "        s.StaffCD,";
            sql += "        s.StaffNM as StaffName";
            sql += "    from PatrolUserInfo t";
            sql += "    inner join StaffMst s";
            sql += "    on t.usercd = s.staffcd";

            return sql;
        }
        
        #endregion

        #region 常量

        public enum HeaderPropertyFlag
        {
            StaffCD,
            StaffName,
            SubCompanyCD,
            CompanyCD
        }
        

        #endregion
    }
}
