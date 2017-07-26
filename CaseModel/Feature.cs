using DataScrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseModel
{
    public class Feature : IFeature
    {
        public int[] Value { get;}

        public string[] Name { get;}

        public bool ToBeExported { get; set; }

        public Feature(int value, string name, bool toBeExported = true)
        {
            this.Value = new int[1];
            this.Value[0] = value;

            this.Name = new string[1];
            this.Name[0] = name;

            this.ToBeExported = toBeExported;
        }

        public Feature(bool value, string name, bool toBeExported = true)
        {
            this.Value = new int[1];
            this.Value[0] = value ? 1 : 0;

            this.Name = new string[1];
            this.Name[0] = name;

            this.ToBeExported = toBeExported;
        }

        public Feature(IEnumerable<int> value, IEnumerable<string> name, bool toBeExported = true)
        {
            this.Value = new int [value.Count()];
            for (var i = 0; i < value.Count(); i++) this.Value[i] = value.ElementAt(i);

            this.Name = new string[name.Count()];
            for (var i = 0; i < name.Count(); i++) this.Name[i] = name.ElementAt(i);

            this.ToBeExported = toBeExported;
        }

        public Feature(IEnumerable<bool> value, IEnumerable<string> name, bool toBeExported = true)
        {
            this.Value = new int[value.Count()];
            for (var i = 0; i < value.Count(); i++) this.Value[i] = value.ElementAt(i) ? 1 : 0;

            this.Name = new string[name.Count()];
            for (var i = 0; i < name.Count(); i++) this.Name[i] = name.ElementAt(i);

            this.ToBeExported = toBeExported;
        }
    }

    public class DocumentsFeature : IFeature
    {
        public int[] Value { get; }

        public string[] Name { get; }

        public bool ToBeExported { get; set; }

        public DocumentsFeature(IList<Document> documents, IDictionary<int, string> documentTypes, bool toBeExported = true)
        {
            if (documentTypes == null) throw new ArgumentNullException("documentTypes");

            this.ToBeExported = toBeExported;

            this.Name = documentTypes.Values.ToArray();
            var typesCount = this.Name.Count();
            this.Value = new int[typesCount];

            if (documents == null)
            {
                for (int i = 0; i < typesCount; i++) this.Value[i] = 0;

                return;
            }         

            for (var i=0; i< typesCount; i++)
            {
                this.Value[i] = documents.Any(d => d.Type == this.Name[i]) ? 1 : 0;
            }
        }
    }
}
