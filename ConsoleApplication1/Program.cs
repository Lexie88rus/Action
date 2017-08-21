using DataScrapper;

namespace ConsoleApplication1
{
    /// <summary>
    /// Консольное приложение для обработки файлов решений суда
    /// </summary>
    class Program
    {

        private static string metaPath = @"D:\Data Monsters\Action\AugustUpdate\Partial.accdb";
        private static string resultsPath = @"D:\Data Monsters\Action\AugustUpdate\Partial_results.accdb";
        private static string filesPath = @"D:\Data Monsters\Action\AugustUpdate\TXT\TXT";


        static void Main(string[] args)
        {
            //Получаем выплаченную сумму
            SolutionAmount.GetSolutionAmount(metaPath, resultsPath, filesPath);
            JudgesAndPresence.GetJudgesAndPresence(metaPath, resultsPath, filesPath);
            FormsOwnershipProcessor.GetFormsOwnership(metaPath, resultsPath);
        }


        
    }
}
