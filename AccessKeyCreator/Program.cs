using DataScrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessKeyCreator
{
    class Program
    {
        static void Main(string[] args)
        {

            //var connector = new KadConnector(@"D:\Data Monsters\Action\Databases\Refuse.accdb");
            var connector = new KadConnector(@"D:\Data Monsters\Action\Databases\Partial.accdb");
            try
            {
                //Ключи для таблицы Sides
                /*var sidesAll = connector.ExecuteQuery("select Sides.Id, Sides.CaseId, Cases.RID from Sides INNER JOIN Cases ON Sides.CaseId = Cases.CaseId");
                while (sidesAll.Read())
                {
                    var id = sidesAll.IsDBNull(0) ? -1 : sidesAll.GetInt32(0);
                    var caseId = sidesAll.IsDBNull(1) ? string.Empty : sidesAll.GetString(1);
                    var RID = sidesAll.IsDBNull(2) ? -1 : sidesAll.GetInt32(2);

                    if (id == -1 || caseId == string.Empty || RID == -1) continue;

                    var r = connector.ExecuteQuery("update Sides set RID = " + RID + " where CaseId = " + caseId);
                }*/

                //Ключи для таблицы Documents
                var docsAll = connector.ExecuteQuery("select Documents.RID, Documents.CaseId, Cases.RID from Documents INNER JOIN Cases ON Documents.CaseId = Cases.CaseId");
                while (docsAll.Read())
                {
                    var id = docsAll.IsDBNull(0) ? -1 : docsAll.GetInt32(0);
                    var caseId = docsAll.IsDBNull(1) ? string.Empty : docsAll.GetString(1);
                    var RID = docsAll.IsDBNull(2) ? -1 : docsAll.GetInt32(2);

                    if (id == -1 || caseId == string.Empty || RID == -1) continue;

                    var r = connector.ExecuteQuery("update Documents set CaseRID = " + RID + " where CaseId = " + caseId);
                }

                //Ключи для таблицы Instances
                var instancesAll = connector.ExecuteQuery("select Instances.Id, Instances.CaseId, Cases.RID from Instances INNER JOIN Cases ON Instances.CaseId = Cases.CaseId");
                while (instancesAll.Read())
                {
                    var id = instancesAll.IsDBNull(0) ? string.Empty : instancesAll.GetString(0);
                    var caseId = instancesAll.IsDBNull(1) ? string.Empty : instancesAll.GetString(1);
                    var RID = instancesAll.IsDBNull(2) ? -1 : instancesAll.GetInt32(2);

                    if (id == string.Empty || caseId == string.Empty || RID == -1) continue;

                    var r = connector.ExecuteQuery("update Instances set CaseRID = " + RID + " where CaseId = " + caseId);
                }
            }
            finally
            {
                connector.CloseConnection();
            }
        }
    }
}
