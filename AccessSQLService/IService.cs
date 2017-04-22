using AccessSQLService.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace AccessSQLService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        IList<Devices> QueryDevices();

        [OperationContract]
        IList<Devices> QueryDevices2();
    }
}
