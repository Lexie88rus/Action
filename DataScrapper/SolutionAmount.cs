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
    /// Класс для извлечения выплаченной суммы из решения суда
    /// </summary>
    public static class SolutionAmount
    {
        /// <summary>
        /// Паттерны для выделения выплаченной суммы из решения
        /// </summary>
        #region Patterns

        //Паттерны для разбора предложения, содержащего выплаченную сумму
        public static string s_pattern1 = @"р\s*е\s*ш\s*и\s*л\s*:\s*.*(\d\.)*\s*Взыскать(?<amount>.*)$";
        public static string s_pattern2 = @"р\s*е\s*ш\s*и\s*л\s*:\s*.*(\d\.)*\s*Взыскать(?<amount>.*)всего.*$";
        public static string s_pattern3 = @"р\s*е\s*ш\s*и\s*л\s*:\s*.*(\d\.)*\s*Взыскать(?<amount>.*)(в\s*том\s*числе|включая).*$";
        public static string s_pattern4 = @"р\s*е\s*ш\s*и\s*л\s*:\s*.*(\d\.)*\s*Взыскать(?<amount>.*)(в\s*том\s*числе|включая).*(а\s*так\s*же)(?<amount>.*)$";

        //паттеры для разбора суммы в рублях
        public static string n_pattern1 = @"(?<rub>\d+\s*\d*[\.|\,]*\s*\d*\D*руб\.*)";
        public static string n_pattern2 = @"(?<amount>(?<rub>\d+\s*\d*\D*(рубля|рублей|рубль))\s*\d+\D*(копеек|копейка|копейки))";

        #endregion

/// <summary>
/// Метод для выделения выплаченной суммы
/// </summary>
/// <param name="storagePath">БД, содеращая метаданные по делам</param>
/// <param name="resultsPath">БД для сохранения результата</param>
/// <param name="filesPath">Путь к директории, содержащей файлы</param>
        public static void GetSolutionAmount(string storagePath, string resultsPath, string filesPath)
        {
            var connector = new KadConnector(resultsPath);
            var connDocs = new KadConnector(storagePath);

            try
            {
                //Получаем все решения из директории (решения должны быть предварительно переконвертированы в txt)
                var files = Directory.GetFiles(filesPath).OrderBy(f => f);
                var i = 0;

                foreach (var file in files)
                {
                    var sw = Stopwatch.StartNew();

                    if (i >= 0)
                    {

                        //Получаем RID документа из названия файла с решенияем
                        var docRID = file.Split('.')[0].Split('\\').LastOrDefault();
                        //Получаем текст решения
                        var text = SolutionProcessor.GetSolutionText(file);
                        int amount;

                        //Применяем паттерны для излечения фразы, содержащей сумму в рублях
                        var am_str = SolutionProcessor.ApplyPattern(text, s_pattern4);
                        if (string.IsNullOrEmpty(am_str))
                        {
                            am_str = SolutionProcessor.ApplyPattern(text, s_pattern3);
                            if (string.IsNullOrEmpty(am_str))
                            {
                                am_str = SolutionProcessor.ApplyPattern(text, s_pattern2);
                                if (string.IsNullOrEmpty(am_str))
                                {
                                    am_str = SolutionProcessor.ApplyPattern(text, s_pattern1);
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(am_str)) continue;

                        //Получаем сумму в рублях
                        amount = SolutionProcessor.GetAmount(am_str, n_pattern1);
                        if (amount == 0)
                        {
                            SolutionProcessor.GetAmount(am_str, n_pattern2);
                        }

                        if (amount == 0) continue;

                        //Получаем идентификатор дела, к которому относится решение
                        var readerDoc = connDocs.ExecuteQuery("select top 1 CaseRID from Documents where RID = " + docRID);
                        int caseRID = 0;

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

                        if (caseRID == 0) continue;

                        //Записываем результат
                        connector.ExecuteQuery("update Data set solutionamount = " + amount + " where RID = " + caseRID.ToString());
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
