using DataScrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessGatheredFeatures
{
    class Program
    {
        static void Main(string[] args)
        {
            var resultsPath = @"D:\Data Monsters\Action\Статьи\Databases\Satisfy_results.accdb";
            var filename = @"D:\Data Monsters\Action\Статьи\satisfy_final.csv";

            ExportResults(resultsPath, filename);
        }

        private static void ReplaceNulls()
        {
            var results = new KadConnector(@"D:\Data Monsters\Action\Статьи\Databases\Refuse_results.accdb");

            try
            {
                var reader = results.ExecuteQuery("select * from refuse_laws order by rid");
                         
                var p = 0;

                while(reader.Read())
                {
                    var sw = Stopwatch.StartNew();

                    var update_query = "update refuse_laws ";
                    var f = 1;

                    var rid = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    if (rid > 0)
                    {
                        for (int i = 1; i <= 251; i++)
                        {
                            if (reader.IsDBNull(i))
                            {
                                if (f == 1)
                                    update_query += "set [" + i + "] = 0";
                                else
                                    update_query += " , [" + i + "] = 0";

                                f++;
                            }

                            if (f == 100)
                            {
                                update_query += " where RID = " + rid;
                                results.ExecuteQuery(update_query);

                                update_query = "update refuse_laws ";
                                f = 1;
                            }
                        }

                        if (f > 1)
                        {
                            update_query += " where RID = " + rid;

                            results.ExecuteQuery(update_query);
                        }
                    }

                    p++;
                    Console.WriteLine("Обработано " + p + " за " + sw.ElapsedMilliseconds + " мс");
                }
            }
            finally
            {
                results.CloseConnection();
            }
        }

        private static void ExportResults(string resultsPath, string filename)
        {
            var results = new KadConnector(resultsPath);

            var fileStream = new System.IO.StreamWriter(filename);

            try
            {
                var readerv1 = results.ExecuteQuery("select * from Data");
                var p = 0;

                while (readerv1.Read())
                {
                    var sw = Stopwatch.StartNew();

                    var rid = readerv1.IsDBNull(0) ? 0 : (int)readerv1.GetInt32(0);
                    var vals = new List<int>();

                    if (rid > 0)
                    {
                        vals.Add(rid);
                        for (int i = 1; i < 100; i++)
                        {
                            //if (i >= 95)
                            //{
                            //    var v = readerv1.IsDBNull(i) ? 0 : (int)readerv1.GetInt32(i);
                            //    vals.Add((int)v);
                            //}
                            //else
                            //{
                                var v = readerv1.IsDBNull(i) ? 0 : (int)readerv1.GetInt32(i);
                                vals.Add((int)v);
                            //}
                        }

                        var readerv2 = results.ExecuteQuery("select * from satisfy_laws where rid = " + rid);

                        while (readerv2.Read())
                        {
                            for (int i = 1; i <= 251; i++)
                            {
                                var v = readerv2.IsDBNull(i) ? "0" : readerv2.GetString(i);
                                vals.Add(int.Parse(v));
                            }
                        }
                    }

                    ExportToCSV(vals, fileStream);

                    p++;
                    Console.WriteLine("Обработано " + p + " за " + sw.ElapsedMilliseconds + " мс");
                }
            }
            finally
            {
                results.CloseConnection();
                fileStream.Close();
            }
        }

        private static void ExportToCSV(List<int> vals, System.IO.StreamWriter stream)
        {
            var caseString = string.Empty;
            foreach (var f in vals)
            {               
                        caseString += f + ";";
            }
            stream.WriteLine(caseString);
        }
    }
}
