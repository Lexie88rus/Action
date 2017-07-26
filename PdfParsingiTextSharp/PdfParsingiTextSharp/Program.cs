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
            var files = Directory.GetFiles(@"D:\Data Monsters\Action\PravoRu_Export\PravoRu_Export");
            foreach (var f in files)
            {
                var text = ExtractTextFromPdf(f);
                var file = f + ".txt";

                var fileStream = new System.IO.StreamWriter(file);
                fileStream.Write(text);

                fileStream.Close();
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
