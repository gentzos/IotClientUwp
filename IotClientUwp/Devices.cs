namespace IotClientUwp
{
    class Room
    {
        // Room properties.
        public string Name { get; set; }
        public string Door { get; set; }
        public string DoorValue { get; set; }
        public string Light { get; set; }
        public string LightValue { get; set; }
        public string Heat { get; set; }
        public string HeatValue { get; set; }
    }

    class Value
    {
        // Room properties.
        public string dId { get; set; }
        public string dValue { get; set; }
        public string dTime { get; set; }
    }
}
