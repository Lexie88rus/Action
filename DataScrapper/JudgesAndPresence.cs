using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrapper
{
    /// <summary>
    /// Класс для обработки решения суда: получения данных по судье и явке истца/ответчика
    /// </summary>
    public class JudgesAndPresence
    {
        /// <summary>
        /// Метод для получения идентификатора судьи, а так же явки/неявки истца/ответчика
        /// Каталог судей прикреплен к решению см. файл Judges.xslx в папке Lookups
        /// </summary>
        /// <param name="storagePath">БД, содеращая метаданные по делам</param>
        /// <param name="resultsPath">БД для сохранения результата</param>
        /// <param name="filesPath">Путь к директории, содержащей файлы</param>
        public static void GetJudgesAndPresence(string storagePath, string resultsPath, string filesPath)
        {
            var connector = new KadConnector(resultsPath);

           try
            {
                //Получаем все файлы с решениями (предварительно они должны быть переконвертированы в txt)
                var files = Directory.GetFiles(filesPath).OrderBy(f => f);

                var i = 0;

                foreach (var file in files)
                {
                    var sw = Stopwatch.StartNew();

                    var connDocs = new KadConnector(storagePath);

                    if (i >= 0)
                    {
                        //получаем идентификатор документа из имени файла
                        var docRID = file.Split('.')[0].Split('\\').LastOrDefault();
                        
                        //получаем идентификатор дела по идентификатору документа
                        var readerDoc = connDocs.ExecuteQuery("select top 1 CaseRID from Documents where RID = " + docRID);
                        int caseRID = 0;
                        int courtId = 0;

                        try
                        {
                            while (readerDoc.Read())
                            {
                                caseRID = readerDoc.IsDBNull(0) ? 0 : (int)readerDoc.GetInt32(0);
                            }

                        }
                        catch (Exception e)
                        {
                            string s = e.ToString();
                            s = s + "";
                            continue;
                        }

                        if (caseRID != 0)
                        {

                            //получаем идентификатор суда
                            var reader = connector.ExecuteQuery("select Court from Data where RID = " + caseRID);

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {

                                    courtId = reader.IsDBNull(0) ? 0 : (int)reader.GetInt32(0);
                                }

                                //получаем текст решения суда
                                var text = SolutionProcessor.GetSolutionText(file);

                                //получаем имя судьи
                                var judgeName = SolutionProcessor.GetJudgeName(text);

                                //находим идентификатор судьи в каталоге
                                var judgeId = SolutionProcessor.GetJudgeId(connDocs, judgeName, courtId);

                                //получаем явку/неявку истца и ответчика
                                int claimantPres = SolutionProcessor.GetClaimantPresence(text);
                                int respondentPres = SolutionProcessor.GetRespondentPres(text);

                                //записываем полученные данные в хранилище результата
                                connector.ExecuteQuery("update Data set Judge = " + judgeId + " , ClaimantPresence = " + claimantPres + ", RespondentPresence = " + respondentPres + " where RID = " + caseRID.ToString());
                            }
                        }
                    }

                    i++;
                    Console.WriteLine("Обработано " + i + " за " + sw.ElapsedMilliseconds + " мс");

                    connDocs.CloseConnection();
                }
            }
            finally
            {

                connector.CloseConnection();

            }
        }
    }
}
