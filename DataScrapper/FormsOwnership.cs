using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataScrapper
{
    /// <summary>
    /// Класс для определение формы собственности
    /// </summary>
    public class FormsOwnershipProcessor
    {
        /// <summary>
        /// Метод для определения формы собственности
        /// </summary>
        /// <param name="storagePath">БД, содеращая метаданные по делам</param>
        /// <param name="resultsPath">БД для сохранения результата</param>
        public static void GetFormsOwnership(string storagePath, string resultsPath)
        {
            var connector = new KadConnector(resultsPath);

            var connDocs = new KadConnector(storagePath);

            var i = 0;

            try
            {
                //получаем контрагенты по делам
                var sideReader = connDocs.ExecuteQuery("select Name, Inn, OGRN, SideTypeId, caseRID from sides where (SideTypeId = 1 or SideTypeId = 2)");

                while (sideReader.Read())
                {
                    var sw = Stopwatch.StartNew();

                    var caseRID = sideReader.IsDBNull(4) ? 0 : (int)sideReader.GetInt32(4);

                    int claimant_ownership = -1;
                    int respondent_ownership = -1;

                    if (caseRID != 0)
                    {
                        if (sideReader.IsDBNull(3)) break;

                        var sideType = (int)sideReader.GetInt32(3);
                        switch (sideType)
                        {
                            //ответчик
                            case (int)Participants.Respondent:
                                {
                                    var Name = sideReader.IsDBNull(0) ? null : sideReader.GetString(0);
                                    var Inn = sideReader.IsDBNull(1) ? null : sideReader.GetString(1);
                                    var Ogrn = sideReader.IsDBNull(2) ? null : sideReader.GetString(2);

                                    //получаем форму собственности
                                    claimant_ownership = GetOwnership(Name, Inn, Ogrn);
                                }
                                break;
                            //истец
                            case (int)Participants.Claimant:
                                {
                                    var Name = sideReader.IsDBNull(0) ? null : sideReader.GetString(0);
                                    var Inn = sideReader.IsDBNull(1) ? null : sideReader.GetString(1);
                                    var Ogrn = sideReader.IsDBNull(2) ? null : sideReader.GetString(2);

                                    //получаем форму собственности
                                    respondent_ownership = GetOwnership(Name, Inn, Ogrn);
                                }
                                break;
                            default:
                                break;
                        }

                    }

                    //записываем форму собственности в БД, где сохраняется результат
                    connector.ExecuteQuery("update data set claimantformownership = " + claimant_ownership + " , " + "respondentformownership = " + respondent_ownership + " where rid = " + caseRID);

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

        /// <summary>
        /// Применение фаблона
        /// </summary>
        /// <param name="pattern">Шаблон</param>
        /// <param name="text">Текст, к которому применяется шаблон</param>
        /// <returns></returns>
        private static bool ApplyPattern(string pattern, string text)
        {
            Regex rx = new Regex(pattern,
             RegexOptions.IgnoreCase | RegexOptions.Singleline);

            MatchCollection matches = rx.Matches(text, 0);

            if (matches.Count == 0) return false;
            else return true;
        }

        /// <summary>
        /// Получение формы собственности
        /// </summary>
        /// <param name="name">Имя контрагента</param>
        /// <param name="inn">ИНН</param>
        /// <param name="ogrn">ОГРН</param>
        /// <returns></returns>
        private static int GetOwnership(string name, string inn, string ogrn)
        {
            var pt_OOO = @"ООО";
            var pt_ZAO = @"ЗАО";
            var pt_gov = @"Федеральная налоговая служба|ИФНС|МИФНС|ФНС|Министерство|Инспекция|государств|федеральн";
            var pt_eng = @"[a-zA-Z]+";
            var pt_ip = @"ИП";

            if (ApplyPattern(pt_OOO, name)) return (int)FormsOwnership.OOO;
            if (ApplyPattern(pt_ZAO, name)) return (int)FormsOwnership.ZAO;
            if (ApplyPattern(pt_gov, name)) return (int)FormsOwnership.Gov;

            if ((!string.IsNullOrEmpty(inn) && inn.Trim().Length == 10) || ApplyPattern(pt_eng, name) || !string.IsNullOrEmpty(ogrn)) return (int)FormsOwnership.OtherComp;

            if (!string.IsNullOrEmpty(inn) && inn.Trim().Length == 12)
            {
                if (ApplyPattern(pt_ip, name)) return (int)FormsOwnership.IndividualEntrepreneur;
                else return (int)FormsOwnership.Person;
            }

            if (name.Split(' ').Count() == 3) return (int)FormsOwnership.Person;

            return (int)FormsOwnership.Unknown;
        }
    }
}
