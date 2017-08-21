using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrapper
{
    /// <summary>
    /// Класс для извлчения документов по делу из метаданных
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Тип документа
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Тип содержимого документа
        /// </summary>
        public string DocContent { get; set; }

        /// <summary>
        /// Название файла с документом (VersionId из таблицы Documents)
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="docType">Тип документа</param>
        /// <param name="docContent">Тип содержимого документа</param>
        /// <param name="filename">Название файла (VersionId из таблицы Documents)</param>
        public Document(string docType, string docContent = "", string filename = "")
        {
            if (string.IsNullOrEmpty(docType))
                throw new ArgumentNullException("docType");

            this.Type = docType;
            this.DocContent = docContent;
            this.Filename = filename;
        }
    }
}
