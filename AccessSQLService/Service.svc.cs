using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using AccessSQLService.Model;
using System.Diagnostics;

namespace AccessSQLService
{
    public class Service1 : IService
    {
        public string SqlConStr = "Server=tcp:sqlsmarthomeserver.database.windows.net,1433;Initial Catalog=SmartHomeDB;Persist Security Info=False;User ID=sqlSmartHome;Password=sensorsDBsql!2;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        public IList<Devices> QueryDevices()
        {
            DataSet ds = new DataSet();
            using (SqlConnection sqlCon = new SqlConnection(SqlConStr))
            {
                try
                {
                    string sqlStr = "SELECT * FROM Devices";
                    SqlDataAdapter sqlDa = new SqlDataAdapter(sqlStr, sqlCon);
                    sqlDa.Fill(ds);
                }
                catch
                {
                    return null;
                }
                finally
                {
                    sqlCon.Close();
                }
            }

            List<Devices> deviceList = new List<Devices>();
            DataTable dt = ds.Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                deviceList.Add(new Devices()
                {
                    DeviceId = dr["deviceId"] as string,
                    DeviceType = dr["deviceType"] as string,
                    DeviceDescription = dr["deviceDescription"] as string,
                    DeviceRoom = dr["deviceRoom"] as string,
                    DeviceHouse = dr["deviceHouse"] as string
                });
            }
            return deviceList;
        }

        public IList<Devices> QueryDevices2()
        {
            DataSet ds = new DataSet();
            using (SqlConnection sqlCon = new SqlConnection(SqlConStr))
            {
                try
                {
                    string sqlStr = "SELECT * FROM Devices";
                    SqlDataAdapter sqlDa = new SqlDataAdapter(sqlStr, sqlCon);
                    sqlDa.Fill(ds);
                }
                catch
                {
                    return null;
                }
                finally
                {
                    sqlCon.Close();
                }
            }

            List<Devices> deviceList = new List<Devices>();
            DataTable dt = ds.Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                deviceList.Add(new Devices()
                {
                    DeviceId = dr["deviceId"] as string,
                    DeviceType = dr["deviceType"] as string,
                    DeviceDescription = dr["deviceDescription"] as string,
                    DeviceRoom = dr["deviceRoom"] as string,
                    DeviceHouse = dr["deviceHouse"] as string
                });
            }
            return deviceList;
        }
    }
}
