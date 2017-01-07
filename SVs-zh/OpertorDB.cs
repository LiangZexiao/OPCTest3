using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SVs
{
    public class OpertorDB
    {
        //数据库连接
        SqlConnection Conn = null;
        SqlCommand cmd = null;
        //string ConnStr = "Data Source=PC-201602051039;Initial Catalog=Plastic;User ID=sa;Password=sa";
        private static string ConnStr =
            "Server = FDCSERVER\\CKYER;" +
            "Database = DataHistory;" +
            "User ID = sa;" +
            "Password = 123456;";

        public OpertorDB()
        {
            SqlConnection conn = ConnDB(ConnStr);
        }
        public SqlConnection ConnDB(string ConnStr)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                Conn = conn;
                return conn;
            }
            catch
            {
                return null;
            }
        }
        public string GetDispatchNo(string MachineNo)
        {

            //select MAX(TotalNum) from DataHistory where ClientIP ='192.168.8.13' and DispatchOrder = (select top 1  do_no from DispatchOrder where DispatchStatus=1 and MachineNo='IM022107')
            string strSQL = "select do_no from DispatchOrder where DispatchStatus=1 and MachineNo='{0}'";
            strSQL = string.Format(strSQL, MachineNo);
            using (Conn)
            {
                if (Conn.State == ConnectionState.Closed)
                {
                    Conn.ConnectionString = ConnStr;
                    Conn.Open();
                }
                try
                {
                    DataTable dt = new DataTable();
                    cmd = new SqlCommand(strSQL, Conn);
                    SqlDataAdapter oda = new SqlDataAdapter(cmd);
                    oda.Fill(dt);
                    DataRow dr = dt.Rows[0];
                    if (dt.Rows.Count > 0)
                    {
                        return dr[0].ToString();
                    }
                    else
                    {
                        return "???????????????";
                    }

                }
                catch (Exception ex)
                {
                    return ex.Message.ToString();
                }
            }

        }
        public int GetDispatchTotalNum(string MachineIP, string Do_No)
        {

            string strSQL = "select MAX(TotalNum) from DataHistory where ClientIP ='{0}' and DispatchOrder = '{1}'";

            strSQL = string.Format(strSQL, MachineIP, Do_No);
            using (Conn)
            {
                if (Conn.State == ConnectionState.Closed)
                {
                    Conn.ConnectionString = ConnStr;
                    Conn.Open();
                }
                try
                {
                    DataTable dt = new DataTable();
                    cmd = new SqlCommand(strSQL, Conn);
                    SqlDataAdapter oda = new SqlDataAdapter(cmd);
                    oda.Fill(dt);
                    DataRow dr = dt.Rows[0];
                    return int.Parse(dr[0].ToString()) + 1;
                }

                catch (Exception ex)
                {
                    return 0;
                }
            }

        }
        public string GetMouldNo(string Do_no)
        {
            string strSQL = "select top 1 MouldNo  from DispatchOrder where DO_No ='{0}'";
            strSQL = string.Format(strSQL, Do_no);
            using (Conn)
            {
                if (Conn.State == ConnectionState.Closed)
                {
                    Conn.ConnectionString = ConnStr;
                    Conn.Open();
                }
                try
                {
                    DataTable dt = new DataTable();
                    cmd = new SqlCommand(strSQL, Conn);
                    SqlDataAdapter oda = new SqlDataAdapter(cmd);
                    oda.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];
                        return dr[0].ToString();
                    }
                    else
                    {
                        return "";
                    }

                }

                catch (Exception ex)
                {
                    return ex.Message.ToString();
                }
            }

        }
        /// <summary>
        /// 向表(DataHistory)中插入所需参数数据
        /// </summary>
        public void Insert(string value1, int value2, string value3, int value4, DateTime value5, decimal value6, decimal value7, decimal value8, decimal value9, decimal fillTime, DateTime updateTime)
        {
            SqlConnection conn = ConnDB(ConnStr);
            string sql =
               "insert into DataHistory(DispatchOrder,MouldID,ClientIP,TotalNum,EndCycle,Cycletime,KeepPress_Max,ShotTemp1,ShotTemp2,FillTime,UpLoadTime) values ('" +
                value1 + "', '" + value2 + "', '" + value3 + "', '" + value4 + "', '" + value5 + "', '" + value6 + "', '" + value7 + "', '" + value8 + "', '" + value8 + "', '" + fillTime + "', '" + updateTime + "')";
            SqlCommand comm = new SqlCommand(sql, conn);
            comm.ExecuteReader();

            conn.Close();
        }
    }
}
