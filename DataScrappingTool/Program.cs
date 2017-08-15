using CaseModel;
using DataScrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrappingTool
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //var connector = new KadConnector(@"D:\Data Monsters\Action\kad_10000.accdb");

               // var filename = @"F:\Work\Action\AugustUpdate\Pravoru_Export\Documents\Partial\TXT\158184757.pdf.txt";
               // var text = SolutionProcessor.GetSolutionText(filename);
              //  var d = SolutionProcessor.GetAmount(text);

                //var str = SolutionProcessor.GetJudgeName(text);

                var connector = new KadConnector(@"D:\Data Monsters\Action\Databases\Refuse.accdb");

                //var connector = new KadConnector(@"F:\Work\Action\AugustUpdate\Satisfy400000.accdb");

                var cases = new List<Case>();

                try
                {

                    //var judgeId = SolutionProcessor.GetJudgeId(connector, str, 1);

                    var scrapper = new Scrapper(connector, @"D:\Data Monsters\Action\Databases\Refuse.accdb");
                    var casesDb = scrapper.RawCases;

                    cases = casesDb.Select(c => c.ConvertToCase((int)Decisions.Fully, scrapper.DocumentTypes)).ToList();
                }
                finally
                {
                    connector.CloseConnection();
                }

                FeatureExport.ExportToCSV(cases, @"D:\Data Monsters\Action\Databases\dataset.csv", ";");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
    }
}
