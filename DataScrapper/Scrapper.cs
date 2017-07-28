using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrapper
{
    public class Scrapper
    {
        public Scrapper(KadConnector connector, string connString)
        {
            this.connector = connector ?? throw new ArgumentNullException("connector");
            this.connString = String.IsNullOrEmpty(connString) ? throw new ArgumentNullException("connString") :  connString;
        }

        private KadConnector connector;

        private string connString;

        private const int tasksNum = 10;

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

        Task[] taskList = new Task[Scrapper.tasksNum];

        static readonly object locker = new object();

        /// <summary>
        /// Получение сырых данных о делах
        /// </summary>
        /// <param name="connector">Курсор для чтения данных</param>
        private void GetRawCasesData(KadConnector connector, string connString)
        {
            if (connector == null) throw new ArgumentNullException("connector");

            this.GetDocumentContentTypes(connector);
            this.GetDocumentTypes(connector);

            //Получаем основные данные о делах
            var reader = connector.ExecuteQuery("select RID, CourtId, Date, Sum, CategoryDisputeId, CaseId from Cases");

            int i = 0;
            while (reader.Read())
            {

                try
                {
                    //получаем основные данные по делу
                    var rawCase = new CaseDb()
                    {
                        RID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        CourtId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                        Date = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2),
                        Amount = reader.IsDBNull(3) ? 0 : (int)reader.GetDouble(3),
                        CategoryId = reader.IsDBNull(4) ? 0 : (int)reader.GetInt32(4),
                        CaseId = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    };

                    GetData(this.connString, rawCase);

                    i++;

                    if (i % 50 == 0)
                    {
                        Console.WriteLine("Собрано данных по делам:" + i);
                        var cases = this.rawCases.Select(c => c.ConvertToCase((int)Decisions.Partial, this.DocumentTypes)).ToList();
                        FeatureExport.ExportToCSV(cases, "D:\\Data Monsters\\Action\\dataset" + i.ToString() +".csv", ";");
                    }
                }
                catch
                {
                    continue;
                }
            }

            reader.Close();
        }

        private void GetData(string connString, CaseDb rawCase)
        {
            var connector = new KadConnector(connString);

            try
            {

                //получаем всех участников процесса
                var sidesReader = connector.ExecuteQuery("select Name, Inn, Ogrn, SideTypeId, Address from Sides where CaseId = " + rawCase.CaseId);
                while (sidesReader.Read())
                {
                    if (sidesReader.IsDBNull(3)) break;

                    var sideType = (int)sidesReader.GetInt32(3);
                    switch (sideType)
                    {
                        //ответчик
                        case (int)Participants.Respondent:
                            if (rawCase.Responent == null) rawCase.Responent = new List<Participant>();
                            rawCase.Responent.Add(new Participant() { Name = sidesReader.IsDBNull(0) ? null : sidesReader.GetString(0), Inn = sidesReader.IsDBNull(1) ? null : sidesReader.GetString(1), Ogrn = sidesReader.IsDBNull(2) ? null : sidesReader.GetString(2), Address = sidesReader.IsDBNull(4) ? null : sidesReader.GetString(4) });
                            break;
                        //истец
                        case (int)Participants.Claimant:
                            if (rawCase.Claimant == null) rawCase.Claimant = new List<Participant>();
                            rawCase.Claimant.Add(new Participant() { Name = sidesReader.IsDBNull(0) ? null : sidesReader.GetString(0), Inn = sidesReader.IsDBNull(1) ? null : sidesReader.GetString(1), Ogrn = sidesReader.IsDBNull(2) ? null : sidesReader.GetString(2), Address = sidesReader.IsDBNull(4) ? null : sidesReader.GetString(4) });
                            break;
                        //третья сторона
                        case (int)Participants.ThirdParty:
                            if (rawCase.ThirdParty == null) rawCase.ThirdParty = new List<Participant>();
                            rawCase.ThirdParty.Add(new Participant() { Name = sidesReader.IsDBNull(0) ? null : sidesReader.GetString(0), Inn = sidesReader.IsDBNull(1) ? null : sidesReader.GetString(1), Ogrn = sidesReader.IsDBNull(2) ? null : sidesReader.GetString(2), Address = sidesReader.IsDBNull(4) ? null : sidesReader.GetString(4) });
                            break;
                        default:
                            break;
                    }

                }

                sidesReader.Close();

                //получаем документу по делу в первой инстанции
                var documentReader = connector.ExecuteQuery("select Documents.VersionId, Documents.TypeId, Documents.ContentTypeId from Documents where (Documents.CaseId =" + rawCase.CaseId + ")");
                if (documentReader.HasRows)
                    rawCase.Documents = new List<Document>();

                while (documentReader.Read())
                {
                    try
                    {
                        if (!documentReader.IsDBNull(1))
                        {
                            var doc = new Document(documentTypes[(int)documentReader.GetInt32(1)]);

                            //получаем тип содержимого документа
                            if (!documentReader.IsDBNull(2))
                            {
                                var contId = (int)documentReader.GetInt32(2);
                                if (documentContentTypes.ContainsKey(contId))
                                {
                                    doc.DocContent = documentContentTypes[contId];
                                    if (doc.DocContent.Contains("удовлетворить ходатайство"))
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

                            //получаем название файла
                            if (!documentReader.IsDBNull(0))
                            {
                                if (rawCase.SolutionFilename == null) rawCase.SolutionFilename = new List<string>();
                                rawCase.SolutionFilename.Add(documentReader.GetString(0));

                                doc.Filename = documentReader.GetString(0);
                            }
                            rawCase.Documents.Add(doc);
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                documentReader.Close();

                var finalInstanceReader = connector.ExecuteQuery("select FinalInstance from [Финальная инстанция1] where [Финальная инстанция1].CaseId =" + rawCase.CaseId);
                while (finalInstanceReader.Read())
                {
                    rawCase.FinalInstanceId = finalInstanceReader.IsDBNull(0) ? -1 : finalInstanceReader.GetInt32(0);
                }

                finalInstanceReader.Close();

                var reconsiderationReader = connector.ExecuteQuery("select count(RegistrationDate) from Instances where InstanceLevelId = 1 and CaseId =" + rawCase.CaseId);
                while (reconsiderationReader.Read())
                {
                    if (!reconsiderationReader.IsDBNull(0))
                    {
                        rawCase.Reconsideration = reconsiderationReader.GetInt32(0) > 1 ? true : false;
                    }
                }

                reconsiderationReader.Close();
            }
            finally
            {
                connector.CloseConnection();
            }

            rawCases.Add(rawCase);
        }
    }
}
