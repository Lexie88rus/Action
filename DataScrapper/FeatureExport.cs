using CaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrapper
{
    /// <summary>
    /// Класс для экспорта выявленных фич в файл с результатом
    /// </summary>
    public class FeatureExport
    {
        /// <summary>
        /// Экспортировать фичи по делу
        /// </summary>
        /// <param name="stream">Поток к файлу с результатом</param>
        /// <param name="c">Дело</param>
        /// <param name="delimeter">Разделитель для csv файла</param>
        private static void ExportCase(System.IO.StreamWriter stream, ICase c, string delimeter)
        {
            var caseString = string.Empty;
            foreach (var f in c.Features)
            {
                if (f.ToBeExported)
                {
                    foreach (var v in f.Value)
                    {
                        caseString += v + delimeter;
                    }
                }
            }
            stream.WriteLine(caseString);
        }

        /// <summary>
        /// Экспортировать в csv
        /// </summary>
        /// <param name="cases">Набор дел</param>
        /// <param name="filename">Название файла, куда экспортировать</param>
        /// <param name="delimeter">Разделитель для csv</param>
        public static void ExportToCSV(IEnumerable<ICase> cases, string filename, string delimeter)
        {
            var fileStream = new System.IO.StreamWriter(filename);
            var heading = string.Empty;
            var c1 = cases.FirstOrDefault();
            if (c1 != null)
            {
                foreach (var f in c1.Features)
                {
                    if (f.ToBeExported)
                    {
                        foreach (var n in f.Name)
                        {
                            heading += n + delimeter;
                        }
                    }
                }
                fileStream.WriteLine(heading);

                foreach (var c in cases)
                {
                    ExportCase(fileStream, c, delimeter);
                }
            }

            fileStream.Close();
        }
    }
}
