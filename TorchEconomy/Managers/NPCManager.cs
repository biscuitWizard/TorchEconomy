using System.Linq;
using Dapper;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Data.Types;

namespace TorchEconomy.Managers
{
    public class NPCManager : BaseManager
    {
        public NPCManager(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public Promise<NPCDataObject[]> GetNPCs()
        {
            return new Promise<NPCDataObject[]>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    resolve(connection.Query<NPCDataObject>(SQL.SELECT_NPCS).ToArray());
                }
            });
        }

        public Promise<NPCDataObject> CreateNPC(string name, IndustryTypeEnum industryType = IndustryTypeEnum.None)
        {
            return new Promise<NPCDataObject>((resolve, reject) =>
            {
                using (var connection = ConnectionFactory.Open())
                {
                    connection.Execute(SQL.INSERT_NPC, new {name = name, industryType = industryType});
                    resolve(
                        connection.QueryFirstOrDefault<NPCDataObject>(
                        "SELECT * FROM `NPC` WHERE `Name`=@name AND `IsDeleted`=0",
                        new {name = name})
                    );
                }
            });
        }
    }
}