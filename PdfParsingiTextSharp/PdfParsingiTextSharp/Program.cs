using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.IO;

namespace PdfParsingiTextSharp
{
    /// <summary>
    /// Консольное приложение для перевода всех решений в txt
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //получаем все файлы с решениями
            var files = Directory.GetFiles(@"F:\Work\Action\AugustUpdate\Pravoru_Export\Documents\Partial");
            var i = 0;

            foreach (var f in files)
            {
                try
                {
                    //получаем текст решения
                    var text = ExtractTextFromPdf(f);
                    //открываем файл, куда запишем текст решения
                    var file = @"F:\Work\Action\AugustUpdate\Pravoru_Export\Documents\Partial\TXT\" + Path.GetFileName(f) + ".txt";

                    var fileStream = new System.IO.StreamWriter(file);
                    fileStream.Write(text);

                    fileStream.Close();

                    i++;
                    Console.WriteLine("Сконвертировано " + i + " из " + files.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

        /// <summary>
        /// Получаем текст решения суда
        /// </summary>
        /// <param name="path">Путь к файлу с решением суда</param>
        /// <returns></returns>
        public static string ExtractTextFromPdf(string path)
        {
            using (PdfReader reader = new PdfReader(path))
            {
                StringBuilder text = new StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                }

                return text.ToString();
            }
        }
    }
}
