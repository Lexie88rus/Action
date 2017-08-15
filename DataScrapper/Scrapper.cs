using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataScrapper
{
    public class Scrapper
    {
        public Scrapper(KadConnector connector, string connString)
        {
            if (connector == null) throw new ArgumentNullException("connector");
            if (string.IsNullOrEmpty(connString)) throw new ArgumentNullException("connString");

            this.connector = connector; //?? throw new ArgumentNullException("connector");
            this.connString = connString;
                //String.IsNullOrEmpty(connString) ? throw new ArgumentNullException("connString") :  connString;
        }

        private KadConnector connector;

        private string connString;

        private const int maxTasksNum = 5;

        private int currentTasksNum = 0;

        static readonly object locker = new object();

        private List<Task> tasks = new List<Task>();

        private int processed = 0;

        /// <summary>
        /// Сырые данные о делах
        /// </summary>
        private List<CaseDb> rawCases = new List<CaseDb>();

        /// <summary>
        /// Типы документов
        /// </summary>
        private Dictionary<int, string> documentTypes = new Dictionary<int, string>();

        /// <summary>
        /// Типы содержимого
        /// </summary>
        private Dictionary<int, string> documentContentTypes = new Dictionary<int, string>();

        public IList<CaseDb> RawCases
        {
            get
            {
                if (rawCases == null || rawCases.Count == 0)
                {
                    GetRawCasesData(connector, connString);
                    return rawCases;
                }
                else return rawCases;
            }
        }

        public IDictionary<int, string> DocumentTypes
        {
            get
            {
                if (documentTypes == null || documentTypes.Count == 0)
                {
                    GetDocumentTypes(connector);
                    return documentTypes;
                }
                else return documentTypes;
            }
        }

        public IDictionary<int, string> DocumentContentTypes
        {
            get
            {
                if (documentContentTypes == null || documentContentTypes.Count == 0)
                {
                    GetDocumentContentTypes(connector);
                    return documentContentTypes;
                }
                else return documentContentTypes;
            }
        }

        /// <summary>
        /// Получить типы документов
        /// </summary>
        private void GetDocumentTypes(KadConnector connector)
        {
            var reader = connector.ExecuteQuery("select Id, Name from Document_Type");
            while (reader.Read())
            {
                if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                {
                    documentTypes.Add(reader.GetInt32(0), reader.GetString(1));
                }
            }

            reader.Close();
        }

        /// <summary>
        /// Получить типы содержимого
        /// </summary>
        private void GetDocumentContentTypes(KadConnector connector)
        {
            var reader = connector.ExecuteQuery("select Id, Name from Contenttype");
            while (reader.Read())
            {
                if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                {
                    documentContentTypes.Add(reader.GetInt32(0), reader.GetString(1));
                }
            }

            reader.Close();
        }

        /// <summary>
        /// Получение сырых данных о делах
        /// </summary>
        /// <param name="connector">Курсор для чтения данных</param>
        private void GetRawCasesData(KadConnector connector, string connString)
        {
            if (connector == null) throw new ArgumentNullException("connector");

            this.GetDocumentContentTypes(connector);
            this.GetDocumentTypes(connector);

            var factory = new TaskFactory();

            //Получаем основные данные о делах
            var reader = connector.ExecuteQuery("select RID, CourtId, Date, Sum, CategoryDisputeId from Cases");

            while (reader.Read())
            {

                try
                {
                    Stopwatch s1 = Stopwatch.StartNew();
                    //Console.WriteLine("1 Прошло: " + s1.ElapsedMilliseconds + " мс.");

                    //получаем основные данные по делу
                    var rawCase = new CaseDb()
                    {
                        RID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        CourtId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                        Date = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2),
                        Amount = reader.IsDBNull(3) ? 0 : (int)reader.GetDouble(3),
                        CategoryId = reader.IsDBNull(4) ? 0 : (int)reader.GetInt32(4),
                        //CaseId = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    };

                    //Console.WriteLine("2 Прошло: " + s1.ElapsedMilliseconds + " мс.");

                    //while (this.currentTasksNum >= maxTasksNum) Thread.Sleep(2000);

                    //tasks.Add(factory.StartNew(() => GetData(this.connString, rawCase)));

                    //жопа тут
                    GetData(connector, rawCase);

                    rawCases.Add(rawCase);

                    if (processed % 10000 == 0)
                    {
                        //Console.WriteLine("Собрано данных по делам:" + processed);
                        var pr = rawCases.Select(c => c.ConvertToCase((int)Decisions.Fully, this.DocumentTypes)).ToList();
                        FeatureExport.ExportToCSV(pr, "F:\\Work\\Action\\AugustUpdate\\dataset" + processed.ToString() +".csv", ";");
                    }

                    Console.WriteLine(processed + " Прошло: " + s1.ElapsedMilliseconds + " мс.");
                }
                catch
                {
                    continue;
                }
            }

            //Task.WaitAll(tasks.ToArray());

            var cases = this.rawCases.Select(c => c.ConvertToCase((int)Decisions.Fully, this.DocumentTypes)).ToList();
            FeatureExport.ExportToCSV(cases, "F:\\Work\\Action\\AugustUpdate\\dataset" + processed.ToString() + ".csv", ";");

            reader.Close();
        }

        private void GetData(KadConnector connector, CaseDb rawCase)
        {
            //lock(locker)
            //{
            //    this.currentTasksNum++;
            //}

            //var connector = new KadConnector(connString);

            try
            {
                Stopwatch s2 = Stopwatch.StartNew();
                //получаем всех участников процесса
                var sidesReader =
                    connector.ExecuteQuery("select Name, Inn, Ogrn, SideTypeId, Address from Sides where CaseRID = " +
                                           rawCase.RID);

                //Console.WriteLine("1 Прошло: " + s2.ElapsedMilliseconds + " мс.");
                while (sidesReader.Read())
                {
                    //Console.WriteLine("2 Прошло: " + s2.ElapsedMilliseconds + " мс.");
                    //здесь пол секунды
                    if (sidesReader.IsDBNull(3)) break;

                    var sideType = (int) sidesReader.GetInt32(3);
                    switch (sideType)
                    {
                        //ответчик
                        case (int) Participants.Respondent:
                            if (rawCase.Responent == null) rawCase.Responent = new List<Participant>();
                            rawCase.Responent.Add(new Participant()
                            {
                                Name = sidesReader.IsDBNull(0) ? null : sidesReader.GetString(0),
                                Inn = sidesReader.IsDBNull(1) ? null : sidesReader.GetString(1),
                                Ogrn = sidesReader.IsDBNull(2) ? null : sidesReader.GetString(2),
                                Address = sidesReader.IsDBNull(4) ? null : sidesReader.GetString(4)
                            });
                            break;
                        //истец
                        case (int) Participants.Claimant:
                            if (rawCase.Claimant == null) rawCase.Claimant = new List<Participant>();
                            rawCase.Claimant.Add(new Participant()
                            {
                                Name = sidesReader.IsDBNull(0) ? null : sidesReader.GetString(0),
                                Inn = sidesReader.IsDBNull(1) ? null : sidesReader.GetString(1),
                                Ogrn = sidesReader.IsDBNull(2) ? null : sidesReader.GetString(2),
                                Address = sidesReader.IsDBNull(4) ? null : sidesReader.GetString(4)
                            });
                            break;
                        //третья сторона
                        case (int) Participants.ThirdParty:
                            if (rawCase.ThirdParty == null) rawCase.ThirdParty = new List<Participant>();
                            rawCase.ThirdParty.Add(new Participant()
                            {
                                Name = sidesReader.IsDBNull(0) ? null : sidesReader.GetString(0),
                                Inn = sidesReader.IsDBNull(1) ? null : sidesReader.GetString(1),
                                Ogrn = sidesReader.IsDBNull(2) ? null : sidesReader.GetString(2),
                                Address = sidesReader.IsDBNull(4) ? null : sidesReader.GetString(4)
                            });
                            break;
                        default:
                            break;
                    }

                }

                //Console.WriteLine("3 Прошло: " + s2.ElapsedMilliseconds + " мс.");

                sidesReader.Close();

                //Console.WriteLine("4 Прошло: " + s2.ElapsedMilliseconds + " мс.");

                //получаем документу по делу в первой инстанции
                var documentReader =
                    connector.ExecuteQuery(
                        "select Documents.VersionId, Documents.TypeId, Documents.ContentTypeId from Documents where (Documents.CaseRID =" +
                        rawCase.RID + ")");
                if (documentReader.HasRows)
                    rawCase.Documents = new List<Document>();

                //Console.WriteLine("5 Прошло: " + s2.ElapsedMilliseconds + " мс.");

                while (documentReader.Read())
                {
                    try
                    {
                        if (!documentReader.IsDBNull(1))
                        {
                            var doc = new Document(documentTypes[(int) documentReader.GetInt32(1)]);

                            //Console.WriteLine("6 Прошло: " + s2.ElapsedMilliseconds + " мс.");

                            //получаем тип содержимого документа
                            if (!documentReader.IsDBNull(2))
                            {
                                var contId = (int) documentReader.GetInt32(2);
                                if (documentContentTypes.ContainsKey(contId))
                                {
                                    doc.DocContent = documentContentTypes[contId];
                                    if (doc.DocContent.Contains("удовлетворить ходатайство") || doc.DocContent.Contains("Удовлетворить ходатайство"))
                                    {
                                        rawCase.Petition = true;
                                    }

                                    if (doc.DocContent.Contains("встречный иск"))
                                    {
                                        rawCase.Counterclaim = true;
                                    }
                                }
                                else doc.DocContent = string.Empty;
                            }

                            //Console.WriteLine("7 Прошло: " + s2.ElapsedMilliseconds + " мс.");

                            //получаем название файла
                            if (!documentReader.IsDBNull(0))
                            {
                                if (rawCase.SolutionFilename == null) rawCase.SolutionFilename = new List<string>();
                                rawCase.SolutionFilename.Add(documentReader.GetString(0));

                                doc.Filename = documentReader.GetString(0);
                            }
                            //Console.WriteLine("8 Прошло: " + s2.ElapsedMilliseconds + " мс.");
                            rawCase.Documents.Add(doc);

                            //Console.WriteLine("9 Прошло: " + s2.ElapsedMilliseconds + " мс.");
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                documentReader.Close();

                //Console.WriteLine("10 Прошло: " + s2.ElapsedMilliseconds + " мс.");

                var finalInstanceReader =
                    connector.ExecuteQuery(
                        "select FinalInstance from [Финальная инстанция] where [Финальная инстанция].CaseRID =" +
                        rawCase.RID);
                while (finalInstanceReader.Read())
                {
                    //Console.WriteLine("11 Прошло: " + s2.ElapsedMilliseconds + " мс.");
                    rawCase.FinalInstanceId = finalInstanceReader.IsDBNull(0) ? -1 : finalInstanceReader.GetInt32(0);
                }

                finalInstanceReader.Close();
                //Console.WriteLine("12 Прошло: " + s2.ElapsedMilliseconds + " мс.");

                var reconsiderationReader =
                    connector.ExecuteQuery(
                        "select count(RegistrationDate) from Instances where InstanceLevelId = 1 and CaseRID =" +
                        rawCase.RID);
                while (reconsiderationReader.Read())
                {
                    //Console.WriteLine("13 Прошло: " + s2.ElapsedMilliseconds + " мс.");
                    if (!reconsiderationReader.IsDBNull(0))
                    {
                        rawCase.Reconsideration = reconsiderationReader.GetInt32(0) > 1 ? true : false;
                    }
                }

                reconsiderationReader.Close();
                //Console.WriteLine("14 Прошло: " + s2.ElapsedMilliseconds + " мс.");
                //Console.WriteLine("15 Прошло: " + s2.ElapsedMilliseconds + " мс.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //finally
            //{
            //    connector.CloseConnection();
            //}

            this.processed++;

            //lock (locker)
            //{
            //    rawCases.Add(rawCase);
            //    this.currentTasksNum--;
            //    this.processed++;
            //}
        }
    }
}
