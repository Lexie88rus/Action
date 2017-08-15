using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataScrapper;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            SelectPartial1();
            //GetJudges();
            //GetPresence();
            //GetFormsOwnership();
        }

        private static void SelectPartial2()
        {
            var connector = new KadConnector(@"D:\Data Monsters\Action\AugustUpdate\Partial_results.accdb");
            

            try
            {              
                var i = 0;

                var resReader = connector.ExecuteQuery("select RID from Data");

                int caseRID;

                while (resReader.Read())
                {
                    var connDocs = new KadConnector(@"D:\Data Monsters\Action\AugustUpdate\Partial.accdb");

                    var sw = Stopwatch.StartNew();

                    caseRID = resReader.IsDBNull(0) ? 0 : (int) resReader.GetDouble(0);

                    var docReader =
                        connDocs.ExecuteQuery(
                            "select DecisionType from Documents where FinishInstance = \"True\" and caseRID =" + caseRID);

                    var partial = true;

                    while (docReader.Read())
                    {
                        var decisionType = docReader.IsDBNull(0) ? string.Empty : docReader.GetString(0);

                        if (decisionType.Contains("полностью")) partial = false;
                    }

                    if (!partial)
                    {
                        connector.ExecuteQuery("update Data set Decision = 2 where RID = " + caseRID.ToString());
                    }

                    connDocs.CloseConnection();

                    i++;
                    Console.WriteLine("Обработано " + i + " за " + sw.ElapsedMilliseconds + " мс");
                }

            }
            finally
            {
                connector.CloseConnection();       
            }
        }

        public static string s_pattern1 = @"р\s*е\s*ш\s*и\s*л\s*:\s*.*(\d\.)*\s*Взыскать(?<amount>.*)$";
        public static string s_pattern2 = @"р\s*е\s*ш\s*и\s*л\s*:\s*.*(\d\.)*\s*Взыскать(?<amount>.*)всего.*$";
        public static string s_pattern3 = @"р\s*е\s*ш\s*и\s*л\s*:\s*.*(\d\.)*\s*Взыскать(?<amount>.*)(в\s*том\s*числе|включая).*$";
        public static string s_pattern4 = @"р\s*е\s*ш\s*и\s*л\s*:\s*.*(\d\.)*\s*Взыскать(?<amount>.*)(в\s*том\s*числе|включая).*(а\s*так\s*же)(?<amount>.*)$";

        public static string n_pattern1 = @"(?<rub>\d+\s*\d*[\.|\,]*\s*\d*\D*руб\.*)";
        public static string n_pattern2 = @"(?<amount>(?<rub>\d+\s*\d*\D*(рубля|рублей|рубль))\s*\d+\D*(копеек|копейка|копейки))";

        private static void SelectPartial1()
        {
            var connector = new KadConnector(@"D:\Data Monsters\Action\AugustUpdate\Partial_results.accdb");
            var connDocs = new KadConnector(@"D:\Data Monsters\Action\AugustUpdate\Partial.accdb");

            try
            {
                var files = Directory.GetFiles(@"D:\Data Monsters\Action\AugustUpdate\TXT\TXT").OrderBy(f=>f);
                var i = 0;

                foreach (var file in files)
                {
                    var sw = Stopwatch.StartNew();

                    if (i >= 24966)
                    {

                        var docRID = file.Split('.')[0].Split('\\').LastOrDefault();
                        var text = SolutionProcessor.GetSolutionText(file);
                        int amount;

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

                        amount = SolutionProcessor.GetAmount(am_str, n_pattern1);
                        if (amount == 0)
                        {
                            SolutionProcessor.GetAmount(am_str, n_pattern2);
                        }

                        if (amount == 0) continue;

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

                        //var reader = connector.ExecuteQuery("select Amount from Data where RID = " + caseRID);

                        //var caseAmount = 0;

                        //while (reader.Read())
                        //{

                        //    caseAmount = reader.IsDBNull(0) ? 0 : (int)reader.GetDouble(0);
                        //}

                        connector.ExecuteQuery("update Data set solutionamount = " + amount + " where RID = " + caseRID.ToString());

                        //if (amount >= caseAmount * 0.8)
                        //{
                        //    connector.ExecuteQuery("update Data set Decision = 2, solutionamount = " + amount + " where RID = " + caseRID.ToString());
                        //}
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

        private static void GetJudges()
        {

            var connector = new KadConnector(@"D:\Data Monsters\Action\AugustUpdate\Partial_results.accdb");

            var connDocs = new KadConnector(@"D:\Data Monsters\Action\AugustUpdate\Partial.accdb");

            try
            {
                var files = Directory.GetFiles(@"D:\Data Monsters\Action\AugustUpdate\TXT\TXT").OrderBy(f => f);

                var i = 0;

                foreach (var file in files)
                {
                    var sw = Stopwatch.StartNew();

                    if (i >= 67615)
                    {

                        var docRID = file.Split('.')[0].Split('\\').LastOrDefault();
                        var text = SolutionProcessor.GetSolutionText(file);

                        var readerDoc = connDocs.ExecuteQuery("select top 1 CaseRID from Documents where RID = " + docRID);
                        int caseRID = 0;
                        int courtId = 0;

                        try
                        {
                            while (readerDoc.Read())
                            {
                                caseRID = readerDoc.IsDBNull(0) ? 30 : (int)readerDoc.GetInt32(0);
                            }

                        }
                        catch (Exception e)
                        {
                            string s = e.ToString();
                            s = s + "";
                            continue;
                        }

                        var reader = connector.ExecuteQuery("select Court from Data where RID = " + caseRID);

                        while (reader.Read())
                        {

                            courtId = reader.IsDBNull(0) ? 0 : (int)reader.GetDouble(0);
                        }

                        var judgeName = SolutionProcessor.GetJudgeName(text);
                        var judgeId = SolutionProcessor.GetJudgeId(connDocs, judgeName, courtId);
                        connector.ExecuteQuery("update Data set Judge = " + judgeId + " where RID = " + caseRID.ToString());
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

        private static void GetPresence()
        {
            var connector = new KadConnector(@"D:\Data Monsters\Action\AugustUpdate\Partial_results.accdb");

            var connDocs = new KadConnector(@"D:\Data Monsters\Action\AugustUpdate\Partial.accdb");

            try
            {
                var files = Directory.GetFiles(@"D:\Data Monsters\Action\AugustUpdate\TXT\TXT").OrderBy(f => f);

                var i = 0;

                foreach (var file in files)
                {
                    var sw = Stopwatch.StartNew();

                    if (i >= 0)
                    {

                        var docRID = file.Split('.')[0].Split('\\').LastOrDefault();
                        var text = SolutionProcessor.GetSolutionText(file);

                        var readerDoc = connDocs.ExecuteQuery("select top 1 CaseRID from Documents where RID = " + docRID);
                        int caseRID = 0;
                        int courtId = 0;

                        try
                        {
                            while (readerDoc.Read())
                            {
                                caseRID = readerDoc.IsDBNull(0) ? 30 : (int)readerDoc.GetInt32(0);
                            }

                        }
                        catch (Exception e)
                        {
                            string s = e.ToString();
                            s = s + "";
                            continue;
                        }

                        int claimantPres = SolutionProcessor.GetClaimantPresence(text);
                        int respondentPres = SolutionProcessor.GetRespondentPres(text);

                        connector.ExecuteQuery("update Data set ClaimantPresence = " + claimantPres + ", RespondentPresence = " + respondentPres +" where RID = " + caseRID.ToString());
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

        private static void GetFormsOwnership()
        {
            var connector = new KadConnector(@"D:\Data Monsters\Action\AugustUpdate\Partial_results.accdb");

            var connDocs = new KadConnector(@"D:\Data Monsters\Action\AugustUpdate\Partial.accdb");

            var i = 0;

            try
            {

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

                                    claimant_ownership = GetOwnership(Name, Inn, Ogrn);
                                }
                                break;
                            //истец
                            case (int)Participants.Claimant:
                                {
                                    var Name = sideReader.IsDBNull(0) ? null : sideReader.GetString(0);
                                    var Inn = sideReader.IsDBNull(1) ? null : sideReader.GetString(1);
                                    var Ogrn = sideReader.IsDBNull(2) ? null : sideReader.GetString(2);

                                    respondent_ownership = GetOwnership(Name, Inn, Ogrn);
                                }
                                break;
                            default:
                                break;
                        }

                    }

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

        private static bool ApplyPattern(string pattern, string text)
        {
            Regex rx = new Regex(pattern,
             RegexOptions.IgnoreCase | RegexOptions.Singleline);

            MatchCollection matches = rx.Matches(text, 0);

            if (matches.Count == 0) return false;
            else return true;
        }

        private static int GetOwnership(string name, string inn, string ogrn)
        {
            var pt_OAO = @"ООО";
            var pt_ZAO = @"ЗАО";
            var pt_gov = @"Федеральная налоговая служба|ИФНС|МИФНС|ФНС|Министерство|Инспекция|государств|федеральн";
            var pt_eng = @"[a-zA-Z]+";
            var pt_ip = @"ИП";

            if (ApplyPattern(pt_OAO, name)) return (int)FormsOwnership.OAO;
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
