using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrapper
{
    /// <summary>
    /// Класс для выделения частично и полностью удовлетворенных дел с помощью поля DecisonType в таблице Documents
    /// </summary>
   public class SelectPartialByMeta
    {
        /// <summary>
        /// Метод для проставления типа решения по метаданным
        /// </summary>
        /// <param name="storagePath">БД с метаданными</param>
        /// <param name="resultsPath">БД с результатами обработки</param>
        public static void SelectPartial(string storagePath, string resultsPath)
        {
            var connector = new KadConnector(resultsPath);

            try
            {
                var i = 0;

                var resReader = connector.ExecuteQuery("select RID from Data");

                int caseRID;

                //Идем по всем RID дел
                while (resReader.Read())
                {
                    var connDocs = new KadConnector(storagePath);

                    var sw = Stopwatch.StartNew();

                    caseRID = resReader.IsDBNull(0) ? 0 : (int)resReader.GetDouble(0);

                    //Получаем DecisionType всех документов по делу, где FinishInstance = "True"
                    var docReader =
                        connDocs.ExecuteQuery(
                            "select DecisionType from Documents where FinishInstance = \"True\" and caseRID =" + caseRID);

                    var partial = true;

                    //Если DecisionType содержит "полностью", то считаем дело полностью удовлетворенным
                    while (docReader.Read())
                    {
                        var decisionType = docReader.IsDBNull(0) ? string.Empty : docReader.GetString(0);

                        if (decisionType.Contains("полностью")) partial = false;
                    }

                    //Если дело полсностью удовлетворено, проставляем данное решение в БД
                    if (!partial)
                    {
                        connector.ExecuteQuery("update Data set Decision = 2 where RID = " + caseRID.ToString());
                    }

                    connDocs.CloseConnection();

                    i++;
                    Console.WriteLine("Обработано " + i + " за " + sw.ElapsedMilliseconds + " мс");
                }

            }
            finally
            {
                connector.CloseConnection();
            }
        }
    }
}
