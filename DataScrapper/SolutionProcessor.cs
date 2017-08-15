using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataScrapper
{
    public class SolutionProcessor
    {
        /// <summary>
        /// Получение текста решения из файла
        /// </summary>
        /// <param name="path">Путь к файлу с решением</param>
        /// <returns>Текст файла решения суда</returns>
        public static string GetSolutionText(string path)
        {
            var filestream = new StreamReader(path);

            return filestream.ReadToEnd();
        }

        /// <summary>
        /// Получить имя судьи
        /// </summary>
        /// <param name="solutionText">Текст решения суда</param>
        /// <returns>Фамилию И.О. судьи</returns>
        public static string GetJudgeName(string solutionText)
        {
            Regex rx = new Regex(@"\b(Судья)\b(\s*.)*\s*",
              RegexOptions.Compiled | RegexOptions.IgnoreCase);

            MatchCollection matches = rx.Matches(solutionText);
            if (matches.Count > 0)
            {
                var judgestring = matches[0].Value.Trim();

                Regex rxw = new Regex(@"\b(?<word>\w+)",
              RegexOptions.Compiled | RegexOptions.IgnoreCase);

                var m1 = rxw.Matches(judgestring);

                if (m1.Count > 0)
                {
                    if (m1.Count >= 4)
                    {
                        return m1[m1.Count - 1].Value + " " + m1[m1.Count - 3].Value + "." + m1[m1.Count - 2].Value + ".";
                    }
                    return m1[m1.Count - 1].Value;
                }
                else return string.Empty;
            }
            else return string.Empty;
        }

        /// <summary>
        /// Возвращает идентификатор судьи
        /// </summary>
        /// <param name="connector"></param>
        /// <returns></returns>
        public static int GetJudgeId(KadConnector connector, string judgeName, int courtId = 0)
        {
            if (connector == null) throw new ArgumentNullException("connector");

            try
            {
                var reader = courtId > 0 ? connector.ExecuteQuery("select Id from Judges where CourtId = " + courtId + " and Name = \"" + judgeName + "\"") : connector.ExecuteQuery("select Id from Judges where Name = \"" + judgeName + "\"");

                reader.Read();

                if (reader.IsDBNull(0) || !reader.HasRows) return -1;

                var val = reader.GetValue(0);
                var judgeId = reader.GetInt16(0);

                return judgeId;
            }
            catch(Exception)
            {
                return -1;
            }
        }

        public static string ApplyPattern(string text, string pattern)
        {
            Regex rx = new Regex(pattern,
             RegexOptions.IgnoreCase | RegexOptions.Singleline);

            MatchCollection matches = rx.Matches(text, 0);

            string amount = string.Empty;

            if (matches.Count == 0) return amount;

            foreach (Match match in matches)
            {
                amount += match.Groups["amount"];
            }

            return amount;
        }

        public static int GetAmount(string text, string pattern)
        {
            Regex rx = new Regex(pattern,
             RegexOptions.IgnoreCase | RegexOptions.Singleline);

            MatchCollection matches = rx.Matches(text, 0);

            int rub = 0;

            if (matches.Count == 0) return rub;

            foreach (Match match in matches)
            {
                char[] decsym = { '.', ',' };
                var str = match.Groups["rub"].Value.Replace(" ", "").Split(decsym).FirstOrDefault();   

                var pt = new Regex(@"\d+",
             RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var m = pt.Matches(str);

                foreach(Match mtch in m)
                {
                    double r;
                    if (double.TryParse(mtch.Value, out r))
                    {
                        rub += (int)r;
                    }
                }
            }

            return rub;
        }

        public static int GetClaimantPresence(string text)
        {
            var pattern = @"от\s*истц(а|ов)\s*\W*\s*представител(ь|и)\s*не\s*явил(ись|ся)";

            Regex rx = new Regex(pattern,
             RegexOptions.IgnoreCase | RegexOptions.Singleline);

            MatchCollection matches = rx.Matches(text, 0);

            if (matches.Count == 0) return 0;
            else return 1;
        }

        public static int GetRespondentPres(string text)
        {
            var pattern = @"от\s*ответчик(а|ов)\s*\W*\s*представител(ь|и)\s*не\s*явил(ись|ся)";

            Regex rx = new Regex(pattern,
             RegexOptions.IgnoreCase | RegexOptions.Singleline);

            MatchCollection matches = rx.Matches(text, 0);

            if (matches.Count == 0) return 0;
            else return 1;
        }
    }
}
