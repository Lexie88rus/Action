using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.IO;

namespace PdfParsingiTextSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = Directory.GetFiles(@"F:\Work\Action\AugustUpdate\Pravoru_Export\Documents\Partial");
            var i = 0;

            foreach (var f in files)
            {
                try
                {
                    var text = ExtractTextFromPdf(f);
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
