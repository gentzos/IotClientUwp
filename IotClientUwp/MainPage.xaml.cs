using Microsoft.Azure.Devices;
using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IotClientUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string[] typesOfDevices = { "lightbulb", "thermostat", "doorlock"};
        
        static ServiceClient serviceClient;
        static string connectionString = "";
                
        private string welcome = "Welcome!";

        private LuisBot botIntentsEntities = new LuisBot();

        private LuisResult prevResult { get; set; }

        
        private List<Room> retrievedRooms = new List<Room>();

        // Create a new dictionary of strings, with string keys.
        private Dictionary<string, string> luisResult = new Dictionary<string, string>();
        private Dictionary<string, string> deviceRoomRelation = new Dictionary<string, string>();

        public MainPage()
        {
            this.InitializeComponent();

            // Client for Cloud to Device communication.
            // To control the individual devices from this application.
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            // Retrieve devices from Database.
            retrievedRooms = MySqlDB.devicesFromDB();
            
            //Try to update the UI based on the data from the database.
            try
            {
                updateUI();
            }
            catch (Exception)
            {
                Debug.WriteLine("UI not update!");
            }       
        }

        // Start communication with the Bot (Microsoft LUIS).
        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text != "")
            {
                string status = Convert.ToString(buttonSend.Content);
                if (status == "Send")
                {
                    buttonSend.Content = "Reply";
                    Predict();
                }
                else
                {
                    if (prevResult == null || (prevResult.DialogResponse != null
                        && prevResult.DialogResponse.Status == "Finished"))
                    {
                        Debug.WriteLine("There is nothing to reply to.");
                        textBlockWelcome.Text = welcome;
                        buttonSend.Content = "Send";
                        clearLuisResult();
                        return;
                    }
                    try
                    {
                        Reply();
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception.Message);
                    }
                }
            }
        }

        // To see if LUIS was accurate in what the user typed/said.
        public async void Predict()
        {
            string appId = "";
            string subscriptionKey = "";
            bool preview = true;
            string textToPredict = textBox.Text;
            //string forceSetParameterName = txtForceSet.Text;
            try
            {
                LuisClient client = new LuisClient(appId, subscriptionKey, preview);
                LuisResult res = await client.Predict(textToPredict);
                processRes(res);
                Debug.WriteLine("Predicted successfully.");
            }
            catch (System.Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        // Keep the conversation going with LUIS. Context-aware.
        public async void Reply()
        {
            string appId = "";
            string subscriptionKey = "";
            bool preview = true;
            string textToPredict = textBox.Text;
            //string forceSetParameterName = txtForceSet.Text;
            try
            {
                LuisClient client = new LuisClient(appId, subscriptionKey, preview);
                LuisResult res = await client.Reply(prevResult, textToPredict);
                processRes(res);
                Debug.WriteLine("Replied successfully.");
            }
            catch (System.Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        // Process the responses from LUIS.
        private async void processRes(LuisResult res)
        {
            textBox.Text = "";
            prevResult = res;
            var entities = res.GetAllEntities();
            
            // Iterate all entities.
            foreach (Entity entity in entities)
            {
                // if entity."Appliance" == botIntentsEntities.luisEntities.entity.["Appliance"].
                // if entity."Equipment" == botIntentsEntities.luisEntities.entity.["Equipment"].
                // if entity."Room" == botIntentsEntities.luisEntities.entity.["Room"].
                // if entity."Temperature" == botIntentsEntities.luisEntities.entity.["Temperature"].
                if (entity.Name == botIntentsEntities.luisEntities[entity.Name])
                {
                    // Appliance = heat or lights, etc.
                    // Equipment = door or windows, etc.
                    // Room = bedroom or living room, etc.
                    // Temperature = 12 or 24, etc.
                    luisResult.Add(entity.Name, entity.Value);
                }
            }

            if (res.DialogResponse != null)
            {
                // The conversation has not finished so keep asking the user questions about the initial query.
                if (res.DialogResponse.Status != "Finished")
                {
                    textBlockWelcome.Text = res.DialogResponse.Prompt;
                }
                // If the dialog has finished display the final message and send data to individual devices.
                else
                {       
                    bool roomExist = false;
                    bool sensorExist = false;
                    // Check all the rooms saved on the database and compare them with the room returned from the Bot (LUIS).
                    for (int i = 0; i < retrievedRooms.Count; i++)
                    {
                        // If the room exists.
                        if (string.Equals(luisResult["Room"], retrievedRooms[i].Name, StringComparison.OrdinalIgnoreCase))
                        {
                            roomExist = true;

                            // if intent."AdjustTemp" == botIntentsEntities.luisEntities.entity.["AdjustTemp"].
                            // if intent."TurnOn" == botIntentsEntities.luisEntities.entity.["TurnOn"].
                            // if intent."TurnOff" == botIntentsEntities.luisEntities.entity.["TurnOff"].
                            // if intent."Lock" == botIntentsEntities.luisEntities.entity.["Lock"].
                            // if intent."Unlock" == botIntentsEntities.luisEntities.entity.["Unlock"].
                            if (res.Intents[0].Name == botIntentsEntities.luisIntents[res.Intents[0].Name])
                            {
                                if ((res.Intents[0].Name == botIntentsEntities.luisIntents["AdjustTemp"]) && 
                                    (retrievedRooms[i].Heat != null) && 
                                    (string.Equals(luisResult["Appliance"], "heat", StringComparison.OrdinalIgnoreCase)))
                                {
                                        sensorExist = true;

                                        // Send data to the coresponding IoT device.
                                        await SendCloudToDeviceMessageAsync(retrievedRooms[i].Heat, luisResult["Temperature"]);

                                        // Display success message.
                                        textBlockWelcome.Text = "Adjusting the " + luisResult["Appliance"] + " in the " +
                                            luisResult["Room"] + " at " + luisResult["Temperature"] + " Celsius!";

                                        // Update the object containing the rooms and device ids.
                                        retrievedRooms[i].HeatValue = luisResult["Temperature"] + " °C";

                                        // Update the UI.
                                        updateUI();

                                        Debug.WriteLine(retrievedRooms[i].Name + " " +
                                        retrievedRooms[i].Heat + " " + retrievedRooms[i].HeatValue);

                                        break;
                                }
                                else if (res.Intents[0].Name == botIntentsEntities.luisIntents["TurnOn"] && 
                                    retrievedRooms[i].Light != null)
                                {
                                    sensorExist = true;

                                    if (string.Equals(luisResult["Appliance"], "lights", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(luisResult["Appliance"], "light", StringComparison.OrdinalIgnoreCase))
                                    {
                                        //sensorExist = true;

                                        // Send data to the coresponding IoT device.
                                        await SendCloudToDeviceMessageAsync(retrievedRooms[i].Light, "1");

                                        // Display success message.
                                        textBlockWelcome.Text = "Turning on the " + luisResult["Appliance"] + " in the " +
                                            luisResult["Room"] + " !";

                                        // Update the object containing the rooms and device ids.
                                        retrievedRooms[i].LightValue = "On";

                                        // Update the UI.
                                        updateUI();

                                        Debug.WriteLine(retrievedRooms[i].Name + " " +
                                        retrievedRooms[i].Light + " " + retrievedRooms[i].LightValue);

                                        break;
                                    }
                                    break;
                                }
                                else if (res.Intents[0].Name == botIntentsEntities.luisIntents["TurnOff"] &&
                                    retrievedRooms[i].Light != null)
                                {
                                    sensorExist = true;

                                    if (string.Equals(luisResult["Appliance"], "lights", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(luisResult["Appliance"], "light", StringComparison.OrdinalIgnoreCase))
                                    {
                                        //sensorExist = true;

                                        // Send data to the coresponding IoT device.
                                        await SendCloudToDeviceMessageAsync(retrievedRooms[i].Light, "0");

                                        // Display success message.
                                        textBlockWelcome.Text = "Turning off the " + luisResult["Appliance"] + " in the " +
                                            luisResult["Room"] + " !";

                                        // Update the object containing the rooms and device ids.
                                        retrievedRooms[i].LightValue = "Off";

                                        // Update the UI.
                                        updateUI();

                                        Debug.WriteLine(retrievedRooms[i].Name + " " +
                                        retrievedRooms[i].Light + " " + retrievedRooms[i].LightValue);

                                        break;
                                    }
                                    break;
                                }
                                else if (res.Intents[0].Name == botIntentsEntities.luisIntents["Lock"] && 
                                    retrievedRooms[i].Door != null)
                                {
                                    sensorExist = true;

                                    if (string.Equals(luisResult["Equipment"], "door", StringComparison.OrdinalIgnoreCase))
                                    {
                                        //sensorExist = true;

                                        // Send data to the coresponding IoT device.
                                        await SendCloudToDeviceMessageAsync(retrievedRooms[i].Door, "1");

                                        // Display success message.
                                        textBlockWelcome.Text = "Locking the " + luisResult["Equipment"] + " in the " +
                                            luisResult["Room"] + " !";

                                        // Update the object containing the rooms and device ids.
                                        retrievedRooms[i].DoorValue = "Locked";

                                        // Update the UI.
                                        updateUI();

                                        Debug.WriteLine(retrievedRooms[i].Name + " " +
                                        retrievedRooms[i].Door + " " + retrievedRooms[i].DoorValue);

                                        break;
                                    }
                                    break;
                                }
                                else if (res.Intents[0].Name == botIntentsEntities.luisIntents["Unlock"] &&
                                    retrievedRooms[i].Door != null)
                                {
                                    sensorExist = true;

                                    if (string.Equals(luisResult["Equipment"], "door", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // Send data to the coresponding IoT device.
                                        await SendCloudToDeviceMessageAsync(retrievedRooms[i].Door, "0");

                                        // Display success message.
                                        textBlockWelcome.Text = "Unlocking the " + luisResult["Equipment"] + " in the " +
                                            luisResult["Room"] + " !";

                                        // Update the object containing the rooms and device ids.
                                        retrievedRooms[i].DoorValue = "Unlocked";

                                        // Update the UI.
                                        updateUI();

                                        Debug.WriteLine(retrievedRooms[i].Name + " " +
                                        retrievedRooms[i].Door + " " + retrievedRooms[i].DoorValue);

                                        break;
                                    }
                                    break;
                                }
                            }
                            // Room found so break the loop.
                            break;
                        }
                    }

                    // If room does not exist notify the user.
                    if (!roomExist)
                    {
                        textBlockWelcome.Text = "This room doesn't exists!";
                    }

                    // If room does not exist notify the user.
                    if (!sensorExist)
                    {
                        textBlockWelcome.Text = "This room isn't equipped with these type of sensors!";
                    }

                    // Start fresh for a new conversation with LUIS.
                    // Remove all the previous results.
                    buttonSend.Content = "Send";
                    clearLuisResult();

                    // Wait 5 seconds before showing the welcome message again.
                    await Wait();
                }
            }
            else
            {
                // Start fresh for a new conversation with LUIS.
                // Remove all the previous results.
                buttonSend.Content = "Send";
                clearLuisResult();

                // Wait 5 seconds before showing the welcome message again.
                await Wait();
            }
        }

        // Start fresh for a new conversation with LUIS.
        // Remove all the previous results.
        private void clearLuisResult()
        {
            luisResult.Clear();
        }

        // Wait 5 seconds before showing the welcome message again.
        public async Task Wait()
        {
            await Task.Delay(5000);
            textBlockWelcome.Text = welcome;
        }

        // Send message to individual IoT devices.
        private async static Task SendCloudToDeviceMessageAsync(string deviceId, string deviceValue)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes(deviceValue));
            await serviceClient.SendAsync(deviceId, commandMessage);
        }

        private void buttonListen_Click(object sender, RoutedEventArgs e)
        {
            //updateUi();
        }

        // Update the UI based on the data from the database.
        private void updateUI()
        {
            // Room 1
            textBlockRoom1.Text = retrievedRooms[0].Name;
            textBlockRoom1Sensor1.Text = "Door:";
            textBlockRoom1Sensor2.Text = "Light:";
            textBlockRoom1Sensor3.Text = "Heat:";
            
            textBlockRoom1Sensor1Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[0].Door);
            textBlockRoom1Sensor2Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[0].Light);
            textBlockRoom1Sensor3Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[0].Heat);

            if (retrievedRooms[0].DoorValue != null)
            {
                textBlockRoom1Sensor1Status.Text = retrievedRooms[0].DoorValue;
            }
            if (retrievedRooms[0].LightValue != null)
            {
                textBlockRoom1Sensor2Status.Text = retrievedRooms[0].LightValue;
            }
            if (retrievedRooms[0].HeatValue != null)
            {
                textBlockRoom1Sensor3Status.Text = retrievedRooms[0].HeatValue;
            }

            // Room 2
            textBlockRoom2.Text = retrievedRooms[1].Name;
            textBlockRoom2Sensor1.Text = "Door:";
            textBlockRoom2Sensor2.Text = "Light:";
            textBlockRoom2Sensor3.Text = "Heat:";

            textBlockRoom2Sensor1Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[1].Door);
            textBlockRoom2Sensor2Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[1].Light);
            textBlockRoom2Sensor3Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[1].Heat);

            if (retrievedRooms[1].DoorValue != null)
            {
                textBlockRoom2Sensor1Status.Text = retrievedRooms[1].DoorValue;
            }
            if (retrievedRooms[1].LightValue != null)
            {
                textBlockRoom2Sensor2Status.Text = retrievedRooms[1].LightValue;
            }
            if (retrievedRooms[1].HeatValue != null)
            {
                textBlockRoom2Sensor3Status.Text = retrievedRooms[1].HeatValue;
            }

            // Room 3
            textBlockRoom3.Text = retrievedRooms[2].Name;
            textBlockRoom3Sensor1.Text = "Door:";
            textBlockRoom3Sensor2.Text = "Light:";
            textBlockRoom3Sensor3.Text = "Heat:";

            textBlockRoom3Sensor1Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[2].Door);
            textBlockRoom3Sensor2Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[2].Light);
            textBlockRoom3Sensor3Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[2].Heat);

            if (retrievedRooms[2].DoorValue != null)
            {
                textBlockRoom3Sensor1Status.Text = retrievedRooms[2].DoorValue;
            }
            if (retrievedRooms[2].LightValue != null)
            {
                textBlockRoom3Sensor2Status.Text = retrievedRooms[2].LightValue;
            }
            if (retrievedRooms[2].HeatValue != null)
            {
                textBlockRoom3Sensor3Status.Text = retrievedRooms[2].HeatValue;
            }

            // Room 4
            textBlockRoom4.Text = retrievedRooms[3].Name;
            textBlockRoom4Sensor1.Text = "Door:";
            textBlockRoom4Sensor2.Text = "Light:";
            textBlockRoom4Sensor3.Text = "Heat:";

            textBlockRoom4Sensor1Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[3].Door);
            textBlockRoom4Sensor2Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[3].Light);
            textBlockRoom4Sensor3Status.Text = MySqlDB.devicesValuesFromDB(retrievedRooms[3].Heat);

            if (retrievedRooms[3].DoorValue != null)
            {
                textBlockRoom4Sensor1Status.Text = retrievedRooms[3].DoorValue;
            }
            if (retrievedRooms[3].LightValue != null)
            {
                textBlockRoom4Sensor2Status.Text = retrievedRooms[3].LightValue;
            }
            if (retrievedRooms[3].HeatValue != null)
            {
                textBlockRoom4Sensor3Status.Text = retrievedRooms[3].HeatValue;
            }
        }
    }
}
