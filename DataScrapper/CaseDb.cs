using CaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScrapper
{
    /// <summary>
    /// Контейнер для сырых данных по делу из БД
    /// </summary>
    public class CaseDb
    {
        /// <summary>
        /// Идентификатор дела
        /// </summary>
        public int RID { get; set; }

        /// <summary>
        /// Guid дела
        /// </summary>
        public string CaseId { get; set; }

        /// <summary>
        /// Дата регистрации дела в первой инстанции
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Заявленная сумма иска
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Идентификатор категории дела
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Идентификатор суда
        /// </summary>
        public int CourtId { get; set; }

        /// <summary>
        /// Название файла с решением суда по делу
        /// </summary>
        public IList<string> SolutionFilename { get; set; }

        /// <summary>
        /// Истцы
        /// </summary>
        public IList<Participant> Claimant { get; set; }

        /// <summary>
        /// Ответчики
        /// </summary>
        public IList<Participant> Responent { get; set; }

        /// <summary>
        /// Третья сторона
        /// </summary>
        public IList<Participant> ThirdParty { get; set; }

        /// <summary>
        /// Документы по делу из арбитра
        /// </summary>
        public IList<Document> Documents { get; set; }

        /// <summary>
        /// Идентификатор финальной инстанции
        /// </summary>
        public int FinalInstanceId { get; set; }

        /// <summary>
        /// Направление на повторное рассмотрение
        /// </summary>
        public bool Reconsideration { get; set; }

        /// <summary>
        /// Приняты ли доказательства судьей
        /// </summary>
        public bool Petition { get; set; }

        /// <summary>
        /// Наличие встречного иска
        /// </summary>
        public bool Counterclaim { get; set; }

        public CaseDb()
        {
            this.Petition = false;
            this.Counterclaim = false;
            this.Reconsideration = false;
            this.Reconsideration = false;
        }

        private static int ToInt(DateTime date)
        {
            return date.Day + date.Month * 100 + date.Year * 10000;
        }

        public Case ConvertToCase(int decision, IDictionary<int, string> doctypes)
        {
            var features = new List<IFeature>();

            var RID = new Feature(this.RID, "RID");
            features.Add(RID);

            var amount = new Feature(this.Amount, "Amount");
            features.Add(amount);

            var date = new Feature(CaseDb.ToInt(this.Date), "Date");
            features.Add(date);

            var court = new Feature(this.CourtId, "Court");
            features.Add(court);

            var finalInstance = new Feature(this.FinalInstanceId, "FinalInstance");
            features.Add(finalInstance);

            var reconsideration = new Feature(this.Reconsideration, "Reconsideration");
            features.Add(reconsideration);

            var claimantNum = new Feature(this.Claimant == null ? 0 : this.Claimant.Count(), "ClaimantNum");
            features.Add(claimantNum);

            var docNum = new Feature(this.Documents == null ? 0 : this.Documents.Count(), "DocumentNum");
            features.Add(docNum);

            var docs = new DocumentsFeature(this.Documents, doctypes);
            features.Add(docs);

            var category = new Feature(this.CategoryId, "Category");
            features.Add(category);

            var thirdParty = new Feature(this.ThirdParty == null || this.ThirdParty.Count() == 0 ? 0 : 1, "ThirdParty");
            features.Add(thirdParty);

            var petition = new Feature(this.Petition, "Petition");
            features.Add(petition);

            var counterclaim = new Feature(this.Counterclaim, "Counterclaim");
            features.Add(counterclaim);

            var dec = new Feature(decision, "Decision");
            features.Add(dec);

            var cs = new Case(features);

            return cs;
        }
    }
}
