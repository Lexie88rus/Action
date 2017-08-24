using DataScrapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetLaws
{
    /// <summary>
    /// Программа для извлечения статей
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ProcessOptimized();
        }

        private static void Process()
        {
            SqlConnection con = new SqlConnection(@"Data source = (local)\SQLEXPRESS; Initial catalog = AHL_Base; persist security info=True; Integrated Security = SSPI; ");

            con.Open();

            var results = new KadConnector(@"D:\Data Monsters\Action\Статьи\Databases\Partial_results.accdb");

            var connector = new KadConnector(@"D:\Data Monsters\Action\Статьи\Databases\Partial.accdb");

            int i = 0;

            try
            {
                var laws = GetLaws();

                var docs = connector.ExecuteQuery("select rid, caserid from Documents order by rid");

                while (docs.Read())
                {
                    while (i >= 0)
                    {
                        var sw = Stopwatch.StartNew();

                        var docRID = docs.IsDBNull(0) ? 0 : docs.GetInt32(0);
                        var caseRID = docs.IsDBNull(1) ? 0 : docs.GetInt32(1);

                        if (docRID != 0 && caseRID != 0)
                        {
                            SqlCommand cmd = new SqlCommand("select [docName], [docNumber] from [dbo].[HlsCandidates_Partial] where [file] = '" + docRID + "'", con);
                            SqlDataReader read = cmd.ExecuteReader();

                            while (read.Read())
                            {

                                var docName = read.IsDBNull(0) ? "NULL" : read.GetString(0);
                                var docNumber = read.IsDBNull(1) ? "NULL" : read.GetString(1);

                                if (docName != "NULL" || docNumber != "NULL")
                                {
                                    var law = docName + " " + docNumber;

                                    var lawId = laws.Where(l => l.Value == law).Select(l => l.Key).FirstOrDefault();

                                    if (lawId > 0)
                                    {
                                        results.ExecuteQuery(@"update Partial_laws set " + lawId + " = 1 where RID = " + caseRID);
                                    }
                                }
                            }

                            read.Close();
                        }

                        i++;

                        Console.WriteLine("Обработано " + i + " за " + sw.ElapsedMilliseconds + " мс");
                    }
                }
            }
            finally
            {
                results.CloseConnection();
                con.Close();
                connector.CloseConnection();
            }
        }

        private static void ProcessOptimized()
        {
            SqlConnection con = new SqlConnection(@"Data source = (local)\SQLEXPRESS; Initial catalog = AHL_Base; persist security info=True; Integrated Security = SSPI; ");

            con.Open();

            var results = new KadConnector(@"D:\Data Monsters\Action\Статьи\Databases\Satisfy_results.accdb");

            int i = 0;

            try
            {
                SqlCommand cmd = new SqlCommand("select distinct [file], [docName], [docNumber] from [dbo].[HlsCandidates_Satisfy]", con);
                cmd.CommandTimeout = 10000;
                SqlDataReader read = cmd.ExecuteReader();

                var laws = GetLaws();

                while (read.Read())
                {
                    var sw = Stopwatch.StartNew();

                    var docRID = read.IsDBNull(0) ? string.Empty : read.GetString(0);
                    var docName = read.IsDBNull(1) ? "NULL" : read.GetString(1);
                    var docNumber = read.IsDBNull(2) ? "NULL" : read.GetString(2);

                    if (docRID != string.Empty && (docName != "NULL" || docNumber != "NULL"))
                    {
                        var connector = new KadConnector(@"D:\Data Monsters\Action\Статьи\Databases\Satisfy.accdb");

                        var reader = connector.ExecuteQuery("select top 1 caserid from Documents where rid = " + docRID);

                        while (reader.Read())
                        {
                            var caseRID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);

                            if (caseRID != 0)
                            {
                                var law = docName + " " + docNumber;

                                var lawId = laws.Where(l => l.Value == law).Select(l => l.Key).FirstOrDefault();

                                if (lawId > 0)
                                {
                                    results.ExecuteQuery(@"update Satisfy_laws set " + lawId + " = 1 where RID = " + caseRID);
                                }
                            }
                        }

                        connector.CloseConnection();
                    }

                    i++;
                    Console.WriteLine("Обработано " + i + " за " + sw.ElapsedMilliseconds + " мс");
                }
            }
            finally
            {
                con.Close();
                results.CloseConnection();
            }
        }

        private static Dictionary<int, string> GetLaws()
        {
            var connector = new KadConnector(@"D:\Data Monsters\Action\Статьи\Databases\Refuse1.accdb");

            var laws = new Dictionary<int, string>();

            try
            {
                var reader = connector.ExecuteQuery("select Id, Feature from Laws");

                while (reader.Read())
                {
                    var id = reader.IsDBNull(0) ? 0 : reader.GetInt16(0);
                    var feature = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);

                    laws.Add(id, feature);
                }

                return laws;
            }
            finally
            {
                connector.CloseConnection();
            }

        }
    }
}
