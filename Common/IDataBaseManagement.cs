using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IDatabaseManagement
    {
        [OperationContract]
        void CreateDatabase();
        [OperationContract]
        void ArchiveDatabase();
        [OperationContract]
        void DeleteDatabase();
        [OperationContract]
        void AddEntry(DatabaseEntry entry);
        [OperationContract]
        void ModifyEntry(DatabaseEntry entry);
        [OperationContract]
        double AvgCityConsumption(string city);
        [OperationContract]
        double AvgRegionConsumption(string region);
        [OperationContract]
        DatabaseEntry HighestRegionConsumer(string region);
    }
}
