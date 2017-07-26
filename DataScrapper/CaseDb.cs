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
    }
}
