using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SVs
{
    public class SQLHelp
    {
        private static string connectionString =
            "Server = FDCSERVER\\CKYER;" +
            "Database = DataHistory;" +
            "User ID = sa;" +
            "Password = 123456;";

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <returns></returns>
        private SqlConnection ConnectionOpen()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// 向表(DataHistory)中插入一条数据
        /// </summary>
        public void Insert(string value1, int value2, string value3, int value4, DateTime value5, decimal value6, decimal value7, decimal value8, decimal value9,  decimal fillTime, DateTime updateTime)
        {
            SqlConnection conn = ConnectionOpen();
            string sql =
               "insert into DataHistory(DispatchOrder,MouldID,ClientIP,TotalNum,EndCycle,Cycletime,KeepPress_Max,ShotTemp1,ShotTemp2,FillTime,UpLoadTime) values ('" +
                value1 + "', '" + value2 + "', '" + value3 + "', '" + value4 + "', '" + value5 + "', '" + value6 + "', '" + value7 + "', '" + value8 + "', '" + value8 + "', '" + fillTime + "', '" + updateTime + "')";
            SqlCommand comm = new SqlCommand(sql, conn);
            comm.ExecuteReader();

            conn.Close();
        }
        //public string searchno(string value)
        //{
        //    SqlConnection conn = ConnectionOpen();
        //    string sql =
        //       "select MouldID from DataHistory where totalnum ="+value+"";
        //    SqlCommand comm = new SqlCommand(sql, conn);
        //    comm.ExecuteReader();
        //}


    }
}
