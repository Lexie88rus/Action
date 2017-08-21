using DataScrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseModel
{
    /// <summary>
    /// Класс для описания фичи
    /// </summary>
    public class Feature : IFeature
    {
        /// <summary>
        /// Значение фичи
        /// </summary>
        public int[] Value { get;}

        /// <summary>
        /// Название фичи
        /// </summary>
        public string[] Name { get;}

        /// <summary>
        /// Экспортировать ли фичу в файл с результатом
        /// </summary>
        public bool ToBeExported { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="value">Значение фичи</param>
        /// <param name="name">Название фичи</param>
        /// <param name="toBeExported">Экспортировать ли фичу, по умолчинию "true"</param>
        public Feature(int value, string name, bool toBeExported = true)
        {
            this.Value = new int[1];
            this.Value[0] = value;

            this.Name = new string[1];
            this.Name[0] = name;

            this.ToBeExported = toBeExported;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="value">Значение фичи</param>
        /// <param name="name">Название фичи</param>
        /// <param name="toBeExported">Экспортировать ли фичу, по умолчинию "true"</param>
        public Feature(bool value, string name, bool toBeExported = true)
        {
            this.Value = new int[1];
            this.Value[0] = value ? 1 : 0;

            this.Name = new string[1];
            this.Name[0] = name;

            this.ToBeExported = toBeExported;
        }

        /// <summary>
        /// Конструктор из массива значений
        /// </summary>
        /// <param name="value">Значение фичи</param>
        /// <param name="name">Название фичи</param>
        /// <param name="toBeExported">Экспортировать ли фичу, по умолчинию "true"</param>
        public Feature(IEnumerable<int> value, IEnumerable<string> name, bool toBeExported = true)
        {
            this.Value = new int [value.Count()];
            for (var i = 0; i < value.Count(); i++) this.Value[i] = value.ElementAt(i);

            this.Name = new string[name.Count()];
            for (var i = 0; i < name.Count(); i++) this.Name[i] = name.ElementAt(i);

            this.ToBeExported = toBeExported;
        }

        /// <summary>
        /// Конструктор из массива значений
        /// </summary>
        /// <param name="value">Значение фичи</param>
        /// <param name="name">Название фичи</param>
        /// <param name="toBeExported">Экспортировать ли фичу, по умолчинию "true"</param>
        public Feature(IEnumerable<bool> value, IEnumerable<string> name, bool toBeExported = true)
        {
            this.Value = new int[value.Count()];
            for (var i = 0; i < value.Count(); i++) this.Value[i] = value.ElementAt(i) ? 1 : 0;

            this.Name = new string[name.Count()];
            for (var i = 0; i < name.Count(); i++) this.Name[i] = name.ElementAt(i);

            this.ToBeExported = toBeExported;
        }
    }

    /// <summary>
    /// Фича для представления документов
    /// </summary>
    public class DocumentsFeature : IFeature
    {
        /// <summary>
        /// Значение
        /// </summary>
        public int[] Value { get; }

        /// <summary>
        /// Названия документов
        /// </summary>
        public string[] Name { get; }

        /// <summary>
        /// Экспортировать ли в файл с результатами
        /// </summary>
        public bool ToBeExported { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="documents">Документы</param>
        /// <param name="documentTypes">Типы документов</param>
        /// <param name="toBeExported">Экспортировать ли</param>
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
