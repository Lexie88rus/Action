using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrapper
{
    public class Document
    {
        public string Type { get; set; }

        public string DocContent { get; set; }

        public string Filename { get; set; }

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
