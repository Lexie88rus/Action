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

                //var filename = @"D:\Data Monsters\Action\PravoRu_Export\PravoRu_Export\dc6370bb-db3f-4985-ab81-4b3550f26e20.pdf.txt";
                //var text = SolutionProcessor.GetSolutionText(filename);
                //var str = SolutionProcessor.GetJudgeName(text);

                var connector = new KadConnector(@"D:\Data Monsters\Action\Databases\Refuse.accdb");

                //var connector = new KadConnector(@"D:\Data Monsters\Action\Databases\Partial.accdb");

                var cases = new List<Case>();

                try
                {

                    //var judgeId = SolutionProcessor.GetJudgeId(connector, str, 1);

                    var scrapper = new Scrapper(connector);
                    var casesDb = scrapper.RawCases;

                    cases = casesDb.Select(c => new Case(c, scrapper.DocumentTypes, (int)Decisions.Refuse)).ToList();
                }
                finally
                {
                    connector.CloseConnection();
                }

                FeatureExport.ExportToCSV(cases, "D:\\Data Monsters\\Action\\dataset.csv", ";");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
    }
}
