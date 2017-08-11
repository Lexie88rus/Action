using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataScrapper;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            SelectPartial2();
            
        }

        private static void SelectPartial2()
        {
            var connector = new KadConnector(@"F:\Work\Action\AugustUpdate\Partial_results.accdb");
            

            try
            {              
                var i = 0;

                var resReader = connector.ExecuteQuery("select RID from Data");

                int caseRID;

                while (resReader.Read())
                {
                    var connDocs = new KadConnector(@"F:\Work\Action\AugustUpdate\Partial.accdb");

                    var sw = Stopwatch.StartNew();

                    caseRID = resReader.IsDBNull(0) ? 0 : (int) resReader.GetDouble(0);

                    var docReader =
                        connDocs.ExecuteQuery(
                            "select DecisionType from Documents where FinishInstance = \"True\" and caseRID =" + caseRID);

                    var partial = true;

                    while (docReader.Read())
                    {
                        var decisionType = docReader.IsDBNull(0) ? string.Empty : docReader.GetString(0);

                        if (decisionType.Contains("полностью")) partial = false;
                    }

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

        private static void SelectPartial1()
        {
            var connector = new KadConnector(@"F:\Work\Action\AugustUpdate\Partial_results.accdb");
            var connDocs = new KadConnector(@"F:\Work\Action\AugustUpdate\Partial.accdb");

            try
            {
                var files = Directory.GetFiles(@"F:\Work\Action\AugustUpdate\Pravoru_Export\Documents\Partial\TXT");
                var i = 0;

                foreach (var file in files)
                {
                    var sw = Stopwatch.StartNew();

                    var docRID = file.Split('.')[0].Split('\\').LastOrDefault();
                    var text = SolutionProcessor.GetSolutionText(file);
                    double amount;

                    amount = SolutionProcessor.GetAmount(text);

                    var readerDoc = connDocs.ExecuteQuery("select top 1 CaseRID from Documents where RID = " + docRID);
                    int caseRID = 0;

                    try
                    {
                        while (readerDoc.Read())
                        {
                            caseRID = readerDoc.IsDBNull(0) ? 30 : (int)readerDoc.GetInt32(0);
                        }

                    }
                    catch (Exception e)
                    {
                        string s = e.ToString();
                        s = s + "";
                        continue;
                    }

                    var reader = connector.ExecuteQuery("select Amount from Data where RID = " + caseRID);

                    var caseAmount = 0;

                    while (reader.Read())
                    {

                        caseAmount = reader.IsDBNull(0) ? 0 : (int)reader.GetDouble(0);
                    }

                    if (amount >= caseAmount * 0.8)
                    {
                        connector.ExecuteQuery("update Data set Decision = 2 where RID = " + caseRID.ToString());
                    }

                    i++;
                    Console.WriteLine("Обработано " + i + " за " + sw.ElapsedMilliseconds + " мс");
                }
            }
            finally
            {
                connector.CloseConnection();
                connDocs.CloseConnection();
            }
        }
    }
}
