using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;


namespace AccessSQLService.Model
{
    [DataContract]
    public class Devices
    {
        [DataMember]
        public string DeviceId { get; set; }

        [DataMember]
        public string DeviceType { get; set; }

        [DataMember]
        public string DeviceDescription { get; set; }

        [DataMember]
        public string DeviceRoom { get; set; }

        [DataMember]
        public string DeviceHouse { get; set; }
    }
}