using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotClientUwp
{
    class MySqlDB
    {
        //private static string server = "";
        private static string server = "127.0.0.1";
        private static string database = "";
        private static string user = "";
        private static string pswd = "";
        private static string connectionString = "Server = " + server + ";database = " + database + ";uid = " + user + ";password = " + pswd + ";SslMode=None";
        
        // Retrieve devices from the database.
        public static List<Room> devicesFromDB()
        {
            // Change the character encoding.
            EncodingProvider encode;
            encode = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encode);

            string deviceId = "";
            string deviceDescription = "";
            string deviceRoom = "";

            List<Room> rooms = new List<Room>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception)
                {

                    Debug.WriteLine("There was a problem connecting to the database. Maybe is not active?");
                }

                MySqlCommand retrieveDevices = new MySqlCommand("SELECT * FROM Devices2", connection);

                try
                {
                    using (MySqlDataReader reader = retrieveDevices.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // get the results of each column
                            deviceId = (string)reader["deviceId"];
                            deviceDescription = (string)reader["deviceDescription"];
                            deviceRoom = (string)reader["deviceRoom"];
                            
                            if (rooms.Count == 0)
                            {
                                // Add a new room.
                                rooms.Add(addNewRoom(deviceId, deviceDescription, deviceRoom));
                            }
                            else
                            {
                                bool exists = false;
                                for (int i = 0; i < rooms.Count; i++)
                                {
                                    if (deviceRoom == rooms[i].Name)
                                    {
                                        if (deviceDescription == "Door")
                                        {
                                            rooms[i].Door = deviceId;
                                        }
                                        else if (deviceDescription == "Light")
                                        {
                                            rooms[i].Light = deviceId;
                                        }
                                        else if (deviceDescription == "Heat")
                                        {
                                            rooms[i].Heat = deviceId;
                                        }
                                        exists = true;
                                        break;
                                    }
                                }

                                if (exists == false)
                                {
                                    // Add a new room.
                                    rooms.Add(addNewRoom(deviceId, deviceDescription, deviceRoom));
                                }
                            }
                        }

                        for (int i = 0; i < rooms.Count; i++)
                        {
                            Debug.WriteLine(rooms[i].Name + " " + rooms[i].Door + " " + rooms[i].Light + " " + rooms[i].Heat);
                        }

                        Debug.WriteLine("Devices Retrieved! " + rooms.Count);
                        connection.Close();
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("There was a problem with sending the device value!");
                    connection.Close();
                }
            }
            return rooms;
        }

        private static Room addNewRoom(string deviceId, string deviceDescription, string deviceRoom)
        {
            Room nRoom = new Room();
            nRoom.Name = deviceRoom;
            if (deviceDescription == "Door")
            {
                nRoom.Door = deviceId;
            }
            else if (deviceDescription == "Light")
            {
                nRoom.Light = deviceId;
            }
            else if (deviceDescription == "Heat")
            {
                nRoom.Heat = deviceId;
            }
            return nRoom;
        }

        // Retrieve the values of the devices from the database.
        public static string devicesValuesFromDB(string deviceId)
        {
            // Change the character encoding.
            EncodingProvider encode;
            encode = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encode);

            string deviceValue = "N/A";

            if (deviceId != null)
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception)
                    {

                        Debug.WriteLine("There was a problem connecting to the database. Maybe is not active?");
                    }

                    MySqlCommand retrieveDevices = new MySqlCommand("SELECT deviceValue FROM streamdata2 WHERE deviceId = \"" + deviceId + "\" ORDER BY id DESC LIMIT 1", connection);
                    try
                    {
                        using (MySqlDataReader reader = retrieveDevices.ExecuteReader())
                        {
                        reader.Read();

                            deviceValue = Convert.ToString((double)reader["deviceValue"]);

                            connection.Close();

                            if (deviceValue == "1")
                            {
                                deviceValue = "On";
                            }
                            else if (deviceValue == "0")
                            {
                                deviceValue = "Off";
                            }
                        }
                }
                    catch (Exception)
                {
                    Debug.WriteLine("There was a problem retrieving the device value!");
                    connection.Close();
                }
            }
            }
            return deviceValue;
        }
    }
}

