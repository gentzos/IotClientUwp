using System.Collections.Generic;

namespace IotClientUwp
{
    class LuisBot
    {
        public Dictionary<string, string> luisIntents = new Dictionary<string, string>();
        public Dictionary<string, string> luisEntities = new Dictionary<string, string>();

        public LuisBot()
        {
            // Bot Intents
            luisIntents.Add("AdjustTemp", "AdjustTemp"); //Adjusting the
            luisIntents.Add("TurnOn", "TurnOn"); //Turning on the
            luisIntents.Add("TurnOff", "TurnOff"); //Turning off the
            luisIntents.Add("Lock", "Lock"); //Locking the
            luisIntents.Add("Unlock", "Unlock"); //Unlocking the

            // Bot Entities
            luisEntities.Add("Appliance", "Appliance");
            luisEntities.Add("Equipment", "Equipment");
            luisEntities.Add("Room", "Room");
            luisEntities.Add("Temperature", "Temperature");
        }
    }
}
